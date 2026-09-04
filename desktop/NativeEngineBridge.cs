using System;
using System.Runtime.InteropServices;

namespace OmniAgent.Desktop
{
    public static class NativeEngineBridge
    {
        private const string LibraryName = "omni_engine";

        [DllImport(LibraryName, EntryPoint = "omni_init_engine", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr InitEngine(string modelPath, int nThreads);

        [DllImport(LibraryName, EntryPoint = "omni_generate", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Generate(IntPtr ctx, string prompt, byte[] outputBuffer, UIntPtr maxOutputLen, float temperature);

        [DllImport(LibraryName, EntryPoint = "omni_free_engine", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeEngine(IntPtr ctx);
    }
}
