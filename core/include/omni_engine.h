/**
 * OmniAgent Engine — Core C API Exports
 *
 * This header defines the platform-agnostic C ABI functions exported by
 * libomni_engine.so / omni_engine.dll for P/Invoke (C#) and JNI (Java) interop.
 */

#ifndef OMNI_ENGINE_H
#define OMNI_ENGINE_H

#include <stdint.h>
#include <stddef.h>

#ifdef __cplusplus
extern "C" {
#endif

#if defined(_WIN32) || defined(__CYGWIN__)
  #ifdef OMNI_ENGINE_EXPORTS
    #define OMNI_API __declspec(dllexport)
  #else
    #define OMNI_API __declspec(dllimport)
  #endif
#else
  #define OMNI_API __attribute__((visibility("default")))
#endif

/* Opaque engine handle */
typedef struct omni_context omni_context_t;

/**
 * Initialize the C++ inference engine with a GGUF model path.
 * Returns a handle pointer or NULL on failure.
 */
OMNI_API omni_context_t* omni_init_engine(const char* model_path, int32_t n_threads);

/**
 * Perform on-device text generation.
 * Output buffer is filled up to max_output_len.
 */
OMNI_API int32_t omni_generate(
    omni_context_t* ctx,
    const char* prompt,
    char* output_buffer,
    size_t max_output_len,
    float temperature
);

/**
 * Free engine resources and unload model from RAM/VRAM.
 */
OMNI_API void omni_free_engine(omni_context_t* ctx);

#ifdef __cplusplus
}
#endif

#endif /* OMNI_ENGINE_H */
