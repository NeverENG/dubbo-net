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

using DubboNet.Clients.DataModle;
using DubboNet.Clients.RegistryClient;

namespace UnitTestForDubboNet;

public class ProviderDiscoveryTest
{
    [Fact]
    public async Task ConcurrentCallers_ShareOneLookupAndRetryAfterCompletion()
    {
        var gate = new TaskCompletionSource<DubboServiceEndPointInfos>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        int lookups = 0;
        using var discovery = new ProviderDiscovery((_, _) =>
        {
            Interlocked.Increment(ref lookups);
            return gate.Task;
        });

        Task<DubboServiceEndPointInfos>[] callers = Enumerable.Range(0, 32)
            .Select(_ => discovery.GetProviderEndPointsAsync("com.foo.Service"))
            .ToArray();
        Assert.Equal(1, Volatile.Read(ref lookups));
        var snapshot = new DubboServiceEndPointInfos();
        gate.SetResult(snapshot);
        Assert.All(await Task.WhenAll(callers), result => Assert.Same(snapshot, result));
        Assert.Same(snapshot, await discovery.GetProviderEndPointsAsync("com.foo.Service"));
        Assert.Equal(2, Volatile.Read(ref lookups));
    }

    [Fact]
    public async Task FullPathLookup_DoesNotReuseServiceNameLookup()
    {
        var gate = new TaskCompletionSource<DubboServiceEndPointInfos>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        int lookups = 0;
        using var discovery = new ProviderDiscovery((_, _) =>
        {
            Interlocked.Increment(ref lookups);
            return gate.Task;
        });

        Task first = discovery.GetProviderEndPointsAsync("service");
        Task second = discovery.GetProviderEndPointsAsync("service", isFullPath: true);
        Assert.Equal(2, Volatile.Read(ref lookups));
        gate.SetResult(new DubboServiceEndPointInfos());
        await Task.WhenAll(first, second);
    }

    [Fact]
    public async Task Dispose_StopsNewLookupsButDoesNotAbandonAnInFlightCaller()
    {
        var gate = new TaskCompletionSource<DubboServiceEndPointInfos>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var discovery = new ProviderDiscovery((_, _) => gate.Task);
        Task<DubboServiceEndPointInfos> pending = discovery.GetProviderEndPointsAsync("service");

        discovery.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(() => discovery.GetProviderEndPointsAsync("service"));
        gate.SetResult(new DubboServiceEndPointInfos());
        Assert.NotNull(await pending);
    }
}
