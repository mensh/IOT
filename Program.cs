using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

namespace IOT
{
    class Program
    {

        private static MCP2515 CANHandler;

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Hardware.HardwareInit();
            await Holt.HoltConfigure();
            await InitCanAsync();
            Console.WriteLine("Hello World!");
            // turn LED on and off
            while (true)
            {      
                Thread.Sleep(1000);
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
