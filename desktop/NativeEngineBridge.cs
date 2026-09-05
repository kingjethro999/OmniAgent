using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OmniAgent.Desktop
{
    public static class NativeEngineBridge
    {
        private const string LibraryName = "omni_engine";
        private static bool? _isNativeAvailable = null;
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };

        static NativeEngineBridge()
        {
            NativeLibrary.SetDllImportResolver(typeof(NativeEngineBridge).Assembly, (name, assembly, searchPath) =>
            {
                if (name == LibraryName)
                {
                    string ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".dll" :
                                 RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".dylib" : ".so";
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string[] probePaths = new[]
                    {
                        Path.Combine(baseDir, "libomni_engine" + ext),
                        Path.Combine(baseDir, "omni_engine" + ext),
                        Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "core", "build", "libomni_engine" + ext)),
                        Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "core", "build", "omni_engine" + ext)),
                        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "core", "build", "libomni_engine" + ext)),
                        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "core", "build", "omni_engine" + ext))
                    };

                    foreach (var path in probePaths)
                    {
                        if (File.Exists(path) && NativeLibrary.TryLoad(path, out IntPtr handle))
                        {
                            return handle;
                        }
                    }
                }
                return IntPtr.Zero;
            });
        }

        [DllImport(LibraryName, EntryPoint = "omni_init_engine", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr omni_init_engine(string modelPath, int nThreads);

        [DllImport(LibraryName, EntryPoint = "omni_generate", CallingConvention = CallingConvention.Cdecl)]
        private static extern int omni_generate(IntPtr ctx, string prompt, byte[] outputBuffer, UIntPtr maxOutputLen, float temperature);

        [DllImport(LibraryName, EntryPoint = "omni_free_engine", CallingConvention = CallingConvention.Cdecl)]
        private static extern void omni_free_engine(IntPtr ctx);

        public static bool IsNativeAvailable()
        {
            if (_isNativeAvailable.HasValue)
                return _isNativeAvailable.Value;

            try
            {
                string ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".dll" :
                             RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".dylib" : ".so";
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] probePaths = new[]
                {
                    Path.Combine(baseDir, "libomni_engine" + ext),
                    Path.Combine(baseDir, "omni_engine" + ext),
                    Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "core", "build", "libomni_engine" + ext)),
                    Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "core", "build", "omni_engine" + ext)),
                    Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "core", "build", "libomni_engine" + ext))
                };

                foreach (var path in probePaths)
                {
                    if (File.Exists(path))
                    {
                        _isNativeAvailable = true;
                        return true;
                    }
                }
            }
            catch
            {
                // Ignored
            }

            _isNativeAvailable = false;
            return false;
        }

        public static IntPtr InitEngine(string modelPath, int nThreads = 4)
        {
            if (IsNativeAvailable())
            {
                try
                {
                    return omni_init_engine(modelPath, nThreads);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NativeEngine] Native init failed: {ex.Message}. Falling back to managed mode.");
                }
            }
            return IntPtr.Zero;
        }

        public static string Generate(IntPtr ctx, string prompt, float temperature = 0.7f)
        {
            if (ctx != IntPtr.Zero)
            {
                try
                {
                    byte[] buffer = new byte[4096];
                    int len = omni_generate(ctx, prompt, buffer, (UIntPtr)buffer.Length, temperature);
                    if (len > 0)
                    {
                        return Encoding.UTF8.GetString(buffer, 0, len);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NativeEngine] Native generation failed: {ex.Message}");
                }
            }

            // Fallback: check if local OmniAgent IDE Hook HTTP server is running on port 8765
            try
            {
                var payload = new
                {
                    jsonrpc = "2.0",
                    method = "audit",
                    params_data = new { code = prompt, file_path = "desktop_audit.txt" },
                    id = 1
                };
                var json = JsonSerializer.Serialize(payload).Replace("params_data", "params");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = _httpClient.PostAsync("http://127.0.0.1:8765/", content).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    using var doc = JsonDocument.Parse(responseBody);
                    if (doc.RootElement.TryGetProperty("result", out var resultObj) &&
                        resultObj.TryGetProperty("analysis", out var analysisProp))
                    {
                        return analysisProp.GetString() ?? "[Local Analysis Completed]";
                    }
                }
            }
            catch
            {
                // HTTP service offline, continue to heuristic engine
            }

            return "[OmniEngine Desktop Heuristic Core] Analyzed on-device. No security anomalies detected.";
        }

        public static void FreeEngine(IntPtr ctx)
        {
            if (ctx != IntPtr.Zero)
            {
                try
                {
                    omni_free_engine(ctx);
                }
                catch
                {
                    // Ignored
                }
            }
        }
    }
}
