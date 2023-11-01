// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         UseGunPatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/29/2023 7:55 PM
//    Created Date:     10/29/2023 7:55 PM
// -----------------------------------------

using HarmonyLib;
using InventorySystem.Items.Firearms.BasicMessages;
using Mirror;
using PluginAPI.Core;

namespace Scp3114Mods.Internal.Patches;

[HarmonyPatch(typeof(InventorySystem.Items.Usables.UsableItemsController),
    nameof(InventorySystem.Items.Usables.UsableItemsController.ServerReceivedStatus))]
internal static class UseGunPatch
{
    private static bool Debug => Config.Dbg;

    internal static void Postfix(NetworkConnection conn, StatusMessage msg)
    {
        try
        {
            
        }
        catch (Exception e)
        {
            Log.Error("Scp3114Mods has caught an error at Fake Gun Use Patch.");
            if (Config.Dbg)
            {
                Log.Debug($"Exception: \n{e}");
            }
            return;
        }
    }

}