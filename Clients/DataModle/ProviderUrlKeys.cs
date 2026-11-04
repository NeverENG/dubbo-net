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

namespace DubboNet.Clients.DataModle
{
    /// <summary>
    /// Provider 注册 URL 中由 Nacos 转换器写入、端点解析器读取的查询参数键。
    /// <para>EN: Provider registry URL query keys shared by Nacos conversion and endpoint parsing.</para>
    /// </summary>
    internal static class ProviderUrlKeys
    {
        internal const string Interface = "interface";
        internal const string Disabled = "disabled";
        internal const string Dynamic = "dynamic";
        internal const string Weight = "weight";
    }
}
