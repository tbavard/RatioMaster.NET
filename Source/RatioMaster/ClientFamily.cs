namespace RatioMaster_source
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    public class ClientFamily
    {
        public string Name { get; set; }
        public List<string> Versions { get; set; }
        public int DefNumWant { get; set; }
    }
}
