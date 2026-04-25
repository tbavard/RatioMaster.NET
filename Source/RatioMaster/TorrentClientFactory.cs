namespace RatioMaster_source
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    public static class TorrentClientFactory
    {
        private const string DefaultClientName = "uTorrent 3.3.2";

        private static readonly RandomStringGenerator stringGenerator = new RandomStringGenerator();
        private static readonly Dictionary<string, TorrentClientConfig> clientConfigs;
        private static readonly List<ClientFamily> clientFamilies;
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
                string version = config.Version ?? string.Empty;

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

        private static List<TorrentClientConfig> LoadConfigs()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    var configs = JsonSerializer.Deserialize<List<TorrentClientConfig>>(json);
                    if (configs is { Count: > 0 })
                    {
                        return configs;
                    }
                }
            }
            catch (Exception)
            {
                // Fall through to default config
            }

            return new List<TorrentClientConfig>
            {
                new TorrentClientConfig
                {
                    Name = DefaultClientName,
                    Family = "uTorrent",
                    Version = "3.3.2",
                    HttpProtocol = "HTTP/1.1",
                    HashUpperCase = true,
                    KeyTemplate = "$alphanumeric:8:false:true",
                    PeerIDTemplate = "-UT3320-$alphanumeric:12:false:false",
                    Headers = "User-Agent: uTorrent/3320(30000)\r\nAccept-Encoding: gzip",
                    Query = "info_hash={infohash}&peer_id={peerid}&port={port}&uploaded={uploaded}&downloaded={downloaded}&left={left}&corrupt=0&key={key}&event={event}&numwant={numwant}&compact=1&no_peer_id=1",
                    DefNumWant = 200,
                    Parse = true,
                    SearchString = string.Empty,
                    ProcessName = "uTorrent",
                    StartOffset = 0,
                    MaxOffset = 0,
                }
            };
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
