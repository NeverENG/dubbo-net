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

namespace DubboNet.DubboService.Native.Protocol
{
    /// <summary>
    /// Dubbo 响应体的结果类型；是否携带附件由线上 kind 值决定。
    /// <para>EN: Wire result kinds in a Dubbo response body, including attachment variants.</para>
    /// </summary>
    internal static class DubboResponseKinds
    {
        internal const int Exception = 0;
        internal const int Value = 1;
        internal const int NullValue = 2;
        internal const int ExceptionWithAttachments = 3;
        internal const int ValueWithAttachments = 4;
        internal const int NullValueWithAttachments = 5;
    }
}
