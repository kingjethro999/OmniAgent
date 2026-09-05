package io.omniagent.mobile;

import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * OmniAgent Mobile Companion — On-Device Phone Automation Engine
 *
 * Converts natural speech / text commands into direct on-device OS automation
 * actions without requiring 3rd-party cloud APIs.
 */
public class PhoneAutomationEngine {

    // Common app package name mappings
    private static final Map<String, String> APP_PACKAGES = new HashMap<>();
    static {
        APP_PACKAGES.put("whatsapp", "com.whatsapp");
        APP_PACKAGES.put("tiktok", "com.zhiliaoapp.musically");
        APP_PACKAGES.put("gallery", "com.google.android.apps.photos");
        APP_PACKAGES.put("photos", "com.google.android.apps.photos");
        APP_PACKAGES.put("youtube", "com.google.android.youtube");
        APP_PACKAGES.put("spotify", "com.spotify.music");
        APP_PACKAGES.put("camera", "com.google.android.GoogleCamera");
        APP_PACKAGES.put("gmail", "com.google.android.gm");
        APP_PACKAGES.put("email", "com.google.android.gm");
        APP_PACKAGES.put("mail", "com.google.android.gm");
        APP_PACKAGES.put("maps", "com.google.android.apps.maps");
        APP_PACKAGES.put("settings", "com.android.settings");
        APP_PACKAGES.put("chrome", "com.android.chrome");
        APP_PACKAGES.put("browser", "com.android.chrome");
        APP_PACKAGES.put("clock", "com.google.android.deskclock");
        APP_PACKAGES.put("calendar", "com.google.android.calendar");
        APP_PACKAGES.put("calculator", "com.google.android.calculator");
        APP_PACKAGES.put("telegram", "org.telegram.messenger");
        APP_PACKAGES.put("instagram", "com.instagram.android");
        APP_PACKAGES.put("twitter", "com.twitter.android");
        APP_PACKAGES.put("x", "com.twitter.android");
        APP_PACKAGES.put("contacts", "com.google.android.contacts");
        APP_PACKAGES.put("messages", "com.google.android.apps.messaging");
        APP_PACKAGES.put("files", "com.google.android.documentsui");
        APP_PACKAGES.put("drive", "com.google.android.apps.docs");
        APP_PACKAGES.put("notes", "com.google.android.keep");
        APP_PACKAGES.put("keep", "com.google.android.keep");
        APP_PACKAGES.put("netflix", "com.netflix.mediaclient");
        APP_PACKAGES.put("uber", "com.ubercab");
    }

    public PhoneAutomationEngine() {}

    /**
     * Parses natural language into an executable device action.
     */
    public DeviceAction parse(String command) {
        if (command == null || command.trim().isEmpty()) {
            return new DeviceAction(
                DeviceAction.ActionType.GENERAL_QUERY,
                "Empty Query",
                "I didn't catch that. How can I help you?",
                null, null, null
            );
        }

        String raw = command.trim();
        String lower = raw.toLowerCase(Locale.ROOT);

        // 1. Telecom Control: Answer / Pick Call
        if (matchesAny(lower, "pick the call", "pick up", "answer call", "answer the call", "accept call", "take the call")) {
            return new DeviceAction(
                DeviceAction.ActionType.ANSWER_CALL,
                "Answer Call",
                "Answering incoming call...",
                "android.telecom.action.ACCEPT_HANDOVER",
                null,
                "com.google.android.dialer"
            );
        }

        // 2. Telecom Control: End Call / Hang Up
        if (matchesAny(lower, "end call", "end the call", "hang up", "decline call", "reject call", "cut the call")) {
            return new DeviceAction(
                DeviceAction.ActionType.END_CALL,
                "End Call",
                "Ending call.",
                "android.telecom.action.END_CALL",
                null,
                "com.google.android.dialer"
            );
        }

        // 3. Media Playback (Spotify / Music): "play the box by roddy rich"
        Pattern playPattern = Pattern.compile("^\\s*(?:play|listen to|put on)\\s+(.+?)(?:\\s+on\\s+(spotify|youtube|apple music))?$", Pattern.CASE_INSENSITIVE);
        Matcher playMatcher = playPattern.matcher(raw);
        if (playMatcher.find()) {
            String query = playMatcher.group(1).trim();
            String targetApp = playMatcher.group(2) != null ? playMatcher.group(2).toLowerCase(Locale.ROOT) : "spotify";
            String pkg = targetApp.contains("youtube") ? "com.google.android.apps.youtube.music" : "com.spotify.music";
            String encodedQuery = urlEncode(query);

            return new DeviceAction(
                DeviceAction.ActionType.PLAY_MUSIC,
                "Play Music: " + query,
                "Playing \"" + query + "\" on Spotify.",
                "android.media.action.MEDIA_PLAY_FROM_SEARCH",
                "spotify:search:" + encodedQuery,
                pkg
            ).withParam("query", query).withParam("service", targetApp);
        }

        // 4. Phone Calling: "call mum", "phone dad", "dial 555-0199"
        Pattern callPattern = Pattern.compile("^\\s*(?:call|phone|dial)\\s+(.+)$", Pattern.CASE_INSENSITIVE);
        Matcher callMatcher = callPattern.matcher(raw);
        if (callMatcher.find()) {
            String contact = callMatcher.group(1).trim();
            return new DeviceAction(
                DeviceAction.ActionType.CALL_CONTACT,
                "Call: " + contact,
                "Calling " + contact + "...",
                "android.intent.action.CALL",
                "tel:" + urlEncode(contact),
                "com.google.android.dialer"
            ).withParam("contact", contact);
        }

        // 5. Set Alarm: "set an alarm for 7:00 AM", "wake me up at 6:30"
        Pattern alarmPattern = Pattern.compile("(?:set\\s+(?:an\\s+)?alarm(?:\\s+for)?|wake\\s+me\\s+up\\s+at)\\s+(\\d{1,2}(?::\\d{2})?\\s*(?:am|pm)?)", Pattern.CASE_INSENSITIVE);
        Matcher alarmMatcher = alarmPattern.matcher(raw);
        if (alarmMatcher.find()) {
            String timeStr = alarmMatcher.group(1).trim();
            return new DeviceAction(
                DeviceAction.ActionType.SET_ALARM,
                "Set Alarm: " + timeStr,
                "Setting an alarm for " + timeStr + ".",
                "android.intent.action.SET_ALARM",
                null,
                "com.google.android.deskclock"
            ).withParam("time", timeStr);
        }

        // 6. Set Timer: "set timer for 10 minutes"
        Pattern timerPattern = Pattern.compile("(?:set\\s+(?:a\\s+)?timer(?:\\s+for)?)\\s+(\\d+)\\s*(minutes?|seconds?|mins?|secs?|hours?)", Pattern.CASE_INSENSITIVE);
        Matcher timerMatcher = timerPattern.matcher(raw);
        if (timerMatcher.find()) {
            String duration = timerMatcher.group(1) + " " + timerMatcher.group(2);
            return new DeviceAction(
                DeviceAction.ActionType.SET_TIMER,
                "Set Timer: " + duration,
                "Starting a timer for " + duration + ".",
                "android.intent.action.SET_TIMER",
                null,
                "com.google.android.deskclock"
            ).withParam("duration", duration);
        }

        // 7. Draft / Send Email: "draft a gmail to boss saying working remotely today"
        Pattern gmailPattern = Pattern.compile("(?:draft\\s+(?:a\\s+)?(?:gmail|email)\\s+to|send\\s+(?:a\\s+)?(?:gmail|email)\\s+to)\\s+(.+?)(?:\\s+saying\\s+(.+)|\\s+about\\s+(.+))?$", Pattern.CASE_INSENSITIVE);
        Matcher gmailMatcher = gmailPattern.matcher(raw);
        if (gmailMatcher.find()) {
            String recipient = gmailMatcher.group(1).trim();
            String body = gmailMatcher.group(2) != null ? gmailMatcher.group(2).trim() :
                          gmailMatcher.group(3) != null ? gmailMatcher.group(3).trim() : "Hello,";
            String uri = "mailto:" + urlEncode(recipient) + "?body=" + urlEncode(body);

            return new DeviceAction(
                DeviceAction.ActionType.DRAFT_GMAIL,
                "Draft Email to: " + recipient,
                "Opening email to draft a message to " + recipient + ".",
                "android.intent.action.SENDTO",
                uri,
                "com.google.android.gm"
            ).withParam("recipient", recipient).withParam("body", body);
        }

        // 8. Send WhatsApp Message: "send whatsapp to John saying meeting now"
        Pattern whatsappPattern = Pattern.compile("(?:send\\s+whatsapp(?:\\s+message)?\\s+to|whatsapp)\\s+(.+?)(?:\\s+saying\\s+(.+))?$", Pattern.CASE_INSENSITIVE);
        Matcher whatsappMatcher = whatsappPattern.matcher(raw);
        if (whatsappMatcher.find()) {
            String contact = whatsappMatcher.group(1).trim();
            String msg = whatsappMatcher.group(2) != null ? whatsappMatcher.group(2).trim() : "";
            String uri = "whatsapp://send?text=" + urlEncode(msg);

            return new DeviceAction(
                DeviceAction.ActionType.SEND_WHATSAPP,
                "WhatsApp: " + contact,
                "Opening WhatsApp to send a message to " + contact + ".",
                "android.intent.action.VIEW",
                uri,
                "com.whatsapp"
            ).withParam("contact", contact).withParam("message", msg);
        }

        // 9. Send SMS / Text Message: "send message to mum saying on my way", "text John hey"
        Pattern smsPattern = Pattern.compile("(?:send\\s+(?:a\\s+)?(?:message|sms|text)\\s+to|text)\\s+(.+?)(?:\\s+saying\\s+(.+))?$", Pattern.CASE_INSENSITIVE);
        Matcher smsMatcher = smsPattern.matcher(raw);
        if (smsMatcher.find()) {
            String contact = smsMatcher.group(1).trim();
            String msg = smsMatcher.group(2) != null ? smsMatcher.group(2).trim() : "";
            String uri = "smsto:" + urlEncode(contact);

            return new DeviceAction(
                DeviceAction.ActionType.SEND_SMS,
                "Text: " + contact,
                "Opening Messages to text " + contact + ".",
                "android.intent.action.SENDTO",
                uri,
                "com.google.android.apps.messaging"
            ).withParam("contact", contact).withParam("message", msg);
        }

        // 10. Open Application: "open whatsapp", "open tiktok", "open gallery"
        Pattern openPattern = Pattern.compile("^\\s*(?:open|launch|start|show)\\s+(.+)$", Pattern.CASE_INSENSITIVE);
        Matcher openMatcher = openPattern.matcher(raw);
        if (openMatcher.find()) {
            String appKey = openMatcher.group(1).trim().toLowerCase(Locale.ROOT);
            String pkg = APP_PACKAGES.getOrDefault(appKey, null);
            if (pkg == null) {
                // Approximate matching
                for (Map.Entry<String, String> entry : APP_PACKAGES.entrySet()) {
                    if (appKey.contains(entry.getKey())) {
                        pkg = entry.getValue();
                        appKey = entry.getKey();
                        break;
                    }
                }
            }

            if (pkg != null) {
                return new DeviceAction(
                    DeviceAction.ActionType.OPEN_APP,
                    "Open App: " + capitalize(appKey),
                    "Opening " + capitalize(appKey) + "...",
                    "android.intent.action.MAIN",
                    null,
                    pkg
                ).withParam("app", appKey).withParam("package", pkg);
            }
        }

        // 11. Notification Summary: "summarize my notifications", "any messages"
        if (lower.contains("notification") || lower.contains("digest") || lower.contains("unread message")) {
            return new DeviceAction(
                DeviceAction.ActionType.SUMMARIZE_NOTIFICATIONS,
                "Notifications",
                "Checking your recent notifications...",
                null,
                null,
                null
            );
        }

        // 12. Fallback to General Query
        return new DeviceAction(
            DeviceAction.ActionType.GENERAL_QUERY,
            "Question: " + (raw.length() > 30 ? raw.substring(0, 30) + "..." : raw),
            null,
            null,
            null,
            null
        ).withParam("prompt", raw);
    }

    private boolean matchesAny(String input, String... candidates) {
        for (String c : candidates) {
            if (input.equals(c) || input.startsWith(c)) return true;
        }
        return false;
    }

    private String urlEncode(String value) {
        try {
            return URLEncoder.encode(value, StandardCharsets.UTF_8.toString());
        } catch (UnsupportedEncodingException e) {
            return value;
        }
    }

    private String capitalize(String str) {
        if (str == null || str.isEmpty()) return str;
        return str.substring(0, 1).toUpperCase(Locale.ROOT) + str.substring(1);
    }
}
