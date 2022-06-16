using System;

namespace LuaVarWatcher
{
    public interface LuaProviderInterface
    {
        IntPtr GetLuaPointer();
    }
}