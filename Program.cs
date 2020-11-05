using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IOT
{
    class Program
    {



        static async System.Threading.Tasks.Task Main(string[] args)
        {

            // turn LED on and off
            CreateHostBuilder(args).Build().Run();


        }


        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Listen(IPAddress.Any, 5001, options => options.Protocols = HttpProtocols.Http2);


                    });
                    webBuilder.UseStartup<Startup>();
                });

    }
}
