package io.omniagent.mobile;

import java.util.Scanner;

/**
 * OmniAgent Mobile Companion — Standalone Phone Assistant Runner
 *
 * Runs the full Phone Assistant pipeline directly on workstation JVM or server:
 *  - Entry onboarding: Select On-Device SLM vs Custom Remote Server
 *  - Free wake word spotting ("Hey Omni")
 *  - Native phone automation: Spotify playback, Alarms, Calls, SMS, WhatsApp, Gmail, App launches
 */
public class MobileAgentRunner {

    public static void main(String[] args) {
        System.out.println("==========================================================");
        System.out.println("  OmniAgent Phone Assistant & Mobile Companion");
        System.out.println("  Native Voice Automation • Zero 3rd-Party Cloud API Cost");
        System.out.println("==========================================================");

        AssistantConfig config = new AssistantConfig();

        // 1. Direct Command Mode (CLI arguments)
        if (args.length > 0) {
            MobileAgentService service = new MobileAgentService(config);
            String input = String.join(" ", args);
            System.out.println("\n[Voice Input]: \"" + input + "\"");
            MobileAgentService.VoiceCommandResult result = service.processVoiceCommand(input);
            System.out.println("\n" + result + "\n");
            service.destroy();
            return;
        }

        // 2. Interactive Assistant Mode with Entry Onboarding
        Scanner scanner = new Scanner(System.in);
        System.out.println("\n┌────────────────────────────────────────────────────────────┐");
        System.out.println("│  Choose Assistant Backend on Entry:                        │");
        System.out.println("│  [1] On-Device Engine (Local 0 MB Smart Intent Routing)    │");
        System.out.println("│  [2] Custom Remote Server (Point to self-hosted HTTP API)  │");
        System.out.println("└────────────────────────────────────────────────────────────┘");
        System.out.print("Select Setup [1 or 2, default: 1]: ");

        String setupChoice = scanner.hasNextLine() ? scanner.nextLine().trim() : "1";
        if (setupChoice.equals("2")) {
            System.out.print("Enter your OmniAgent Server URL (default: http://127.0.0.1:8765): ");
            String serverUrl = scanner.hasNextLine() ? scanner.nextLine().trim() : "";
            if (serverUrl.isEmpty()) serverUrl = "http://127.0.0.1:8765";
            config.setMode(AssistantConfig.EngineMode.REMOTE_SERVER);
            config.setServerUrl(serverUrl);
            System.out.println("[Configured] Remote Server: " + serverUrl);
        } else {
            config.setMode(AssistantConfig.EngineMode.ON_DEVICE_SLM);
            System.out.println("[Configured] On-Device Smart Engine (0 MB Download Required)");
        }

        MobileAgentService service = new MobileAgentService(config);
        System.out.println("\nActive Engine: " + (NativeEngineJNI.isNativeAvailable() ? "Native NPU/CPU JNI (Active)" : "Edge Fallback Simulation"));
        System.out.println("Wake Word:     \"" + config.getWakeWord() + "\"");
        System.out.println("Battery Level: " + service.getBatteryPercent() + "% (Power Save: " + service.isPowerSaveMode() + ")");
        System.out.println("Privacy:       100% On-Device (Zero data leaves phone for routine tasks)\n");

        printHelp();

        while (true) {
            System.out.print("\nVoice Command > ");
            if (!scanner.hasNextLine()) break;
            String line = scanner.nextLine().trim();

            if (line.equalsIgnoreCase("exit") || line.equalsIgnoreCase("quit") || line.equals("0")) {
                break;
            }

            if (line.equalsIgnoreCase("help") || line.equals("?")) {
                printHelp();
                continue;
            }

            if (line.equalsIgnoreCase("mode")) {
                boolean isRemote = config.getMode() == AssistantConfig.EngineMode.REMOTE_SERVER;
                config.setMode(isRemote ? AssistantConfig.EngineMode.ON_DEVICE_SLM : AssistantConfig.EngineMode.REMOTE_SERVER);
                service.setConfig(config);
                System.out.println("Switched mode to: " + config.getMode());
                continue;
            }

            if (line.equalsIgnoreCase("battery")) {
                boolean newMode = !service.isPowerSaveMode();
                service.setBatteryStatus(newMode ? 12 : 85, newMode);
                System.out.println("Battery updated: " + service.getBatteryPercent() + "% (Power Save: " + service.isPowerSaveMode() + ")");
                continue;
            }

            // Process voice or typed assistant command
            MobileAgentService.VoiceCommandResult result = service.processVoiceCommand(line);
            System.out.println("\n" + result);
        }

        service.destroy();
        System.out.println("\nPhone Assistant session stopped.");
    }

    private static void printHelp() {
        System.out.println("Try speaking or typing commands (with or without 'Hey Omni'):");
        System.out.println("  • Music:        \"Hey Omni, play the box by roddy rich\"");
        System.out.println("  • Clock:        \"Hey Omni, set an alarm for 7:00 AM\"");
        System.out.println("  • Calls:        \"Hey Omni, call mum\" | \"Hey Omni, pick the call\" | \"Hey Omni, end call\"");
        System.out.println("  • Messages:     \"Hey Omni, send message to Sarah saying on my way\"");
        System.out.println("  • WhatsApp:     \"Hey Omni, send whatsapp to John saying meeting in 5 minutes\"");
        System.out.println("  • Gmail:        \"Hey Omni, draft a gmail to boss saying working remotely today\"");
        System.out.println("  • Launch Apps:  \"Hey Omni, open whatsapp\" | \"open tiktok\" | \"open gallery\" | \"open youtube\"");
        System.out.println("  • Notifications:\"Hey Omni, summarize my notifications\"");
        System.out.println("  • Controls:     'mode' (toggle server/local) | 'battery' (toggle saver) | 'exit'");
    }
}
