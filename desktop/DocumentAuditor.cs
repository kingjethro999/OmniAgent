using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace OmniAgent.Desktop
{
    public class AuditFinding
    {
        public string FilePath { get; set; } = string.Empty;
        public int LineNumber { get; set; }
        public string Severity { get; set; } = "INFO"; // CRITICAL, WARNING, INFO
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty;
    }

    public class AuditReport
    {
        public string TargetPath { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int ScannedFilesCount { get; set; }
        public List<AuditFinding> Findings { get; set; } = new();
        public string AIAnalysisSummary { get; set; } = string.Empty;
    }

    public class DocumentAuditor
    {
        private static readonly Regex SecretRegex = new(
            @"(?i)(api[_-]?key|secret[_-]?key|password|bearer\s+[a-zA-Z0-9_\-\.]+|ghp_[a-zA-Z0-9]{36}|sk-[a-zA-Z0-9]{32,}|AKIA[0-9A-Z]{16})\s*[:=]\s*['""][^'""]+['""]",
            RegexOptions.Compiled);

        private static readonly Regex SqlInjectionRegex = new(
            @"(?i)(select|insert|update|delete|drop)\s+.*(\+|\$|\{).*from",
            RegexOptions.Compiled);

        private static readonly Regex PrivateKeyRegex = new(
            @"-----BEGIN\s+(RSA|EC|OPENSSH|DSA|PGP)?\s*PRIVATE\s+KEY-----",
            RegexOptions.Compiled);

        public AuditReport AuditDirectory(string directoryPath, IntPtr engineCtx)
        {
            var report = new AuditReport { TargetPath = directoryPath };
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"[Auditor] Directory not found: {directoryPath}");
                return report;
            }

            var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
            report.ScannedFilesCount = files.Length;

            Console.WriteLine($"[Auditor] Scanning {files.Length} files in {directoryPath} (100% On-Device / Zero Network)...");

            foreach (var file in files)
            {
                AuditFileInternal(file, report);
            }

            // Local SLM summary of findings
            string prompt = $"Summarize the security audit findings for {report.ScannedFilesCount} files. Found {report.Findings.Count} potential vulnerabilities.";
            report.AIAnalysisSummary = NativeEngineBridge.Generate(engineCtx, prompt);

            return report;
        }

        public AuditReport AuditFile(string filePath, IntPtr engineCtx)
        {
            var report = new AuditReport { TargetPath = filePath, ScannedFilesCount = 1 };
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[Auditor] File not found: {filePath}");
                return report;
            }

            AuditFileInternal(filePath, report);

            string prompt = $"Audit security of file {Path.GetFileName(filePath)} with {report.Findings.Count} alerts.";
            report.AIAnalysisSummary = NativeEngineBridge.Generate(engineCtx, prompt);

            return report;
        }

        private void AuditFileInternal(string file, AuditReport report)
        {
            // Skip binary or huge files
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext is ".exe" or ".dll" or ".so" or ".bin" or ".gguf" or ".zip" or ".tar" or ".gz")
                return;

            try
            {
                var lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (SecretRegex.IsMatch(line))
                    {
                        report.Findings.Add(new AuditFinding
                        {
                            FilePath = file,
                            LineNumber = i + 1,
                            Severity = "CRITICAL",
                            RuleName = "Hardcoded Secret / API Key",
                            Description = "Potential hardcoded credential or token detected in source file.",
                            Snippet = line.Trim()
                        });
                    }

                    if (PrivateKeyRegex.IsMatch(line))
                    {
                        report.Findings.Add(new AuditFinding
                        {
                            FilePath = file,
                            LineNumber = i + 1,
                            Severity = "CRITICAL",
                            RuleName = "Exposed Private Key",
                            Description = "Cryptographic private key header found in plain text.",
                            Snippet = line.Trim()
                        });
                    }

                    if (SqlInjectionRegex.IsMatch(line))
                    {
                        report.Findings.Add(new AuditFinding
                        {
                            FilePath = file,
                            LineNumber = i + 1,
                            Severity = "WARNING",
                            RuleName = "SQL Injection Pattern",
                            Description = "String concatenation or formatting detected in SQL query.",
                            Snippet = line.Trim()
                        });
                    }
                }
            }
            catch
            {
                // Skip unreadable files
            }
        }
    }
}
