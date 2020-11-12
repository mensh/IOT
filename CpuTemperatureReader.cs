using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace IOT
{
    public class CpuTemperatureReader 
    {   
        public static double Reader()
        {
            var result = "";
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/cat",
                    Arguments = $"\"/sys/class/thermal/thermal_zone0/temp\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            string replacement = result.Replace(Environment.NewLine, "");
            var temperature = 0.0f;
            if (float.TryParse(replacement, out temperature))
            {
     
                return temperature/1000.0;
            }
            else
                return 0.0f;
        }

       
        
    }
}