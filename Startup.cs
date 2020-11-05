using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
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

        public async Task ConfigureServicesAsync(IServiceCollection services)
        {
            Hardware.HardwareInit();
            await Holt.HoltConfigure();
            await InitCanAsync();
            Console.WriteLine("Hello World!");
            services.AddGrpc();
            services.AddSingleton<ArincRXQueue>();


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
            if (await CANHandler.InitCANAsync(MCP2515.enBaudRate.CAN_1000KBPS) == false)
            {
                Console.WriteLine("InitCanFail");
                return false;
            }
            // Set to normal operation mode.
            await CANHandler.SetCANNormalModeAsync();
            return true;
        }
    }
}
