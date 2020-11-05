using Google.Protobuf.WellKnownTypes;
using IOT.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading.Tasks;

namespace IOT
{
    public class Startup
    {
        private static MCP2515 CANHandler;
        private ArincRXQueue _arincRXQueue;
        private ArincTXQueue _arincTXQueue;
        private CANTXQueue _CANTXQueue;
        private CANRXQueue _CANRXQueue;
        private int counterRecive = 0;
        public void ConfigureServices(IServiceCollection services)
        {
            Hardware.HardwareInit();

            Console.WriteLine("Hello World!");
            Task.Run(async () => await Holt.HoltConfigure().ContinueWith(async x => await InitCanAsync())).ContinueWith(_ =>
               {
                   Task.Run(async () => await TaskTXCAN());
                   Task.Run(async () => await TransmitHolt());
                   Task.Run(async () => await ReadHolt());
               });
            Hardware.ReadMessage = TaskRXCAN;

            services.AddGrpc();
            services.AddSingleton<ArincRXQueue>();
            services.AddSingleton<ArincTXQueue>();
            services.AddSingleton<CANTXQueue>();
            services.AddSingleton<CANRXQueue>();

        }
        private async Task ReadHolt()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var swoff = System.Diagnostics.Stopwatch.StartNew();
            bool state = true;
            bool flag_off = true;

            while (true)
            {
                var status = await Holt.ReadStatus();
                if (status == 1)
                {
                    swoff = System.Diagnostics.Stopwatch.StartNew();
                    flag_off = true;
                    if (sw.ElapsedMilliseconds > 1000)
                    {
                        Hardware.RedLed(state ? PinValue.High : PinValue.Low);
                        sw = System.Diagnostics.Stopwatch.StartNew();
                        state = !state;
                    }
                    var message = await Holt.ReadMessageAsync(false);
                    var time = Timestamp.FromDateTime(DateTime.UtcNow);
                    _arincRXQueue?.AddMailQueue(new ArincRX() { data = message, DateTime = time });
                }
                else
                {
                    if (swoff.ElapsedMilliseconds > 2000)
                    {
                        if (flag_off)
                        {
                            Hardware.RedLed(PinValue.Low);
                            flag_off = false;
                        }

                    }
                    udelay(40);
                }
            }
        }

        private async Task TransmitHolt()
        {
            while (true)
            {
                if (_arincTXQueue?.Count() > 0)
                {
                    var newTime = DateTime.Now.Ticks;
                    foreach (var tx in _arincTXQueue.Values())
                    {
                        if ((new TimeSpan(newTime) - new TimeSpan(tx.time)).TotalMilliseconds > tx.freq)
                        {
                            if (tx.enable == true)
                            {
                                if (tx.isNonContinue == true)
                                {
                                    await Holt.TXMessageAsync(tx.address, tx.low, tx.hi, tx.sup);
                                    tx.time = newTime;
                                    if (tx.numberpacket > 0)
                                    {
                                        tx.numberpacket = tx.numberpacket - 1;
                                    }
                                    if (tx.numberpacket == 0)
                                        _arincTXQueue.RemoveItem(tx.id);
                                }
                                else
                                {
                                    await Holt.TXMessageAsync(tx.address, tx.low, tx.hi, tx.sup);
                                    tx.time = newTime;
                                }
                            }
                        }
                    }

                    udelay(40);
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }
        private async Task TaskTXCAN()
        {
            int cnt = 0;
            while (true)
            {
                MCP2515.CANMSG txMessage = new MCP2515.CANMSG();
                txMessage.data = new byte[] { 0xCC, 0xAA, 0xAA, 0xAA, 0x11, 0x00, 0xFF, 0xFF };
                txMessage.CANID = 0x1AA;

                // Create extended TX message.
                MCP2515.CANMSG txMessageExt = new MCP2515.CANMSG();
                txMessageExt.CANID = 0x1FEDCBA1;
                txMessageExt.data = new byte[] { 0x00, 0xEE };

                txMessage.data[6] = (byte)(cnt >> 8);
                txMessage.data[7] = (byte)cnt;
                // Transmit messages.
                await CANHandler.TransmitAsync(txMessage, 10);
                await Task.Delay(100);
                await CANHandler.TransmitAsync(txMessageExt, 10);
                cnt++;
                await Task.Delay(100);
            }
        }

        private async Task TaskRXCAN()
        {

            MCP2515.CANMSG rxMessage = new MCP2515.CANMSG();

            // Check if a message was received.
            if (await CANHandler.ReceiveAsync(rxMessage))
            {
                if (rxMessage.IsExtended)
                {
                    Console.WriteLine(rxMessage.CANID + ":" + BitConverter.ToString(rxMessage.data));
                    Console.WriteLine("Total Recive Message = " + counterRecive);
                }
                else
                {
                    Console.WriteLine(rxMessage.CANID + ":" + BitConverter.ToString(rxMessage.data));
                    Console.WriteLine("Total Recive Message = " + counterRecive);
                   
                }
                counterRecive++;
            }
            
        }


        static void udelay(long us)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long v = (us * System.Diagnostics.Stopwatch.Frequency) / 1000000;
            while (sw.ElapsedTicks < v)
            {
            }
        }


        private static async System.Threading.Tasks.Task<bool> InitCanAsync()
        {
            CANHandler = new MCP2515();
            if (await CANHandler.InitCANAsync(MCP2515.enBaudRate.CAN_500KBPS) == false)
            {
                Console.WriteLine("InitCanFail");
                return false;
            }
            // Set to normal operation mode.
            await CANHandler.SetCANNormalModeAsync();
            return true;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ServerService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            var serviceProvider = app.ApplicationServices;
            _arincRXQueue = serviceProvider.GetService<ArincRXQueue>();
            _arincTXQueue = serviceProvider.GetService<ArincTXQueue>();
            _CANTXQueue = serviceProvider.GetService<CANTXQueue>();
            _CANRXQueue = serviceProvider.GetService<CANRXQueue>();
        }
    }

}
