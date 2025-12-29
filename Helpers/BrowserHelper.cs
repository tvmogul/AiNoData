using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AiNoData.Helpers
{
    public static class BrowserHelper
    {
        public static void OpenUrlInChromeOrDefault(string url)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string chromePath = GetChromePathWindows();
                    if (!string.IsNullOrEmpty(chromePath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = chromePath,
                            Arguments = url,
                            UseShellExecute = true
                        });
                        return;
                    }

                    // Open with default browser on Windows
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Try opening Chrome on Mac
                    var chromeMac = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
                    if (System.IO.File.Exists(chromeMac))
                    {
                        Process.Start(chromeMac, url);
                    }
                    else
                    {
                        // Fallback to default browser on Mac
                        Process.Start("open", url);
                    }
                }
                else
                {
                    // Fallback for Linux or unsupported OS
                    Process.Start("xdg-open", url);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening URL: {ex.Message}");
            }
        }

        private static string GetChromePathWindows()
        {
            string[] registryKeys =
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe",
                @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe"
            };

            foreach (string key in registryKeys)
            {
                using var regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key);
                if (regKey?.GetValue(null) is string path)
                    return path;
            }

            return string.Empty;
        }
    }
}
