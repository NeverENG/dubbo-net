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
    /// Dubbo2 wire frame layout and values; not configurable size or safety limits.
    /// </summary>
    internal static class DubboFrameConstants
    {
        internal const int HeaderLength = 16;
        internal const int MagicFirstOffset = 0;
        internal const int MagicSecondOffset = 1;
        internal const int FlagsOffset = 2;
        internal const int StatusOffset = 3;
        internal const int RequestIdOffset = 4;
        internal const int RequestIdLength = 8;
        internal const int BodyLengthOffset = 12;
        internal const int BodyLengthLength = 4;

        internal const byte MagicFirstByte = 0xda;
        internal const byte MagicSecondByte = 0xbb;
        internal const byte RequestFlag = 0x80;
        internal const byte TwoWayFlag = 0x40;
        internal const byte EventFlag = 0x20;
        internal const byte SerializationMask = 0x1f;
        internal const byte Hessian2SerializationId = 2;
        internal const byte OkStatus = 20;
    }
}
