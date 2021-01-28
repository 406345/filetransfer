using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FileTransfer
{
    class Config
    {
        public string remote { get; set; }
        public int port { get; set; }
        public string password { get; set; }

        public static Config Create()
        {
            var filename = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            var configPath = Path.Combine(filename, "config.json");
            var ret = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));

            return ret;
        }
    }
}
