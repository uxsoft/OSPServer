using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPServerTests
{
    public class Book
    {
        public Book(string path)
        {
            Random random = new Random();
            string contents = File.ReadAllText(path);
            var words = contents.SplitByWhiteSpace();

            HashSet<string> uniqueWords = new HashSet<string>();
            foreach (string word in words)
                uniqueWords.Add(word);

            Count = uniqueWords.Count;

            Parts = words.GroupBy(w => random.Next(10))
                .Select(g => string.Join(" ", g))
                .ToList();

        }

        public int Count { get; set; }
        public IEnumerable<string> Parts { get; set; }
    }
}
