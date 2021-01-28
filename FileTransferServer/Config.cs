using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FileTransferServer
{
    class Config
    {
        public string output { get; set; }
        public int port { get; set; }

        public static Config Create()
        {
            Config ret = null;

            var filename = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            var configPath = Path.Combine(filename, "config.json");

            
            if(!System.IO.File.Exists(configPath))
            {
                ret = new Config();
                ret.output = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                Console.WriteLine("Load config file: "+ configPath);
                ret = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath,Encoding.UTF8));
            }

            return ret;
        }
    }
}
