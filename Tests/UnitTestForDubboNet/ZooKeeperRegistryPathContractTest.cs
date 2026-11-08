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
using DubboNet.Clients.RegistryClient;
using DubboNet.Registry;

namespace UnitTestForDubboNet
{
    public class ZooKeeperRegistryPathContractTest
    {
        [Fact]
        public void RegistryImplementationHasItsOwnNamespace_WithoutMovingPublicClients()
        {
            Assert.Equal("DubboNet.Registry", typeof(ProviderDiscovery).Namespace);
            Assert.Equal("DubboNet.Registry", typeof(RegistryCategories).Namespace);
            Assert.Equal("DubboNet.Registry", typeof(ConsumerRegistrationConfiguration).Namespace);
            Assert.Equal("DubboNet.Clients.RegistryClient", typeof(MyNacos).Namespace);
            Assert.Equal("DubboNet.Clients.RegistryClient", typeof(MyZookeeper).Namespace);
            Assert.Equal("DubboNet.Clients.RegistryClient", typeof(ZNode).Namespace);
        }

        [Fact]
        public void RegistryCategories_AreSharedWithoutIncludingZooKeeperPathSeparators()
        {
            Assert.Equal("providers", RegistryCategories.Providers);
            Assert.Equal("consumers", RegistryCategories.Consumers);
            Assert.Equal("/providers", ZooKeeperRegistryPaths.ProvidersSuffix);
            Assert.Equal("/consumers/", ZooKeeperRegistryPaths.ConsumersDirectory);
        }

        [Theory]
        [InlineData(null, "/dubbo/", "/dubbo")]
        [InlineData("apps", "/apps/", "/apps")]
        [InlineData(" /apps/ ", "/apps/", "/apps")]
        public void RegistryRoots_PreserveExistingZooKeeperNormalization(
            string root, string expectedProviderRoot, string expectedMetadataRoot)
        {
            var config = new DubboClient.DubboClientConf { DubboRootPath = root };

            Assert.Equal((expectedProviderRoot, expectedMetadataRoot),
                DubboClient.ResolveRegistryPaths(false, false, config));
        }

        [Fact]
        public void ProviderChangePath_RecognizesOnlyProviderNodesWithinConfiguredRoot()
        {
            using var client = new DubboClient("127.0.0.1:2181",
                new DubboClient.DubboClientConf { DubboRootPath = "/apps/" });

            Assert.True(client.TryGetServiceNameFromProviderPath(
                "/apps/com.foo.Service/providers", out string serviceName));
            Assert.Equal("com.foo.Service", serviceName);
            Assert.False(client.TryGetServiceNameFromProviderPath("/apps//providers", out _));
            Assert.False(client.TryGetServiceNameFromProviderPath("/dubbo/com.foo.Service/providers", out _));
            Assert.False(client.TryGetServiceNameFromProviderPath("/apps/com.foo.Service/consumers", out _));
            Assert.False(client.TryGetServiceNameFromProviderPath("/apps/com.foo.Service/providers/child", out _));
        }

        [Fact]
        public void ConsumerRegistration_PreservesZooKeeperPathAndNacosServiceName()
        {
            var options = new ConsumerRegistrationOptions
            {
                Enabled = true,
                ApplicationName = "orders-consumer",
                Host = "127.0.0.1",
                InstanceId = "test-instance"
            }.Snapshot();
            var registration = new ConsumerRegistration(options, "owner", "/apps/",
                "com.foo.Service", "1", "prod");

            Assert.Equal("/apps/com.foo.Service/consumers/" + Uri.EscapeDataString(registration.ConsumerUrl),
                registration.ZooKeeperPath);
            Assert.Equal("consumers:com.foo.Service:1:prod", registration.NacosServiceName);
            Assert.Equal("consumers", registration.Metadata["category"]);
        }
    }
}
