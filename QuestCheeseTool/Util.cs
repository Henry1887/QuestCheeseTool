using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestCheeseTool
{
    internal class Util
    {
        internal static (int ExitCode, List<string> Output) RunADBCommand(List<string> arguments)
        {
            var outputLines = new List<string>();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Config.ADBExePath,
                    Arguments = string.Join(" ", arguments),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputLines.Add(e.Data);
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputLines.Add(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return (process.ExitCode, outputLines);
        }

        internal static void DownloadAndExtractPlatformTools()
        {
            if (File.Exists(Config.ADBExePath))
            {
                Console.WriteLine("Platform tools are already downloaded.");
                return;
            }
            Console.WriteLine("Downloading platform tools...");
            using (var httpClient = new HttpClient())
            {
                var data = httpClient.GetByteArrayAsync(Config.PlatformToolsLink).GetAwaiter().GetResult();
                File.WriteAllBytes("platform-tools.zip", data);
            }
            Console.WriteLine("Extracting platform tools...");
            System.IO.Compression.ZipFile.ExtractToDirectory("platform-tools.zip", "platform-toolsS", true);
            Directory.Move("platform-toolsS/platform-tools", "platform-tools");
            Console.WriteLine("Platform tools downloaded and extracted successfully.");
            File.Delete("platform-tools.zip");
            Directory.Delete("platform-toolsS", true);
        }

        internal static void DownloadAndExtractExploit()
        {
            if (Directory.Exists(Config.ExploitPath))
            {
                Console.WriteLine("Exploit is already downloaded.");
                return;
            }
            Console.WriteLine("Downloading exploit...");
            using (var httpClient = new HttpClient())
            {
                var data = httpClient.GetByteArrayAsync(Config.ExploitLink).GetAwaiter().GetResult();
                File.WriteAllBytes("exploit.zip", data);
            }
            Console.WriteLine("Extracting exploit...");
            System.IO.Compression.ZipFile.ExtractToDirectory("exploit.zip", "exploitT", true);
            Directory.Move("exploitT/exploit", Config.ExploitPath);
            Console.WriteLine("Exploit downloaded and extracted successfully.");
            File.Delete("exploit.zip");
            Directory.Delete("exploitT", true);
        }

        internal static bool IsExploitInstalled(AdbDevice device)
        {
            var output = device.RunCommand(new List<string> { "shell", "\"[ -d /data/local/tmp/exploit ] && echo 'Exists' || echo 'Does not exist'\"" });
            if (output.ExitCode == 0 && output.Output[0].Equals("Exists"))
            {
                return true;
            }
            return false;
        }

        internal static bool VerifyRoot(AdbDevice device)
        {
            Console.WriteLine("Verifying root access... Check the headset for a prompt to allow root if this takes too long.");
            var output = device.RunCommand(new List<string> { "shell", "su -c 'id'" });
            if (output.ExitCode == 0 && output.Output.Count > 0 && output.Output[0].Contains("uid=0"))
            {
                return true;
            }
            return false;
        }

        internal static bool UninstallExploit(AdbDevice device)
        {
            Console.WriteLine("Uninstalling exploit...");
            var output = device.RunCommand(new List<string> { "shell", "rm -rf /data/local/tmp/exploit" });
            if (output.ExitCode == 0)
            {
                Console.WriteLine("Exploit uninstalled successfully.");
                return true;
            }
            else
            {
                Console.WriteLine("Failed to uninstall exploit:");
                foreach (var line in output.Output)
                {
                    Console.WriteLine(line);
                }
                return false;
            }
        }

        internal static bool InstallExploit(AdbDevice device)
        {
            Console.WriteLine("Installing exploit...");
            var output = device.RunCommand(new List<string> { "push", $"{Config.ExploitPath}", "/data/local/tmp/exploit" });
            if (output.ExitCode != 0)
            {
                Console.WriteLine("Failed to push exploit:");
                foreach (var line in output.Output)
                {
                    Console.WriteLine(line);
                }
                return false;
            }
            output = device.RunCommand(new List<string> { "shell", "chmod +x /data/local/tmp/exploit/exploit" });
            if (output.ExitCode != 0)
            {
                Console.WriteLine("Failed to set execute permissions on exploit:");
                foreach (var line in output.Output)
                {
                    Console.WriteLine(line);
                }
            }
            Console.WriteLine("Exploit installed successfully.");
            return true;
        }

        internal static bool RunExploit(AdbDevice device)
        {
            Console.WriteLine("Running exploit...");
            var output = device.RunCommand(new List<string> { "shell", "/data/local/tmp/exploit/exploit", "--headless" });
            if (output.ExitCode == 0)
            {
                var liveSetupOutput = device.RunCommand(new List<string> { "shell", "su", "-c", "\"/data/local/tmp/live_setup.sh\"" });
                Console.WriteLine("Exploit executed successfully.");
                return true;
            }
            else
            {
                Console.WriteLine("Try rebooting the headset.");
                Console.WriteLine("Failed to run exploit:");
                foreach (var line in output.Output)
                {
                    Console.WriteLine(line);
                }
                return false;
            }
        }

        internal static bool RebootDevice(AdbDevice device)
        {
            Console.WriteLine("Rebooting device...");
            var output = device.RunCommand(new List<string> { "reboot" });
            if (output.ExitCode == 0)
            {
                Console.WriteLine("Device is rebooting...");
                return true;
            }
            else
            {
                Console.WriteLine("Failed to reboot device:");
                foreach (var line in output.Output)
                {
                    Console.WriteLine(line);
                }
                return false;
            }
        }
    }
}
