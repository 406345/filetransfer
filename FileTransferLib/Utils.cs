using FileTransferProtocol;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace FileTransferLib
{
    public class Utils
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

        static public byte[] MD5BlockBytes(byte[] buffer)
        {
            return MD5.Create().ComputeHash(buffer);
        }

        static public byte[] MD5BlockBytes(string data)
        {
            return MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }
}
