using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace OmniAgent.Desktop
{
    public class SystemAutomation
    {
        public bool FormatCsv(string inputPath, string? outputPath = null)
        {
            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"[Automation] File not found: {inputPath}");
                return false;
            }

            outputPath ??= inputPath;
            try
            {
                var lines = File.ReadAllLines(inputPath);
                var formatted = new StringBuilder();

                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        parts[i] = parts[i].Trim();
                    }
                    formatted.AppendLine(string.Join(",", parts));
                }

                File.WriteAllText(outputPath, formatted.ToString());
                Console.WriteLine($"[Automation] Formatted CSV ({lines.Length} rows) -> {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Automation] Error formatting CSV: {ex.Message}");
                return false;
            }
        }

        public int OrganizeDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"[Automation] Directory not found: {directoryPath}");
                return 0;
            }

            int moved = 0;
            var files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                string targetFolder = ext switch
                {
                    ".cs" or ".py" or ".cpp" or ".h" or ".js" or ".ts" or ".java" => "Code",
                    ".pdf" or ".docx" or ".txt" or ".md" or ".rtf" => "Documents",
                    ".csv" or ".json" or ".xml" or ".sql" => "Data",
                    ".png" or ".jpg" or ".jpeg" or ".svg" or ".gif" => "Media",
                    _ => "Other"
                };

                string destDir = Path.Combine(directoryPath, targetFolder);
                Directory.CreateDirectory(destDir);
                string destFile = Path.Combine(destDir, Path.GetFileName(file));

                if (!File.Exists(destFile))
                {
                    File.Move(file, destFile);
                    moved++;
                }
            }

            Console.WriteLine($"[Automation] Organized {moved} files into categorized folders.");
            return moved;
        }

        public string RunGitStatus(string repoPath)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "status --short",
                    WorkingDirectory = repoPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                    return string.IsNullOrWhiteSpace(output) ? "Working tree clean." : output;
                }
            }
            catch (Exception ex)
            {
                return $"Git execution failed: {ex.Message}";
            }
            return "Unable to start git process.";
        }
    }
}
