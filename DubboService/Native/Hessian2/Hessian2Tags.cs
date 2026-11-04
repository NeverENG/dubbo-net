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

namespace DubboNet.DubboService.Native.Hessian2
{
    // Wire-format values used by the dynamic Hessian2 response reader.
    internal static class Hessian2Tags
    {
        internal const byte Null = (byte)'N';
        internal const byte True = (byte)'T';
        internal const byte False = (byte)'F';
        internal const byte End = (byte)'Z';
        internal const byte Reference = 0x51;

        internal const byte Int = (byte)'I';
        internal const byte CompactIntFirstTag = 0x80;
        internal const byte CompactIntLastTag = 0xbf;
        internal const byte CompactIntZeroTag = 0x90;
        internal const byte CompactIntTwoByteFirstTag = 0xc0;
        internal const byte CompactIntTwoByteLastTag = 0xcf;
        internal const byte CompactIntTwoByteZeroTag = 0xc8;
        internal const byte CompactIntThreeByteFirstTag = 0xd0;
        internal const byte CompactIntThreeByteLastTag = 0xd7;
        internal const byte CompactIntThreeByteZeroTag = 0xd4;

        internal const byte Long = (byte)'L';
        internal const byte LongAsInt = 0x59;
        internal const byte CompactLongFirstTag = 0xd8;
        internal const byte CompactLongLastTag = 0xef;
        internal const byte CompactLongZeroTag = 0xe0;
        internal const byte CompactLongTwoByteFirstTag = 0xf0;
        internal const byte CompactLongTwoByteLastTag = 0xff;
        internal const byte CompactLongTwoByteZeroTag = 0xf8;
        internal const byte CompactLongThreeByteFirstTag = 0x38;
        internal const byte CompactLongThreeByteLastTag = 0x3f;
        internal const byte CompactLongThreeByteZeroTag = 0x3c;

        internal const byte Double = (byte)'D'; // 0x44
        internal const byte DoubleZero = 0x5b;
        internal const byte DoubleOne = 0x5c;
        internal const byte DoubleAsByte = 0x5d;
        internal const byte DoubleAsShort = 0x5e;
        // Dubbo hessian-lite uses 0x5f as BC_DOUBLE_MILL (32-bit integer / 1000),
        // unlike the Caucho bytecode map, which describes a 32-bit float here.
        internal const byte DoubleAsMillisecond = 0x5f;
        internal const byte DateMilliseconds = 0x4a;
        internal const byte DateMinutes = 0x4b;

        internal const byte String = (byte)'S';
        internal const byte StringChunk = (byte)'R';
        internal const byte ShortStringLastTag = 0x1f;
        internal const byte CompactStringFirstTag = 0x30;
        internal const byte CompactStringLastTag = 0x33;
        internal const byte Binary = (byte)'B';
        internal const byte BinaryChunk = (byte)'A';
        internal const byte ShortBinaryFirstTag = 0x20;
        internal const byte ShortBinaryLastTag = 0x2f;
        internal const byte CompactBinaryFirstTag = 0x34;
        internal const byte CompactBinaryLastTag = 0x37;

        internal const byte TypedVariableList = 0x55;
        internal const byte TypedFixedList = 0x56;
        internal const byte UntypedVariableList = 0x57;
        internal const byte UntypedFixedList = 0x58;
        internal const byte CompactTypedListFirstTag = 0x70;
        internal const byte CompactTypedListLastTag = 0x77;
        internal const byte CompactUntypedListFirstTag = 0x78;
        internal const byte CompactUntypedListLastTag = 0x7f;

        internal const byte ClassDefinition = (byte)'C';
        internal const byte UntypedMap = (byte)'H';
        internal const byte TypedMap = (byte)'M';
        internal const byte Object = (byte)'O';
        internal const byte CompactObjectFirstTag = 0x60;
        internal const byte CompactObjectLastTag = 0x6f;
    }
}
