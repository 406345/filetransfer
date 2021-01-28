using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileTransferClientWPF
{
    class Utils
    {
        public static void DelayAction(int millisecond, Func<bool> act)
        {
            new Thread(() => {

                do
                {
                    Thread.Sleep(millisecond);
                }
                while (act());
            }).Start();
        }
    }
}
