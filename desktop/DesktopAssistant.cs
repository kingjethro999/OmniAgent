using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OmniAgent.Desktop
{
    public class DesktopAssistant
    {
        private readonly DesktopActionRouter _router;
        private readonly DesktopSpeechEngine _speech;
        private readonly VoiceProfileManager _voiceProfile;
        private readonly ConcurrentDictionary<string, Timer> _activeTimers = new();
        private bool _isRunning = false;

        public DesktopAssistant(DesktopActionRouter router, DesktopSpeechEngine speech, VoiceProfileManager voiceProfile)
        {
            _router = router;
            _speech = speech;
            _voiceProfile = voiceProfile;
        }

        public async Task ProcessCommandAsync(string command, bool speakResponse = true)
        {
            string cleanCommand = _voiceProfile.StripWakeWord(command);
            if (string.IsNullOrWhiteSpace(cleanCommand)) cleanCommand = command;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\nYou: \"{command}\"");
            Console.ResetColor();

            var result = await _router.RouteAndExecuteAsync(cleanCommand);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\nOmniAssistant: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(result.SpokenResponse);
            Console.ResetColor();

            if (!string.IsNullOrWhiteSpace(result.ActionSummary) && result.ActionSummary != result.SpokenResponse)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[Action] {result.ActionSummary}");
                Console.ResetColor();
            }

            // Desktop notification
            _speech.ShowNotification("Omni Desktop Assistant", result.SpokenResponse);

            // Handle background timer if requested
            if (result.Type == ActionType.SetTimer && result.TimerSeconds > 0)
            {
                StartTimer(result.TimerSeconds, result.TimerLabel);
            }

            // Speak response via TTS
            if (speakResponse)
            {
                await _speech.SpeakAsync(result.SpokenResponse);
            }
        }

        private void StartTimer(int totalSeconds, string label)
        {
            string timerId = Guid.NewGuid().ToString();
            var timer = new Timer(_ =>
            {
                _speech.PlayChime();
                string alertMsg = $"Your timer for {label} has completed!";
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\n[ALARM] {alertMsg}");
                Console.ResetColor();

                _speech.ShowNotification("Omni Timer Alarm", alertMsg);
                _speech.SpeakAsync(alertMsg).Wait();

                if (_activeTimers.TryRemove(timerId, out var t))
                {
                    t.Dispose();
                }
            }, null, TimeSpan.FromSeconds(totalSeconds), Timeout.InfiniteTimeSpan);

            _activeTimers[timerId] = timer;
        }

        public async Task RunInteractiveHudAsync()
        {
            _isRunning = true;
            PrintAssistantHud();

            while (_isRunning)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("OmniAssistant [Voice / Input] > ");
                Console.ResetColor();

                string? input = Console.ReadLine();
                if (input == null) break;

                string trimmed = input.Trim();
                if (string.Equals(trimmed, "exit", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(trimmed, "quit", StringComparison.OrdinalIgnoreCase))
                {
                    await _speech.SpeakAsync("Goodbye!");
                    break;
                }

                if (string.Equals(trimmed, "calibrate", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(trimmed, "train", StringComparison.OrdinalIgnoreCase))
                {
                    _voiceProfile.RunCalibrationWizard();
                    PrintAssistantHud();
                    continue;
                }

                if (string.Equals(trimmed, "help", StringComparison.OrdinalIgnoreCase))
                {
                    PrintCapabilities();
                    continue;
                }

                await ProcessCommandAsync(trimmed, speakResponse: true);
            }
        }

        public async Task RunListeningLoopAsync()
        {
            _isRunning = true;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[OmniAssistant] Hands-Free Wake Word Mode Active.");
            Console.WriteLine($"Listening for calibrated wake words: {string.Join(", ", _voiceProfile.Profile.CalibratedWakeWords)}");
            Console.WriteLine("Accent Matching: " + (_voiceProfile.Profile.IsTrained ? "Personalized Calibrated Profile" : "Adaptive Auto-Detect"));
            Console.WriteLine("(Type or speak natural commands. Type 'exit' to quit.)\n");
            Console.ResetColor();

            while (_isRunning)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[Listening...] > ");
                Console.ResetColor();

                string? line = Console.ReadLine();
                if (line == null) break;

                string query = line.Trim();
                if (string.Equals(query, "exit", StringComparison.OrdinalIgnoreCase)) break;

                // Check wake word or accent variant
                if (_voiceProfile.MatchesWakeWord(query))
                {
                    _speech.PlayChime();
                    await ProcessCommandAsync(query, speakResponse: true);
                }
                else
                {
                    // If entered directly in console, process as direct command
                    await ProcessCommandAsync(query, speakResponse: true);
                }
            }
        }

        private void PrintAssistantHud()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║            OmniAgent Desktop Home & System Assistant (v0.2.1)            ║");
            Console.WriteLine("║                 Zero-Cloud Siri for Windows & Linux                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            string trainingStatus = _voiceProfile.Profile.IsTrained
                ? "[CALIBRATED] Voice Match Active"
                : "[DEFAULT] Run 'calibrate' to train your voice & accent";

            Console.WriteLine($"Voice Match: {trainingStatus}");
            Console.WriteLine("Speech (TTS): Active (Native System Audio & Desktop Notifications)");
            Console.WriteLine("Privacy:      100% On-Device (Zero telemetry leaves workstation)\n");
            Console.WriteLine("Commands to try:");
            Console.WriteLine("  • \"Hey Omni, play the box by roddy rich on spotify\"");
            Console.WriteLine("  • \"Hey Omni, open chrome\" / \"open vscode\" / \"open terminal\"");
            Console.WriteLine("  • \"Hey Omni, set a timer for 10 seconds\"");
            Console.WriteLine("  • \"Hey Omni, what is the weather in Tokyo?\"");
            Console.WriteLine("  • \"Hey Omni, volume up\" / \"volume down\" / \"mute\"");
            Console.WriteLine("  • \"Hey Omni, take a screenshot\" / \"lock screen\"");
            Console.WriteLine("  • \"Hey Omni, how is my system doing?\"");
            Console.WriteLine("  • \"calibrate\" (to run voice & accent calibration wizard)\n");
        }

        private void PrintCapabilities()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n── Supported Desktop Automations ────────────────────────");
            Console.WriteLine("  Spotify:      play <song>, pause, resume, next song, previous track");
            Console.WriteLine("  App Launch:   open <chrome|vscode|terminal|calc|spotify|files|steam>");
            Console.WriteLine("  Controls:     volume up/down/mute/unmute/50%, lock screen, screenshot");
            Console.WriteLine("  Clock/Timer:  set timer for X secs/mins, what time is it, today's date");
            Console.WriteLine("  Info/Web:     weather in <city>, system telemetry, search google/youtube");
            Console.WriteLine("  Audit/Files:  audit my files, organize my folder, format csv");
            Console.WriteLine("  Voice Match:  type 'calibrate' to train accent and acoustic profile");
            Console.WriteLine("─────────────────────────────────────────────────────────\n");
            Console.ResetColor();
        }
    }
}
