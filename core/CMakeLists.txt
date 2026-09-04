cmake_minimum_required(VERSION 3.18)
project(omni_engine VERSION 0.1.0 LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

# Include header paths
include_directories(${CMAKE_CURRENT_SOURCE_DIR}/include)

# Source files
set(SOURCES
    src/engine.cpp
    src/tokenizer.cpp
    src/memory_pool.cpp
)

# Shared library build target
add_library(omni_engine SHARED ${SOURCES})

# Target definitions and flags
target_compile_definitions(omni_engine PRIVATE OMNI_ENGINE_EXPORTS)

# Install rules
install(TARGETS omni_engine DESTINATION lib)
install(FILES include/omni_engine.h DESTINATION include)
