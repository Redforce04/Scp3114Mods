// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         DisguiseRemovedPatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/31/2023 6:46 PM
//    Created Date:     10/31/2023 6:46 PM
// -----------------------------------------

using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;
using Scp3114Mods.API;

namespace Scp3114Mods.Internal.Patches.Postfixes;


/// <summary>
/// Patches for setting a disguise cooldown.
/// </summary>
[HarmonyPatch(typeof(Scp3114Identity),nameof(Scp3114Identity.OnRagdollRemoved))]
internal static class IdentityOnRagdollRemovedPostfix
{
    /// <summary>
    /// Allows us to set a custom cooldown after the disguise is removed.
    /// This needs some testing.
    /// </summary>
    [HarmonyPostfix]
    private static void Postfix(Scp3114Identity __instance)
    {
        try
        {

            if (__instance.CurIdentity.Status != Scp3114Identity.DisguiseStatus.None) return;

            float cooldown = Scp3114Mods.Singleton.Config.DisguiseCooldown;
            if (cooldown <= 0)
                return;
            if (__instance.Role is not Scp3114Role role)
                return;
            if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
                return;

            disguise.Cooldown.Trigger(cooldown);
            Logging.Debug($"Disguise Cooldown Triggered {cooldown}");
        }
        catch (Exception e)
        {
            Logging.Debug($"An error has been caught at IdentityOnRagdollRemovedPostfix. Exception: \n{e}");

        }
    }
}