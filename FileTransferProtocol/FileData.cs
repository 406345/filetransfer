using System;
using System.IO;

namespace FileTransferProtocol
{
    public class FileData : BaseMessage
    {
        public UInt16 version { get; set; } = Consts.VERSION;
        public UInt32 blockId { get; set; } = 0;
        public UInt32 blockSize { get; set; } = 0;
        // single data frame should not greater than 1MB;
        public Byte[] buffer { get; set; } = new byte[1024*1024];
        public byte[] hash { get; set; } = null;

        public override void Parse(CircleBuffer buffer, int offset, int length)
        {
            this.version = buffer.ReadUInt16();
            this.blockId = buffer.ReadUInt32();
            this.blockSize = buffer.ReadUInt32();
            this.hash = buffer.ReadBuffer(16);
            this.buffer = buffer.ReadBuffer((int)this.blockSize);
        }

        public override byte[] ToBytes()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(this.version);
            bw.Write(this.blockId);
            bw.Write(this.blockSize);
            bw.Write(this.hash);
            bw.Write(this.buffer, 0, (int)this.blockSize);

            bw.Flush();
            bw.Close();

            return ms.ToArray();
        }
    }
}
