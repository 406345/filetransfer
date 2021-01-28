using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace FileTransferProtocol
{
    public class BlockedStream
    {
        Semaphore semaphore = new Semaphore(0, 0xFFFF);
        int BUFFER_SIZE = 1024;
        byte[] buffer;
        UInt32 head = 0;
        UInt32 tail = 0;
        Queue<byte> buffer2 = new Queue<byte>();

        public BlockedStream(int size)
        {
            this.BUFFER_SIZE = size;
            this.buffer = new byte[this.BUFFER_SIZE];
        }

        public void reset()
        {
            head = 0;
            tail = 0;
        }

        public void write(byte[] buffer, int offset, int size)
        {
            //buffer2.Enqueue(buffer.AsSpan().Slice(offset,size).ToArray());
            //for (int i = 0; i < size; i++)
            //{
            //    this.buffer[tail % BUFFER_SIZE] = buffer[offset + i];
            //    ++tail;
            //}
        }

        public UInt32 length()
        {
            return (uint)this.buffer2.Count;
            return this.tail - this.head;
        }

        public byte[] read(uint size)
        { 
            if (this.length() < size) return null;

            return null;
            //return this.buffer2.
            //byte[] ret = new byte[size];

            //for (int i = 0; i < size; i++)
            //{
            //    ret[i] = this.buffer[(head % BUFFER_SIZE)];
            //    ++head;
            //}

            //return ret;
        }
        public ushort readUInt16()
        {
            const int size = 2;
            return BitConverter.ToUInt16(this.read(size), 0);
        }
        public uint readUInt32()
        {
            const int size = 4;
            return BitConverter.ToUInt32(this.read(size), 0);
        }
      
        public ulong readUInt64()
        {
            const int size = 8;
            return BitConverter.ToUInt64(this.read(size), 0);
        }

    }
}
