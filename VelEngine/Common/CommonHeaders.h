#pragma once

#pragma warning(disable: 4530) // disable exception warning

// C/C++
// NOTE: don't put here any headers that include std::vector or std::deque
#include <cstdint>
#include <string>
#include <assert.h>
#include <typeinfo>
#include <memory>
#include <unordered_map>
#include <mutex>
#include <cstring>

#if defined(_WIN64)
#include <DirectXMath.h>
#pragma warning(disable: 4530) // disable exception warning
#endif

#ifndef DISABLE_COPY
#define DISABLE_COPY(T)						\
			explicit T(const T&) = delete;	\
			T& operator=(const T&) = delete;
#endif // !DISABLE_COPY

#ifndef DISABLE_MOVE
#define DISABLE_MOVE(T)						\
			explicit T(T&&) = delete;		\
			T& operator=(T&&) = delete;
#endif // !DISABLE_MOVE

#ifndef DISABLE_COPY_AND_MOVE
#define DISABLE_COPY_AND_MOVE(T) DISABLE_COPY(T) DISABLE_MOVE(T)
#endif // !DISABLE_COPY_AND_MOVE

#ifdef _DEBUG
#define DEBUG_OP(x) x
#else
#define DEBUG_OP(x)
#endif

// common headers
#include "PrimitiveTypes.h"
#include "../Utilities/Utilities.h"
#include "../Utilities/MathTypes.h"
#include "../Utilities/Math.h"
#include "id.h"


namespace vel
{
	using string_hash = std::hash<std::string>;

	template<typename T>
	using Scope = std::unique_ptr<T>;
	template<typename T, typename ... Args>
	constexpr Scope<T> CreateScope(Args&& ... args)
	{
		return std::make_unique<T>(std::forward<Args>(args)...);
	}

	template<typename T>
	using Ref = std::shared_ptr<T>;
	template<typename T, typename ... Args>
	constexpr Ref<T> CreateRef(Args&& ... args)
	{
		return std::make_shared<T>(std::forward<Args>(args)...);
	}
}