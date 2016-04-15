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
        public Startup()
        {
            Sessions.Enqueue(new Session());
        }

        public Queue<Session> Sessions { get; set; } = new Queue<Session>();

        public void Configure(IApplicationBuilder app)
        {
            app.UseIISPlatformHandler();

            app.Run(async (context) =>
            {
                try
                {
                    if (context.Request.Path.Value.Contains("data"))
                    {
                        await PostData(context);
                    }
                    else if (context.Request.Path.Value.Contains("count"))
                    {
                        await GetCount(context);
                    }
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync(ex.ToString());
                }
            });
        }

        public async Task PostData(HttpContext context)
        {
            Session currentSession;
            lock (Sessions)
            {
                currentSession = Sessions.Peek();
            }
            await currentSession.ProcessData(context.Request.Body);
        }

        public async Task GetCount(HttpContext context)
        {
            Session currentSession;
            lock (Sessions)
            {
                currentSession = Sessions.Dequeue();
                Sessions.Enqueue(new Session());
            }
            int count = await currentSession.Count();
            await context.Response.WriteAsync(count.ToString());
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
