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
                    try
                    {
                        new FileReceiver(new SocketEx((Socket)obj)).receive();
                    }
                    catch (Exception ex)
                    {

                    }
                }).Start(client);
            }
        }

        public FileReceiver(SocketEx socket)
        { 
            this.socket = socket;
        }

        public void receive()
        {
            var meta = this.socket.ReadMessage<FileMeta>(new FileMeta());

            if ( meta == null)
            {
                return;
            }

            string filePath = Path.Combine(config.output, meta.fileName)
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            string outputDir = Path.GetDirectoryName(filePath);
            System.IO.Directory.CreateDirectory(outputDir);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            var file = System.IO.File.Open(filePath, FileMode.OpenOrCreate);
            int recvBlockNum = 0;

            var block = new FileData();

            this.socket.SendMessage(new FileResult()
            {
                Code = 0,
                Message = "OK",
            });

            while (recvBlockNum < (int)meta.fileBlockCount)
            {
                var body = this.socket.ReadMessage<FileData>(block);

                if ( body == null)
                {
                    return;
                }

                ++recvBlockNum;
                //file.Seek(body.blockId * meta.blockSize, System.IO.SeekOrigin.Begin);
                file.Write(body.buffer, 0, (int)body.blockSize);

                this.socket.SendMessage(new FileResult()
                {
                    Code = 0,
                    Message = "OK",
                });

            }

            file.Flush();
            file.Close();
            
            Console.WriteLine("[{0}] send {1} save to {2}", socket.RemoteIP, meta.fileName, filePath);
            
            //this.socket.SendMessage(new FileResult()
            //{
            //    Code = 0,
            //    Message = "OK",
            //});
        }
    }
}
