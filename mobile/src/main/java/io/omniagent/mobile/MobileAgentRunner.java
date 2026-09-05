package io.omniagent.mobile;

import java.util.Scanner;

/**
 * OmniAgent Mobile Companion — Standalone Java Runner
 *
 * Allows testing and running the complete Consumer Mobile Companion engine
 * directly on workstation JVM or server without requiring an Android emulator.
 */
public class MobileAgentRunner {

    public static void main(String[] args) {
        System.out.println("==========================================================");
        System.out.println("  OmniAgent Consumer Mobile Companion (Android / Java)");
        System.out.println("  Battery-Saver SLM Assistant & Native System Integration");
        System.out.println("==========================================================");

        MobileAgentService service = new MobileAgentService();
        System.out.println("Engine:  " + (NativeEngineJNI.isNativeAvailable() ? "Native NPU/CPU JNI (Active)" : "Edge Fallback Simulation"));
        System.out.println("Battery: " + service.getBatteryPercent() + "% (Power Save: " + service.isPowerSaveMode() + ")");
        System.out.println("Privacy: 100% On-Device Execution for Routine Queries\n");

        if (args.length > 0) {
            String prompt = String.join(" ", args);
            System.out.println("Processing query: \"" + prompt + "\"");
            MobileTaskRouter.RoutingResult route = service.routeTask(prompt);
            System.out.println("Routing: " + route);
            String output = service.executeTask(prompt);
            System.out.println("\nResponse:\n" + output);
            service.destroy();
            return;
        }

        // Run interactive CLI demo
        Scanner scanner = new Scanner(System.in);
        System.out.println("Simulated Mobile Assistant Actions:");
        System.out.println("  [1] Summarize recent notifications (\"Summarize notifications from the last hour\")");
        System.out.println("  [2] Draft a quick reply to Mom (\"Draft reply to Mom\")");
        System.out.println("  [3] Test task routing (Local NPU vs Cloud Offload)");
        System.out.println("  [4] Toggle battery saver mode (<15% enforcement)");
        System.out.println("  [0] Exit\n");

        while (true) {
            System.out.print("Mobile Action > ");
            if (!scanner.hasNextLine()) break;
            String choice = scanner.nextLine().trim();

            if (choice.equals("0") || choice.equalsIgnoreCase("exit")) {
                break;
            }

            switch (choice) {
                case "1":
                    System.out.println("\n--- Notifications Digest ---");
                    for (NotificationAssistant.NotificationItem n : service.getNotificationAssistant().getRecentNotifications()) {
                        System.out.println("  " + n);
                    }
                    System.out.println("\n🤖 AI Summary:");
                    System.out.println(service.summarizeNotifications() + "\n");
                    break;

                case "2":
                    System.out.println("\nIncoming Message from Mom: \"Are you coming over for dinner tonight?\"");
                    System.out.println("🤖 Drafted Quick Reply:");
                    System.out.println(service.draftReply("Mom", "Are you coming over for dinner tonight?") + "\n");
                    break;

                case "3":
                    System.out.print("Enter prompt to route: ");
                    String prompt = scanner.nextLine().trim();
                    if (!prompt.isEmpty()) {
                        MobileTaskRouter.RoutingResult route = service.routeTask(prompt);
                        System.out.println("Routing Decision: " + route);
                        System.out.println("Execution Output: " + service.executeTask(prompt) + "\n");
                    }
                    break;

                case "4":
                    boolean newMode = !service.isPowerSaveMode();
                    service.setBatteryStatus(newMode ? 12 : 85, newMode);
                    System.out.println("\nBattery mode updated: " + service.getBatteryPercent() + "% (Power Save: " + service.isPowerSaveMode() + ")\n");
                    break;

                default:
                    System.out.println("Unknown option.");
                    break;
            }
        }

        service.destroy();
        System.out.println("\nMobile companion stopped.");
    }
}
