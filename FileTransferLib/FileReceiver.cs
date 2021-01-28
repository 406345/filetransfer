using FileTransferProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileTransferLib
{
    class FileReceiver
    {
        string outputDir;
        const int MAX_MESSAGE_SIZE = 1024 * 1024 * 5;
        CircleBuffer bs = new CircleBuffer(MAX_MESSAGE_SIZE);
        Socket socket; 

        public static void Start(Socket socket, string outputDir)
        {
            new FileReceiver(socket,outputDir).receive();
        }

        public FileReceiver(Socket socket, String outputDir)
        {
            this.socket = socket;
            this.outputDir = outputDir;
        }

        public async void receive()
        {
            var meta = ReadMessage<FileMeta>(new FileMeta());
            string path = Path.Combine(this.outputDir, meta.fileName);
            string dir = Path.GetDirectoryName(path);

            if( !System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            
            var file = System.IO.File.OpenWrite(path);
            int recvBlockNum = 0;

            var block = new FileData();
            while (recvBlockNum < (int)meta.fileBlockCount)
            {
                var body = ReadMessage<FileData>(block);
                ++recvBlockNum;
                //file.Seek(body.blockId * meta.blockSize, System.IO.SeekOrigin.Begin);
                await file.WriteAsync(body.buffer, 0, (int)body.blockSize);
            }

            file.Close();
            socket.Send(new byte[1] { 0x00 });
        }

        public T ReadMessage<T>(T instance) where T : BaseMessage
        {
            byte[] recvBuffer = new byte[1024];
            int state = 0;
            uint msgLength = 0;
            int readLength = 4;
            while (true)
            {
                int recvNum = this.socket.Receive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None);
                bs.WriteBuffer(recvBuffer, 0, recvNum);

                while (true)
                {
                    if (state == 0)
                    {
                        if (bs.Length() >= 4)
                        {
                            msgLength = bs.ReadUInt32();

                            if (msgLength > MAX_MESSAGE_SIZE)
                                return null;

                            state = 1;
                            readLength = recvBuffer.Length;
                            continue;
                        }
                        break;

                    }
                    else if (state == 1)
                    {
                        if (bs.Length() >= msgLength)
                        {
                            instance.Parse(bs, 0, (int)msgLength);
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
