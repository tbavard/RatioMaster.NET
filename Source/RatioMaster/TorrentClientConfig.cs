namespace RatioMaster_source
{
    internal class TorrentClientConfig
    {
        public string Name { get; set; }

        public string Family { get; set; }

        public string Version { get; set; }

        public string HttpProtocol { get; set; }

        public bool HashUpperCase { get; set; }

        public string KeyTemplate { get; set; }

        public string Headers { get; set; }

        public string PeerIDTemplate { get; set; }

        public string Query { get; set; }

        public int DefNumWant { get; set; }

        public bool Parse { get; set; }

        public string SearchString { get; set; }

        public string ProcessName { get; set; }

        public long StartOffset { get; set; }

        public long MaxOffset { get; set; }
    }
}
