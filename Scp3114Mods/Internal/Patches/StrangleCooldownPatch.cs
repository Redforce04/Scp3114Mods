// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         StrangleCooldownPatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/30/2023 11:25 AM
//    Created Date:     10/30/2023 11:25 AM
// -----------------------------------------

using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerStatsSystem;
using PluginAPI.Core;

namespace Scp3114Mods.Internal.Patches;

[HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.OnAnyPlayerDied))]
internal static class StrangleCooldownPatch
{
    internal static void Postfix(Scp3114Strangle __instance, ReferenceHub deadPly, DamageHandlerBase handler)
    {
        if (handler is Scp3114DamageHandler damageHandler)
        {
            if (damageHandler.Attacker.Hub == __instance.Owner &&
                damageHandler.Subtype == Scp3114DamageHandler.HandlerType.Strangulation)
            {
                Scp3114Mods.Singleton.AddCooldownForPlayer(Player.Get(__instance.Owner), false);

            }
        }
    }
}
