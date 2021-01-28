using FileTransferLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileTransferProtocol
{
    public class FileResult : BaseMessage
    {
        public UInt16 Code { get; set; } = 0;
        public String Message { get; set; } = "";

        public override void Parse(CircleBuffer buffer, int offset, int length)
        {
            this.Code = buffer.ReadByte();
        }

        public override byte[] ToBytes()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(this.Code);
            bw.Write(this.Message);

            bw.Flush();
            bw.Close();

            return ms.ToArray();
        }
    }
}
