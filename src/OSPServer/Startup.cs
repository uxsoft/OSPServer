using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Threading;

namespace OSPServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public async Task ProcessCount(HttpContext context, HashSet<string> Words)
        {
            string count = Words.Count.ToString();
            Words.Clear();
            await context.Response.WriteAsync(count);
        }

        public async Task ProcessData(HttpContext context, HashSet<string> Words)
        {
            using (var degzip = new GZipStream(context.Request.Body, CompressionMode.Decompress))
            using (var sr = new StreamReader(degzip))
            {
                string part = await sr.ReadToEndAsync();
                foreach (string word in part.Split(new string[] { " ", "\t", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                    Words.Add(word);
            }
        }

        static HashSet<string> CurrentWords = new HashSet<string>();
        static List<Task> DataTasks = new List<Task>();
        static object triageLock = new object() { };

        public void Configure(IApplicationBuilder app)
        {
            app.UseIISPlatformHandler();

            app.Run(async (context) =>
            {
                try
                {
                    if (context.Request.Path.Value.Contains("data"))
                    {
                        Task processingData;
                        lock (triageLock)
                        {
                            processingData = ProcessData(context, CurrentWords);
                            DataTasks.Add(processingData);
                        }

                        await processingData;
                    }
                    else if (context.Request.Path.Value.Contains("count"))
                    {
                        IEnumerable<Task> tasksThatNeedToBeCompleted;
                        HashSet<string> Words;
                        lock (triageLock)
                        {
                            Words = CurrentWords;
                            tasksThatNeedToBeCompleted = DataTasks;

                            CurrentWords = new HashSet<string>();
                            DataTasks = new List<Task>();
                        }

                        await Task.WhenAll(tasksThatNeedToBeCompleted);
                        await ProcessCount(context, Words);
                    }
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync(ex.ToString());
                }
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
