#pragma once
#include <cstdint>
#include <string_view>

namespace vel::utl {

    constexpr uint64_t fnv_offset_basis = 14695981039346656037ull;
    constexpr uint64_t fnv_prime = 1099511628211ull;

    inline constexpr uint64_t fnv1a_hash(const char* str, uint64_t hash = fnv_offset_basis)
    {
        return (*str == 0) ? hash : fnv1a_hash(str + 1, (hash ^ static_cast<uint8_t>(*str)) * fnv_prime);
    }

    inline uint64_t fnv1a_hash(std::string_view str)
    {
        uint64_t hash = fnv_offset_basis;
        for (char c : str)
            hash = (hash ^ static_cast<uint8_t>(c)) * fnv_prime;
        return hash;
    }

} // namespace vel