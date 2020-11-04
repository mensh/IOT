using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

namespace IOT
{
    class Program
    {

        private static void TurnOff(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine("test Interuupt");
        }

        static void Main(string[] args)
        {
            Hardware.HardwareInit();
            Hardware.HoltConfigure();

            Console.WriteLine("Hello World!");
            // turn LED on and off
            while (true)
            {
                
                Thread.Sleep(1000);
               
            }


        }
    }
}
