using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using System.Threading.Tasks;

namespace IOT
{
    public class Holt
    {
        private static UInt16 ARING_SPEED = 0xE028;
        public static async Task<bool> HoltConfigure()
        {
            UInt16 HOLT_CFG_REGISTER = 0;
            HOLT_CFG_REGISTER = ARING_SPEED;
            await Hardware.SPIWork(new byte[] { 0x10, (byte)(HOLT_CFG_REGISTER >> 8), (byte)(HOLT_CFG_REGISTER & 0x00ff) }, null, Hardware.CS_HOLT);
            byte[] dataread = { 0x00, 0x00 };
            await Hardware.SPIWork(new byte[] { 0x0B }, dataread, Hardware.CS_HOLT);
            var config = dataread[1] | (dataread[0] << 8);
            Console.WriteLine("ReadConfig result = " + config.ToString("X4"));

            if (config != HOLT_CFG_REGISTER)
            {
                Console.WriteLine("Error Init Holt");
                return false;
            }
            return true;
        }

        public static async Task<int> ReadStatus()
        {
            byte[] dataread = { 0x00 };

            await Hardware.SPIWork(new byte[] { 0x0a }, dataread, Hardware.CS_HOLT);

            if ((dataread[0] & 0x01) == 1)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public static async Task TXMessageAsync(byte ad, byte low, byte hi, byte sup)
        {
            byte[] data = new byte[5];
            data[4] = ad;
            data[3] = (byte)(((sup & 0x1F) << 0) | ((low & 0x07) << 5));
            data[2] = (byte)((((low >> 3) & 0x1F) << 0) | ((hi & 0x07) << 5));//|((temph&0x07)<<5));
            data[1] = (byte)((((hi >> 3) & 0x1F) << 0) | (((sup >> 5) & 0x07) << 5));
            data[0] = 0x0e;
            await Hardware.SPIWork(data, null, Hardware.CS_HOLT);
        }

        public static async Task<uint> ReadMessageAsync(bool rec)
        {
            byte[] result = { 0x00, 0x00, 0x00, 0x00 };
            await Hardware.SPIWork(new byte[] { 0x08 }, result, Hardware.CS_HOLT);
            byte tempadr = result[3]; // çàáðàëè èç SPI
            byte tempsup = (byte)(((result[2] & 0x1F) << 0) | (((result[0] >> 5) & 0x07) << 5));
            byte templ = (byte)((((result[2] >> 5) & 0x07) << 0) | ((result[1] & 0x1F) << 3));
            byte temph = (byte)((((result[1] >> 5) & 0x07) << 0) | ((result[0] & 0x1F) << 3));
            return (UInt32)(tempadr | templ << 8 | temph << 16 | tempsup << 24);
        }

        public static async Task SetRX100kHZAsync()
        {
            ARING_SPEED = (UInt16)((ARING_SPEED & 0xfffe) | 0x00);
            await HoltConfigure();
        }

        public static async Task SetRX12kHZAsync()
        {
            ARING_SPEED = (UInt16)((ARING_SPEED & 0xfffe) | 0x01);
            await HoltConfigure();
        }


        public static async Task SetTX100kHZAsync()
        {
            ARING_SPEED = (UInt16)((ARING_SPEED & 0xfbff) | 0x0000);
            await HoltConfigure();
        }

        public static async Task SetTX12kHZAsync()
        {
            ARING_SPEED = (UInt16)((ARING_SPEED & 0xfbff) | 0x0400);
            await HoltConfigure();
        }
    }
}