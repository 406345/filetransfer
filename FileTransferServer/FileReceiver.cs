using FileTransferLib;
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
        SocketEx socket;
        static Config config;

        public static void Start()
        {
            Console.WriteLine("Server started");
            config = Config.Create();
            Console.WriteLine("Output set to " + config.output);

            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, config.port));
            Console.WriteLine("Server listening to " + config.port);
            server.Listen(1024);
            
            while (true)
            {

                var client = server.Accept();
                new Thread((Object obj) =>
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            new FileReceiver(new SocketEx((Socket)obj)).receive();
                        }
                        catch (Exception ex)
                        {

                        }
                    }).Wait();
                }).Start(client);
            }
        }

        public FileReceiver(SocketEx socket)
        { 
            this.socket = socket;
        }

        public async void receive()
        {
            var meta = this.socket.ReadMessage<FileMeta>(new FileMeta());
           
            Console.WriteLine("[{0}] send {1}", socket.RemoteIP, meta.fileName);

            string filePath = Path.Combine(config.output, meta.fileName);
            string outputDir = Path.GetDirectoryName(filePath);
            System.IO.Directory.CreateDirectory(outputDir);

            var file = System.IO.File.OpenWrite(filePath);
            int recvBlockNum = 0;

            var block = new FileData();
            while (recvBlockNum < (int)meta.fileBlockCount)
            {
                var body = this.socket.ReadMessage<FileData>(block);
                ++recvBlockNum;
                //file.Seek(body.blockId * meta.blockSize, System.IO.SeekOrigin.Begin);
                await file.WriteAsync(body.buffer, 0, (int)body.blockSize);
            }

            file.Close();
            this.socket.SendMessage(new FileResult()
            {
                Code = 0,
                Message = "OK",
            });
        }
    }
}
