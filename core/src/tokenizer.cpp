#include "tokenizer.h"

namespace omni {

Tokenizer::Tokenizer() {}
Tokenizer::~Tokenizer() {}

std::vector<int32_t> Tokenizer::encode(const std::string& text) const {
    std::vector<int32_t> tokens;
    for (char c : text) {
        tokens.push_back(static_cast<int32_t>(c));
    }
    return tokens;
}

std::string Tokenizer::decode(const std::vector<int32_t>& tokens) const {
    std::string text;
    for (int32_t t : tokens) {
        text += static_cast<char>(t);
    }
    return text;
}

} // namespace omni
