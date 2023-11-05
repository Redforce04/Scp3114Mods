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
using Scp3114Mods.API;

namespace Scp3114Mods.Internal.Patches.Postfixes;

/// <summary>
/// Cooldown patch for strangle kills.
/// </summary>
[HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.OnAnyPlayerDied))]
internal static class StrangleOnAnyPlayerDiedPostfix
{
    /// <summary>
    /// Triggers a cooldown when scp3114 kills a player. Thanks Sr. Licht!
    /// </summary>
    [HarmonyPostfix]
    private static void Postfix(Scp3114Strangle __instance, ReferenceHub deadPly, DamageHandlerBase handler)
    {
        try
        {

            if (Scp3114Mods.Singleton.Config.StrangleCooldown <= 0)
                return;
            if (handler is Scp3114DamageHandler damageHandler)
            {
                if (damageHandler.Attacker.Hub == __instance.Owner &&
                    damageHandler.Subtype == Scp3114DamageHandler.HandlerType.Strangulation)
                {
                    __instance.Cooldown.Trigger(Scp3114Mods.Singleton.Config.StrangleCooldown);
                }
            }
        }
        catch (Exception e)
        {
            Logging.Debug($"An error has been caught at StrangleOnAnyPlayerDiedPostfix. Exception: \n{e}");

        }
    }
}
