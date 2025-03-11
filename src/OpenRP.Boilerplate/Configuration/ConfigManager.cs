using Newtonsoft.Json;

namespace OpenRP.Boilerplate.Configuration
{
    public class ConfigManager
    {
        private static readonly ConfigManager _Instance = new ConfigManager();

        static ConfigManager()
        {
        }

        public static ConfigManager Instance
        {
            get
            {
                return _Instance;
            }
        }

        public Config Data;

        private ConfigManager()
        {
            string filePath = GetFileOrDirectory(null, null);
            string fileName = GetFileOrDirectory(null, "Config.json");

            Console.WriteLine($"Config located at {fileName}");

            if (!(Directory.Exists(filePath)))
            {
                Directory.CreateDirectory(filePath);
            }

            if (!(File.Exists(fileName)))
            {
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    Data = new Config();

                    string json = JsonConvert.SerializeObject(Data, Formatting.Indented);

                    sw.Write(json);
                    sw.Close();
                }
            }
            string config = File.ReadAllText(fileName);

            Data = JsonConvert.DeserializeObject<Config>(config);
        }

        public static string GetFileOrDirectory(string? path = null, string? file = null)
        {
            string rootPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}{System.IO.Path.DirectorySeparatorChar}OpenRP.GameMode";

            if (!String.IsNullOrEmpty(path))
            {
                string tmpPath = $"{rootPath}{System.IO.Path.DirectorySeparatorChar}{path}";

                if (!(Directory.Exists(tmpPath)))
                {
                    Directory.CreateDirectory(tmpPath);
                }

                if (String.IsNullOrEmpty(file))
                {
                    return tmpPath;
                }
            }

            if (!String.IsNullOrEmpty(file))
            {
                return $"{rootPath}{System.IO.Path.DirectorySeparatorChar}{path}{System.IO.Path.DirectorySeparatorChar}{file}";
            }

            return rootPath;
        }

        public void Save()
        {
            string fileName = GetFileOrDirectory(null, @"Config.json");
            string json = JsonConvert.SerializeObject(Data, Formatting.Indented);

            using (StreamWriter sw = new StreamWriter(fileName, false))
            {
                sw.Write(json);
                sw.Close();
            }
        }
    }
}
