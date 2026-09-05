package io.omniagent.mobile;

import java.util.Locale;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * OmniAgent Mobile Companion — Free Wake Word Detector
 *
 * Lightweight, zero-dependency wake word detection engine that spots
 * configurable wake phrases (default: "Hey Omni", "OK Omni", "Omni")
 * and extracts the command payload with zero external cloud API calls.
 */
public class WakeWordDetector {

    public static class WakeWordMatch {
        public final boolean detected;
        public final String wakeWordUsed;
        public final String commandPayload;

        public WakeWordMatch(boolean detected, String wakeWordUsed, String commandPayload) {
            this.detected = detected;
            this.wakeWordUsed = wakeWordUsed;
            this.commandPayload = commandPayload != null ? commandPayload.trim() : "";
        }
    }

    private String primaryWakeWord = "hey omni";
    private final String[] aliasWakeWords = new String[] {
        "hey omni", "ok omni", "omni", "hey agent", "hey phone"
    };

    public WakeWordDetector() {}

    public WakeWordDetector(String wakeWord) {
        if (wakeWord != null && !wakeWord.trim().isEmpty()) {
            this.primaryWakeWord = wakeWord.trim().toLowerCase(Locale.ROOT);
        }
    }

    /**
     * Inspects incoming voice transcription or text for wake words.
     * If detected, returns the extracted command payload.
     */
    public WakeWordMatch inspect(String input) {
        if (input == null || input.trim().isEmpty()) {
            return new WakeWordMatch(false, null, "");
        }

        String lower = input.toLowerCase(Locale.ROOT).trim();

        // 1. Check primary wake word
        Pattern primaryPattern = Pattern.compile("^\\s*(" + Pattern.quote(primaryWakeWord) + ")[,!?:]*\\s*(.*)$", Pattern.CASE_INSENSITIVE);
        Matcher m = primaryPattern.matcher(lower);
        if (m.find()) {
            String payload = input.substring(m.end(1)).replaceAll("^[,!?:]*\\s*", "");
            return new WakeWordMatch(true, primaryWakeWord, payload);
        }

        // 2. Check aliases
        for (String alias : aliasWakeWords) {
            Pattern aliasPattern = Pattern.compile("^\\s*(" + Pattern.quote(alias) + ")[,!?:]*\\s*(.*)$", Pattern.CASE_INSENSITIVE);
            Matcher am = aliasPattern.matcher(lower);
            if (am.find()) {
                String payload = input.substring(am.end(1)).replaceAll("^[,!?:]*\\s*", "");
                return new WakeWordMatch(true, alias, payload);
            }
        }

        // 3. Check if wake word appears anywhere at the start or mid-phrase
        for (String alias : aliasWakeWords) {
            int idx = lower.indexOf(alias);
            if (idx >= 0) {
                int payloadStart = idx + alias.length();
                String payload = input.substring(payloadStart).replaceAll("^[,!?:]*\\s*", "");
                return new WakeWordMatch(true, alias, payload);
            }
        }

        // Wake word not found
        return new WakeWordMatch(false, null, input);
    }

    public String getPrimaryWakeWord() {
        return primaryWakeWord;
    }

    public void setPrimaryWakeWord(String primaryWakeWord) {
        this.primaryWakeWord = primaryWakeWord.toLowerCase(Locale.ROOT).trim();
    }
}
