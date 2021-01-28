using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FileTransferClientWPF
{
    class Config
    {
        public string remote { get; set; } = "127.0.0.1";
        public int port { get; set; } = 4444;
        public string password { get; set; } = "";

        public static Config Create(string home)
        {
            var filename = Path.GetDirectoryName(home);
            var configPath = Path.Combine(filename, "config.json");

            if (!File.Exists(configPath))
                return new Config();

            var ret = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));

            return ret;
        }

        public static Config Create()
        {
            return Create(Process.GetCurrentProcess().MainModule.FileName);
        }
    }
}
