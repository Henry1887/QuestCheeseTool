using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestCheeseTool
{
    internal class Menu
    {
        static bool IsAdbDownloaded { get { return File.Exists(Path.Join(Directory.GetCurrentDirectory(), Config.ADBExePath)); } }
        static bool IsExploitDownloaded { get { return Directory.Exists(Path.Join(Directory.GetCurrentDirectory(), Config.ExploitPath)); } }
        bool isExploitInstalled = false;
        bool isRunning = true;
        AdbDevice? currentDevice = null;
        internal void MainLoop()
        {
            while (isRunning)
            {
                Console.Clear();
                if (currentDevice != null && !currentDevice.IsConnected())
                {
                    Console.WriteLine($"Device {currentDevice.Id} is no longer connected.");
                    currentDevice = null;
                    isExploitInstalled = false;
                }
                Console.WriteLine($"QuestCheeseTool - Version {Config.Version} - By {Config.Author}");
                Console.WriteLine($"Current Device: {(currentDevice != null ? currentDevice.Id : "None")}");
                List<Intents> validOptions = GetValidOptions();
                Console.WriteLine("Available Options:");
                for (int i = 0; i < validOptions.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {validOptions[i]}");
                }
                Console.Write("Select an option (1-" + validOptions.Count + "): ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out int choice) && choice > 0 && choice <= validOptions.Count)
                {
                    Intents selectedIntent = validOptions[choice - 1];
                    HandleIntent(selectedIntent);
                }
                else
                {
                    Console.WriteLine("Invalid option. Please try again.");
                }
            }
        }

        public void HandleIntent(Intents intent)
        {
            switch (intent)
            {
                case Intents.InstallPlatformTools:
                    Util.DownloadAndExtractPlatformTools();
                    break;
                case Intents.RefreshConnectedDevices:
                    AdbHandler.RefreshConnectedDevices();
                    break;
                case Intents.ChooseDevice:
                    ChooseDevice();
                    break;
                case Intents.RebootDevice:
                    if (currentDevice != null)
                    {
                        var isRebooting = Util.RebootDevice(currentDevice);
                        if (isRebooting)
                        {
                            currentDevice = null; // Reset current device as it will be disconnected during reboot
                            isExploitInstalled = false; // Reset exploit installation status
                        }
                    }
                    break;
                case Intents.DownloadExploit:
                    Util.DownloadAndExtractExploit();
                    break;
                case Intents.InstallExploit:
                    if (currentDevice != null && !isExploitInstalled)
                    {
                        isExploitInstalled = Util.InstallExploit(currentDevice);
                    }
                    break;
                case Intents.UninstallExploit:
                    if (currentDevice != null && isExploitInstalled)
                    {
                        isExploitInstalled = !Util.UninstallExploit(currentDevice);
                    }
                    break;
                case Intents.RunExploit:
                    if (currentDevice != null && isExploitInstalled)
                    {
                        Util.RunExploit(currentDevice);
                    }
                    break;
                case Intents.VerifyRoot:
                    if (currentDevice != null)
                    {
                        bool isRooted = Util.VerifyRoot(currentDevice);
                        Console.WriteLine(isRooted ? "Device is rooted." : "Device is not rooted.");
                    }
                    break;
                case Intents.Exit:
                    isRunning = false;
                    return;
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public void ChooseDevice()
        {
            if (AdbHandler.ConnectedDevices.Count == 0)
            {
                Console.WriteLine("No connected devices found. Please connect a device and refresh.");
                return;
            }
            Console.WriteLine("Available Devices:");
            int index = 1;
            foreach (var device in AdbHandler.ConnectedDevices.Values)
            {
                Console.WriteLine($"{index++}. {device.Id}");
            }
            Console.Write("Select a device by number: ");
            string input = Console.ReadLine();
            if (int.TryParse(input, out int choice) && choice > 0 && choice <= AdbHandler.ConnectedDevices.Count)
            {
                currentDevice = AdbHandler.ConnectedDevices.ElementAt(choice - 1).Value;
                isExploitInstalled = Util.IsExploitInstalled(currentDevice);
                Console.WriteLine($"Selected Device: {currentDevice.Id}");
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }

        public List<Intents> GetValidOptions()
        {
            var validOptions = new List<Intents>();
            if (!IsExploitDownloaded)
                validOptions.Add(Intents.DownloadExploit);
            if (!IsAdbDownloaded)
                validOptions.Add(Intents.InstallPlatformTools);
            else
            {
                validOptions.Add(Intents.RefreshConnectedDevices);
                validOptions.Add(Intents.ChooseDevice);
                if (currentDevice != null)
                {
                    validOptions.Add(Intents.RebootDevice);
                    if (!isExploitInstalled)
                        validOptions.Add(Intents.InstallExploit);
                    else
                    {
                        validOptions.Add(Intents.UninstallExploit);
                        validOptions.Add(Intents.RunExploit);
                        validOptions.Add(Intents.VerifyRoot);
                    }
                }
            }
            validOptions.Add(Intents.Exit);
            return validOptions;
        }

        public enum Intents
        {
            InstallPlatformTools,
            RefreshConnectedDevices,
            ChooseDevice,
            DownloadExploit,
            InstallExploit,
            UninstallExploit,
            RunExploit,
            VerifyRoot,
            RebootDevice,
            Exit,
        }
    }
}
