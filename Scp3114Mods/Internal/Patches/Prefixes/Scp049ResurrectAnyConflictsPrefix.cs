// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         049Revive.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/04/2023 5:11 PM
//    Created Date:     11/04/2023 5:11 PM
// -----------------------------------------

using HarmonyLib;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.Ragdolls;

namespace Scp3114Mods.Internal.Patches.Prefixes;

[HarmonyPatch(typeof(Scp049ResurrectAbility), nameof(Scp049ResurrectAbility.AnyConflicts))]
internal static class Scp049ResurrectAnyConflictsPrefix 
{
    // This patch really doesnt need to be a transpiler or postfix.
    // All the method does is block 3114 from reviving, so making a transpiler for it is pretty useless.
    // A postfix would be useless as well, as it would just be returning true, while running twice as many instructions for no reason.
    private static bool Prefix(Scp049ResurrectAbility __instance, ref bool __result, BasicRagdoll ragdoll)
    {
        if (!Scp3114Mods.Singleton.Config.Allow049ResurrectStolenSkinPlayer)
            return true;
        
        __result = false;
        return false;
    }
}