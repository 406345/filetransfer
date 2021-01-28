using FileTransferLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransferProtocol
{
    public abstract class BaseMessage
    {
        public BaseMessage() { }
        public abstract void Parse(CircleBuffer buffer, int offset, int length);
        public abstract byte[] ToBytes();

    }
}
