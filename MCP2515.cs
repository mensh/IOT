using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

namespace IOT
{
    public class MCP2515
    {
        //-------------------------------------
        #region REGISTERS
        //-------------------------------------
        private const byte RXF0SIDH = 0x00;
        private const byte RXF0SIDL = 0x01;
        private const byte RXF0EID8 = 0x02;
        private const byte RXF0EID0 = 0x03;
        private const byte RXF1SIDH = 0x04;
        private const byte RXF1SIDL = 0x05;
        private const byte RXF1EID8 = 0x06;
        private const byte RXF1EID0 = 0x07;
        private const byte RXF2SIDH = 0x08;
        private const byte RXF2SIDL = 0x09;
        private const byte RXF2EID8 = 0x0A;
        private const byte RXF2EID0 = 0x0B;
        private const byte BFPCTRL = 0x0C;
        private const byte TXRTSCTRL = 0x0D;
        private const byte CANSTAT = 0x0E;
        private const byte CANCTRL = 0x0F;
        private const byte RXF3SIDH = 0x10;
        private const byte RXF3SIDL = 0x11;
        private const byte RXF3EID8 = 0x12;
        private const byte RXF3EID0 = 0x13;
        private const byte RXF4SIDH = 0x14;
        private const byte RXF4SIDL = 0x15;
        private const byte RXF4EID8 = 0x16;
        private const byte RXF4EID0 = 0x17;
        private const byte RXF5SIDH = 0x18;
        private const byte RXF5SIDL = 0x19;
        private const byte RXF5EID8 = 0x1A;
        private const byte RXF5EID0 = 0x1B;
        private const byte TEC = 0x1C;
        private const byte REC = 0x1D;
        private const byte RXM0SIDH = 0x20;
        private const byte RXM0SIDL = 0x21;
        private const byte RXM0EID8 = 0x22;
        private const byte RXM0EID0 = 0x23;
        private const byte RXM1SIDH = 0x24;
        private const byte RXM1SIDL = 0x25;
        private const byte RXM1EID8 = 0x26;
        private const byte RXM1EID0 = 0x27;
        private const byte CNF3 = 0x28;
        private const byte CNF2 = 0x29;
        private const byte CNF1 = 0x2A;
        private const byte CANINTE = 0x2B;
        private const byte MERRE = 7;
        private const byte WAKIE = 6;
        private const byte ERRIE = 5;
        private const byte TX2IE = 4;
        private const byte TX1IE = 3;
        private const byte TX0IE = 2;
        private const byte RX1IE = 1;
        private const byte RX0IE = 0;
        private const byte CANINTF = 0x2C;
        private const byte MERRF = 7;
        private const byte WAKIF = 6;
        private const byte ERRIF = 5;
        private const byte TX2IF = 4;
        private const byte TX1IF = 3;
        private const byte TX0IF = 2;
        private const byte TX0IF_MASK = 0x04;
        private const byte RX1IF = 1;
        private const byte RX0IF = 0;
        private const byte RX1IF_MASK = 0x02;
        private const byte RX0IF_MASK = 0x01;
        private const byte EFLG = 0x2D;
        private const byte TXB0CTRL = 0x30;
        private const byte TXREQ = 3;
        private const byte TXB0SIDH = 0x31;
        private const byte TXB0SIDL = 0x32;
        private const byte EXIDE = 3;
        private const byte EXIDE_MASK = 0x08;
        private const byte TXB0EID8 = 0x33;
        private const byte TXB0EID0 = 0x34;
        private const byte TXB0DLC = 0x35;
        private const byte TXRTR = 7;
        private const byte TXB0D0 = 0x36;
        private const byte RXB0CTRL = 0x60;
        private const byte RXM1 = 6;
        private const byte RXM0 = 5;
        private const byte RXRTR = 3;
        // Bits 2:0 FILHIT2:0
        private const byte RXB0SIDH = 0x61;
        private const byte RXB0SIDL = 0x62;
        private const byte RXB0EID8 = 0x63;
        private const byte RXB0EID0 = 0x64;
        private const byte RXB0DLC = 0x65;
        private const byte RXB0D0 = 0x66;
        //-------------------------------------
        #endregion REGISTERS
        //-------------------------------------

        //MCP2515 Command Bytes
        private readonly byte RESET = 0xC0;
        private readonly byte READ = 0x03;
        private readonly byte READ_RX_BUFFER = 0x90;
        private readonly byte WRITE = 0x02;
        private readonly byte LOAD_TX_BUFFER = 0x40;
        private readonly byte RTS = 0x80;
        private readonly byte READ_STATUS = 0xA0;
        private readonly byte RX_STATUS = 0xB0;
        private readonly byte BIT_MODIFY = 0x05;

        /// <summary>Represent a LOW  (false) state.</summary>
        private const bool LOW = false;
        /// <summary>Represent a HIGH (true) state.</summary>
        private const bool HIGH = true;
        /// <summary></summary>
        private const byte DataIndexOffset = 2;
        /// <summary>The max CAN ID that can be represented on an 11 bit ID.</summary>
        private const int CANID_11BITS = 0x7FF;

        /// <summary>
        /// Represents the available baud rates.
        /// </summary>
        public enum enBaudRate
        {
            CAN_5KBPS,
            CAN_10KBPS,
            CAN_20KBPS,
            CAN_31K25BPS,
            CAN_33KBPS,
            CAN_40KBPS,
            CAN_50KBPS,
            CAN_80KBPS,
            CAN_83K3BPS,
            CAN_95KBPS,
            CAN_100KBPS,
            CAN_125KBPS,
            CAN_200KBPS,
            CAN_250KBPS,
            CAN_500KBPS,
            CAN_1000KBPS
        }


        private const byte MCP_8MHz_1000kBPS_CFG1 = (0x00);
        private const byte MCP_8MHz_1000kBPS_CFG2 = (0x80);
        private const byte MCP_8MHz_1000kBPS_CFG3 = (0x80);

        private const byte MCP_8MHz_500kBPS_CFG1 = (0x00);
        private const byte MCP_8MHz_500kBPS_CFG2 = (0x90);
        private const byte MCP_8MHz_500kBPS_CFG3 = (0x82);

        private const byte MCP_8MHz_250kBPS_CFG1 = (0x00);
        private const byte MCP_8MHz_250kBPS_CFG2 = (0xB1);
        private const byte MCP_8MHz_250kBPS_CFG3 = (0x85);

        private const byte MCP_8MHz_200kBPS_CFG1 = (0x00);
        private const byte MCP_8MHz_200kBPS_CFG2 = (0xB4);
        private const byte MCP_8MHz_200kBPS_CFG3 = (0x86);

        private const byte MCP_8MHz_125kBPS_CFG1 = (0x01);
        private const byte MCP_8MHz_125kBPS_CFG2 = (0xB1);
        private const byte MCP_8MHz_125kBPS_CFG3 = (0x85);

        private const byte MCP_8MHz_100kBPS_CFG1 = (0x01);
        private const byte MCP_8MHz_100kBPS_CFG2 = (0xB4);
        private const byte MCP_8MHz_100kBPS_CFG3 = (0x86);

        private const byte MCP_8MHz_80kBPS_CFG1 = (0x01);
        private const byte MCP_8MHz_80kBPS_CFG2 = (0xBF);
        private const byte MCP_8MHz_80kBPS_CFG3 = (0x87);

        private const byte MCP_8MHz_50kBPS_CFG1 = (0x03);
        private const byte MCP_8MHz_50kBPS_CFG2 = (0xB4);
        private const byte MCP_8MHz_50kBPS_CFG3 = (0x86);

        private const byte MCP_8MHz_40kBPS_CFG1 = (0x03);
        private const byte MCP_8MHz_40kBPS_CFG2 = (0xBF);
        private const byte MCP_8MHz_40kBPS_CFG3 = (0x87);

        private const byte MCP_8MHz_33k3BPS_CFG1 = (0x47);
        private const byte MCP_8MHz_33k3BPS_CFG2 = (0xE2);
        private const byte MCP_8MHz_33k3BPS_CFG3 = (0x85);

        private const byte MCP_8MHz_31k25BPS_CFG1 = (0x07);
        private const byte MCP_8MHz_31k25BPS_CFG2 = (0xA4);
        private const byte MCP_8MHz_31k25BPS_CFG3 = (0x84);

        private const byte MCP_8MHz_20kBPS_CFG1 = (0x07);
        private const byte MCP_8MHz_20kBPS_CFG2 = (0xBF);
        private const byte MCP_8MHz_20kBPS_CFG3 = (0x87);

        private const byte MCP_8MHz_10kBPS_CFG1 = (0x0F);
        private const byte MCP_8MHz_10kBPS_CFG2 = (0xBF);
        private const byte MCP_8MHz_10kBPS_CFG3 = (0x87);

        private const byte MCP_8MHz_5kBPS_CFG1 = (0x1F);
        private const byte MCP_8MHz_5kBPS_CFG2 = (0xBF);
        private const byte MCP_8MHz_5kBPS_CFG3 = (0x87);

        /// <summary>Represents a CAN message.</summary>
        public class CANMSG
        {
            public uint CANID { get; set; }
            public bool IsExtended { get { return CANID > CANID_11BITS; } }
            public bool IsRemote { get; set; }
            public int DataLength { get { return data.Length; } }
            public byte[] data = new byte[8];
        }

        /// <summary>Initialize the CAN transceiver.</summary>
        /// <param name="baudrate">The selected baud rate.</param>
        /// <returns>True if configuration was successful.</returns>
        /// <remarks>Transceiver needs to be set to normal mode before starting TX/RX operations.</remarks>
        public async System.Threading.Tasks.Task<bool> InitCANAsync(enBaudRate baudrate)
        {
            // Configure SPI            

            byte mode = 0;
            int counterInit = 0;
            // Write reset to the CAN transceiver.
            while (mode != 0x04)
            {

                await Hardware.SPIWork(new byte[] { RESET }, null, Hardware.CS_CAN);

                //Read mode and make sure it is config
                Thread.Sleep(100);

                mode = (byte)(await ReadRegisterAsync(CANSTAT) >> 5);
                if (mode != 0x04)
                {
                    counterInit++;
                    if (counterInit > 100)
                    {
                        return false;
                    }
                }
                else
                {
                    await SetCANBaudAsync(baudrate);
                    return true;
                }
            }
            return false;
        }

        /// <summary>Set the CAN baud rate.</summary>
        /// <param name="baudrate">The configured baudrate.</param>
        /// <returns>True if configured.</returns>
        private async System.Threading.Tasks.Task<bool> SetCANBaudAsync(enBaudRate baudrate)
        {

            byte cfg1, cfg2, cfg3;
            //BRP<5:0> = 00h, so divisor (0+1)*2 for 125ns per quantum at 16MHz for 500K   
            //SJW<1:0> = 00h, Sync jump width = 1
            switch (baudrate)
            {
                case (enBaudRate.CAN_5KBPS):                                               //   5KBPS
                    cfg1 = MCP_8MHz_5kBPS_CFG1;
                    cfg2 = MCP_8MHz_5kBPS_CFG2;
                    cfg3 = MCP_8MHz_5kBPS_CFG3;
                    break;

                case (enBaudRate.CAN_10KBPS):                                              //  10KBPS
                    cfg1 = MCP_8MHz_10kBPS_CFG1;
                    cfg2 = MCP_8MHz_10kBPS_CFG2;
                    cfg3 = MCP_8MHz_10kBPS_CFG3;
                    break;

                case (enBaudRate.CAN_20KBPS):                                              //  20KBPS
                    cfg1 = MCP_8MHz_20kBPS_CFG1;
                    cfg2 = MCP_8MHz_20kBPS_CFG2;
                    cfg3 = MCP_8MHz_20kBPS_CFG3;
                    break;

                case (enBaudRate.CAN_31K25BPS):                                            //  31.25KBPS
                    cfg1 = MCP_8MHz_31k25BPS_CFG1;
                    cfg2 = MCP_8MHz_31k25BPS_CFG2;
                    cfg3 = MCP_8MHz_31k25BPS_CFG3;
                    break;

                case (enBaudRate.CAN_33KBPS):                                              //  33.333KBPS
                    cfg1 = MCP_8MHz_33k3BPS_CFG1;
                    cfg2 = MCP_8MHz_33k3BPS_CFG2;
                    cfg3 = MCP_8MHz_33k3BPS_CFG3;
                    break;

                case (enBaudRate.CAN_40KBPS):                                              //  40Kbps
                    cfg1 = MCP_8MHz_40kBPS_CFG1;
                    cfg2 = MCP_8MHz_40kBPS_CFG2;
                    cfg3 = MCP_8MHz_40kBPS_CFG3;
                    break;

                case (enBaudRate.CAN_50KBPS):                                              //  50Kbps
                    cfg1 = MCP_8MHz_50kBPS_CFG1;
                    cfg2 = MCP_8MHz_50kBPS_CFG2;
                    cfg3 = MCP_8MHz_50kBPS_CFG3;
                    break;

                case (enBaudRate.CAN_80KBPS):                                              //  80Kbps
                    cfg1 = MCP_8MHz_80kBPS_CFG1;
                    cfg2 = MCP_8MHz_80kBPS_CFG2;
                    cfg3 = MCP_8MHz_80kBPS_CFG3;
                    break;

                case (enBaudRate.CAN_100KBPS):                                             // 100Kbps
                    cfg1 = MCP_8MHz_100kBPS_CFG1;
                    cfg2 = MCP_8MHz_100kBPS_CFG2;
                    cfg3 = MCP_8MHz_100kBPS_CFG3;
                    break;

                case (enBaudRate.CAN_125KBPS):                                             // 125Kbps
                    cfg1 = MCP_8MHz_125kBPS_CFG1;
                    cfg2 = MCP_8MHz_125kBPS_CFG2;
                    cfg3 = MCP_8MHz_125kBPS_CFG3;
                    break;

                case (enBaudRate.CAN_200KBPS):                                             // 200Kbps
                    cfg1 = MCP_8MHz_200kBPS_CFG1;
                    cfg2 = MCP_8MHz_200kBPS_CFG2;
                    cfg3 = MCP_8MHz_200kBPS_CFG3;
                    break;

                case (enBaudRate.CAN_250KBPS):                                             // 250Kbps
                    cfg1 = MCP_8MHz_250kBPS_CFG1;
                    cfg2 = MCP_8MHz_250kBPS_CFG2;
                    cfg3 = MCP_8MHz_250kBPS_CFG3;
                    break;

                case (enBaudRate.CAN_500KBPS):                                             // 500Kbps
                    cfg1 = MCP_8MHz_500kBPS_CFG1;
                    cfg2 = MCP_8MHz_500kBPS_CFG2;
                    cfg3 = MCP_8MHz_500kBPS_CFG3;
                    break;

                case (enBaudRate.CAN_1000KBPS):                                            //   1Mbps
                    cfg1 = MCP_8MHz_1000kBPS_CFG1;
                    cfg2 = MCP_8MHz_1000kBPS_CFG2;
                    cfg3 = MCP_8MHz_1000kBPS_CFG3;
                    break;
                default:
                    return false;
            }

            //  byte[] cmdBuffer = new byte[] { WRITE, CNF1, (byte)(brp & 0x3F) };
            byte[] cmdBuffer = new byte[] { WRITE, CNF1, (byte)(cfg1) };

            await Hardware.SPIWork(cmdBuffer, null, Hardware.CS_CAN);


            //PRSEG<2:0> = 0x01, 2 time quantum for prop
            //PHSEG<2:0> = 0x06, 7 time constants to PS1 sample
            //SAM = 0, just 1 sampling
            //BTLMODE = 1, PS2 determined by CNF3


            //      Hardware.ReadWriteSPi(new byte[] { WRITE, CNF2, 0xB1 }, 0);
            await Hardware.SPIWork(new byte[] { WRITE, CNF2, cfg2 }, null, Hardware.CS_CAN);


            //PHSEG2<2:0> = 5 for 6 time constants after sample

            //   Hardware.ReadWriteSPi(new byte[] { WRITE, CNF3, 0x05 }, 0);
            await Hardware.SPIWork(new byte[] { WRITE, CNF3, cfg3 }, null, Hardware.CS_CAN);



            //SyncSeg + PropSeg + PS1 + PS2 = 1 + 2 + 7 + 6 = 16
            return true;
        }

        /// <summary>Writes a value to the selected register.</summary>
        /// <param name="registerAddress">The address of the register.</param>
        /// <param name="value">The value to be write to the register.</param>
        private async System.Threading.Tasks.Task WriteRegisterAsync(byte registerAddress, byte value)
        {

            await Hardware.SPIWork(new byte[] { WRITE, registerAddress, value }, null, Hardware.CS_CAN);


        }

        /// <summary>Reads the value of the selected register.</summary>
        /// <param name="registerAddress">The address of the register.</param>
        /// <returns>A byte with the value read from the register.</returns>
        private async System.Threading.Tasks.Task<byte> ReadRegisterAsync(byte registerAddress)
        {
            byte[] CmdBuffer = new byte[] { READ, registerAddress };
            byte[] outData = new byte[1];


            await Hardware.SPIWork(CmdBuffer, outData, Hardware.CS_CAN);

            return outData[0];
        }

        /// <summary>Writes a bit to a register.</summary>
        /// <param name="registerAddress">The address of the register that supports bit writing.</param>
        /// <param name="bitNumber">The zero index based of the bit to write.</param>
        /// <param name="value">the value of the bit to write.</param>
        private async System.Threading.Tasks.Task WriteRegisterBitAsync(byte registerAddress, byte bitNumber, byte value)
        {
            //spi.Write(new byte[] { BIT_MODIFY, regno, (byte)(1 << bitno) });
            if (value != 0)
            {

                await Hardware.SPIWork(new byte[] { BIT_MODIFY, registerAddress, (byte)(1 << bitNumber), 0xFF }, null, Hardware.CS_CAN);

            }
            else
            {
                await Hardware.SPIWork(new byte[] { BIT_MODIFY, registerAddress, (byte)(1 << bitNumber), 0x00 }, null, Hardware.CS_CAN);

            }
        }

        /// <summary>Transmit a CAN message to the bus.</summary>
        /// <param name="message">The CAN Message to be transmitted.</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<bool> TransmitAsync(CANMSG message, int timeout)
        {
            // Holds if message was sent or not.
            bool sentMessage = false;

            // Calculate the end time based on current device time.
            TimeSpan startTime = DateTime.Now.TimeOfDay;
            TimeSpan endTime = startTime.Add(new TimeSpan(0, 0, 0, 0, timeout));

            //--------------------------------------
            // Set the CAN ID.
            byte val = 0x00;
            uint bit11Address = 0x00;
            uint bit18Address = 0x00;

            // Build Extended CAN ID 
            if (message.IsExtended)
            {
                // Split the 11 bit and 18 bit sections of the ID.
                bit11Address = (uint)((message.CANID >> 16) & 0xFFFF);
                bit18Address = (uint)(message.CANID & 0xFFFFF);
                // Set the first part of the ID
                val = (byte)(bit11Address >> 5);
                await WriteRegisterAsync(TXB0SIDH, val);
                // Set the second part of the ID.
                val = (byte)(bit11Address << 3);
                val |= (byte)(bit11Address & 0x07);
                // Mark the message as extended.
                val |= 1 << EXIDE;
                await WriteRegisterAsync(TXB0SIDL, val);
                // Write the 18 bit part of the ID
                val = (byte)(bit18Address >> 8);
                await WriteRegisterAsync(TXB0EID8, val);
                val = (byte)(bit18Address);
                await WriteRegisterAsync(TXB0EID0, val);
            }
            else
            {
                // Transmit a 11 bit ID.
                bit11Address = (uint)(message.CANID);
                val = (byte)(bit11Address >> 3);
                await WriteRegisterAsync(TXB0SIDH, val);
                val = (byte)(bit11Address << 5);
                await WriteRegisterAsync(TXB0SIDL, val);
            }


            //--------------------------------------
            val = (byte)(message.DataLength & 0x0f);
            // Check if is a remote frame
            if (message.IsRemote)
            {
                // Mark the frame as remote
                val |= (byte)(1UL << (TXRTR));
                await WriteRegisterBitAsync(val, TXRTR, 1);
            }
            await WriteRegisterAsync(TXB0DLC, val);

            //--------------------------------------
            //Write Message Data
            byte[] txDATA = new byte[10];
            txDATA[0] = WRITE;
            txDATA[1] = TXB0D0;
            for (int i = DataIndexOffset; i < message.DataLength + DataIndexOffset; i++)
            {
                txDATA[i] = message.data[i - DataIndexOffset];
            }


            await Hardware.SPIWork(txDATA, null, Hardware.CS_CAN);


            //----------------------
            // Command to transmit the CAN message
            await WriteRegisterBitAsync(TXB0CTRL, TXREQ, 1);

            //----------------------
            // Wait untile time out to get confirmation of message was sent.
            //  while (DateTime.Now.TimeOfDay < endTime)
            // {
            val = await ReadRegisterAsync(CANINTF);
            if ((val & TX0IF_MASK) == TX0IF_MASK)
            {
                sentMessage = true;
                //break;
            }
            // }

            ////Abort the send if failed
            await WriteRegisterBitAsync(TXB0CTRL, TXREQ, 0);

            ////And clear write interrupt
            await WriteRegisterBitAsync(CANINTF, TX0IF, 0);

            return sentMessage;
        }

        /// <summary>
        /// Check if a new message was received by the transceiver.
        /// </summary>
        /// <param name="msg">The CAN message that will contain the retrived message.</param>
        /// <param name="timeout">The time to wait if a message become available.</param>
        /// <returns>True if a message was received.</returns>
        public async System.Threading.Tasks.Task<bool> ReceiveAsync(CANMSG msg, int timeout)
        {
            bool gotMessage = false;
            byte val;
            msg = new CANMSG();

            TimeSpan startTime = DateTime.Now.TimeOfDay;
            TimeSpan endTime = startTime.Add(new TimeSpan(0, 0, 0, 0, timeout));

            gotMessage = false;
            // while (DateTime.Now.TimeOfDay < endTime)
            // {
            val = await ReadRegisterAsync(CANINTF);
            //If we have a message available, read it
            if ((val & RX0IF_MASK) == RX0IF_MASK)
            {
                gotMessage = true;
                //break;
            }
            //  }

            if (gotMessage)
            {
                val = await ReadRegisterAsync(RXB0CTRL);
                msg.IsRemote = ((val & 0x04) == 0x04) ? true : false;

                //Address received from
                int adddresVal = 0;
                val = await ReadRegisterAsync(RXB0SIDH);
                adddresVal |= (val << 3);
                val = await ReadRegisterAsync(RXB0SIDL);
                adddresVal |= (val >> 5);

                bool isExtended = ((val & EXIDE_MASK) == EXIDE_MASK) ? true : false;
                uint adddresExtVal = 0;
                if (isExtended)
                {
                    adddresExtVal = (uint)((val & 0x03) << 16);
                    val = await ReadRegisterAsync(RXB0EID8);
                    adddresExtVal |= (uint)(val << 8);
                    val = await ReadRegisterAsync(RXB0EID0);
                    adddresExtVal |= val;

                    adddresVal = (int)((adddresVal << 18) | adddresExtVal);
                }
                msg.CANID = (uint)adddresVal;

                //Read data bytes
                val = await ReadRegisterAsync(RXB0DLC);
                int dataLen = (val & 0xf);

                byte[] CmdBuffer = new byte[] { READ, RXB0D0 };
                msg.data = new byte[dataLen];


                await Hardware.SPIWork(CmdBuffer, msg.data, Hardware.CS_CAN);




                //And clear read interrupt
                await WriteRegisterBitAsync(CANINTF, RX0IF, 0);
            }
            else
            {
            }
            return gotMessage;
        }



        /// <summary>Set transceiver to normal state.</summary>
        /// <remarks>This state needs to be selected before starting TX/RX.</remarks>
        public async System.Threading.Tasks.Task SetCANNormalModeAsync()
        {
            //REQOP2<2:0> = 000 for normal mode
            //ABAT = 0, do not abort pending transmission
            //OSM = 0, not one shot
            //CLKEN = 1, disable output clock
            //CLKPRE = 0b11, clk/8
            await WriteRegisterAsync(CANCTRL, 0x07);
            //Read mode and make sure it is normal
            byte mode = await ReadRegisterAsync(CANSTAT);
            mode = (byte)(mode >> 5);
            if (mode != 0)
            { }

            // Set RX buffer control to turn filters OFF and receive any message.
            await WriteRegisterAsync(RXB0CTRL, 0x60);
            await WriteRegisterBitAsync(CANINTE, RX0IE, 1); //Enable INT RX

        }
    }
}
