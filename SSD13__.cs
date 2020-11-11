using System;
using System.Device.Gpio;
using System.Device.I2c;
using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Commands;
using Ssd1306Cmnds = Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using SixLabors.Fonts;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
namespace IOT
{
    public class SSD13__
    {

        public Ssd1306 device;

        public SSD13__()
        {
            device = GetSsd1306WithI2c();
            InitializeSsd1306(device);
            ClearScreenSsd1306(device);
           // SendMessageSsd1306(device, DisplayIpAddress());
        }
        I2cDevice GetI2CDevice()
        {
            Console.WriteLine("Using I2C protocol");
            I2cConnectionSettings connectionSettings = new I2cConnectionSettings(3, 0x3C);
            return I2cDevice.Create(connectionSettings);
            
        }
        public void ClearScreenSsd1306(Ssd1306 device)
        {
            device.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
            device.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page0,
                Ssd1306Cmnds.PageAddress.Page3));

            for (int cnt = 0; cnt < 32; cnt++)
            {
                byte[] data = new byte[16];
                device.SendData(data);
            }
        }
        Ssd1306 GetSsd1306WithI2c()
        {
            var device = GetI2CDevice();
            return new Ssd1306(GetI2CDevice());
            
        }


        string DisplayIpAddress()
        {
            string ipAddress = GetIpAddress();

            if (!string.IsNullOrEmpty(ipAddress))
            {
                return $"IP:{ipAddress}";
            }
            else
            {
                return $"Error: IP Address Not Found";
            }
        }




        public void SendMessageSsd1306(Ssd1306 device, string message)
        {
            device.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
            device.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page0,
                Ssd1306Cmnds.PageAddress.Page3));

            foreach (char character in message)
            {
                device.SendData(BasicFont.GetCharacterBytes(character));
            }
        }

        string GetIpAddress()
        {
            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection).
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                // Read the IP configuration for each network
                IPInterfaceProperties properties = network.GetIPProperties();

                if (network.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    network.OperationalStatus == OperationalStatus.Up &&
                    !network.Description.ToLower().Contains("virtual") &&
                    !network.Description.ToLower().Contains("pseudo"))
                {
                    // Each network interface may have multiple IP addresses.
                    foreach (IPAddressInformation address in properties.UnicastAddresses)
                    {
                        // We're only interested in IPv4 addresses for now.
                      //  if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                      //  {
                      //      continue;
                      //  }

                        // Ignore loopback addresses (e.g., 127.0.0.1).
                        if (IPAddress.IsLoopback(address.Address))
                        {
                            continue;
                        }

                        return address.Address.ToString();
                    }
                }
            }

            return null;
        }


        void InitializeSsd1306(Ssd1306 device)
        {
            device.SendCommand(new SetDisplayOff());
            device.SendCommand(new Ssd1306Cmnds.SetDisplayClockDivideRatioOscillatorFrequency(0x00, 0x08));
            device.SendCommand(new SetMultiplexRatio(0x1F));
            device.SendCommand(new Ssd1306Cmnds.SetDisplayOffset(0x00));
            device.SendCommand(new Ssd1306Cmnds.SetDisplayStartLine(0x00));
            device.SendCommand(new Ssd1306Cmnds.SetChargePump(true));
            device.SendCommand(
                new Ssd1306Cmnds.SetMemoryAddressingMode(Ssd1306Cmnds.SetMemoryAddressingMode.AddressingMode
                    .Horizontal));
            device.SendCommand(new Ssd1306Cmnds.SetSegmentReMap(true));
            device.SendCommand(new Ssd1306Cmnds.SetComOutputScanDirection(false));
            device.SendCommand(new Ssd1306Cmnds.SetComPinsHardwareConfiguration(false, false));
            device.SendCommand(new SetContrastControlForBank0(0x8F));
            device.SendCommand(new Ssd1306Cmnds.SetPreChargePeriod(0x01, 0x0F));
            device.SendCommand(
                new Ssd1306Cmnds.SetVcomhDeselectLevel(Ssd1306Cmnds.SetVcomhDeselectLevel.DeselectLevel.Vcc1_00));
            device.SendCommand(new Ssd1306Cmnds.EntireDisplayOn(false));
            device.SendCommand(new Ssd1306Cmnds.SetNormalDisplay());
            device.SendCommand(new SetDisplayOn());
            device.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
            device.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page1,
                Ssd1306Cmnds.PageAddress.Page3));
        }
    }
    internal class BasicFont
    {
        private static IDictionary<char, byte[]> FontCharacterData =>
            new Dictionary<char, byte[]>
            {
                // Special Characters.
                { ' ', new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
                { '!', new byte[] { 0x00, 0x00, 0x5F, 0x00, 0x00, 0x00 } },
                { '\"', new byte[] { 0x00, 0x06, 0x00, 0x06, 0x00, 0x00 } },
                { '#', new byte[] { 0x14, 0x7F, 0x14, 0x7F, 0x14, 0x00 } },
                { '$', new byte[] { 0x04, 0x2A, 0x6D, 0x2A, 0x10, 0x00 } },
                { '%', new byte[] { 0x27, 0x16, 0x08, 0x34, 0x32, 0x00 } },
                { '&', new byte[] { 0x20, 0x56, 0x49, 0x36, 0x50, 0x00 } },
                { '\'', new byte[] { 0x00, 0x03, 0x05, 0x00, 0x00, 0x00 } },
                { '(', new byte[] { 0x00, 0x00, 0x1D, 0x22, 0x41, 0x00 } },
                { ')', new byte[] { 0x41, 0x22, 0x1D, 0x00, 0x00, 0x00 } },
                { '*', new byte[] { 0x14, 0x08, 0x3E, 0x08, 0x14, 0x00 } },
                { '+', new byte[] { 0x20, 0x56, 0x49, 0x36, 0x50, 0x00 } },
                { ',', new byte[] { 0x00, 0x50, 0x30, 0x00, 0x00, 0x00 } },
                { '-', new byte[] { 0x08, 0x08, 0x08, 0x08, 0x08, 0x00 } },
                { '.', new byte[] { 0x00, 0x30, 0x30, 0x00, 0x00, 0x00 } },
                { '/', new byte[] { 0x20, 0x10, 0x08, 0x04, 0x02, 0x00 } },
                { ':', new byte[] { 0x00, 0x36, 0x36, 0x00, 0x00, 0x00 } },
                { ';', new byte[] { 0x00, 0x56, 0x36, 0x00, 0x00, 0x00 } },
                { '<', new byte[] { 0x08, 0x14, 0x22, 0x41, 0x00, 0x00 } },
                { '=', new byte[] { 0x14, 0x14, 0x14, 0x14, 0x14, 0x00 } },
                { '>', new byte[] { 0x00, 0x41, 0x22, 0x14, 0x08, 0x00 } },
                { '?', new byte[] { 0x02, 0x01, 0x51, 0x09, 0x06, 0x00 } },
                { '@', new byte[] { 0x3E, 0x41, 0x4D, 0x4D, 0x06, 0x00 } },
                { '[', new byte[] { 0x00, 0x7F, 0x41, 0x41, 0x00 } },
                { '\\', new byte[] { 0x02, 0x04, 0x08, 0x10, 0x20 } },
                { ']', new byte[] { 0x00, 0x41, 0x41, 0x7F, 0x00 } },
                { '^', new byte[] { 0x04, 0x02, 0x01, 0x02, 0x04 } },
                { '`', new byte[] { 0x00, 0x00, 0x05, 0x03, 0x00 } },

                // Numbers.
                { '0', new byte[] { 0x3E, 0x51, 0x49, 0x45, 0x3E, 0x00 } },
                { '1', new byte[] { 0x00, 0x42, 0x7F, 0x40, 0x00, 0x00 } },
                { '2', new byte[] { 0x42, 0x61, 0x51, 0x49, 0x46, 0x00 } },
                { '3', new byte[] { 0x21, 0x41, 0x45, 0x4B, 0x31, 0x00 } },
                { '4', new byte[] { 0x18, 0x14, 0x12, 0x7F, 0x10, 0x00 } },
                { '5', new byte[] { 0x27, 0x45, 0x45, 0x45, 0x39, 0x00 } },
                { '6', new byte[] { 0x3C, 0x4A, 0x49, 0x49, 0x30, 0x00 } },
                { '7', new byte[] { 0x01, 0x71, 0x09, 0x05, 0x03, 0x00 } },
                { '8', new byte[] { 0x36, 0x49, 0x49, 0x49, 0x36, 0x00 } },
                { '9', new byte[] { 0x06, 0x49, 0x49, 0x29, 0x1E, 0x00 } },

                // Characters.
                { 'A', new byte[] { 0x7E, 0x11, 0x11, 0x11, 0x7E, 0x00 } },
                { 'B', new byte[] { 0x7F, 0x49, 0x49, 0x49, 0x36, 0x00 } },
                { 'C', new byte[] { 0x3E, 0x41, 0x41, 0x41, 0x22, 0x00 } },
                { 'D', new byte[] { 0x7F, 0x41, 0x41, 0x22, 0x1C, 0x00 } },
                { 'E', new byte[] { 0x7F, 0x49, 0x49, 0x49, 0x41, 0x00 } },
                { 'F', new byte[] { 0x7F, 0x09, 0x09, 0x09, 0x01, 0x00 } },
                { 'G', new byte[] { 0x3E, 0x41, 0x49, 0x49, 0x7A, 0x00 } },
                { 'H', new byte[] { 0x7F, 0x08, 0x08, 0x08, 0x7F, 0x00 } },
                { 'I', new byte[] { 0x00, 0x41, 0x7F, 0x41, 0x00, 0x00 } },
                { 'J', new byte[] { 0x20, 0x40, 0x41, 0x3F, 0x01, 0x00 } },
                { 'K', new byte[] { 0x7F, 0x08, 0x14, 0x22, 0x41, 0x00 } },
                { 'L', new byte[] { 0x7F, 0x40, 0x40, 0x40, 0x40, 0x00 } },
                { 'M', new byte[] { 0x7F, 0x02, 0x0C, 0x02, 0x7F, 0x00 } },
                { 'N', new byte[] { 0x7F, 0x04, 0x08, 0x10, 0x7F, 0x00 } },
                { 'O', new byte[] { 0x3E, 0x41, 0x41, 0x41, 0x3E, 0x00 } },
                { 'P', new byte[] { 0x7F, 0x09, 0x09, 0x09, 0x06, 0x00 } },
                { 'Q', new byte[] { 0x3E, 0x41, 0x51, 0x21, 0x5E, 0x00 } },
                { 'R', new byte[] { 0x7F, 0x09, 0x19, 0x29, 0x46, 0x00 } },
                { 'S', new byte[] { 0x46, 0x49, 0x49, 0x49, 0x31, 0x00 } },
                { 'T', new byte[] { 0x01, 0x01, 0x7F, 0x01, 0x01, 0x00 } },
                { 'U', new byte[] { 0x3F, 0x40, 0x40, 0x40, 0x3F, 0x00 } },
                { 'V', new byte[] { 0x1F, 0x20, 0x40, 0x20, 0x1F, 0x00 } },
                { 'W', new byte[] { 0x3F, 0x40, 0x38, 0x40, 0x3F, 0x00 } },
                { 'X', new byte[] { 0x63, 0x14, 0x08, 0x14, 0x63, 0x00 } },
                { 'Y', new byte[] { 0x07, 0x08, 0x70, 0x08, 0x07, 0x00 } },
                { 'Z', new byte[] { 0x61, 0x51, 0x49, 0x45, 0x43, 0x00 } },
                // Small letters
                { 'a', new byte[] { 0x20, 0x54, 0x54, 0x54, 0x78 } },
                { 'b', new byte[] { 0x7F, 0x48, 0x44, 0x44, 0x38 } },
                { 'c', new byte[] { 0x38, 0x44, 0x44, 0x44, 0x20 } },
                { 'd', new byte[] { 0x38, 0x44, 0x44, 0x48, 0x7F } },
                { 'e', new byte[] { 0x38, 0x54, 0x54, 0x54, 0x18 } },
                { 'f', new byte[] { 0x08, 0x7E, 0x09, 0x01, 0x02 } },
                { 'g', new byte[] { 0x04, 0x2A, 0x2A, 0x2A, 0x1C } },
                { 'h', new byte[] { 0x7F, 0x08, 0x04, 0x04, 0x78 } },
                { 'i', new byte[] { 0x00, 0x44, 0x7D, 0x40, 0x00 } },
                { 'j', new byte[] { 0x20, 0x40, 0x44, 0x3D, 0x00 } },
                { 'k', new byte[] { 0x7F, 0x10, 0x28, 0x44, 0x00 } },
                { 'l', new byte[] { 0x00, 0x41, 0x7F, 0x40, 0x00 } },
                { 'm', new byte[] { 0x7C, 0x04, 0x18, 0x04, 0x78 } },
                { 'n', new byte[] { 0x7C, 0x08, 0x04, 0x04, 0x78 } },
                { 'o', new byte[] { 0x38, 0x44, 0x44, 0x44, 0x38 } },
                { 'p', new byte[] { 0x7C, 0x14, 0x14, 0x14, 0x08 } },
                { 'q', new byte[] { 0x08, 0x14, 0x14, 0x18, 0x7C } },
                { 'r', new byte[] { 0x7C, 0x08, 0x04, 0x04, 0x08 } },
                { 's', new byte[] { 0x48, 0x54, 0x54, 0x54, 0x20 } },
                { 't', new byte[] { 0x04, 0x3F, 0x44, 0x40, 0x20 } },
                { 'u', new byte[] { 0x3C, 0x40, 0x40, 0x20, 0x7C } },
                { 'v', new byte[] { 0x1C, 0x20, 0x40, 0x20, 0x1C } },
                { 'w', new byte[] { 0x3C, 0x40, 0x30, 0x40, 0x3C } },
                { 'x', new byte[] { 0x44, 0x28, 0x10, 0x28, 0x44 } },
                { 'y', new byte[] { 0x0C, 0x50, 0x50, 0x50, 0x3C } },
                { 'z', new byte[] { 0x44, 0x64, 0x54, 0x4C, 0x44 } }
            };

        public static byte[] GetCharacterBytes(char character)
        {
            return FontCharacterData[character];
        }
    }
}