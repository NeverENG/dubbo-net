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

using DubboNet.DubboService.Native.Hessian2;

namespace UnitTestForDubboNet;

public class Hessian2ByteCursorTest
{
    [Fact]
    public void PeekDoesNotAdvanceAndSequentialReadsConsumeExactlyOneByte()
    {
        var input = new Hessian2ByteCursor(new byte[] { 0x80, 0x01 });
        Assert.Equal(2, input.Remaining);
        Assert.Equal(0x80, input.PeekByte());
        Assert.Equal(0x80, input.PeekByte());
        Assert.Equal(2, input.Remaining);
        Assert.Equal(0x80, input.ReadByte());
        Assert.Equal(1, input.Remaining);
        Assert.Equal(0x01, input.ReadByte());
        Assert.Equal(0, input.Remaining);
    }

    [Fact]
    public void FixedWidthIntegersUseBigEndianOrderAndAdvanceOnce()
    {
        var input = new Hessian2ByteCursor(Convert.FromHexString(
            "1234010203040102030405060708FF"));
        Assert.Equal(0x1234, input.ReadUInt16());
        Assert.Equal(0x01020304, input.ReadInt32());
        Assert.Equal(0x0102030405060708L, input.ReadInt64());
        Assert.Equal(0xff, input.PeekByte());
        Assert.Equal(1, input.Remaining);
    }

    [Fact]
    public void ReadBytesCopiesWithoutChangingTheSourceBuffer()
    {
        byte[] source = { 0x11, 0x22, 0x33 };
        var input = new Hessian2ByteCursor(source);
        byte[] copy = input.ReadBytes(2);
        source[0] = 0xff;
        Assert.Equal(new byte[] { 0x11, 0x22 }, copy);
        Assert.Equal(0x33, input.ReadByte());
        Assert.Equal(0, input.Remaining);
    }

    [Fact]
    public void CopyToWritesOnlyRequestedBytesAndLeavesNextTagUnread()
    {
        var input = new Hessian2ByteCursor(new byte[] { 0xab, 0xcd, 0x54 });
        using var destination = new MemoryStream();
        input.CopyTo(destination, 2);
        Assert.Equal(new byte[] { 0xab, 0xcd }, destination.ToArray());
        Assert.Equal(0x54, input.PeekByte());
        Assert.Equal(1, input.Remaining);
        input.CopyTo(destination, 0);
        Assert.Equal(1, input.Remaining);
    }

    [Fact]
    public void ConstructorPreservesTheExistingNullArgumentName()
    {
        var error = Assert.Throws<ArgumentNullException>(() => new Hessian2ByteCursor(null!));
        Assert.Equal("buffer", error.ParamName);
    }

    [Theory]
    [InlineData(0)] // Peek
    [InlineData(1)] // Single byte
    [InlineData(2)] // Two-byte integer
    [InlineData(4)] // Four-byte integer
    [InlineData(8)] // Eight-byte integer
    public void TruncatedReadsDoNotConsumeInput(int width)
    {
        var input = new Hessian2ByteCursor(new byte[] { 0x12 });
        Assert.Equal(0x12, input.PeekByte());
        Assert.Equal(0x12, input.ReadByte());
        Assert.Equal(0, input.Remaining);
        Action read = width switch
        {
            0 => () => input.PeekByte(),
            1 => () => input.ReadByte(),
            2 => () => input.ReadUInt16(),
            4 => () => input.ReadInt32(),
            _ => () => input.ReadInt64()
        };
        var error = Assert.Throws<EndOfStreamException>(read);
        Assert.Equal("Unexpected end of Hessian2 payload.", error.Message);
        Assert.Equal(0, input.Remaining);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidCopyOrReadLengthDoesNotConsumeInput(int length)
    {
        var input = new Hessian2ByteCursor(new byte[] { 0x12 });
        using var destination = new MemoryStream();
        Assert.Throws<EndOfStreamException>(() => input.ReadBytes(length));
        Assert.Throws<EndOfStreamException>(() => input.CopyTo(destination, length));
        Assert.Equal(0, destination.Length);
        Assert.Equal(1, input.Remaining);
        Assert.Equal(0x12, input.ReadByte());
    }
}
