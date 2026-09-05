using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OmniAgent.Desktop
{
    public class SystemAutomation
    {
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

        // ══════════════════════════════════════════════════════
        // 1. Media & Spotify Controls
        // ══════════════════════════════════════════════════════

        public bool PlaySpotify(string searchQuery)
        {
            try
            {
                string encoded = Uri.EscapeDataString(searchQuery);
                string uri = $"spotify:search:{encoded}";

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Try launching spotify URI directly
                    RunProcess("xdg-open", uri);
                    return true;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    RunProcess("cmd.exe", $"/c start {uri}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Automation] Failed to open Spotify: {ex.Message}");
            }
            return false;
        }

        public bool ControlMedia(string action)
        {
            // action: play, pause, playpause, next, previous, stop
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string mprisMethod = action.ToLowerInvariant() switch
                    {
                        "play" => "Play",
                        "pause" => "Pause",
                        "next" => "Next",
                        "previous" => "Previous",
                        "stop" => "Stop",
                        _ => "PlayPause"
                    };

                    // Try playerctl first if installed
                    int exit = RunProcess("playerctl", action.ToLowerInvariant());
                    if (exit == 0) return true;

                    // Fallback to direct D-Bus MPRIS call
                    string dbusArgs = $"--type=method_call --dest=org.mpris.MediaPlayer2.spotify /org/mpris/MediaPlayer2 org.mpris.MediaPlayer2.Player.{mprisMethod}";
                    RunProcess("dbus-send", dbusArgs);
                    return true;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Media keys simulation via PowerShell
                    // VK_MEDIA_NEXT_TRACK (0xB0), VK_MEDIA_PREV_TRACK (0xB1), VK_MEDIA_STOP (0xB2), VK_MEDIA_PLAY_PAUSE (0xB3)
                    byte vk = action.ToLowerInvariant() switch
                    {
                        "next" => 0xB0,
                        "previous" => 0xB1,
                        "stop" => 0xB2,
                        _ => 0xB3 // play/pause
                    };

                    string ps = $@"Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; public class WinMedia {{ [DllImport(""user32.dll"")] public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo); }}'; [WinMedia]::keybd_event({vk}, 0, 1, [UIntPtr]::Zero); [WinMedia]::keybd_event({vk}, 0, 2, [UIntPtr]::Zero);";
                    RunProcess("powershell.exe", $"-NoProfile -Command \"{ps}\"");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Automation] Media control failed: {ex.Message}");
            }
            return false;
        }

        // ══════════════════════════════════════════════════════
        // 2. Application Launching
        // ══════════════════════════════════════════════════════

        public bool LaunchApplication(string appName)
        {
            string app = appName.Trim().ToLowerInvariant();
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string cmd = app switch
                    {
                        "chrome" or "google chrome" => "google-chrome",
                        "firefox" => "firefox",
                        "terminal" or "bash" => "x-terminal-emulator",
                        "vscode" or "code" => "code",
                        "calculator" or "calc" => "gnome-calculator",
                        "files" or "file manager" or "explorer" => "nautilus",
                        "spotify" => "spotify",
                        "settings" => "gnome-control-center",
                        "text editor" or "editor" => "gedit",
                        "steam" => "steam",
                        "discord" => "discord",
                        "slack" => "slack",
                        _ => app
                    };

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = cmd,
                        UseShellExecute = true
                    });
                    return true;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string target = app switch
                    {
                        "calculator" or "calc" => "calc.exe",
                        "notepad" or "editor" => "notepad.exe",
                        "files" or "explorer" or "file manager" => "explorer.exe",
                        "terminal" or "cmd" => "cmd.exe",
                        "powershell" => "powershell.exe",
                        "chrome" or "google chrome" => "chrome.exe",
                        "code" or "vscode" => "code.cmd",
                        "spotify" => "spotify.exe",
                        "settings" => "ms-settings:",
                        _ => app
                    };

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = target,
                        UseShellExecute = true
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Automation] Could not launch app '{appName}': {ex.Message}");
            }
            return false;
        }

        // ══════════════════════════════════════════════════════
        // 3. System Controls (Volume, Brightness, Screen Lock)
        // ══════════════════════════════════════════════════════

        public bool SetVolume(string action)
        {
            // action: up, down, mute, unmute, or percentage e.g. "50%"
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string args = action.ToLowerInvariant() switch
                    {
                        "up" => "set-sink-volume @DEFAULT_SINK@ +10%",
                        "down" => "set-sink-volume @DEFAULT_SINK@ -10%",
                        "mute" => "set-sink-mute @DEFAULT_SINK@ 1",
                        "unmute" => "set-sink-mute @DEFAULT_SINK@ 0",
                        _ when action.Contains("%") => $"set-sink-volume @DEFAULT_SINK@ {action}",
                        _ => "set-sink-volume @DEFAULT_SINK@ +10%"
                    };
                    return RunProcess("pactl", args) == 0;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Simulate Volume Up (0xAF) or Volume Down (0xAE) or Mute (0xAD)
                    byte vk = action.ToLowerInvariant() switch
                    {
                        "down" => 0xAE,
                        "mute" or "unmute" => 0xAD,
                        _ => 0xAF // up
                    };

                    string ps = $@"Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; public class WinVol {{ [DllImport(""user32.dll"")] public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo); }}'; [WinVol]::keybd_event({vk}, 0, 1, [UIntPtr]::Zero); [WinVol]::keybd_event({vk}, 0, 2, [UIntPtr]::Zero);";
                    RunProcess("powershell.exe", $"-NoProfile -Command \"{ps}\"");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Automation] Volume adjustment failed: {ex.Message}");
            }
            return false;
        }

        public bool LockWorkstation()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (RunProcess("loginctl", "lock-session") == 0) return true;
                    if (RunProcess("xdg-screensaver", "lock") == 0) return true;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    RunProcess("rundll32.exe", "user32.dll,LockWorkStation");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Automation] Lock screen failed: {ex.Message}");
            }
            return false;
        }

        public bool TakeScreenshot(out string savedPath)
        {
            savedPath = "";
            try
            {
                string picsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");
                Directory.CreateDirectory(picsDir);
                string filename = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                savedPath = Path.Combine(picsDir, filename);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Try gnome-screenshot, scrot, or import
                    if (RunProcess("gnome-screenshot", $"-f \"{savedPath}\"") == 0) return true;
                    if (RunProcess("scrot", $"\"{savedPath}\"") == 0) return true;
                    if (RunProcess("import", $"-window root \"{savedPath}\"") == 0) return true;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string ps = $@"[reflection.assembly]::loadwithpartialname('System.Windows.Forms') | Out-Null;
[reflection.assembly]::loadwithpartialname('System.Drawing') | Out-Null;
$bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds;
$bmp = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height;
$graphics = [System.Drawing.Graphics]::FromImage($bmp);
$graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size);
$bmp.Save('{savedPath.Replace("\\", "\\\\")}', [System.Drawing.Imaging.ImageFormat]::Png);
$graphics.Dispose();
$bmp.Dispose();";

                    RunProcess("powershell.exe", $"-NoProfile -Command \"{ps}\"");
                    return File.Exists(savedPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Automation] Screenshot failed: {ex.Message}");
            }
            return false;
        }

        // ══════════════════════════════════════════════════════
        // 4. System Telemetry & Weather
        // ══════════════════════════════════════════════════════

        public string GetSystemTelemetry()
        {
            var sb = new StringBuilder();
            sb.AppendLine("System Telemetry Report:");
            sb.AppendLine($"OS: {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})");
            sb.AppendLine($"Machine Name: {Environment.MachineName}");
            sb.AppendLine($"Logical Cores: {Environment.ProcessorCount}");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    if (File.Exists("/proc/meminfo"))
                    {
                        var memLines = File.ReadAllLines("/proc/meminfo");
                        foreach (var l in memLines)
                        {
                            if (l.StartsWith("MemTotal:") || l.StartsWith("MemAvailable:"))
                            {
                                sb.AppendLine(l.Trim());
                            }
                        }
                    }
                    if (File.Exists("/proc/uptime"))
                    {
                        string uptime = File.ReadAllText("/proc/uptime").Split(' ')[0];
                        if (double.TryParse(uptime, out double secs))
                        {
                            var ts = TimeSpan.FromSeconds(secs);
                            sb.AppendLine($"Uptime: {ts.Days}d {ts.Hours}h {ts.Minutes}m");
                        }
                    }
                }
                catch { }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    long memBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
                    sb.AppendLine($"Available Physical Memory: {memBytes / (1024 * 1024 * 1024)} GB");
                }
                catch { }
            }

            // Disk Drives
            try
            {
                var drives = DriveInfo.GetDrives();
                foreach (var d in drives)
                {
                    if (d.IsReady && d.TotalSize > 1024L * 1024 * 1024 && !d.Name.StartsWith("/snap") && !d.Name.StartsWith("/dev") && !d.Name.StartsWith("/sys") && !d.Name.StartsWith("/proc") && !d.Name.StartsWith("/run"))
                    {
                        long freeGb = d.AvailableFreeSpace / (1024 * 1024 * 1024);
                        long totalGb = d.TotalSize / (1024 * 1024 * 1024);
                        sb.AppendLine($"Drive {d.Name}: {freeGb} GB free of {totalGb} GB");
                    }
                }
            }
            catch { }

            return sb.ToString();
        }

        public async Task<string> GetWeatherAsync(string city)
        {
            try
            {
                string target = string.IsNullOrWhiteSpace(city) ? "" : Uri.EscapeDataString(city);
                string url = $"https://wttr.in/{target}?format=%l:+%c+%t+(%C)&m";
                var response = await _httpClient.GetStringAsync(url);
                return response.Trim();
            }
            catch (Exception ex)
            {
                return $"Weather information currently unavailable: {ex.Message}";
            }
        }

        public void OpenWebSearch(string query, string engine = "google")
        {
            string url = engine.ToLowerInvariant() switch
            {
                "youtube" => $"https://www.youtube.com/results?search_query={Uri.EscapeDataString(query)}",
                "wikipedia" => $"https://en.wikipedia.org/wiki/Special:Search?search={Uri.EscapeDataString(query)}",
                _ => $"https://www.google.com/search?q={Uri.EscapeDataString(query)}"
            };

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    RunProcess("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    RunProcess("cmd.exe", $"/c start {url}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Automation] Failed to open search: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════
        // 5. Enterprise File Operations
        // ══════════════════════════════════════════════════════

        public bool FormatCsv(string inputPath, string? outputPath = null)
        {
            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"[Automation] File not found: {inputPath}");
                return false;
            }

            outputPath ??= inputPath;
            try
            {
                var lines = File.ReadAllLines(inputPath);
                var formatted = new StringBuilder();

                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        parts[i] = parts[i].Trim();
                    }
                    formatted.AppendLine(string.Join(",", parts));
                }

                File.WriteAllText(outputPath, formatted.ToString());
                Console.WriteLine($"[Automation] Formatted CSV ({lines.Length} rows) -> {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Automation] Error formatting CSV: {ex.Message}");
                return false;
            }
        }

        public int OrganizeDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"[Automation] Directory not found: {directoryPath}");
                return 0;
            }

            int moved = 0;
            var files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                string targetFolder = ext switch
                {
                    ".cs" or ".py" or ".cpp" or ".h" or ".js" or ".ts" or ".java" => "Code",
                    ".pdf" or ".docx" or ".txt" or ".md" or ".rtf" => "Documents",
                    ".csv" or ".json" or ".xml" or ".sql" => "Data",
                    ".png" or ".jpg" or ".jpeg" or ".svg" or ".gif" => "Media",
                    _ => "Other"
                };

                string destDir = Path.Combine(directoryPath, targetFolder);
                Directory.CreateDirectory(destDir);
                string destFile = Path.Combine(destDir, Path.GetFileName(file));

                if (!File.Exists(destFile))
                {
                    File.Move(file, destFile);
                    moved++;
                }
            }

            Console.WriteLine($"[Automation] Organized {moved} files into categorized folders.");
            return moved;
        }

        public string RunGitStatus(string repoPath)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "status --short",
                    WorkingDirectory = repoPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                    return string.IsNullOrWhiteSpace(output) ? "Working tree clean." : output;
                }
            }
            catch (Exception ex)
            {
                return $"Git execution failed: {ex.Message}";
            }
            return "Unable to start git process.";
        }

        private static int RunProcess(string filename, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(5000);
                return proc?.ExitCode ?? -1;
            }
            catch
            {
                return -1;
            }
        }
    }
}
