using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestCheeseTool
{
    internal class AdbDevice
    {
        public string Id { get; private set; }
        public AdbDevice(string id)
        {
            Id = id;
        }

        public (int ExitCode, List<string> Output) RunCommand(List<string> arguments)
        {
            arguments.Insert(0, "-s");
            arguments.Insert(1, Id);
            var output = Util.RunADBCommand(arguments);
            return output;
        }

        public bool IsConnected()
        {
            var output = RunCommand(new List<string> { "get-state" });
            if (output.ExitCode != 0 || output.Output.Count == 0)
            {
                Console.WriteLine("Failed to retrieve device state.");
                return false;
            }
            return output.Output[0].Trim().Equals("device", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsSupportedDevice()
        {
            var output = RunCommand(new List<string> { "shell", "getprop", "ro.product.product.device" });
            if (output.ExitCode != 0 || output.Output.Count == 0)
            {
                Console.WriteLine("Failed to retrieve device properties.");
                return false;
            }
            string deviceName = output.Output[0].Trim();

            return deviceName.Equals("eureka", StringComparison.OrdinalIgnoreCase) ||
                   deviceName.Equals("panther", StringComparison.OrdinalIgnoreCase);
        }
    }
}
