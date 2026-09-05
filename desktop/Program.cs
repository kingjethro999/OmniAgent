using System;
using System.IO;
using System.Threading;

namespace OmniAgent.Desktop
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            PrintHeader();

            string modelPath = Environment.GetEnvironmentVariable("OMNI_LOCAL_MODEL_PATH") ?? "../models/phi-4-mini.gguf";
            IntPtr engineCtx = NativeEngineBridge.InitEngine(modelPath);

            try
            {
                if (args.Length > 0)
                {
                    HandleCliArgs(args, engineCtx);
                    return;
                }

                RunInteractiveMenu(engineCtx);
            }
            finally
            {
                if (engineCtx != IntPtr.Zero)
                {
                    NativeEngineBridge.FreeEngine(engineCtx);
                }
            }
        }

        static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==========================================================");
            Console.WriteLine("  OmniAgent Enterprise Desktop Worker (.NET 8)");
            Console.WriteLine("  Silent Background Automation & Local Document Auditing");
            Console.WriteLine("==========================================================");
            Console.ResetColor();

            bool hasNative = NativeEngineBridge.IsNativeAvailable();
            string engineStatus = hasNative ? "Native C++ Core (Active)" : "Managed Fallback (Local Heuristics / HTTP)";
            Console.WriteLine($"Engine:  {engineStatus}");
            Console.WriteLine($"Privacy: 100% On-Device (Zero data leaves workstation)\n");
        }

        static void HandleCliArgs(string[] args, IntPtr engineCtx)
        {
            string command = args[0].ToLowerInvariant();
            var auditor = new DocumentAuditor();
            var automation = new SystemAutomation();

            switch (command)
            {
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
                    Console.WriteLine($"Unknown option: {command}. Use --help for available commands.");
                    break;
            }
        }

        static void RunInteractiveMenu(IntPtr engineCtx)
        {
            var auditor = new DocumentAuditor();
            var automation = new SystemAutomation();

            while (true)
            {
                Console.WriteLine("\nSelect an enterprise automation action:");
                Console.WriteLine("  [1] Audit a file or folder for security/secrets");
                Console.WriteLine("  [2] Start silent background dropzone watcher");
                Console.WriteLine("  [3] Organize files in a directory");
                Console.WriteLine("  [4] Format a CSV document");
                Console.WriteLine("  [5] Test local SLM generation");
                Console.WriteLine("  [0] Exit");
                Console.Write("\nChoice: ");

                string? choice = Console.ReadLine()?.Trim();
                if (choice == "0" || choice?.ToLowerInvariant() == "exit")
                    break;

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter path to file or directory to audit (default: .): ");
                        string? auditPath = Console.ReadLine()?.Trim();
                        if (string.IsNullOrEmpty(auditPath)) auditPath = ".";
                        RunAudit(auditPath, engineCtx, auditor);
                        break;

                    case "2":
                        Console.Write("Enter dropzone directory path (default: ./dropzone): ");
                        string? dropzone = Console.ReadLine()?.Trim();
                        if (string.IsNullOrEmpty(dropzone)) dropzone = "./dropzone";
                        StartWatcher(dropzone, engineCtx);
                        break;

                    case "3":
                        Console.Write("Enter directory path to organize: ");
                        string? orgPath = Console.ReadLine()?.Trim();
                        if (!string.IsNullOrEmpty(orgPath))
                            automation.OrganizeDirectory(orgPath);
                        break;

                    case "4":
                        Console.Write("Enter path to CSV file: ");
                        string? csvPath = Console.ReadLine()?.Trim();
                        if (!string.IsNullOrEmpty(csvPath))
                            automation.FormatCsv(csvPath);
                        break;

                    case "5":
                        Console.Write("Enter prompt for local engine: ");
                        string? prompt = Console.ReadLine()?.Trim();
                        if (!string.IsNullOrEmpty(prompt))
                        {
                            string result = NativeEngineBridge.Generate(engineCtx, prompt);
                            Console.WriteLine($"\n[Inference Output]:\n{result}");
                        }
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        static void RunAudit(string target, IntPtr engineCtx, DocumentAuditor auditor)
        {
            AuditReport report = Directory.Exists(target)
                ? auditor.AuditDirectory(target, engineCtx)
                : auditor.AuditFile(target, engineCtx);

            Console.WriteLine($"\n══════════════════════════════════════════════════════════");
            Console.WriteLine($"  Audit Report for: {report.TargetPath}");
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
                Console.WriteLine("\n✅ Clean: No exposed credentials or vulnerabilities found.");
                Console.ResetColor();
            }

            Console.WriteLine($"\n🤖 Local SLM Analysis: {report.AIAnalysisSummary}\n");
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
            Console.WriteLine("Options:");
            Console.WriteLine("  --audit, -a <path>    Audit a file or directory for security & leaked secrets");
            Console.WriteLine("  --watch, -w [path]    Start silent background dropzone watcher (default: ./dropzone)");
            Console.WriteLine("  --organize <path>     Organize and sort files in a folder into categories");
            Console.WriteLine("  --format-csv <path>   Normalize and clean CSV formatting");
            Console.WriteLine("  --git-status          Run git status check on repository");
            Console.WriteLine("  --status              Check worker and engine bridge status");
            Console.WriteLine("  --help, -h            Show this help message");
        }
    }
}
