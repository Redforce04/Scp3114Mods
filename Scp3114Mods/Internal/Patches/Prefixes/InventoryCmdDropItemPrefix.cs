// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         DropItemPatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/29/2023 8:42 PM
//    Created Date:     10/29/2023 8:42 PM
// -----------------------------------------

using HarmonyLib;
using InventorySystem;
using PlayerRoles;
using PluginAPI.Core;

namespace Scp3114Mods.Internal.Patches.Prefixes;

/// <summary>
/// Patches for scp3114 dropping item. This is used to trigger fake shooting.
/// </summary>
[HarmonyPatch(typeof(Inventory), nameof(Inventory.UserCode_CmdDropItem__UInt16__Boolean))]
internal static class InventoryCmdDropItemPrefix
{
    /// <summary>
    /// The logic may need to be re-worked as some players want to be able to throw items like the vase.
    /// </summary>
    private static bool Prefix(Inventory __instance, ushort itemSerial, bool tryThrow)
    {
        if (!tryThrow)
            return true;
        Player ply = Player.Get(__instance._hub);
        if (ply.Role != RoleTypeId.Scp3114)
            return true;

        return Scp3114Mods.Singleton.Handlers.OnPlayerThrowItem(ply, itemSerial, tryThrow);
    }
}