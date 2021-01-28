using FileTransferProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileTransferServer
{
    class FileReceiver
    {
        const int MAX_MESSAGE_SIZE = 1024 * 1024 * 5;
        CircleBuffer bs = new CircleBuffer(MAX_MESSAGE_SIZE);
        Socket socket;
        Config config;

        public static void Start()
        {
            Console.WriteLine("Server started");
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 4444));
            socket.Listen(1024);

            while (true)
            {
                var client = socket.Accept();
                Console.WriteLine("New client accepted from {0}", client.RemoteEndPoint.ToString());
                new Thread((Object obj) => {
                    Task.Run(() => {
                        try
                        {
                            new FileReceiver((Socket)obj).receive();
                        }
                        catch (Exception ex)
                        {
                        }
                    }).Wait();
                }).Start(client);
            }
        }

        public FileReceiver(Socket socket)
        {
            this.config = Config.Create();
            this.socket = socket;
        }

        public async void receive()
        {
            var meta = ReadMessage<FileMeta>(new FileMeta());
            var file = System.IO.File.OpenWrite(Path.Combine(config.output, meta.fileName));
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
