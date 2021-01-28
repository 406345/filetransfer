using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileTransferProtocol
{
    public class FileMeta : BaseMessage
    {
        public UInt16 version { get; set; } = Consts.VERSION;
        public UInt32 blockSize { get; set; } = 1024;
        public String fileName { get; set; } = "";
        public UInt64 fileBlockCount { get; set; } = 0;
        public UInt64 fileSize { get; set; } = 0;
        public byte[] hash { get; set; } = null;

        public override void Parse(CircleBuffer buffer, int offset,int size)
        {
            this.version = buffer.ReadUInt16();
            this.blockSize = buffer.ReadUInt32();
            this.fileName = buffer.ReadString();
            this.fileBlockCount = buffer.ReadUInt64();
            this.fileSize = buffer.ReadUInt64();
            this.hash = buffer.ReadBuffer(16);
        }

        public override byte[] ToBytes()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(this.version);
            bw.Write(this.blockSize);
            bw.Write(this.fileName);
            bw.Write(this.fileBlockCount);
            bw.Write(this.fileSize);
            bw.Write(this.hash);

            bw.Flush();
            bw.Close();

            return ms.ToArray();
        }
    }
}
