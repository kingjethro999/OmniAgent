using System;
using System.IO;
using System.Threading;

namespace OmniAgent.Desktop
{
    public class FolderWatcher
    {
        private FileSystemWatcher? _watcher;
        private readonly DocumentAuditor _auditor = new();
        private readonly IntPtr _engineCtx;

        public FolderWatcher(IntPtr engineCtx)
        {
            _engineCtx = engineCtx;
        }

        public void Start(string watchPath)
        {
            if (!Directory.Exists(watchPath))
            {
                Directory.CreateDirectory(watchPath);
            }

            _watcher = new FileSystemWatcher(watchPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = "*.*",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnFileCreated;
            _watcher.Changed += OnFileChanged;

            Console.WriteLine($"[Dropzone Watcher] Actively monitoring folder: {Path.GetFullPath(watchPath)}");
            Console.WriteLine("Drop any source code or documents into this folder for instant on-device auditing.");
            Console.WriteLine("Press Ctrl+C to stop.\n");
        }

        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            // Give file writing a moment to complete
            Thread.Sleep(300);
            Console.WriteLine($"\n[Dropzone] New file detected: {e.Name}");
            RunAudit(e.FullPath);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Optional: avoid duplicate rapid firing
        }

        private void RunAudit(string filePath)
        {
            var report = _auditor.AuditFile(filePath, _engineCtx);
            if (report.Findings.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️ [Audit Alert] {report.Findings.Count} potential issue(s) detected in {Path.GetFileName(filePath)}:");
                Console.ResetColor();
                foreach (var f in report.Findings)
                {
                    Console.WriteLine($"   • Line {f.LineNumber} [{f.Severity}]: {f.RuleName} — {f.Snippet}");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ [Audit Clean] No secrets or vulnerabilities detected in {Path.GetFileName(filePath)}.");
                Console.ResetColor();
            }
            Console.WriteLine($"🤖 AI Local Summary: {report.AIAnalysisSummary}");
        }
    }
}
