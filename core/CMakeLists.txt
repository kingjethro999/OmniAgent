cmake_minimum_required(VERSION 3.18)
project(omni_engine VERSION 0.1.1 LANGUAGES C CXX)

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

# Shared library build target for C ABI / C# P/Invoke
add_library(omni_engine SHARED ${SOURCES})
target_compile_definitions(omni_engine PRIVATE OMNI_ENGINE_EXPORTS)

# JNI Bindings for Mobile / Java Android Companion
find_package(JNI QUIET)
if (JNI_FOUND)
    message(STATUS "JNI found: ${JNI_INCLUDE_DIRS}")
    add_library(omni_engine_jni SHARED bindings/jni/omni_jni.cpp)
    target_include_directories(omni_engine_jni PRIVATE ${JNI_INCLUDE_DIRS} ${CMAKE_CURRENT_SOURCE_DIR}/include)
    target_link_libraries(omni_engine_jni PRIVATE omni_engine)
else()
    # Fallback to standard OpenJDK paths if find_package didn't locate it
    set(JAVA_HOME "/usr/lib/jvm/java-21-openjdk-amd64")
    if (EXISTS "${JAVA_HOME}/include/jni.h")
        message(STATUS "Using fallback JNI headers at ${JAVA_HOME}")
        add_library(omni_engine_jni SHARED bindings/jni/omni_jni.cpp)
        target_include_directories(omni_engine_jni PRIVATE
            "${JAVA_HOME}/include"
            "${JAVA_HOME}/include/linux"
            ${CMAKE_CURRENT_SOURCE_DIR}/include
        )
        target_link_libraries(omni_engine_jni PRIVATE omni_engine)
    endif()
endif()

# Install rules
install(TARGETS omni_engine DESTINATION lib)
if (TARGET omni_engine_jni)
    install(TARGETS omni_engine_jni DESTINATION lib)
endif()
install(FILES include/omni_engine.h DESTINATION include)
