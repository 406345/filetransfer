using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace FileTransferServer
{
    class Utils
    {
        static public String MD5Block(byte[] buffer)
        {
            MD5 md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(buffer);

            String t = "";
            foreach (var item in hashBytes)
            {
                t += item.ToString("x2");
            }

            return t;

        }
    }
}
