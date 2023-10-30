// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         ReloadFirearmPatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/29/2023 9:19 PM
//    Created Date:     10/29/2023 9:19 PM
// -----------------------------------------

using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;

namespace Scp3114Mods.Internal.Patches;

//[HarmonyPatch(typeof(Scp3114Reveal), nameof(Scp3114Reveal.OnKeyUp))]
internal static class ReloadFirearmPatch
{
    internal static bool Prefix(Scp3114Reveal __instance)
    {

        return true;
    }
}