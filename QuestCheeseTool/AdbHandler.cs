using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestCheeseTool
{
    internal class AdbHandler
    {
        public static Dictionary<string, AdbDevice> ConnectedDevices = new Dictionary<string, AdbDevice>();
        public static void RefreshConnectedDevices()
        {
            for (int i = ConnectedDevices.Count - 1; i >= 0; i--)
            {
                var deviceId = ConnectedDevices.ElementAt(i).Key;
                if (!ConnectedDevices[deviceId].IsConnected() || !ConnectedDevices[deviceId].IsSupportedDevice())
                {
                    Console.WriteLine($"Removing device {deviceId} from connected devices.");
                    ConnectedDevices.Remove(deviceId);
                }
            }
            var output = Util.RunADBCommand(new List<string> { "devices" });
            if (output.ExitCode != 0)
            {
                Console.WriteLine("Error retrieving connected devices:");
                foreach (var line in output.Output)
                {
                    Console.WriteLine(line);
                }
            }
            foreach (var line in output.Output)
            {
                if (line.Contains("\tdevice"))
                {
                    var deviceId = line.Split('\t')[0];
                    if (!ConnectedDevices.ContainsKey(deviceId))
                    {
                        var device = new AdbDevice(deviceId);
                        if (device.IsConnected() && device.IsSupportedDevice())
                        {
                            ConnectedDevices[deviceId] = device;
                            Console.WriteLine($"Device {deviceId} is connected and supported.");
                        }
                        else
                        {
                            Console.WriteLine($"Device {deviceId} is either not connected or not supported.");
                        }
                    }
                }
            }
        }
    }
}
