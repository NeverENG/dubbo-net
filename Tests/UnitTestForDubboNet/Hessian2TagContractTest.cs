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

namespace UnitTestForDubboNet;

public class Hessian2TagContractTest
{
    // These literal wire bytes intentionally do not reference production constants.
    [Theory]
    [InlineData("443FF0000000000000", 1d)] // 'D' (0x44): full double
    [InlineData("5B", 0d)]               // compact zero
    [InlineData("5C", 1d)]               // compact one
    [InlineData("5DFF", -1d)]            // compact byte
    [InlineData("5E0100", 256d)]         // compact short
    [InlineData("5F000003E8", 1d)]       // compact millisecond precision
    [InlineData("5F000009C4", 2.5d)]    // Dubbo BC_DOUBLE_MILL, not IEEE-754 float
    [InlineData("5F000000FA", 0.25d)]
    public void DoubleTagsDecodeFromIndependentWireBytes(string hex, double expected)
    {
        var reader = new DubboHessian2Reader(Convert.FromHexString(hex));
        Assert.Equal(expected, Assert.IsType<double>(reader.ReadValue()));
    }

    [Theory]
    [InlineData("5501614E5A", 1)]               // typed, variable length
    [InlineData("560161914E", 1)]               // typed, fixed length
    [InlineData("574E5A", 1)]                   // untyped, variable length
    [InlineData("58914E", 1)]                   // untyped, fixed length
    [InlineData("700161", 0)]                   // compact typed, zero elements
    [InlineData("7701614E4E4E4E4E4E4E", 7)]   // compact typed, seven elements
    [InlineData("78", 0)]                       // compact untyped, zero elements
    [InlineData("7F4E4E4E4E4E4E4E", 7)]       // compact untyped, seven elements
    public void ListTagBoundariesDecodeFromIndependentWireBytes(string hex, int expectedLength)
    {
        var reader = new DubboHessian2Reader(Convert.FromHexString(hex));
        var list = Assert.IsType<List<object>>(reader.ReadValue());
        Assert.Equal(expectedLength, list.Count);
        Assert.All(list, value => Assert.Null(value));
    }

    [Theory]
    [InlineData("80", -16)]
    [InlineData("BF", 47)]
    [InlineData("C000", -2048)]
    [InlineData("CFFF", 2047)]
    [InlineData("D00000", -262144)]
    [InlineData("D7FFFF", 262143)]
    [InlineData("4900000001", 1)] // 'I': full integer
    public void IntegerRangeBoundariesDecodeFromIndependentWireBytes(string hex, int expected)
    {
        var reader = new DubboHessian2Reader(Convert.FromHexString(hex));
        Assert.Equal(expected, Assert.IsType<int>(reader.ReadValue()));
    }

    [Theory]
    [InlineData("D8", -8L)]
    [InlineData("EF", 15L)]
    [InlineData("F000", -2048L)]
    [InlineData("FF00", 1792L)] // last two-byte long tag
    [InlineData("FFFF", 2047L)]
    [InlineData("380000", -262144L)]
    [InlineData("3FFFFF", 262143L)]
    [InlineData("5900000001", 1L)] // 32-bit long
    [InlineData("4C0000000000000001", 1L)] // 'L': full long
    public void LongRangeBoundariesDecodeFromIndependentWireBytes(string hex, long expected)
    {
        var reader = new DubboHessian2Reader(Convert.FromHexString(hex));
        Assert.Equal(expected, Assert.IsType<long>(reader.ReadValue()));
    }

    [Theory]
    [InlineData("90")]
    [InlineData("C800")]
    [InlineData("D40000")]
    public void IntegerZeroPointTagsDecodeToZero(string hex)
    {
        var reader = new DubboHessian2Reader(Convert.FromHexString(hex));
        Assert.Equal(0, Assert.IsType<int>(reader.ReadValue()));
    }

    [Theory]
    [InlineData("E0")]
    [InlineData("F800")]
    [InlineData("3C0000")]
    public void LongZeroPointTagsDecodeToZero(string hex)
    {
        var reader = new DubboHessian2Reader(Convert.FromHexString(hex));
        Assert.Equal(0L, Assert.IsType<long>(reader.ReadValue()));
    }

    [Fact]
    public void CompactObjectFirstTagRefersToTheFirstClassDefinition()
    {
        // Class definition 'a', zero fields, then a compact object at index zero.
        var reader = new DubboHessian2Reader(Convert.FromHexString("4301619060"));
        var instance = Assert.IsType<Dictionary<object, object>>(reader.ReadValue());
        Assert.Equal("a", instance["class"]);
    }

    [Theory]
    [InlineData("30", 0)]
    [InlineData("31", 256)]
    [InlineData("33", 768)]
    public void CompactStringLengthIncludesTagHighByte(string tagHex, int length)
    {
        byte[] wire = Convert.FromHexString(tagHex)
            .Concat(new byte[] { 0 })
            .Concat(Enumerable.Repeat((byte)'a', length))
            .ToArray();
        var reader = new DubboHessian2Reader(wire);
        Assert.Equal(new string('a', length), reader.ReadValue());
    }

    [Theory]
    [InlineData("34", 0)]
    [InlineData("35", 256)]
    [InlineData("37", 768)]
    public void CompactBinaryLengthIncludesTagHighByte(string tagHex, int length)
    {
        byte[] payload = Enumerable.Repeat((byte)0xab, length).ToArray();
        byte[] wire = Convert.FromHexString(tagHex)
            .Concat(new byte[] { 0 })
            .Concat(payload)
            .ToArray();
        var reader = new DubboHessian2Reader(wire);
        Assert.Equal(payload, Assert.IsType<byte[]>(reader.ReadValue()));
    }

    [Theory]
    [InlineData("31")]
    [InlineData("35")]
    public void CompactLengthMissingLowByteStillThrowsEndOfStream(string tagHex)
    {
        var reader = new DubboHessian2Reader(Convert.FromHexString(tagHex));
        Assert.Throws<EndOfStreamException>(() => reader.ReadValue());
    }

    [Fact]
    public void AdjacentBooleanTagIsNotAList()
    {
        var reader = new DubboHessian2Reader(Convert.FromHexString("54"));
        Assert.Equal(true, reader.ReadValue());
    }
}
