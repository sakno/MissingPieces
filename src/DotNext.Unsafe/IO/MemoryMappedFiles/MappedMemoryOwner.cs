﻿using System;
using System.Buffers;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace DotNext.IO.MemoryMappedFiles
{
    using Runtime.InteropServices;

    internal unsafe sealed class MappedMemoryOwner : MemoryManager<byte>, IMappedMemoryOwner
    {
        private readonly MemoryMappedViewAccessor accessor;
        private readonly int length;
        private readonly byte* ptr;

        internal MappedMemoryOwner(MemoryMappedViewAccessor accessor)
        {
            if (accessor.Capacity > int.MaxValue)
                throw new ArgumentException(ExceptionMessages.SegmentVeryLarge, nameof(accessor));
            length = (int)accessor.Capacity;
            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
            this.accessor = accessor;
        }

        long IUnmanagedMemory.Size => length;

        public Pointer<byte> Pointer => new Pointer<byte>(ptr + accessor.PointerOffset);

        Span<byte> IUnmanagedMemory.Bytes => GetSpan();

        public Stream AsStream() => Pointer.AsStream(length, accessor.GetFileAccess());

        public void Flush() => accessor.Flush();

        public override Span<byte> GetSpan() => Pointer.ToSpan(length);

        public override MemoryHandle Pin(int elementIndex) => Pointer.GetHandle(elementIndex);

        public override void Unpin()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                accessor.Dispose();
            }
        }
    }
}
