#ifndef OMNI_MEMORY_POOL_H
#define OMNI_MEMORY_POOL_H

#include <cstddef>
#include <vector>

namespace omni {

class MemoryPool {
public:
    explicit MemoryPool(size_t pool_size_bytes);
    ~MemoryPool();

    void* allocate(size_t size);
    void reset();

private:
    size_t capacity_;
    size_t offset_;
    void* buffer_;
};

} // namespace omni

#endif
