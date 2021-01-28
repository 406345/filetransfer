using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileTransferServer
{
    internal class Service : ServiceBase
    {
        Thread thread;
        protected override void OnStop()
        {
            base.OnStop();
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            
        }

        public void Serve()
        {
            
        }
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            
           if(thread == null)
            {
                thread = new Thread(() => {
                    FileReceiver.Start();
                });
                thread.Start();
            }
        }
    }
}
