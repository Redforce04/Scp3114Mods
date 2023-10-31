// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         StrangleKillPatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/30/2023 10:42 PM
//    Created Date:     10/30/2023 10:42 PM
// -----------------------------------------

using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerStatsSystem;

namespace Scp3114Mods.Internal.Patches;

[HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.OnAnyPlayerDied))]
internal static class StrangleKillPatch
{
    private static void Postfix(Scp3114Strangle __instance, ReferenceHub deadPly, DamageHandlerBase handler)
    {
        if (Scp3114Mods.Singleton.Config.StrangleCooldown <= 0)
            return;
        if(handler is Scp3114DamageHandler damageHandler)
        {
            if(damageHandler.Attacker.Hub == __instance.Owner && damageHandler.Subtype == Scp3114DamageHandler.HandlerType.Strangulation)
            {
                __instance.Cooldown.Trigger(Scp3114Mods.Singleton.Config.StrangleCooldown);
            }
        }
    }
}