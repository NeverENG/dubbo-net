/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements. See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using DubboNet.Clients;
using DubboNet.Clients.DataModle;
using MyCommonHelper;
using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DubboNet.Clients.RegistryClient
{
    // Owns provider discovery and per-service in-flight lookups. The client owns
    // the registry leases and the callback that publishes changes to drivers/metadata.
    internal sealed class ProviderDiscovery
    {
        private readonly MyZookeeper _zookeeper;
        private readonly MyNacos _nacos;
        private readonly string _rootPath;
        private readonly object _subscriber;
        private readonly DubboClientZookeeperWatcher _watcher;
        private readonly Func<string, Task> _onNacosChanged;
        private readonly Func<bool> _shouldReload;
        private readonly Func<Task> _reload;
        private readonly ConcurrentDictionary<string, Task<DubboServiceEndPointInfos>> _inFlight = new();

        internal ProviderDiscovery(MyZookeeper zookeeper, MyNacos nacos, string rootPath,
            object subscriber, DubboClientZookeeperWatcher watcher, Func<string, Task> onNacosChanged,
            Func<bool> shouldReload, Func<Task> reload)
        {
            _zookeeper = zookeeper;
            _nacos = nacos;
            _rootPath = rootPath;
            _subscriber = subscriber;
            _watcher = watcher;
            _onNacosChanged = onNacosChanged;
            _shouldReload = shouldReload;
            _reload = reload;
        }

        internal bool TryGetServiceNameFromProviderPath(
            string providerPath,
            out string serviceName)
        {
            serviceName = null;
            if (string.IsNullOrEmpty(providerPath)
                || !providerPath.StartsWith(_rootPath, StringComparison.Ordinal)
                || !providerPath.EndsWith(ZooKeeperRegistryPaths.ProvidersSuffix, StringComparison.Ordinal))
            {
                return false;
            }

            serviceName = providerPath.Substring(
                _rootPath.Length,
                providerPath.Length - _rootPath.Length - ZooKeeperRegistryPaths.ProvidersSuffix.Length);
            return !string.IsNullOrWhiteSpace(serviceName);
        }

        internal async Task<DubboServiceEndPointInfos> GetProviderEndPointsAsync(string serviceName, bool isFullPath = false)
        {
            Task<DubboServiceEndPointInfos> getProviderEndPointsTask =
                _inFlight.GetOrAdd(
                    serviceName,
                    _ => DiscoverAsync(serviceName, isFullPath));
            try
            {
                return await getProviderEndPointsTask.ConfigureAwait(false);
            }
            finally
            {
                _inFlight.TryRemove(
                    new KeyValuePair<string, Task<DubboServiceEndPointInfos>>(
                        serviceName,
                        getProviderEndPointsTask));
            }
        }

        /// <summary>
        /// 从注册中心发现 Provider 地址；服务存在时安装更新观察器。
        /// EN: Discovers provider endpoints in the registry and installs the provider watcher when the service exists.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="isFullPath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<DubboServiceEndPointInfos> DiscoverAsync(string serviceName ,bool isFullPath = false)
        {
            if(string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException(nameof(serviceName));
            }
            if (_nacos != null)
            {
                DubboServiceEndPointInfos nacosResult = new DubboServiceEndPointInfos();
                if (isFullPath)
                {
                    nacosResult.ErrorInfo =
                        $"Nacos discovery expects a Dubbo interface name instead of a ZooKeeper path: {serviceName}";
                    return nacosResult;
                }
                try
                {
                    IReadOnlyList<DubboServiceEndPointInfo> endpoints =
                        await _nacos.GetProviderEndPointsAsync(
                            serviceName,
                            _rootPath,
                            _subscriber,
                            _onNacosChanged).ConfigureAwait(false);
                    nacosResult.EndPoints.AddRange(endpoints);
                    if (nacosResult.EndPoints.Count == 0)
                    {
                        nacosResult.ErrorInfo =
                            $"no healthy provider endpoint for [{serviceName}] in Nacos group " +
                            $"[{MyNacos.NormalizeGroup(_rootPath, NacosRegistryDefaults.DefaultGroup)}]";
                    }
                }
                catch (Exception exception)
                {
                    nacosResult.ErrorInfo =
                        $"can not get [{serviceName}] from Nacos: {exception.Message}";
                }
                return nacosResult;
            }
            string nowFullPath = isFullPath
                ? serviceName
                : $"{_rootPath}{serviceName}{ZooKeeperRegistryPaths.ProvidersSuffix}";
            DubboServiceEndPointInfos serviceEndPointsInfo = new DubboServiceEndPointInfos();
            Stat stat =await _zookeeper.ExistsAsync(nowFullPath);
            if (stat==null)
            {
                if(!_zookeeper.IsConnected)
                {
                    serviceEndPointsInfo.ErrorInfo = $"can not get [{serviceName}] that _zookeeper can not connect";
                }
                else if((await _zookeeper.ExistsAsync($"{_rootPath}{serviceName}"))==null)
                {
                    serviceEndPointsInfo.ErrorInfo = $"serviceName [{serviceName}] is error";
                }
                else
                {
                    serviceEndPointsInfo.ErrorInfo = $"no provider in [{serviceName}]";
                }
            }
            else if(stat.getNumChildren()<=0)
            {
                serviceEndPointsInfo.ErrorInfo = $"no provider endpoint in [{serviceName}]";
            }
            else
            {
                //注册中心之前已经断开了，现在连接上了，需要重新更新watch
                if(_shouldReload())
                {
                    _ = _reload();
                }

                ChildrenResult childrenResult = await _zookeeper.GetChildrenAsync(nowFullPath,_watcher).ConfigureAwait(false);
                if (childrenResult == null)
                {
                    serviceEndPointsInfo.ErrorInfo = $"GetChildrenAsync error [{serviceName}]";
                }
                else
                {
                    foreach(var child in childrenResult.Children)
                    {
                        if (child.StartsWith("dubbo%3A%2F%2F") || child.StartsWith("tri%3A%2F%2F"))
                        {
                            string nowDubboPath = System.Web.HttpUtility.UrlDecode(child, System.Text.Encoding.UTF8);
                            Uri nowDubboUri;
                            if(Uri.TryCreate(nowDubboPath, UriKind.Absolute, out nowDubboUri))
                            {
                                if (IPAddress.TryParse(nowDubboUri.Host, out _))
                                {
                                    serviceEndPointsInfo.EndPoints.Add(DubboServiceEndPointInfo.GetDubboServiceEndPointInfo(nowDubboUri));
                                }
                                else
                                {
                                    //这里如果有使用域名或主机名称的可能性，这里可以继续解析为IP
                                    MyLogger.LogWarning($"[GetSeviceProviderEndPoints] IPAddress.TryParse error {child}");
                                    continue;
                                }
                            }
                            else
                            {
                                MyLogger.LogWarning($"[GetSeviceProviderEndPoints] Uri.TryCreate error {child}");
                                continue;
                            }
                        }
                        else
                        {
                            MyLogger.LogWarning($"[GetSeviceProviderEndPoints] childrenResult.Children formate error {child}");
                            continue;
                        }
                    }
                }
            }
            return serviceEndPointsInfo;
        }

    }
}
