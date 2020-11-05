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
        public const int CS_HOLT = 42;
        public const int CS_CAN = 150;
        public const int Int_CAN = 54;
        public const int BtrSpi = 1000000;

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


        private static void CAN_Interrupt(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            //throw new NotImplementedException();
        }





        public static async Task SPIWork(byte[] datawrite, byte[] dataread, int CS_PIN)
        {
            using (await mutex.LockAsync())
            {
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
            }

        }
    }
}