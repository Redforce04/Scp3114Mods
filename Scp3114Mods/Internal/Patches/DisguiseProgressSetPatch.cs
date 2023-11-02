// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         DisguiseProgressSetPatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/31/2023 6:26 PM
//    Created Date:     10/31/2023 6:26 PM
// -----------------------------------------

using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;

namespace Scp3114Mods.Internal.Patches;

/// <summary>
/// Patches for the Disguise Progress Set.
/// </summary>
[HarmonyPatch(typeof(Scp3114Disguise), nameof(Scp3114Disguise.OnProgressSet))]
internal static class DisguiseProgressSetPatch
{
    
    /// <summary>
    /// Prefix patch to replace the original OnProgressSet.
    /// This may be better as a postfix or transpiler but it would require extra work.
    /// I believe a postfix would require clearing the cooldown, and re-triggering it which may break things. 
    /// </summary>
    private static bool Prefix(Scp3114Disguise __instance)
    {
        return true;
        /*
        Scp3114Identity.StolenIdentity curIdentity = __instance.ScpRole.CurIdentity;
        if (__instance.IsInProgress)
        {
            __instance._equipSkinSound.Play();
            curIdentity.Ragdoll = __instance.CurRagdoll;
            curIdentity.UnitNameId = (byte)(__instance._prevUnitIds.TryGetValue(__instance.CurRagdoll, out var value) ? value : 0);
            curIdentity.Status = Scp3114Identity.DisguiseStatus.Equipping;
        }
        else if (curIdentity.Status == Scp3114Identity.DisguiseStatus.Equipping)
        {
            __instance._equipSkinSound.Stop();
            curIdentity.Status = Scp3114Identity.DisguiseStatus.None;
            if (Scp3114Mods.Singleton.Config.DisguiseFailedCooldown != 0)
                __instance.Cooldown.Trigger(Scp3114Mods.Singleton.Config.DisguiseFailedCooldown == -1 ? __instance.Duration : Scp3114Mods.Singleton.Config.DisguiseFailedCooldown);
            Logging.Debug("Disguise Cooldown Triggered");
        }
        return false;
        */
    }
}