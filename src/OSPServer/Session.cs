using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace OSPServer
{
    public class Session
    {
        public HashSet<string> Words { get; set; } = new HashSet<string>();
        public List<Task> Workers { get; set; } = new List<Task>();

        public void BeginProcessingData(Stream body)
        {
            Task worker = Task.Factory.StartNew(async () =>
            {
                using (var degzip = new GZipStream(body, CompressionMode.Decompress))
                using (var sr = new StreamReader(degzip))
                {
                    string part = await sr.ReadToEndAsync();
                    foreach (string word in part.Split(new string[] { " ", "\t", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                        Words.Add(word);
                }
            });
            Workers.Add(worker);
        }

        public async Task<int> Count()
        {
            await Task.WhenAll(Workers);

            return Words.Count;
        }
    }
}
