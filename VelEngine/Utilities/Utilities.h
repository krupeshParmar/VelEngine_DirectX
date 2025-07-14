#pragma once

#define USE_STL_VECTOR 0
#define USE_STL_DEQUE 1
#define USE_STL_MAP 0

#if USE_STL_VECTOR 
#include <vector>
#include <algorithm>
namespace vel::utl
{
	template<typename T>
	using vector = std::vector<T>;

	template<typename T>
	void erase_unordered(T& v, size_t index)
	{
		if (v.size() > 1)
		{
			std::iter_swap(v.begin() + index, v.end() - 1);
			v.pop_back();
		}
		else
		{
			v.clear();
		}
	}
}
#else
#include "Vector.h"
namespace vel::utl
{
	template<typename T>
	void erase_unordered(T& v, size_t index)
	{
		v.erase_unordered(index);
	}
}
#endif

#if USE_STL_DEQUE 
#include <deque>
namespace vel::utl
{
	template<typename T>
	using deque = std::deque<T>;
}
#endif

namespace vel::utl
{
	// Implement Vel's own containers
}

#if USE_STL_MAP
#else
#include <unordered_map>
namespace vel::utl
{
	template<typename K, typename V>
	using unordered_map = std::unordered_map<K, V>;
}
#endif

#include "FreeList.h"