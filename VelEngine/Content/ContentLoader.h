#pragma once
#include "CommonHeaders.h"

#if !defined(SHIPPING)
namespace vel::content
{
	bool load_game();
	void unload_game();

	bool load_engine_shaders(Scope<u8[]>& shaders, u64& size);
}
#endif // !defined(SHIPPING)