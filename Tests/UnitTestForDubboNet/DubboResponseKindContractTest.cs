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

using DubboNet.DubboService.Native;
using System.IO;

namespace UnitTestForDubboNet
{
    public class DubboResponseKindContractTest
    {
        // 原始字节独立于生产常量：紧凑整数 kind，随后是可选字符串和 map。
        // EN: Raw wire bytes independent of production constants: compact kind, optional string and map.
        [Theory]
        [InlineData(0, "900165", true, false, false)]
        [InlineData(1, "910176", false, true, false)]
        [InlineData(2, "92", false, false, false)]
        [InlineData(3, "93016548016b01765a", true, false, true)]
        [InlineData(4, "94017648016b01765a", false, true, true)]
        [InlineData(5, "9548016b01765a", false, false, true)]
        public void DecodeResponse_PreservesEachKindAndItsPayloadOrder(
            int kind, string hex, bool hasException, bool hasValue, bool hasAttachments)
        {
            NativeDubboResponse response = NativeDubboCodec.DecodeResponse(new NativeDubboFrame
            {
                RequestId = 7,
                Status = 20,
                SerializationId = 2,
                Body = Convert.FromHexString(hex)
            });

            Assert.Equal(kind, response.ResultKind);
            Assert.Equal(7, response.RequestId);
            Assert.Equal(hasException ? "e" : null, response.Exception);
            Assert.Equal(hasValue ? "v" : null, response.Value);
            if (hasAttachments)
            {
                Assert.Equal("v", response.Attachments["k"]);
            }
            else
            {
                Assert.Null(response.Attachments);
            }
        }

        [Fact]
        public void DecodeResponse_RejectsUnknownKindWithoutChangingErrorText()
        {
            InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
                NativeDubboCodec.DecodeResponse(new NativeDubboFrame
                {
                    Status = 20,
                    SerializationId = 2,
                    Body = new byte[] { 0x96 }
                }));

            Assert.Equal("Unknown Dubbo response result kind 6; expected 0 through 5.", error.Message);
        }
    }
}
