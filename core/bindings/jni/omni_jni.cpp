#include "io_omniagent_mobile_NativeEngineJNI.h"
#include "omni_engine.h"

#include <iostream>
#include <string>
#include <vector>

extern "C" {

JNIEXPORT jlong JNICALL Java_io_omniagent_mobile_NativeEngineJNI_initEngine(
    JNIEnv *env,
    jobject /* thiz */,
    jstring modelPath,
    jint nThreads
) {
    if (!modelPath) {
        return 0;
    }

    const char *path = env->GetStringUTFChars(modelPath, nullptr);
    if (!path) {
        return 0;
    }

    omni_context_t *ctx = omni_init_engine(path, static_cast<int32_t>(nThreads));
    env->ReleaseStringUTFChars(modelPath, path);

    return reinterpret_cast<jlong>(ctx);
}

JNIEXPORT jstring JNICALL Java_io_omniagent_mobile_NativeEngineJNI_generateText(
    JNIEnv *env,
    jobject /* thiz */,
    jlong ctx,
    jstring prompt,
    jfloat temperature
) {
    if (ctx == 0 || !prompt) {
        return env->NewStringUTF("[Error] Engine context null or prompt empty");
    }

    const char *promptStr = env->GetStringUTFChars(prompt, nullptr);
    if (!promptStr) {
        return env->NewStringUTF("[Error] Failed to read prompt string");
    }

    std::vector<char> buffer(4096, 0);
    int32_t len = omni_generate(
        reinterpret_cast<omni_context_t *>(ctx),
        promptStr,
        buffer.data(),
        buffer.size(),
        static_cast<float>(temperature)
    );

    env->ReleaseStringUTFChars(prompt, promptStr);

    if (len <= 0) {
        return env->NewStringUTF("[Error] Generation returned 0 bytes");
    }

    return env->NewStringUTF(buffer.data());
}

JNIEXPORT void JNICALL Java_io_omniagent_mobile_NativeEngineJNI_freeEngine(
    JNIEnv * /* env */,
    jobject /* thiz */,
    jlong ctx
) {
    if (ctx != 0) {
        omni_free_engine(reinterpret_cast<omni_context_t *>(ctx));
    }
}

} // extern "C"
