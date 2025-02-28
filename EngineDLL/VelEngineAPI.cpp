

#ifndef VEL_EDITOR_API
#define VEL_EDITOR_API extern "C" __declspec(dllexport)
#endif	// !VEL_EDITOR_API

#include "CommonHeaders.h"
#include "id.h"
#include "..\VelEngine\Components\Entity.h"
#include "..\VelEngine\Components\TransformComponent.h"

using namespace vel;

VEL_EDITOR_API
id::id_type CreateGameEntity()
{

}