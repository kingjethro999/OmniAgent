package io.omniagent.mobile;

/**
 * OmniAgent Mobile Companion — Phone Assistant Configuration
 *
 * Configures the assistant backend mode:
 *  - ON_DEVICE_SLM: Runs quantized model directly on phone NPU/CPU via C++ JNI
 *  - REMOTE_SERVER: Points to user's self-hosted OmniAgent server / desktop IDE hook
 */
public class AssistantConfig {

    public enum EngineMode {
        ON_DEVICE_SLM,
        REMOTE_SERVER
    }

    private EngineMode mode = EngineMode.ON_DEVICE_SLM;
    private String serverUrl = "http://127.0.0.1:8765";
    private String localModelPath = "models/phi-4-mini.gguf";
    private String wakeWord = "Hey Omni";
    private String preferredMusicApp = "com.spotify.music";
    private boolean voiceFeedbackEnabled = true;

    public AssistantConfig() {}

    public AssistantConfig(EngineMode mode, String target) {
        this.mode = mode;
        if (mode == EngineMode.REMOTE_SERVER) {
            this.serverUrl = target;
        } else {
            this.localModelPath = target;
        }
    }

    public EngineMode getMode() {
        return mode;
    }

    public void setMode(EngineMode mode) {
        this.mode = mode;
    }

    public String getServerUrl() {
        return serverUrl;
    }

    public void setServerUrl(String serverUrl) {
        this.serverUrl = serverUrl;
    }

    public String getLocalModelPath() {
        return localModelPath;
    }

    public void setLocalModelPath(String localModelPath) {
        this.localModelPath = localModelPath;
    }

    public String getWakeWord() {
        return wakeWord;
    }

    public void setWakeWord(String wakeWord) {
        this.wakeWord = wakeWord;
    }

    public String getPreferredMusicApp() {
        return preferredMusicApp;
    }

    public void setPreferredMusicApp(String preferredMusicApp) {
        this.preferredMusicApp = preferredMusicApp;
    }

    public boolean isVoiceFeedbackEnabled() {
        return voiceFeedbackEnabled;
    }

    public void setVoiceFeedbackEnabled(boolean enabled) {
        this.voiceFeedbackEnabled = enabled;
    }

    @Override
    public String toString() {
        return String.format(
            "AssistantConfig [Mode: %s, Target: %s, WakeWord: \"%s\", MusicApp: %s]",
            mode,
            mode == EngineMode.REMOTE_SERVER ? serverUrl : localModelPath,
            wakeWord,
            preferredMusicApp
        );
    }
}
