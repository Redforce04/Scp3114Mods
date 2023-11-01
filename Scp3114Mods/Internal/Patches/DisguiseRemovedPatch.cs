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
using PluginAPI.Core;

namespace Scp3114Mods.Internal.Patches;

[HarmonyPatch(typeof(Scp3114Identity),nameof(Scp3114Identity.OnRagdollRemoved))]
internal static class DisguiseRemovedPatch
{
    internal static void Postfix(Scp3114Identity __instance)
    {
        float cooldown = Scp3114Mods.Singleton.Config.DisguiseCooldown;
        if (Config.Dbg) Log.Debug($"Processing Disguise Cooldown {cooldown}");
        if (cooldown <= 0)
            return;
        if (__instance.Role is not Scp3114Role role)
            return;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
            return;
        disguise.Cooldown.Trigger(cooldown);
        if (Config.Dbg) Log.Debug($"Disguise Cooldown Triggered {cooldown}");
    }
}