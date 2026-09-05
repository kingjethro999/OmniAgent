package io.omniagent.mobile;

import java.util.HashMap;
import java.util.Map;

/**
 * OmniAgent Mobile Companion — Device Action Definition
 *
 * Represents an on-device phone automation action derived from voice commands:
 *  - Media playback (Spotify, YouTube Music)
 *  - Telecom control (Call, Answer, End Call)
 *  - Clock & Alarms (Set Alarm, Timer)
 *  - Messaging & Communications (SMS, WhatsApp, Gmail)
 *  - Application launching (WhatsApp, TikTok, Gallery, Camera, etc.)
 */
public class DeviceAction {

    public enum ActionType {
        PLAY_MUSIC,
        SET_ALARM,
        SET_TIMER,
        CALL_CONTACT,
        ANSWER_CALL,
        END_CALL,
        SEND_SMS,
        SEND_WHATSAPP,
        DRAFT_GMAIL,
        OPEN_APP,
        SUMMARIZE_NOTIFICATIONS,
        GENERAL_QUERY
    }

    public final ActionType type;
    public final String title;
    public final String voiceResponse;
    public final Map<String, String> parameters = new HashMap<>();
    public final String androidIntentAction;
    public final String androidDataUri;
    public final String targetPackage;

    public DeviceAction(
        ActionType type,
        String title,
        String voiceResponse,
        String androidIntentAction,
        String androidDataUri,
        String targetPackage
    ) {
        this.type = type;
        this.title = title;
        this.voiceResponse = voiceResponse;
        this.androidIntentAction = androidIntentAction;
        this.androidDataUri = androidDataUri;
        this.targetPackage = targetPackage;
    }

    public DeviceAction withParam(String key, String value) {
        this.parameters.put(key, value);
        return this;
    }

    public String getParam(String key, String defaultValue) {
        return parameters.getOrDefault(key, defaultValue);
    }

    @Override
    public String toString() {
        StringBuilder sb = new StringBuilder();
        sb.append(String.format("[%s] %s\n", type, title));
        sb.append(String.format("  • Voice Feedback: \"%s\"\n", voiceResponse));
        if (targetPackage != null && !targetPackage.isEmpty()) {
            sb.append(String.format("  • Target App:     %s\n", targetPackage));
        }
        if (androidIntentAction != null && !androidIntentAction.isEmpty()) {
            sb.append(String.format("  • Intent Action:  %s\n", androidIntentAction));
        }
        if (androidDataUri != null && !androidDataUri.isEmpty()) {
            sb.append(String.format("  • Data URI:       %s\n", androidDataUri));
        }
        if (!parameters.isEmpty()) {
            sb.append(String.format("  • Parameters:     %s\n", parameters));
        }
        return sb.toString().trim();
    }
}
