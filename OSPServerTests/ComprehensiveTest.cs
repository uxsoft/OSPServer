using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;

namespace OSPServerTests
{
    [TestClass]
    public class ComprehensiveTest
    {
        const string SERVER_PREFIX = "http://localhost:42809/";
        const string SERVER_DATA_URL = "osp/myserver/data";
        const string SERVER_COUNT_URL = "osp/myserver/count";

        [TestMethod]
        public async Task TestAllBooks()
        {
            var books = Directory.EnumerateFiles("Books")
                .Select(file => new Book(file))
                .ToList();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(SERVER_PREFIX);

            var workers = new Dictionary<Book, Task<string>>();
            foreach (Book book in books)
            {
                foreach (string part in book.Parts)
                {
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(part)))
                    using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                    {
                        var content = new StreamContent(gzip);
                        var dataRequest = client.PostAsync(SERVER_DATA_URL, content);
                        dataRequest.Start();
                        await Task.Delay(10);
                    }
                }

                await Task.Delay(10);
                var countRequest = client.GetStringAsync(SERVER_COUNT_URL);
                workers.Add(book, countRequest);
            }

            await Task.WhenAll(workers.Values);

            foreach (var pair in workers)
            {
                Assert.AreEqual(pair.Key.Count, int.Parse(pair.Value.Result));
            }
        }
    }
}
