using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPServerTests
{
    public static class Extensions
    {
        public static string[] SplitByWhiteSpace(this string s)
        {
            return s.Split(new string[] { " ", "\t", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
