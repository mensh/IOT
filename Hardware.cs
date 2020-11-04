using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Mcp25xxx;
using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.BitTimeConfiguration;
using Iot.Device.Mcp25xxx.Register.CanControl;
using Iot.Device.Mcp25xxx.Register.Interrupt;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;

namespace IOT
{
    public class Hardware
    {
        public const int CS_HOLT = 42;
        public const int CS_CAN = 150;
        public const int Int_CAN = 54;
        public const int BtrSpi = 1000000;
        public const UInt16 ARING_SPEED = 0xE028;
        public static SpiDevice SPI;
        public static GpioController controller;
        public static Mcp2515 Mcp2515;
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

                Mcp2515 = GetMcp25xxxDevice();
                Mcp2515_Reset();
                //Mcp2515_ReadAllInterruptRegistersWithDetails();
                Mcp2515_BitModify();
                Mcp2515_ReadAllInterruptRegistersWithDetails();
                Mcp2515_ReadAllRegisters();
                Mcp2515_ReadAllBitTimeConfigurationRegistersWithDetails();
                for (int i=0;i<100;i++)
                {
                    Mcp2515_TransmitMessage();
                }
                Mcp2515_ReadRxBuffer();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private static void Mcp2515_Reset()
        {
            Console.WriteLine("Reset Instruction");
            controller.Write(CS_CAN, PinValue.Low);
            Mcp2515.Reset();
            controller.Write(CS_CAN, PinValue.High);
        }

        private static void ConsoleWriteRegisterItemDetails(IRegister register)
        {
            foreach (System.Reflection.PropertyInfo property in register.GetType().GetProperties())
            {
                Console.WriteLine($"{property.Name,15}: {property.GetValue(register, null)}");
            }
        }

        private static byte ConsoleWriteRegisterAddressDetails(Address address)
        {
            controller.Write(CS_CAN, PinValue.Low);
            byte value = Mcp2515.Read(address);
            controller.Write(CS_CAN, PinValue.High);
            Console.WriteLine($"  0x{(byte)address:X2} - {address}: 0x{value:X2}");
            return value;
        }


        private static void LoopbackMode(Mcp25xxx mcp25xxx)
        {
            Console.WriteLine("Loopback Mode");
            controller.Write(CS_CAN, PinValue.Low);
            mcp25xxx.WriteByte(
                new CanCtrl(
                    CanCtrl.PinPrescaler.ClockDivideBy8,
                    true,
                    false,
                    false,
                    Iot.Device.Mcp25xxx.Tests.Register.CanControl.OperationMode.Loopback));
            controller.Write(CS_CAN, PinValue.High);
        }


        private static void Mcp2515_BitModify()
        {
            Console.WriteLine("Bit Modify Instruction");
            controller.Write(CS_CAN, PinValue.Low);
            Mcp2515.BitModify(Address.CanIntE, 0b1010_0011, 0b1111_1111);
            controller.Write(CS_CAN, PinValue.High);
        }


        private static void Mcp2515_TransmitMessage()
        {
            Console.WriteLine("Transmit Message");
            controller.Write(CS_CAN, PinValue.Low);
            Mcp2515.WriteByte(
                new CanCtrl(CanCtrl.PinPrescaler.ClockDivideBy8,
                    false,
                    false,
                    false,
                    Iot.Device.Mcp25xxx.Tests.Register.CanControl.OperationMode.Loopback));
            controller.Write(CS_CAN, PinValue.High);
            byte[] data = new byte[] { 0b0000_0001, 0b0010_0011, 0b0100_0101, 0b0110_0111, 0b1000_1001 };
            controller.Write(CS_CAN, PinValue.Low);
            Mcp2515.Write(
                Address.TxB0Sidh,
                new byte[]
                {
                    new TxBxSidh(0, 0b0000_1001).ToByte(), new TxBxSidl(0, 0b001, false, 0b00).ToByte(),
                    new TxBxEid8(0, 0b0000_0000).ToByte(), new TxBxEid0(0, 0b0000_0000).ToByte(),
                    new TxBxDlc(0, data.Length, false).ToByte()
                });
            controller.Write(CS_CAN, PinValue.High);
            controller.Write(CS_CAN, PinValue.Low);
            Mcp2515.Write(Address.TxB0D0, data);
            controller.Write(CS_CAN, PinValue.High);

            // Send with TxB0 buffer.
            controller.Write(CS_CAN, PinValue.Low);
            Mcp2515.RequestToSend(true, false, false);
            controller.Write(CS_CAN, PinValue.High);
        }

        private static void Mcp2515_ReadAllBitTimeConfigurationRegistersWithDetails()
        {
            Console.WriteLine("Bit Time Configuration Registers");

            ConsoleWriteRegisterItemDetails(new Cnf1(ConsoleWriteRegisterAddressDetails(Address.Cnf1)));
            ConsoleWriteRegisterItemDetails(new Cnf2(ConsoleWriteRegisterAddressDetails(Address.Cnf2)));
            ConsoleWriteRegisterItemDetails(new Cnf3(ConsoleWriteRegisterAddressDetails(Address.Cnf3)));
        }
        private static void CAN_Interrupt(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            //throw new NotImplementedException();
        }


            private static void Mcp2515_ReadAllInterruptRegistersWithDetails()
        {
            Console.WriteLine("Interrupt Registers");

            ConsoleWriteRegisterItemDetails(new CanIntE(ConsoleWriteRegisterAddressDetails(Address.CanIntE)));
            ConsoleWriteRegisterItemDetails(new CanIntF(ConsoleWriteRegisterAddressDetails(Address.CanIntF)));
        }

        private static Mcp2515 GetMcp25xxxDevice()
        {
            var spiConnectionSettings = new SpiConnectionSettings(1, 0);
            var spiDevice = SpiDevice.Create(spiConnectionSettings);
            return new Mcp2515(spiDevice);
        }


        private static void Mcp2515_ReadAllRegisters()
        {
            Console.WriteLine("Read Instruction for All Registers");
            Array addresses = Enum.GetValues(typeof(Address));

            foreach (Address address in addresses)
            {
                controller.Write(CS_CAN, PinValue.Low);
                byte addressData = Mcp2515.Read(address);
                controller.Write(CS_CAN, PinValue.High);
                Console.WriteLine($"0x{(byte)address:X2} - {address,-10}: 0x{addressData:X2}");
            }
        }

        private static void Mcp2515_ReadRxBuffer()
        {
            Console.WriteLine("Read Rx Buffer Instruction");
            ReadRxBuffer(Mcp2515, RxBufferAddressPointer.RxB0Sidh, 1);
            ReadRxBuffer(Mcp2515, RxBufferAddressPointer.RxB0Sidh, 5);
            ReadRxBuffer(Mcp2515, RxBufferAddressPointer.RxB0D0, 8);
            ReadRxBuffer(Mcp2515, RxBufferAddressPointer.RxB1Sidh, 1);
            ReadRxBuffer(Mcp2515, RxBufferAddressPointer.RxB1Sidh, 5);
            ReadRxBuffer(Mcp2515, RxBufferAddressPointer.RxB1D0, 8);
        }

        private static void ReadRxBuffer(Mcp25xxx mcp25xxx, RxBufferAddressPointer addressPointer, int byteCount)
        {
            controller.Write(CS_CAN, PinValue.Low);
            byte[] data = mcp25xxx.ReadRxBuffer(addressPointer, byteCount);
            controller.Write(CS_CAN, PinValue.High);
            Console.Write($"{addressPointer,10}: ");

            foreach (byte value in data)
            {
                Console.Write($"0x{value:X2} ");
            }

            Console.WriteLine();
        }


        public static bool HoltConfigure()
        {


            UInt16 HOLT_CFG_REGISTER = 0;
            HOLT_CFG_REGISTER = ARING_SPEED;
            controller.Write(CS_HOLT, PinValue.Low);
            SPI.Write(new byte[] { 0x10, (byte)(HOLT_CFG_REGISTER >> 8), (byte)(HOLT_CFG_REGISTER & 0x00ff) });
            controller.Write(CS_HOLT, PinValue.High);


            controller.Write(CS_HOLT, PinValue.Low);
            SPI.WriteByte(0x0B);
            byte[] dataread = { 0x00, 0x00 };
            SPI.Read(dataread);
            controller.Write(CS_HOLT, PinValue.High);
            var config = dataread[1] | (dataread[0] << 8);
            Console.WriteLine("ReadConfig result = " + config.ToString("X4"));

            if (config != HOLT_CFG_REGISTER)
            {
                Console.WriteLine("Error Init Holt");
                return false;
            }
            return true;
        }
    }
}