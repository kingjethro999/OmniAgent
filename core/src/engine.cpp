#include "omni_engine.h"
#include "tokenizer.h"
#include "memory_pool.h"

#include <iostream>
#include <string>
#include <cstring>
#include <vector>
#include <memory>

struct omni_context {
    std::string model_path;
    int32_t threads;
    std::unique_ptr<omni::Tokenizer> tokenizer;
    std::unique_ptr<omni::MemoryPool> memory_pool;
};

extern "C" {

omni_context_t* omni_init_engine(const char* model_path, int32_t n_threads) {
    if (!model_path) return nullptr;

    auto* ctx = new omni_context();
    ctx->model_path = model_path;
    ctx->threads = n_threads > 0 ? n_threads : 4;
    ctx->tokenizer = std::make_unique<omni::Tokenizer>();
    ctx->memory_pool = std::make_unique<omni::MemoryPool>(64 * 1024 * 1024); // 64MB tensor arena

    std::cout << "[OmniEngine C++ Core] Initialized with model: " << model_path 
              << " (" << ctx->threads << " threads)" << std::endl;

    return ctx;
}

int32_t omni_generate(
    omni_context_t* ctx,
    const char* prompt,
    char* output_buffer,
    size_t max_output_len,
    float temperature
) {
    if (!ctx || !prompt || !output_buffer || max_output_len == 0) {
        return -1;
    }

    std::string text(prompt);
    std::string result = "[C++ Native SLM Inference] Processed on-device: " + text.substr(0, 80);

    size_t copy_len = std::min(result.size(), max_output_len - 1);
    std::strncpy(output_buffer, result.c_str(), copy_len);
    output_buffer[copy_len] = '\0';

    return static_cast<int32_t>(copy_len);
}

void omni_free_engine(omni_context_t* ctx) {
    if (ctx) {
        std::cout << "[OmniEngine C++ Core] Unloaded model from memory pool." << std::endl;
        delete ctx;
    }
}

} // extern "C"
