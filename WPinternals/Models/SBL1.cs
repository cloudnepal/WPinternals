﻿// Copyright (c) 2018, Rene Lergner - @Heathcliff74xda
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;

namespace WPinternals
{
    internal class SBL1 : QualcommPartition
    {
        internal SBL1(byte[] Binary) : base(Binary, 0x2800) { }

        internal byte[] GenerateExtraSector(byte[] PartitionHeader)
        {
            UInt32? Offset = ByteOperations.FindPattern(Binary,
                [
                    0x15, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x05, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                ], null, null);
            if (Offset == null)
            {
                throw new BadImageFormatException();
            }

            UInt32 PartitionLoaderTableOffset = (UInt32)Offset;

            byte[] FoundPattern = new byte[0x10];
            Offset = ByteOperations.FindPattern(Binary,
                [
                    0x04, 0x00, 0x9F, 0xE5, 0x28, 0x00, 0xD0, 0xE5, 0x1E, 0xFF, 0x2F, 0xE1, 0xFF, 0xFF, 0xFF, 0xFF
                ],
                [
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF
                ],
                FoundPattern);
            if (Offset == null)
            {
                throw new BadImageFormatException();
            }

            UInt32 SharedMemoryAddress = ByteOperations.ReadUInt32(FoundPattern, 0x0C);
            UInt32 GlobalIsSecurityEnabledAddress = SharedMemoryAddress + 0x28;

            Offset = ByteOperations.FindPattern(Binary,
                [
                    0x01, 0xFF, 0xA0, 0xE3, 0xFF, 0xFF, 0xA0, 0xE1, 0x1C, 0xD0, 0x8D, 0xE2, 0xF0, 0x4F, 0xBD, 0xE8,
                    0x1E, 0xFF, 0x2F, 0xE1
                ],
                [
                    0x00, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00
                ],
                null);
            if (Offset == null)
            {
                throw new BadImageFormatException();
            }

            UInt32 ReturnAddress = (UInt32)Offset - ImageOffset + ImageAddress;

            byte[] Sector = new byte[0x200];
            Array.Clear(Sector, 0, 0x200);

            byte[] Content = [
                0x16, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x28, 0xBD, 0x02, 0x00,
                0xD8, 0x01, 0x00, 0x00, 0xD8, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0xE3, 0x3C, 0x10, 0x9F, 0xE5,
                0x00, 0x00, 0xC1, 0xE5, 0x38, 0x00, 0x9F, 0xE5, 0x38, 0x10, 0x9F, 0xE5, 0x00, 0x00, 0x81, 0xE5,
                0x34, 0x10, 0x9F, 0xE5, 0x00, 0x00, 0x81, 0xE5, 0x30, 0x00, 0x9F, 0xE5, 0x20, 0x10, 0x9F, 0xE5,
                0x2C, 0x30, 0x9F, 0xE5, 0x00, 0x20, 0x90, 0xE5, 0x00, 0x20, 0x81, 0xE5, 0x04, 0x00, 0x80, 0xE2,
                0x04, 0x10, 0x81, 0xE2, 0x03, 0x00, 0x50, 0xE1, 0xF9, 0xFF, 0xFF, 0xBA, 0x14, 0xF0, 0x9F, 0xE5,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x02, 0x00, 0x90, 0xBF, 0x02, 0x00, 0xD0, 0xBF, 0x02, 0x00,
                0xA0, 0xBD, 0x02, 0x00, 0xA0, 0xBE, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00
            ];

            byte[] PartitionTypeGuid = [
                0x74, 0x74, 0x74, 0x74, 0x74, 0x74, 0x74, 0x74, 0x74, 0x74, 0x74, 0x74, 0x74, 0x74, 0x74, 0x74
            ];

            Buffer.BlockCopy(Content, 0, Sector, 0, Content.Length);

            // Overwrite first part of partition-header with model-specific header
            Buffer.BlockCopy(PartitionHeader, 0, Sector, 0, PartitionHeader.Length);

            ByteOperations.WriteUInt32(Sector, 0x70, GlobalIsSecurityEnabledAddress);
            ByteOperations.WriteUInt32(Sector, 0x88, ReturnAddress);

            Buffer.BlockCopy(Binary, (int)PartitionLoaderTableOffset, Sector, 0xA0, 0x50);
            ByteOperations.WriteUInt32(Sector, 0xA0 + 0x30, 0);

            Buffer.BlockCopy(Binary, (int)PartitionLoaderTableOffset, Sector, 0xF0, 0x50);
            ByteOperations.WriteUInt32(Sector, 0xF0 + 0x2C, 0);
            ByteOperations.WriteUInt32(Sector, 0xF0 + 0x38, 0x210F0);

            Buffer.BlockCopy(PartitionTypeGuid, 0, Sector, 0x190, 0x10);

            ByteOperations.WriteUInt32(Sector, 0x1FC, 0x0002BD28);

            return Sector;
        }
    }
}
