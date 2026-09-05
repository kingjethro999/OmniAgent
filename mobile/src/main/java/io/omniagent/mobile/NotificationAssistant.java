package io.omniagent.mobile;

import java.util.ArrayList;
import java.util.List;

/**
 * OmniAgent Mobile Companion — Notification Assistant
 *
 * Captures, groups, and summarizes incoming mobile notifications and
 * drafts context-aware replies locally on-device.
 */
public class NotificationAssistant {

    public static class NotificationItem {
        public final String sender;
        public final String app;
        public final String message;
        public final long timestampMs;

        public NotificationItem(String sender, String app, String message, long timestampMs) {
            this.sender = sender;
            this.app = app;
            this.message = message;
            this.timestampMs = timestampMs;
        }

        @Override
        public String toString() {
            return String.format("[%s] %s: %s", app, sender, message);
        }
    }

    private final List<NotificationItem> recentNotifications = new ArrayList<>();
    private final NativeEngineJNI engine;

    public NotificationAssistant(NativeEngineJNI engine) {
        this.engine = engine;
        seedDefaultNotifications();
    }

    public void addNotification(String sender, String app, String message) {
        recentNotifications.add(new NotificationItem(sender, app, message, System.currentTimeMillis()));
    }

    public String summarizeRecentNotifications(long ctx) {
        if (recentNotifications.isEmpty()) {
            return "No recent notifications to summarize.";
        }

        StringBuilder sb = new StringBuilder();
        sb.append("Summarize these ").append(recentNotifications.size()).append(" notifications:\n");
        for (NotificationItem item : recentNotifications) {
            sb.append("• ").append(item.toString()).append("\n");
        }

        return engine.generate(ctx, sb.toString(), 0.3f);
    }

    public String draftQuickReply(String recipient, String incomingMessage, long ctx) {
        String prompt = String.format("Draft a polite, concise quick reply to %s who said: \"%s\"", recipient, incomingMessage);
        return engine.generate(ctx, prompt, 0.7f);
    }

    public List<NotificationItem> getRecentNotifications() {
        return new ArrayList<>(recentNotifications);
    }

    private void seedDefaultNotifications() {
        recentNotifications.add(new NotificationItem("Mom", "Messages", "Are you coming over for dinner tonight?", System.currentTimeMillis() - 1200000));
        recentNotifications.add(new NotificationItem("Calendar", "Google Calendar", "Sprint Planning in 15 minutes (Room 4B)", System.currentTimeMillis() - 600000));
        recentNotifications.add(new NotificationItem("GitHub", "GitHub Mobile", "Merged PR #14: Hybrid Router v1.0 into main", System.currentTimeMillis() - 300000));
    }
}
