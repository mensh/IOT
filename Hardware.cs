using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace IOT
{
    public class Hardware
    {
        public delegate Task _ReadMessage();
        public static _ReadMessage ReadMessage;
        public const int CS_HOLT = 42;
        public const int CS_CAN = 150;
        public const int Int_CAN = 149;
        public const int RED_LED = 33;
        public const int BtrSpi = 10000000;
        static Mutex mutexObj = new Mutex();
        public static SpiDevice SPI;
        public static GpioController controller;
        readonly static AsyncLock mutex = new AsyncLock();
        public static bool HardwareInit()
        {
            try
            {
                controller = new GpioController();
                controller.OpenPin(CS_HOLT, PinMode.Output);
                controller.OpenPin(CS_CAN, PinMode.Output);
                controller.OpenPin(RED_LED, PinMode.Output);
                controller.OpenPin(Int_CAN, PinMode.Input);
                controller.RegisterCallbackForPinValueChangedEvent(Int_CAN, PinEventTypes.Falling, CAN_Interrupt);
                var settings = new SpiConnectionSettings(1, 0);
                settings.ClockFrequency = BtrSpi;
                SPI = SpiDevice.Create(settings);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static void RedLed(PinValue pinValue)
        {
            controller.Write(RED_LED, pinValue);
        }

      

        private static void CAN_Interrupt(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            //throw new NotImplementedException();
            ReadMessage();
        }





        public static async Task SPIWork(byte[] datawrite, byte[] dataread, int CS_PIN)
        {
                mutexObj.WaitOne();
       
                if (datawrite.Length == 1 && (dataread == null || dataread.Length == 0))
                {
                    controller.Write(CS_PIN, PinValue.Low);
                    SPI.WriteByte(datawrite[0]);
                    controller.Write(CS_PIN, PinValue.High);
                }
                else if (datawrite.Length == 1 && dataread.Length == 1)
                {
                    controller.Write(CS_PIN, PinValue.Low);
                    SPI.WriteByte(datawrite[0]);
                    dataread[0] = SPI.ReadByte();
                    controller.Write(CS_PIN, PinValue.High);
                }
                else if (datawrite.Length > 1 && (dataread == null || dataread.Length == 0))
                {
                    controller.Write(CS_PIN, PinValue.Low);
                    SPI.Write(datawrite);
                    controller.Write(CS_PIN, PinValue.High);
                }
                else
                {
                    controller.Write(CS_PIN, PinValue.Low);
                    SPI.Write(datawrite);
                    SPI.Read(dataread);
                    controller.Write(CS_PIN, PinValue.High);
                }
                mutexObj.ReleaseMutex();
            
           
        }
    }
}