using FileTransferProtocol;
using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace FileTransferServer
{
    class Program
    {
        static void Main(string[] args)
        {
            FileReceiver.Start();
            //BlockedStream bs = new BlockedStream();

            //byte[] buffer = new byte[100];
            //bs.read(buffer, 100);

            //bs.write(buffer, 0, 100);


        }

         
    }
}
