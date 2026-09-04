#ifndef OMNI_TOKENIZER_H
#define OMNI_TOKENIZER_H

#include <string>
#include <vector>
#include <stdint.h>

namespace omni {

class Tokenizer {
public:
    Tokenizer();
    ~Tokenizer();

    std::vector<int32_t> encode(const std::string& text) const;
    std::string decode(const std::vector<int32_t>& tokens) const;
};

} // namespace omni

#endif
