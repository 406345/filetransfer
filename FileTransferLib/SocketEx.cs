using FileTransferProtocol;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FileTransferLib
{
    public class SocketEx
    {
        private const int MAX_MESSAGE_SIZE = 1024 * 1024 * 2;
        private Socket socket;
        private CircleBuffer buffer;

        public SocketEx(Socket socket)
        {
            this.socket = socket;
            this.socket.ReceiveTimeout = 1000 * 5; // 5sec
            this.buffer = new CircleBuffer(MAX_MESSAGE_SIZE);
        }

        public string RemoteIP
        {
            get
            {
                return this.socket.RemoteEndPoint.ToString();
            }
        }

        public int SendMessage(BaseMessage msg)
        {
            try
            {
                var bytes = msg.ToBytes();
                var lengthArr = BitConverter.GetBytes((uint)bytes.Length);
                var count = this.socket.Send(lengthArr);
                count += this.socket.Send(msg.ToBytes());
                return count;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public T ReadMessage<T>(T instance) where T : BaseMessage
        {
            byte[] recvBuffer = new byte[1024];
            int state = 0;
            uint msgLength = 0;

            while (true)
            {
                int recvNum = 0;

                try
                {
                    recvNum = this.socket.Receive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    return null;
                }

                if (recvNum == 0)
                {
                    Thread.Sleep(100);
                    continue;
                }
                
                buffer.WriteBuffer(recvBuffer, 0, recvNum);

                while (true)
                {
                    if (state == 0)
                    {
                        if (buffer.Length() >= 4)
                        {
                            msgLength = buffer.ReadUInt32();

                            if (msgLength > MAX_MESSAGE_SIZE)
                                return null;

                            state = 1;
                            continue;
                        }
                        break;

                    }
                    else if (state == 1)
                    {
                        if (buffer.Length() >= msgLength)
                        {
                            instance.Parse(buffer, 0, (int)msgLength);
                            //bs.reset();
                            return instance;
                        }
                        break;
                    }
                }
            }

            return null;
        }
    }
}
