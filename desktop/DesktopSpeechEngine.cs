using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OmniAgent.Desktop
{
    public class DesktopSpeechEngine
    {
        public bool IsMuted { get; set; } = false;

        public async Task SpeakAsync(string text)
        {
            if (IsMuted || string.IsNullOrWhiteSpace(text)) return;

            // Strip markdown, asterisks, brackets, or code blocks for clean speech
            string cleanText = CleanForSpeech(text);

            await Task.Run(() =>
            {
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        SpeakLinux(cleanText);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        SpeakWindows(cleanText);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SpeechEngine] Speech error: {ex.Message}");
                }
            });
        }

        private void SpeakLinux(string text)
        {
            try
            {
                // Try spd-say (Speech Dispatcher) first
                var psi = new ProcessStartInfo
                {
                    FileName = "spd-say",
                    Arguments = $"-r -5 -p 0 \"{EscapeShellArg(text)}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(4000);
            }
            catch
            {
                // Fallback to espeak if installed
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "espeak",
                        Arguments = $"\"{EscapeShellArg(text)}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using var proc = Process.Start(psi);
                    proc?.WaitForExit(4000);
                }
                catch
                {
                    // Silent audio fallback
                }
            }
        }

        private void SpeakWindows(string text)
        {
            try
            {
                // PowerShell invocation of SAPI SpVoice for zero-dependency Windows TTS
                string psCommand = $"Add-Type -AssemblyName System.Speech; $synth = New-Object System.Speech.Synthesis.SpeechSynthesizer; $synth.Rate = 0; $synth.Speak('{text.Replace("'", "''")}');";
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -NonInteractive -Command \"{psCommand}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(6000);
            }
            catch
            {
                // Silent fallback
            }
        }

        public void ShowNotification(string title, string message)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "notify-send",
                        Arguments = $"\"{EscapeShellArg(title)}\" \"{EscapeShellArg(message)}\" --app-name=\"OmniAgent\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process.Start(psi);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string psCommand = $@"[reflection.assembly]::loadwithpartialname('System.Windows.Forms') | Out-Null;
$notify = new-object system.windows.forms.notifyicon;
$notify.icon = [system.drawing.systemicons]::Information;
$notify.visible = $true;
$notify.showballoontip(8, '{title.Replace("'", "''")}', '{message.Replace("'", "''")}', [system.windows.forms.tooltipicon]::Info);";

                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -NonInteractive -Command \"{psCommand}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process.Start(psi);
                }
            }
            catch
            {
                // Silent fallback
            }
        }

        public void PlayChime()
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    Console.Beep(880, 150);
                    Console.Beep(1174, 200);
                }
                else
                {
                    Console.Write("\a");
                }
            }
            catch
            {
                // Silent
            }
        }

        private static string CleanForSpeech(string text)
        {
            // Remove code blocks
            text = Regex.Replace(text, @"```[\s\S]*?```", "code block omitted.");
            // Remove inline code
            text = Regex.Replace(text, @"`([^`]+)`", "$1");
            // Remove bold/italics
            text = Regex.Replace(text, @"[\*_]{1,3}([^\*_]+)[\*_]{1,3}", "$1");
            // Remove URLs
            text = Regex.Replace(text, @"https?:\/\/\S+", "link");
            // Remove markdown links
            text = Regex.Replace(text, @"\[([^\]]+)\]\([^\)]+\)", "$1");
            // Remove emojis & special symbols
            text = Regex.Replace(text, @"[^\u0000-\u007F]+", " ");
            return text.Trim();
        }

        private static string EscapeShellArg(string arg)
        {
            return arg.Replace("\"", "\\\"").Replace("$", "\\$").Replace("`", "\\`");
        }
    }
}
