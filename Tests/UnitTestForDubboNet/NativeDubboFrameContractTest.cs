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
    public class NativeDubboFrameContractTest
    {
        [Fact]
        public void HeartbeatRequest_HasExactWireHeaderAndBody()
        {
            byte[] frame = NativeDubboCodec.EncodeHeartbeat(0x0102030405060708);

            Assert.Equal(new byte[]
            {
                0xda, 0xbb, 0xe2, 0x00,
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x00, 0x00, 0x00, 0x01, (byte)'N'
            }, frame);
        }

        [Fact]
        public void HeartbeatResponse_PreservesSerializationMaskAndStatus()
        {
            byte[] frame = NativeDubboCodec.EncodeHeartbeatResponse(7, 0xe2);

            Assert.Equal(new byte[]
            {
                0xda, 0xbb, 0x22, 0x14,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07,
                0x00, 0x00, 0x00, 0x01, (byte)'N'
            }, frame);
        }

        [Fact]
        public void DecodeHeader_ExtractsFlagsSerializationAndFieldsFromRawBytes()
        {
            byte[] header =
            {
                0xda, 0xbb, 0xe2, 0x14,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07,
                0x00, 0x00, 0x00, 0x01
            };

            NativeDubboFrame decoded = NativeDubboCodec.DecodeFrameHeader(header, new byte[] { (byte)'N' });

            Assert.True(decoded.IsRequest);
            Assert.True(decoded.IsTwoWay);
            Assert.True(decoded.IsEvent);
            Assert.Equal(2, decoded.SerializationId);
            Assert.Equal(20, decoded.Status);
            Assert.Equal(7, decoded.RequestId);
        }

        [Theory]
        [InlineData(0x00, 0xbb)]
        [InlineData(0xda, 0x00)]
        public void DecodeHeader_RejectsEitherIncorrectMagicByte(byte first, byte second)
        {
            byte[] header = new byte[16];
            header[0] = first;
            header[1] = second;

            Assert.Throws<InvalidDataException>(() => NativeDubboCodec.DecodeFrameHeader(header, Array.Empty<byte>()));
        }
    }
}
