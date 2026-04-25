using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace RatioMaster_source
{
    public class ClientFamily
    {
        public string Name { get; set; }
        public List<string> Versions { get; set; }
        public int DefNumWant { get; set; }
    }

    public static class TorrentClientFactory
    {
        private static readonly RandomStringGenerator stringGenerator = new RandomStringGenerator();
        private static readonly Dictionary<string, TorrentClientConfig> clientConfigs;
        private static readonly List<ClientFamily> clientFamilies;
        private const string DefaultClientName = "uTorrent 3.3.2";
        private static readonly string ConfigFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "clients.json");

        static TorrentClientFactory()
        {
            clientConfigs = new Dictionary<string, TorrentClientConfig>(StringComparer.OrdinalIgnoreCase);
            var configs = LoadConfigs();
            foreach (var config in configs)
            {
                clientConfigs[config.Name] = config;
            }

            var familyDict = new Dictionary<string, ClientFamily>(StringComparer.OrdinalIgnoreCase);
            foreach (var config in configs)
            {
                string family = config.Family ?? config.Name;
                string version = config.Version ?? "";

                ClientFamily cf;
                if (!familyDict.TryGetValue(family, out cf))
                {
                    cf = new ClientFamily { Name = family, Versions = new List<string>(), DefNumWant = config.DefNumWant };
                    familyDict[family] = cf;
                }

                cf.Versions.Add(version);
            }

            clientFamilies = familyDict.Values.ToList();
        }

        public static List<ClientFamily> GetClientFamilies()
        {
            return clientFamilies;
        }

        private static List<TorrentClientConfig> LoadConfigs()
        {
            if (!File.Exists(ConfigFilePath))
                throw new FileNotFoundException(
                    "Client configuration file not found: " + ConfigFilePath);

            string json = File.ReadAllText(ConfigFilePath);
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<List<TorrentClientConfig>>(json);
        }

        public static TorrentClient GetClient(string name)
        {
            TorrentClientConfig config;
            if (!clientConfigs.TryGetValue(name, out config))
            {
                config = clientConfigs[DefaultClientName];
            }

            TorrentClient client = new TorrentClient(name);
            client.Name = config.Name;
            client.HttpProtocol = config.HttpProtocol;
            client.HashUpperCase = config.HashUpperCase;
            client.Key = ResolveTemplate(config.KeyTemplate);
            client.Headers = config.Headers;
            client.PeerID = ResolveTemplate(config.PeerIDTemplate);
            client.Query = config.Query;
            client.DefNumWant = config.DefNumWant;
            client.Parse = config.Parse;
            client.SearchString = config.SearchString ?? string.Empty;
            client.ProcessName = config.ProcessName ?? string.Empty;
            client.StartOffset = config.StartOffset;
            client.MaxOffset = config.MaxOffset;
            return client;
        }

        private static string ResolveTemplate(string template)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            int dollarIndex = template.IndexOf('$');
            if (dollarIndex < 0)
                return template;

            string prefix = template.Substring(0, dollarIndex);
            string directive = template.Substring(dollarIndex + 1);
            string[] parts = directive.Split(':');
            if (parts.Length < 4)
                return template;

            string keyType = parts[0];
            int keyLength = int.Parse(parts[1]);
            bool urlEncoding = bool.Parse(parts[2]);
            bool upperCase = bool.Parse(parts[3]);

            return prefix + GenerateIdString(keyType, keyLength, urlEncoding, upperCase);
        }

        private static string GenerateIdString(string keyType, int keyLength, bool urlencoding, bool upperCase = false)
        {
            string text1;
            switch (keyType)
            {
                case "alphanumeric":
                    text1 = stringGenerator.Generate(keyLength);
                    break;
                case "numeric":
                    text1 = stringGenerator.Generate(keyLength, "0123456789".ToCharArray());
                    break;
                case "random":
                    text1 = stringGenerator.Generate(keyLength, true);
                    break;
                case "hex":
                    text1 = stringGenerator.Generate(keyLength, "0123456789ABCDEF".ToCharArray());
                    break;
                default:
                    text1 = stringGenerator.Generate(keyLength);
                    break;
            }

            if (urlencoding)
            {
                return stringGenerator.Generate(text1, upperCase);
            }

            if (upperCase)
            {
                text1 = text1.ToUpper();
            }

            return text1;
        }
    }
}
