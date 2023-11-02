// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         Log.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/01/2023 10:07 PM
//    Created Date:     11/01/2023 10:07 PM
// -----------------------------------------

namespace Scp3114Mods.API;

public class Logging
{
    public static void Debug(string msg)
    {
        if (!Config.Dbg)
            return;
#if EXILED
        Exiled.API.Features.Log.Debug(msg);
#else
        PluginAPI.Core.Log.Debug(msg);
#endif
    }

    public static void Warning(string msg) => Warn(msg);
    public static void Warn(string msg)
    {
#if EXILED
        Exiled.API.Features.Log.Warn(msg);
#else
        PluginAPI.Core.Log.Warning(msg);
#endif  
    }
    
    public static void Info(string msg)
    {
#if EXILED
        Exiled.API.Features.Log.Info(msg);
#else
        PluginAPI.Core.Log.Info(msg);
#endif
    }
    
    public static void Error(string msg)
    {
#if EXILED
        Exiled.API.Features.Log.Error(msg);
#else
        PluginAPI.Core.Log.Error(msg);
#endif  
    }
}