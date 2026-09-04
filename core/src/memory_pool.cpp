#include "memory_pool.h"
#include <cstdlib>
#include <iostream>

namespace omni {

MemoryPool::MemoryPool(size_t pool_size_bytes)
    : capacity_(pool_size_bytes), offset_(0) {
    buffer_ = std::malloc(pool_size_bytes);
}

MemoryPool::~MemoryPool() {
    if (buffer_) {
        std::free(buffer_);
    }
}

void* MemoryPool::allocate(size_t size) {
    if (offset_ + size > capacity_) {
        return nullptr;
    }
    void* ptr = static_cast<char*>(buffer_) + offset_;
    offset_ += size;
    return ptr;
}

void MemoryPool::reset() {
    offset_ = 0;
}

} // namespace omni
