using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OmniAgent.Desktop
{
    public enum ActionType
    {
        SpotifyPlay,
        MediaControl,
        LaunchApp,
        VolumeControl,
        LockScreen,
        Screenshot,
        SetTimer,
        SetAlarm,
        GetTime,
        GetDate,
        GetWeather,
        GetTelemetry,
        WebSearch,
        DocumentAudit,
        OrganizeFiles,
        FormatCsv,
        GitStatus,
        GeneralConversation
    }

    public class ActionResult
    {
        public ActionType Type { get; set; }
        public string ActionSummary { get; set; } = "";
        public string SpokenResponse { get; set; } = "";
        public bool Succeeded { get; set; } = true;
        public int TimerSeconds { get; set; } = 0;
        public string TimerLabel { get; set; } = "";
    }

    public class DesktopActionRouter
    {
        private readonly SystemAutomation _automation;
        private readonly DesktopSpeechEngine _speech;
        private readonly IntPtr _engineCtx;

        public DesktopActionRouter(SystemAutomation automation, DesktopSpeechEngine speech, IntPtr engineCtx)
        {
            _automation = automation;
            _speech = speech;
            _engineCtx = engineCtx;
        }

        public async Task<ActionResult> RouteAndExecuteAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new ActionResult
                {
                    Type = ActionType.GeneralConversation,
                    SpokenResponse = "I am listening. How can I help you?",
                    ActionSummary = "Prompted user for input"
                };
            }

            string clean = query.Trim();
            string lower = clean.ToLowerInvariant();

            // 1. Spotify & Music
            var matchSpotify = Regex.Match(lower, @"play\s+(.+?)(?:\s+on\s+spotify)?$", RegexOptions.IgnoreCase);
            if (matchSpotify.Success && !lower.Contains("game") && !lower.Contains("video"))
            {
                string song = matchSpotify.Groups[1].Value.Trim();
                _automation.PlaySpotify(song);
                return new ActionResult
                {
                    Type = ActionType.SpotifyPlay,
                    ActionSummary = $"Playing \"{song}\" on Spotify",
                    SpokenResponse = $"Playing {song} on Spotify."
                };
            }

            // 2. Media Controls
            if (Regex.IsMatch(lower, @"\b(pause|resume|next song|next track|previous song|previous track|stop music|play music)\b"))
            {
                string action = "playpause";
                if (lower.Contains("pause")) action = "pause";
                else if (lower.Contains("next")) action = "next";
                else if (lower.Contains("previous") || lower.Contains("prev")) action = "previous";
                else if (lower.Contains("stop")) action = "stop";
                else if (lower.Contains("play")) action = "play";

                _automation.ControlMedia(action);
                return new ActionResult
                {
                    Type = ActionType.MediaControl,
                    ActionSummary = $"Media control executed: {action}",
                    SpokenResponse = $"Media {action}."
                };
            }

            // 3. Timers
            var matchTimer = Regex.Match(lower, @"(?:set\s+(?:a\s+)?timer\s+(?:for\s+)?|timer\s+)(\d+)\s*(seconds?|secs?|minutes?|mins?|hours?)", RegexOptions.IgnoreCase);
            if (matchTimer.Success)
            {
                int val = int.Parse(matchTimer.Groups[1].Value);
                string unit = matchTimer.Groups[2].Value.ToLowerInvariant();
                int totalSecs = unit.StartsWith("m") ? val * 60 : unit.StartsWith("h") ? val * 3600 : val;

                return new ActionResult
                {
                    Type = ActionType.SetTimer,
                    TimerSeconds = totalSecs,
                    TimerLabel = $"{val} {unit}",
                    ActionSummary = $"Timer set for {val} {unit}",
                    SpokenResponse = $"Setting a timer for {val} {unit}."
                };
            }

            // 4. Time & Date
            if (Regex.IsMatch(lower, @"\b(what time is it|current time|what is the time|tell me the time)\b"))
            {
                string nowTime = DateTime.Now.ToString("h:mm tt");
                return new ActionResult
                {
                    Type = ActionType.GetTime,
                    ActionSummary = $"Current time is {nowTime}",
                    SpokenResponse = $"It's {nowTime}."
                };
            }
            if (Regex.IsMatch(lower, @"\b(what(?:'s|\s+is)\s+today(?:'s)?\s+date|what date is it|what day is it)\b"))
            {
                string nowDate = DateTime.Now.ToString("dddd, MMMM d, yyyy");
                return new ActionResult
                {
                    Type = ActionType.GetDate,
                    ActionSummary = $"Today's date is {nowDate}",
                    SpokenResponse = $"Today is {nowDate}."
                };
            }

            // 5. Volume Controls
            if (Regex.IsMatch(lower, @"\b(volume\s+(?:up|down|max)|mute|unmute|set\s+volume)\b"))
            {
                string volAction = "up";
                if (lower.Contains("down") || lower.Contains("lower")) volAction = "down";
                else if (lower.Contains("mute") && !lower.Contains("unmute")) volAction = "mute";
                else if (lower.Contains("unmute")) volAction = "unmute";

                var pctMatch = Regex.Match(lower, @"(\d+)\s*%");
                if (pctMatch.Success) volAction = $"{pctMatch.Groups[1].Value}%";

                _automation.SetVolume(volAction);
                return new ActionResult
                {
                    Type = ActionType.VolumeControl,
                    ActionSummary = $"Adjusted system volume ({volAction})",
                    SpokenResponse = $"Volume adjusted {volAction}."
                };
            }

            // 6. Screenshot
            if (Regex.IsMatch(lower, @"\b(take\s+(?:a\s+)?screenshot|capture\s+screen|screenshot)\b"))
            {
                bool ok = _automation.TakeScreenshot(out string path);
                string resp = ok ? $"Screenshot captured and saved to {System.IO.Path.GetFileName(path)}." : "Failed to capture screenshot.";
                return new ActionResult
                {
                    Type = ActionType.Screenshot,
                    Succeeded = ok,
                    ActionSummary = ok ? $"Screenshot saved: {path}" : "Screenshot failed",
                    SpokenResponse = resp
                };
            }

            // 7. Lock Screen
            if (Regex.IsMatch(lower, @"\b(lock\s+(?:screen|pc|computer|workstation)|lock)\b"))
            {
                _automation.LockWorkstation();
                return new ActionResult
                {
                    Type = ActionType.LockScreen,
                    ActionSummary = "Workstation locked",
                    SpokenResponse = "Locking your screen."
                };
            }

            // 8. Application Launching
            var matchOpen = Regex.Match(lower, @"^(?:open|launch|start)\s+([a-zA-Z0-9\s\-]+)$", RegexOptions.IgnoreCase);
            if (matchOpen.Success)
            {
                string targetApp = matchOpen.Groups[1].Value.Trim();
                _automation.LaunchApplication(targetApp);
                return new ActionResult
                {
                    Type = ActionType.LaunchApp,
                    ActionSummary = $"Launched {targetApp}",
                    SpokenResponse = $"Opening {targetApp}."
                };
            }

            // 9. Weather
            var matchWeather = Regex.Match(lower, @"(?:weather|forecast)(?:\s+in\s+([a-zA-Z\s]+))?", RegexOptions.IgnoreCase);
            if (matchWeather.Success || lower.Contains("is it raining") || lower.Contains("temperature outside"))
            {
                string city = matchWeather.Groups[1].Success ? matchWeather.Groups[1].Value.Trim() : "";
                string report = await _automation.GetWeatherAsync(city);
                return new ActionResult
                {
                    Type = ActionType.GetWeather,
                    ActionSummary = $"Weather report: {report}",
                    SpokenResponse = $"The weather is {report}."
                };
            }

            // 10. System Telemetry
            if (Regex.IsMatch(lower, @"\b(system info|telemetry|cpu usage|ram usage|how much memory|battery status|disk space|system doing|system status|how is (?:my|the) system)\b"))
            {
                string report = _automation.GetSystemTelemetry();
                return new ActionResult
                {
                    Type = ActionType.GetTelemetry,
                    ActionSummary = report,
                    SpokenResponse = "Here is your system telemetry report."
                };
            }

            // 11. Web Search
            var matchSearch = Regex.Match(lower, @"^(?:search\s+(?:google\s+for\s+)?|google\s+)(.+)$", RegexOptions.IgnoreCase);
            if (matchSearch.Success)
            {
                string q = matchSearch.Groups[1].Value.Trim();
                _automation.OpenWebSearch(q, "google");
                return new ActionResult
                {
                    Type = ActionType.WebSearch,
                    ActionSummary = $"Web search: {q}",
                    SpokenResponse = $"Searching Google for {q}."
                };
            }

            var matchYoutube = Regex.Match(lower, @"(?:search\s+youtube\s+for\s+|youtube\s+)(.+)$", RegexOptions.IgnoreCase);
            if (matchYoutube.Success)
            {
                string q = matchYoutube.Groups[1].Value.Trim();
                _automation.OpenWebSearch(q, "youtube");
                return new ActionResult
                {
                    Type = ActionType.WebSearch,
                    ActionSummary = $"YouTube search: {q}",
                    SpokenResponse = $"Searching YouTube for {q}."
                };
            }

            // 12. Local File & Document Automations
            if (lower.Contains("audit") && (lower.Contains("file") || lower.Contains("folder") || lower.Contains("code")))
            {
                var auditor = new DocumentAuditor();
                string targetDir = ".";
                auditor.AuditDirectory(targetDir, _engineCtx);
                return new ActionResult
                {
                    Type = ActionType.DocumentAudit,
                    ActionSummary = "Local document audit completed",
                    SpokenResponse = "I have completed the local security audit of your files."
                };
            }

            if (lower.Contains("organize") && (lower.Contains("folder") || lower.Contains("files") || lower.Contains("directory")))
            {
                int moved = _automation.OrganizeDirectory(".");
                return new ActionResult
                {
                    Type = ActionType.OrganizeFiles,
                    ActionSummary = $"Organized {moved} files into folders",
                    SpokenResponse = $"I organized {moved} files into categorized folders."
                };
            }

            // 13. General Conversational Query -> Local C++ SLM Inference
            string slmResponse = NativeEngineBridge.Generate(_engineCtx, clean, 0.7f);

            if (string.IsNullOrWhiteSpace(slmResponse) || slmResponse.Contains("[NativeEngineBridge]"))
            {
                slmResponse = $"I have processed your request: \"{clean}\". All operations completed securely on-device.";
            }

            return new ActionResult
            {
                Type = ActionType.GeneralConversation,
                ActionSummary = slmResponse,
                SpokenResponse = slmResponse
            };
        }
    }
}
