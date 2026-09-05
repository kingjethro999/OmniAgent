package io.omniagent.mobile;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;

/**
 * OmniAgent Mobile Companion — Background Agent & Phone Assistant Service
 *
 * Coordinates:
 *  - Wake word detection ("Hey Omni")
 *  - On-device phone automation (Spotify, Alarms, Calls, SMS, WhatsApp, Gmail, Apps)
 *  - Dual-mode backend: On-Device SLM (NPU/CPU via C++ JNI) or Remote User Server (HTTP/MCP)
 *  - Battery-aware routing and notifications
 */
public class MobileAgentService {

    public static class VoiceCommandResult {
        public final boolean wakeWordDetected;
        public final String wakeWordUsed;
        public final String rawCommand;
        public final DeviceAction action;
        public final String executionResponse;

        public VoiceCommandResult(
            boolean wakeWordDetected,
            String wakeWordUsed,
            String rawCommand,
            DeviceAction action,
            String executionResponse
        ) {
            this.wakeWordDetected = wakeWordDetected;
            this.wakeWordUsed = wakeWordUsed;
            this.rawCommand = rawCommand;
            this.action = action;
            this.executionResponse = executionResponse;
        }

        @Override
        public String toString() {
            StringBuilder sb = new StringBuilder();
            if (wakeWordDetected) {
                sb.append(String.format("🎙️ [Wake Word Detected]: \"%s\"\n", wakeWordUsed));
            }
            sb.append(action.toString()).append("\n");
            if (executionResponse != null && !executionResponse.isEmpty()) {
                sb.append(String.format("🤖 Engine Output:\n%s", executionResponse));
            }
            return sb.toString().trim();
        }
    }

    private final NativeEngineJNI engine;
    private final MobileTaskRouter router;
    private final NotificationAssistant notificationAssistant;
    private final WakeWordDetector wakeWordDetector;
    private final PhoneAutomationEngine automationEngine;
    private AssistantConfig config;
    private long engineCtx = 0;

    private int batteryPercent = 82;
    private boolean isPowerSaveMode = false;

    public MobileAgentService() {
        this(new AssistantConfig());
    }

    public MobileAgentService(AssistantConfig config) {
        this.config = config != null ? config : new AssistantConfig();
        this.engine = new NativeEngineJNI();
        this.router = new MobileTaskRouter();
        this.notificationAssistant = new NotificationAssistant(engine);
        this.wakeWordDetector = new WakeWordDetector(this.config.getWakeWord());
        this.automationEngine = new PhoneAutomationEngine();

        initBackend();
    }

    private void initBackend() {
        if (config.getMode() == AssistantConfig.EngineMode.ON_DEVICE_SLM) {
            try {
                this.engineCtx = engine.safeInit(config.getLocalModelPath(), 2);
            } catch (Throwable t) {
                this.engineCtx = 0;
            }
        } else {
            // In remote server mode, release local context to save memory
            if (engineCtx != 0) {
                engine.safeFree(engineCtx);
                engineCtx = 0;
            }
        }
    }

    /**
     * Processes spoken or typed voice command:
     * 1. Inspects for wake word ("Hey Omni, play the box by roddy rich")
     * 2. Maps to phone automation action (Spotify, Alarm, Call, SMS, WhatsApp, Gmail, App)
     * 3. Executes on-device intent or routes to local SLM / user's server
     */
    public VoiceCommandResult processVoiceCommand(String rawVoiceInput) {
        WakeWordDetector.WakeWordMatch match = wakeWordDetector.inspect(rawVoiceInput);
        String payload = match.commandPayload.isEmpty() ? rawVoiceInput.trim() : match.commandPayload;

        DeviceAction action = automationEngine.parse(payload);
        String executionOutput = executeAction(action);

        return new VoiceCommandResult(
            match.detected,
            match.wakeWordUsed,
            payload,
            action,
            executionOutput
        );
    }

    /**
     * Executes the parsed phone automation action.
     */
    public String executeAction(DeviceAction action) {
        if (action == null) return "No action specified.";

        switch (action.type) {
            case PLAY_MUSIC:
                return String.format("[Media Intent] Launched %s with search query \"%s\". Audio stream active.",
                    action.getParam("service", "Spotify"), action.getParam("query", ""));

            case SET_ALARM:
                return String.format("[Alarm Clock Intent] Alarm registered for %s via Android DeskClock.",
                    action.getParam("time", ""));

            case SET_TIMER:
                return String.format("[Timer Intent] Countdown timer of %s started.",
                    action.getParam("duration", ""));

            case CALL_CONTACT:
                return String.format("[Telecom Intent] Initiating telephone call to \"%s\" (%s).",
                    action.getParam("contact", ""), action.androidDataUri);

            case ANSWER_CALL:
                return "[Telecom Hook] Incoming call accepted. Microphone and speaker connected.";

            case END_CALL:
                return "[Telecom Hook] Call terminated.";

            case SEND_SMS:
                return String.format("[Messaging Intent] SMS drafted to %s: \"%s\".",
                    action.getParam("contact", ""), action.getParam("message", ""));

            case SEND_WHATSAPP:
                return String.format("[WhatsApp Intent] WhatsApp message dispatched to %s: \"%s\".",
                    action.getParam("contact", ""), action.getParam("message", ""));

            case DRAFT_GMAIL:
                return String.format("[Gmail Intent] Email draft composed to %s: \"%s\".",
                    action.getParam("recipient", ""), action.getParam("body", ""));

            case OPEN_APP:
                return String.format("[App Launcher] Package \"%s\" (%s) opened in foreground.",
                    action.getParam("package", ""), action.getParam("app", ""));

            case SUMMARIZE_NOTIFICATIONS:
                return summarizeNotifications();

            case GENERAL_QUERY:
            default:
                return executeTask(action.getParam("prompt", ""));
        }
    }

    public String executeTask(String prompt) {
        // If configured to point to user's remote server:
        if (config.getMode() == AssistantConfig.EngineMode.REMOTE_SERVER) {
            return queryRemoteServer(prompt);
        }

        // On-device hybrid routing
        MobileTaskRouter.RoutingResult route = router.route(prompt, batteryPercent, isPowerSaveMode);

        if (route.destination == MobileTaskRouter.RoutingDestination.LOCAL_NPU) {
            return engine.generate(engineCtx, prompt, 0.7f);
        } else {
            return "[Cloud Adapter (Encrypted Offload)] Complex query processed via cloud API: " + prompt;
        }
    }

    private String queryRemoteServer(String prompt) {
        try {
            URL url = new URL(config.getServerUrl() + "/");
            HttpURLConnection conn = (HttpURLConnection) url.openConnection();
            conn.setRequestMethod("POST");
            conn.setRequestProperty("Content-Type", "application/json");
            conn.setDoOutput(true);
            conn.setConnectTimeout(4000);
            conn.setReadTimeout(5000);

            String escapedPrompt = prompt.replace("\"", "\\\"");
            String jsonPayload = String.format(
                "{\"jsonrpc\":\"2.0\",\"method\":\"route\",\"params\":{\"prompt\":\"%s\"},\"id\":1}",
                escapedPrompt
            );

            try (OutputStream os = conn.getOutputStream()) {
                os.write(jsonPayload.getBytes(StandardCharsets.UTF_8));
            }

            if (conn.getResponseCode() == 200) {
                try (BufferedReader br = new BufferedReader(new InputStreamReader(conn.getInputStream(), StandardCharsets.UTF_8))) {
                    StringBuilder response = new StringBuilder();
                    String line;
                    while ((line = br.readLine()) != null) {
                        response.append(line);
                    }
                    return "[Remote OmniAgent Server @ " + config.getServerUrl() + "]: " + response.toString();
                }
            }
        } catch (Exception e) {
            // Fall back to local engine if remote is unreachable
        }

        return "[Fallback On-Device NPU] Remote server offline; processed locally: " + engine.generate(engineCtx, prompt, 0.7f);
    }

    public String summarizeNotifications() {
        return notificationAssistant.summarizeRecentNotifications(engineCtx);
    }

    public String draftReply(String recipient, String message) {
        return notificationAssistant.draftQuickReply(recipient, message, engineCtx);
    }

    public void setConfig(AssistantConfig newConfig) {
        this.config = newConfig;
        this.wakeWordDetector.setPrimaryWakeWord(config.getWakeWord());
        initBackend();
    }

    public AssistantConfig getConfig() {
        return config;
    }

    public void setBatteryStatus(int percent, boolean powerSave) {
        this.batteryPercent = percent;
        this.isPowerSaveMode = powerSave;
    }

    public NotificationAssistant getNotificationAssistant() {
        return notificationAssistant;
    }

    public int getBatteryPercent() {
        return batteryPercent;
    }

    public boolean isPowerSaveMode() {
        return isPowerSaveMode;
    }

    public void destroy() {
        if (engineCtx != 0) {
            engine.safeFree(engineCtx);
            engineCtx = 0;
        }
    }
}
