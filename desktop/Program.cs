using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OmniAgent.Desktop
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // If explicit CLI arguments are provided (e.g. --audit, --watch, --say, --cli), run headless / CLI mode
            if (args.Length > 0 && !args[0].Equals("--gui", StringComparison.OrdinalIgnoreCase))
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                PrintHeader();

                string modelPath = Environment.GetEnvironmentVariable("OMNI_LOCAL_MODEL_PATH") ?? "../models/phi-4-mini.gguf";
                IntPtr engineCtx = NativeEngineBridge.InitEngine(modelPath);

                try
                {
                    await HandleCliArgsAsync(args, engineCtx);
                    return;
                }
                finally
                {
                    if (engineCtx != IntPtr.Zero)
                    {
                        NativeEngineBridge.FreeEngine(engineCtx);
                    }
                }
            }

            // Default behavior: Launch Native Siri-Like Desktop GUI Window
            DesktopGuiWindow.Run(args);
        }

        static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==========================================================");
            Console.WriteLine("  OmniAgent Enterprise Desktop Worker v0.2.1 (.NET 10)");
            Console.WriteLine("  Silent Automation & Siri-like Voice Assistant for PC");
            Console.WriteLine("==========================================================");
            Console.ResetColor();

            bool hasNative = NativeEngineBridge.IsNativeAvailable();
            string engineStatus = hasNative ? "Native C++ Core (Active)" : "Managed Fallback (Local Heuristics / HTTP)";
            Console.WriteLine($"Engine:  {engineStatus}");
            Console.WriteLine($"Privacy: 100% On-Device (Zero data leaves workstation)\n");
        }

        static async Task HandleCliArgsAsync(string[] args, IntPtr engineCtx)
        {
            string command = args[0].ToLowerInvariant();
            var auditor = new DocumentAuditor();
            var automation = new SystemAutomation();
            var speech = new DesktopSpeechEngine();
            var voiceProfile = new VoiceProfileManager();
            var router = new DesktopActionRouter(automation, speech, engineCtx);
            var assistant = new DesktopAssistant(router, speech, voiceProfile);

            switch (command)
            {
                case "--cli":
                case "--menu":
                case "-m":
                    await RunInteractiveMenuAsync(engineCtx);
                    break;

                case "--assistant":
                case "-s":
                case "--voice":
                    await assistant.RunInteractiveHudAsync();
                    break;

                case "--listen":
                    await assistant.RunListeningLoopAsync();
                    break;

                case "--say":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: dotnet run -- --say \"<voice-command>\"");
                        return;
                    }
                    string userQuery = string.Join(" ", args, 1, args.Length - 1);
                    await assistant.ProcessCommandAsync(userQuery, speakResponse: true);
                    break;

                case "--train-voice":
                case "--calibrate":
                    voiceProfile.RunCalibrationWizard();
                    break;

                case "--audit":
                case "-a":
                    string target = args.Length > 1 ? args[1] : ".";
                    RunAudit(target, engineCtx, auditor);
                    break;

                case "--watch":
                case "-w":
                    string watchDir = args.Length > 1 ? args[1] : "./dropzone";
                    StartWatcher(watchDir, engineCtx);
                    break;

                case "--organize":
                    string orgDir = args.Length > 1 ? args[1] : ".";
                    automation.OrganizeDirectory(orgDir);
                    break;

                case "--format-csv":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: dotnet run -- --format-csv <path-to-file.csv>");
                        return;
                    }
                    automation.FormatCsv(args[1]);
                    break;

                case "--git-status":
                    Console.WriteLine(automation.RunGitStatus("."));
                    break;

                case "--status":
                    Console.WriteLine("Status: Desktop worker active and responsive.");
                    break;

                case "--help":
                case "-h":
                    PrintHelp();
                    break;

                default:
                    // If unrecognized option, pass as query to assistant!
                    string directQuery = string.Join(" ", args);
                    await assistant.ProcessCommandAsync(directQuery, speakResponse: true);
                    break;
            }
        }

        static async Task RunInteractiveMenuAsync(IntPtr engineCtx)
        {
            var auditor = new DocumentAuditor();
            var automation = new SystemAutomation();
            var speech = new DesktopSpeechEngine();
            var voiceProfile = new VoiceProfileManager();
            var router = new DesktopActionRouter(automation, speech, engineCtx);
            var assistant = new DesktopAssistant(router, speech, voiceProfile);

            while (true)
            {
                Console.WriteLine("\nSelect an enterprise automation or assistant action:");
                Console.WriteLine("  [1] Launch Desktop Siri Assistant (Interactive Voice & HUD)");
                Console.WriteLine("  [2] Hands-Free Wake Word Listening Loop (\"Hey Omni\")");
                Console.WriteLine("  [3] Train Voice Match & Accent Calibration Wizard");
                Console.WriteLine("  [4] Audit a file or folder for security/secrets");
                Console.WriteLine("  [5] Start silent background dropzone watcher");
                Console.WriteLine("  [6] Organize files in a directory");
                Console.WriteLine("  [7] Format a CSV document");
                Console.WriteLine("  [8] Test local SLM generation");
                Console.WriteLine("  [0] Exit");
                Console.Write("\nChoice: ");

                string? choice = Console.ReadLine()?.Trim();
                if (choice == "0" || choice?.ToLowerInvariant() == "exit")
                    break;

                switch (choice)
                {
                    case "1":
                        await assistant.RunInteractiveHudAsync();
                        break;

                    case "2":
                        await assistant.RunListeningLoopAsync();
                        break;

                    case "3":
                        voiceProfile.RunCalibrationWizard();
                        break;

                    case "4":
                        Console.Write("Enter path to file or directory to audit (default: .): ");
                        string? auditPath = Console.ReadLine()?.Trim();
                        if (string.IsNullOrEmpty(auditPath)) auditPath = ".";
                        RunAudit(auditPath, engineCtx, auditor);
                        break;

                    case "5":
                        Console.Write("Enter dropzone directory path (default: ./dropzone): ");
                        string? dropzone = Console.ReadLine()?.Trim();
                        if (string.IsNullOrEmpty(dropzone)) dropzone = "./dropzone";
                        StartWatcher(dropzone, engineCtx);
                        break;

                    case "6":
                        Console.Write("Enter directory path to organize (default: .): ");
                        string? orgPath = Console.ReadLine()?.Trim();
                        if (string.IsNullOrEmpty(orgPath)) orgPath = ".";
                        automation.OrganizeDirectory(orgPath);
                        break;

                    case "7":
                        Console.Write("Enter path to CSV file: ");
                        string? csvFile = Console.ReadLine()?.Trim();
                        if (!string.IsNullOrEmpty(csvFile))
                            automation.FormatCsv(csvFile);
                        break;

                    case "8":
                        Console.Write("Enter prompt for Local SLM: ");
                        string? prompt = Console.ReadLine()?.Trim();
                        if (!string.IsNullOrEmpty(prompt))
                        {
                            Console.WriteLine("\n[Executing Local SLM Inference...]");
                            string output = NativeEngineBridge.Generate(engineCtx, prompt, 0.7f);
                            Console.WriteLine($"\nOutput:\n{output}\n");
                        }
                        break;

                    default:
                        Console.WriteLine("Invalid selection.");
                        break;
                }
            }
        }

        static void RunAudit(string path, IntPtr engineCtx, DocumentAuditor auditor)
        {
            var report = auditor.AuditDirectory(path, engineCtx);

            Console.WriteLine($"\n══════════════════════════════════════════════════════════");
            Console.WriteLine($"  Audit Report for: {path}");
            Console.WriteLine($"  Scanned Files:    {report.ScannedFilesCount}");
            Console.WriteLine($"  Total Alerts:     {report.Findings.Count}");
            Console.WriteLine($"══════════════════════════════════════════════════════════");

            if (report.Findings.Count > 0)
            {
                foreach (var f in report.Findings)
                {
                    Console.ForegroundColor = f.Severity == "CRITICAL" ? ConsoleColor.Red : ConsoleColor.Yellow;
                    Console.WriteLine($"\n[{f.Severity}] {f.RuleName}");
                    Console.ResetColor();
                    Console.WriteLine($"File: {f.FilePath}:{f.LineNumber}");
                    Console.WriteLine($"Note: {f.Description}");
                    Console.WriteLine($"Code: {f.Snippet}");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nClean: No exposed credentials or vulnerabilities found.");
                Console.ResetColor();
            }

            Console.WriteLine($"\nLocal SLM Analysis: {report.AIAnalysisSummary}\n");
        }

        static void StartWatcher(string path, IntPtr engineCtx)
        {
            var watcher = new FolderWatcher(engineCtx);
            watcher.Start(path);

            var done = new ManualResetEvent(false);
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                done.Set();
            };

            done.WaitOne();
            watcher.Stop();
            Console.WriteLine("\n[Dropzone Watcher] Stopped.");
        }

        static void PrintHelp()
        {
            Console.WriteLine("Usage: dotnet run --project desktop [options]\n");
            Console.WriteLine("Desktop Siri Assistant Options:");
            Console.WriteLine("  --assistant, -s, --voice Start Siri-like Desktop Voice Assistant HUD");
            Console.WriteLine("  --say \"<command>\"         Run natural language voice command directly with speech");
            Console.WriteLine("  --listen                 Start hands-free wake word listening loop (\"Hey Omni\")");
            Console.WriteLine("  --train-voice            Run personalized voice & accent calibration wizard\n");
            Console.WriteLine("Enterprise Automation Options:");
            Console.WriteLine("  --audit, -a <path>       Audit a file or directory for security & leaked secrets");
            Console.WriteLine("  --watch, -w [path]       Start silent background dropzone watcher (default: ./dropzone)");
            Console.WriteLine("  --organize <path>        Organize and sort files in a folder into categories");
            Console.WriteLine("  --format-csv <path>      Normalize and clean CSV formatting");
            Console.WriteLine("  --git-status             Run git status check on repository");
            Console.WriteLine("  --status                 Check worker and engine bridge status");
            Console.WriteLine("  --help, -h               Show this help message");
        }
    }
}
