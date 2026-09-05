package io.omniagent.mobile;

import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;

/**
 * OmniAgent Mobile Companion — Battery-Aware Task Router
 *
 * Classifies queries into LOCAL_NPU vs CLOUD_OFFLOAD based on:
 *  - Mobile-specific intent (notifications, replies, alarms, calendar)
 *  - Query complexity scoring
 *  - Device battery level and power saver state
 */
public class MobileTaskRouter {

    public enum RoutingDestination {
        LOCAL_NPU,
        CLOUD_OFFLOAD
    }

    public static class RoutingResult {
        public final RoutingDestination destination;
        public final float complexityScore;
        public final String reasoning;

        public RoutingResult(RoutingDestination destination, float complexityScore, String reasoning) {
            this.destination = destination;
            this.complexityScore = complexityScore;
            this.reasoning = reasoning;
        }

        @Override
        public String toString() {
            return String.format("[%s] Score: %.2f | Reason: %s", destination, complexityScore, reasoning);
        }
    }

    private static final Set<String> LOCAL_KEYWORDS = new HashSet<>(Arrays.asList(
        "notification", "notifications", "reply", "draft", "message", "sms",
        "summarize", "summary", "alarm", "timer", "calendar", "schedule",
        "battery", "power", "volume", "bluetooth", "wifi", "offline", "private"
    ));

    private static final Set<String> CLOUD_KEYWORDS = new HashSet<>(Arrays.asList(
        "research", "analyze", "derive", "integral", "quantum", "essay",
        "comprehensive", "synthesize", "business plan", "architecture", "complex"
    ));

    private float complexityThreshold = 0.55f;

    public MobileTaskRouter() {}

    public MobileTaskRouter(float complexityThreshold) {
        this.complexityThreshold = complexityThreshold;
    }

    public RoutingResult route(String prompt, int batteryPercent, boolean isPowerSaveMode) {
        if (prompt == null || prompt.trim().isEmpty()) {
            return new RoutingResult(RoutingDestination.LOCAL_NPU, 0.0f, "Empty prompt handled on-device");
        }

        // Rule 1: Under extreme low battery or power save mode, strictly prioritize local NPU
        if (isPowerSaveMode || batteryPercent < 15) {
            return new RoutingResult(
                RoutingDestination.LOCAL_NPU,
                0.1f,
                String.format("Battery Saver Override (%d%% battery, powerSave=%b) -> Enforcing Local NPU", batteryPercent, isPowerSaveMode)
            );
        }

        String lower = prompt.toLowerCase();
        int localMatches = 0;
        int cloudMatches = 0;

        for (String word : lower.split("\\s+")) {
            if (LOCAL_KEYWORDS.contains(word)) localMatches++;
            if (CLOUD_KEYWORDS.contains(word)) cloudMatches++;
        }

        int wordCount = prompt.split("\\s+").length;
        float lengthScore = Math.min(1.0f, (float) wordCount / 60.0f);

        float keywordScore = 0.5f;
        int totalMatches = localMatches + cloudMatches;
        if (totalMatches > 0) {
            keywordScore = (float) cloudMatches / (float) totalMatches;
        }

        float finalScore = (keywordScore * 0.6f) + (lengthScore * 0.4f);

        if (finalScore >= complexityThreshold) {
            return new RoutingResult(
                RoutingDestination.CLOUD_OFFLOAD,
                finalScore,
                String.format("High complexity (score: %.2f >= %.2f) -> Offloading to Cloud LLM", finalScore, complexityThreshold)
            );
        } else {
            return new RoutingResult(
                RoutingDestination.LOCAL_NPU,
                finalScore,
                String.format("Routine task (score: %.2f < %.2f) -> Fast on-device NPU", finalScore, complexityThreshold)
            );
        }
    }
}
