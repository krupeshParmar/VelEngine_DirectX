#include "ToolsCommon.h"

namespace vel::tools {
    extern void ShutDownTextureTools();
}

VEL_EDITOR_API void
ShutDownContentTools()
{
    using namespace vel::tools;
    ShutDownTextureTools();
}