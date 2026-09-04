package io.omniagent.mobile;

public class NativeEngineJNI {
    static {
        try {
            System.loadLibrary("omni_engine_jni");
        } catch (UnsatisfiedLinkError e) {
            // Native library pending compilation via NDK
        }
    }

    public native long initEngine(String modelPath, int nThreads);
    public native String generateText(long ctx, String prompt, float temperature);
    public native void freeEngine(long ctx);
}
