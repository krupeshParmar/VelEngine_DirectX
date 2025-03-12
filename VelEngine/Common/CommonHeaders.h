#pragma once

#pragma warning(disable: 4530) // disable exception warning

// C/C++
#include <stdint.h>
#include <string>
#include <assert.h>
#include <typeinfo>
#include <memory>
#include <unordered_map>

#if defined(_WIN64)
#include <DirectXMath.h>
#endif

// common headers
#include "PrimitiveTypes.h"
#include "../Utlilities/Utilities.h"
#include "../Utlilities/MathTypes.h"
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