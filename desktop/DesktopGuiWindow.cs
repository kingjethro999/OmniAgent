using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Photino.NET;

namespace OmniAgent.Desktop;

/// <summary>
/// Native GUI Window host for the OmniAgent Desktop Voice Assistant.
/// Powered by Photino.NET (WebKitGTK on Linux, WebView2 on Windows).
/// </summary>
public static class DesktopGuiWindow
{
    private static PhotinoWindow? _window;
    private static VoiceProfileManager _voiceManager = new();
    private static DesktopSpeechEngine _speech = new();
    private static SystemAutomation _automation = new();
    private static DesktopActionRouter? _router;
    private static CancellationTokenSource? _wakeWordCts;

    public static void Run(string[] args)
    {
        Console.WriteLine("[OmniAgent GUI] Initializing Desktop Voice Assistant Window...");

        string modelPath = Environment.GetEnvironmentVariable("OMNI_LOCAL_MODEL_PATH") ?? "../models/phi-4-mini.gguf";
        IntPtr engineCtx = NativeEngineBridge.InitEngine(modelPath);

        _router = new DesktopActionRouter(_automation, _speech, engineCtx);
        _voiceManager.LoadProfile();

        // Locate wwwroot directory
        string baseDir = AppContext.BaseDirectory;
        string wwwrootDir = Path.Combine(baseDir, "wwwroot");
        if (!Directory.Exists(wwwrootDir))
        {
            string altDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (Directory.Exists(altDir))
            {
                wwwrootDir = altDir;
            }
            else
            {
                string devDir = Path.Combine(Directory.GetCurrentDirectory(), "desktop", "wwwroot");
                if (Directory.Exists(devDir))
                {
                    wwwrootDir = devDir;
                }
            }
        }

        string indexPath = Path.Combine(wwwrootDir, "index.html");
        if (!File.Exists(indexPath))
        {
            Console.WriteLine($"[OmniAgent GUI Error] index.html not found at: {indexPath}");
            return;
        }

        string iconPath = Path.Combine(baseDir, "assets", "app_icon.png");
        if (!File.Exists(iconPath))
        {
            iconPath = Path.Combine(Directory.GetCurrentDirectory(), "desktop", "assets", "app_icon.png");
        }

        _window = new PhotinoWindow()
            .SetTitle("OmniAgent Assistant")
            .SetSize(460, 720)
            .SetMinSize(380, 500)
            .SetResizable(true)
            .SetChromeless(false)
            .SetTopMost(true)
            .Center();

        if (File.Exists(iconPath))
        {
            _window.SetIconFile(iconPath);
        }

        // Register bidirectional IPC message handler
        _window.RegisterWebMessageReceivedHandler(OnWebMessageReceived);

        // Load the Voice Assistant UI
        _window.Load(indexPath);

        // Start background wake-word listener
        StartBackgroundWakeListener();

        // Run message loop (blocking until closed)
        try
        {
            _window.WaitForClose();
        }
        finally
        {
            _wakeWordCts?.Cancel();
            if (engineCtx != IntPtr.Zero)
            {
                NativeEngineBridge.FreeEngine(engineCtx);
            }
        }
    }

    private static async void OnWebMessageReceived(object? sender, string message)
    {
        try
        {
            using var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;
            string type = root.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "";

            switch (type)
            {
                case "command":
                    string query = root.TryGetProperty("query", out var q) ? q.GetString() ?? "" : "";
                    await HandleUserCommandAsync(query);
                    break;

                case "get_status":
                    SendProfileStatus();
                    break;

                case "start_listening":
                    _speech.PlayChime();
                    break;

                case "stop_listening":
                    break;

                case "calibrate_phrase":
                    string phrase = root.TryGetProperty("phrase", out var p) ? p.GetString() ?? "" : "";
                    CalibratePhrase(phrase);
                    SendProfileStatus();
                    break;

                case "save_profile":
                    _voiceManager.SaveProfile();
                    SendProfileStatus();
                    await _speech.SpeakAsync("Voice profile saved. Voice match calibration is now active.");
                    break;

                case "reset_profile":
                    _voiceManager = new VoiceProfileManager();
                    _voiceManager.SaveProfile();
                    SendProfileStatus();
                    break;

                case "minimize":
                    _window?.SetMinimized(true);
                    break;

                case "close":
                    _window?.Close();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OmniAgent GUI IPC Error]: {ex.Message}");
        }
    }

    private static async Task HandleUserCommandAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || _router == null) return;

        Console.WriteLine($"[OmniAgent Assistant GUI] Executing: {query}");

        // 1. Send speech state update to UI
        SendToWebview(new
        {
            type = "speech_transcript",
            text = query
        });

        // 2. Route action through native system automation & SLM
        var result = await _router.RouteAndExecuteAsync(query);

        // 3. Send action result to UI
        SendToWebview(new
        {
            type = "command_result",
            action = result.Type.ToString(),
            feedback = result.SpokenResponse,
            query = query,
            telemetry = result.Type == ActionType.GetTelemetry ? GetStructuredTelemetry() : null
        });

        // 4. Play audio chime and speak response aloud
        _speech.PlayChime();
        
        SendToWebview(new { type = "speech_state", speaking = true });
        await _speech.SpeakAsync(result.SpokenResponse);
        SendToWebview(new { type = "speech_state", speaking = false });
    }

    private static void CalibratePhrase(string phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase)) return;
        string clean = phrase.Trim().ToLowerInvariant();
        if (!_voiceManager.Profile.CalibratedWakeWords.Contains(clean))
        {
            _voiceManager.Profile.CalibratedWakeWords.Add(clean);
        }
        _voiceManager.Profile.IsTrained = true;
        _voiceManager.Profile.CalibratedAt = DateTime.UtcNow;
    }

    private static void SendProfileStatus()
    {
        var profile = _voiceManager.Profile;
        SendToWebview(new
        {
            type = "calibration_status",
            calibrated = profile.IsTrained,
            profileName = profile.ProfileName,
            energyThreshold = 450,
            variants = profile.PhoneticVariants
        });
    }

    private static object GetStructuredTelemetry()
    {
        var (cpu, ram, disk) = SystemAutomation.GetTelemetrySnapshot();
        return new
        {
            cpuPercent = cpu,
            ramPercent = ram,
            diskPercent = disk,
            raw = SystemAutomation.GetQuickSystemTelemetry()
        };
    }

    private static void StartBackgroundWakeListener()
    {
        _wakeWordCts = new CancellationTokenSource();
        var token = _wakeWordCts.Token;

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(3000, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Wake Listener]: {ex.Message}");
                }
            }
        }, token);
    }

    private static void SendToWebview(object data)
    {
        try
        {
            string json = JsonSerializer.Serialize(data);
            _window?.SendWebMessage(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GUI Send Error]: {ex.Message}");
        }
    }
}
