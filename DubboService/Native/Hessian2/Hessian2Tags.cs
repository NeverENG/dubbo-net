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
        internal const byte CompactIntMin = 0x80;
        internal const byte CompactIntMax = 0xbf;
        internal const byte CompactIntBase = 0x90;
        internal const byte CompactIntTwoByteMin = 0xc0;
        internal const byte CompactIntTwoByteMax = 0xcf;
        internal const byte CompactIntTwoByteBase = 0xc8;
        internal const byte CompactIntThreeByteMin = 0xd0;
        internal const byte CompactIntThreeByteMax = 0xd7;
        internal const byte CompactIntThreeByteBase = 0xd4;

        internal const byte Long = (byte)'L';
        internal const byte LongAsInt = 0x59;
        internal const byte CompactLongMin = 0xd8;
        internal const byte CompactLongMax = 0xef;
        internal const byte CompactLongBase = 0xe0;
        internal const byte CompactLongTwoByteMin = 0xf0;
        internal const byte CompactLongTwoByteBase = 0xf8;
        internal const byte CompactLongThreeByteMin = 0x38;
        internal const byte CompactLongThreeByteMax = 0x3f;
        internal const byte CompactLongThreeByteBase = 0x3c;

        internal const byte Double = (byte)'D'; // 0x44
        internal const byte DoubleZero = 0x5b;
        internal const byte DoubleOne = 0x5c;
        internal const byte DoubleAsByte = 0x5d;
        internal const byte DoubleAsShort = 0x5e;
        internal const byte DoubleAsMillisecond = 0x5f;
        internal const byte DateMilliseconds = 0x4a;
        internal const byte DateMinutes = 0x4b;

        internal const byte String = (byte)'S';
        internal const byte StringChunk = (byte)'R';
        internal const byte ShortStringMax = 0x1f;
        internal const byte CompactStringMin = 0x30;
        internal const byte CompactStringMax = 0x33;
        internal const byte Binary = (byte)'B';
        internal const byte BinaryChunk = (byte)'A';
        internal const byte ShortBinaryMin = 0x20;
        internal const byte ShortBinaryMax = 0x2f;
        internal const byte CompactBinaryMin = 0x34;
        internal const byte CompactBinaryMax = 0x37;

        internal const byte TypedVariableList = 0x55;
        internal const byte TypedFixedList = 0x56;
        internal const byte UntypedVariableList = 0x57;
        internal const byte UntypedFixedList = 0x58;
        internal const byte CompactTypedListMin = 0x70;
        internal const byte CompactTypedListMax = 0x77;
        internal const byte CompactUntypedListMin = 0x78;
        internal const byte CompactUntypedListMax = 0x7f;

        internal const byte ClassDefinition = (byte)'C';
        internal const byte UntypedMap = (byte)'H';
        internal const byte TypedMap = (byte)'M';
        internal const byte Object = (byte)'O';
        internal const byte CompactObjectMin = 0x60;
        internal const byte CompactObjectMax = 0x6f;
    }
}
