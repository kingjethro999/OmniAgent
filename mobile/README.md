# OmniAgent Mobile Companion & Phone Assistant

A zero-cloud-cost, privacy-first mobile companion and hands-free voice automation assistant for Android.

## Key Architectural Principles

1. **Zero Required Model Downloads (0 MB Engine Overhead)**
   - Unlike mobile LLM setups that force users to download 1.5 GB+ model weights, OmniAgent leverages Android's smart native intent framework (`AlarmClock`, `MediaStore`, `TelecomManager`, `Uri`, `PackageManager`, `AccessibilityService`).
   - Natural speech commands are parsed in < 5ms into structured system intents.
   - For complex, generative conversational reasoning, users can seamlessly point the app to their self-hosted OmniAgent server (`http://<ip>:8765`).

2. **Optional Accessibility Automation Service**
   - Assistant automation is built on `OmniAccessibilityService`.
   - It functions as an optional accessibility feature users can toggle on or off in system settings.
   - It is strictly isolated from and does not interfere with the core companion background routines, task routing, or native JNI bindings.

3. **ChatGPT-Inspired Cobalt Design System**
   - Clean, professional dark palette following `.21st/design.json`:
     - Cobalt Primary: `#4F5FF7` (Dark Variant: `#6366F1`)
     - Background Surfaces: `#121211` (Surface), `#1E1E1C` (Cards), `#2D2D2A` (Borders)
     - Typography: `#F3F3F0` (Primary), `#A0A098` (Secondary)
     - Status Accents: `#10B981` (Local Core Active)
   - Zero emojis across the entire UI and codebase; utilizes crisp vector drawables for all actions.

---

## Wake Word Detection

OmniAgent listens locally for free wake words:
- "Hey Omni"
- "OK Omni"
- "Omni"
- "Hey Agent"

Example:
```text
"Hey Omni, play the box by roddy rich"
"Hey Omni, set an alarm for 7:00 AM"
"Hey Omni, call mum"
"Hey Omni, open tiktok"
```

---

## Supported On-Device Voice Automations

| Voice Command | Action Type | Target App / Framework | Intent / Mechanism |
|---|---|---|---|
| `play the box by roddy rich` | `PLAY_MUSIC` | `com.spotify.music` | `android.media.action.MEDIA_PLAY_FROM_SEARCH` (`spotify:search:...`) |
| `set an alarm for 7:00 AM` | `SET_ALARM` | `com.google.android.deskclock` | `android.intent.action.SET_ALARM` |
| `set a timer for 15 minutes` | `SET_TIMER` | `com.google.android.deskclock` | `android.intent.action.SET_TIMER` |
| `call mum` | `CALL_CONTACT` | `com.google.android.dialer` | `android.intent.action.CALL` (`tel:mum`) |
| `pick the call` | `ANSWER_CALL` | `com.google.android.dialer` | `TelecomManager.acceptRingingCall()` |
| `end call` | `END_CALL` | `com.google.android.dialer` | `TelecomManager.endCall()` |
| `send message to Sarah saying on my way` | `SEND_SMS` | Default SMS Handler | `android.intent.action.SENDTO` (`smsto:Sarah`) |
| `send whatsapp to John saying meeting in 5` | `SEND_WHATSAPP` | `com.whatsapp` | `android.intent.action.VIEW` (`https://api.whatsapp.com/send?...`) |
| `draft a gmail to boss saying running late` | `DRAFT_GMAIL` | `com.google.android.gm` | `android.intent.action.SENDTO` (`mailto:boss`) |
| `open tiktok` | `OPEN_APP` | `com.zhiliaoapp.musically` | `PackageManager.getLaunchIntentForPackage()` |
| `open whatsapp` | `OPEN_APP` | `com.whatsapp` | `PackageManager.getLaunchIntentForPackage()` |
| `open gallery` | `OPEN_APP` | `com.google.android.apps.photos` | `PackageManager.getLaunchIntentForPackage()` |
| `open youtube` | `OPEN_APP` | `com.google.android.youtube` | `PackageManager.getLaunchIntentForPackage()` |
| `summarize notifications` | `SUMMARIZE_NOTIFICATIONS` | `NotificationAssistant` | On-device NotificationListenerService |

---

## Building Production Signed APK

To assemble the production signed release APK:
```bash
./gradlew assembleRelease
```
The production APK is generated at:
`mobile/build/outputs/apk/release/OmniAgentMobile-release.apk`
(and copied to repository root `./OmniAgent-release.apk`)

### Run Standalone CLI Runner (Workstation Testing)
To test voice parsing, wake words, and intent dispatch without an attached phone:
```bash
# Compile standalone classes
javac -d mobile/bin -cp "mobile/src/main/java" mobile/src/main/java/io/omniagent/mobile/*.java

# Run direct voice command
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner "Hey Omni, play the box by roddy rich"

# Run interactive assistant loop
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner
```
