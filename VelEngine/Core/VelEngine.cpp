
#if !defined(SHIPPING)
#include "..\Content\ContentLoader.h"
#include "..\Components\ScriptComponent.h"
#include <thread>

bool engine_intitalize()
{
	bool result{ vel::content::load_game() };
	return result;
}
void engine_update()
{
	vel::script::update_scripts(10.f);
	std::this_thread::sleep_for(std::chrono::milliseconds(10));
}
void engine_shutdown()
{
	vel::content::unload_game();
}
#endif // !defined(SHIPPING)