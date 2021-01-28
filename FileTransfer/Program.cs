using FileTransferProtocol;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace FileTransfer
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No parameters assigned");
                return 255;
            }

            String filename = args[0];// "D:\\ea6f0f731325a2ba8f898beb0e8452a6.mp4";

            if (!System.IO.File.Exists(filename))
            {
                Console.WriteLine("%d is not exist", filename);
                return 255;
            }
            var config = Config.Create();

            System.IO.FileInfo fi = new System.IO.FileInfo(filename);

            const int BLOCK_SIZE = 1024 * 1024; // 1MB
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("Connecting to {0}", config.remote);

            socket.Connect(new IPEndPoint(IPAddress.Parse(config.remote), config.port));

            FileMeta meta = new FileMeta();
            meta.version = 1;
            meta.blockSize = BLOCK_SIZE;
            meta.fileName = Path.GetFileName(filename);
            meta.fileBlockCount = (ulong)((fi.Length + BLOCK_SIZE - 1) / BLOCK_SIZE);
            meta.fileSize = (ulong)fi.Length;
            meta.hash = MD5Block(Guid.NewGuid().ToByteArray());

            SendMessage(socket, meta);

            var fs = System.IO.File.OpenRead(filename);
            byte[] buffer = new byte[BLOCK_SIZE];
            FileData fd = new FileData();
            Console.WriteLine("Start to send file: {0}", filename);

            for (ulong i = 0; i < meta.fileBlockCount; i++)
            {
                int reads = fs.Read(buffer, 0, BLOCK_SIZE);

                fd.blockId = (uint)i;
                fd.blockSize = (uint)reads;
                fd.buffer = buffer;
                fd.hash = MD5Block(fd.buffer);

                SendMessage(socket, fd);
                LogProgress((int)i, (int)meta.fileBlockCount);
                //Console.Write(fd.blockId + "/" + meta.fileBlockCount + "(" + (1 + fd.blockId) * 100 / meta.fileBlockCount + "%)\r");
            }

            Console.WriteLine();
            byte[] result = new byte[1] { 0 };
            socket.Receive(result);

            if (result[0] == 0)
            {
                Console.WriteLine("Done");
            }
            else
            {
                Console.WriteLine("Failed to send {0}, error code: {}", filename, result[0]);
                Console.ReadLine();
            }

            return result[0];
        }

        static byte[] MD5Block(byte[] buffer)
        {
            MD5 md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(buffer);
            return hashBytes;

        }

        static void SendMessage(Socket socket, BaseMessage msg)
        {
            var bytes = msg.ToBytes();
            var lengthArr = BitConverter.GetBytes((uint)bytes.Length);
            socket.Send(lengthArr);
            socket.Send(bytes);
        }

        static void LogProgress(int cur, int total)
        {
            Console.Write("({0}%)", cur * 100 / total);
            Console.Write("[");
            for (int i = 0; i < 100; i++)
            {
                if (i <= (cur * 100 / total))
                {
                    Console.Write("=");
                }
                else
                {
                    Console.Write(" ");
                }
            }
            Console.Write("]\r");
        }
    }
}
