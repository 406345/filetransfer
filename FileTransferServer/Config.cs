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

        public static Config Create()
        {
            var filename = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            var configPath = Path.Combine(filename, "config.json");
            var ret = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));

            return ret;
        }
    }
}
