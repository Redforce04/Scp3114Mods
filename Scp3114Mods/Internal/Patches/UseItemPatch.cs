// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         Scp3114Patch.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/29/2023 5:35 PM
//    Created Date:     10/29/2023 5:35 PM
// -----------------------------------------

using HarmonyLib;
using InventorySystem.Items.Usables;
using MEC;
using Mirror;
using PlayerRoles;
using PluginAPI.Core;
using Utils.Networking;

namespace Scp3114Mods.Internal.Patches;

/// <summary>
/// Processes fake item usage.
/// </summary>
[HarmonyPatch(typeof(InventorySystem.Items.Usables.UsableItemsController), nameof(InventorySystem.Items.Usables.UsableItemsController.ServerReceivedStatus))]
internal class UseItemPatch
{
    private static void Postfix(NetworkConnection conn, StatusMessage msg)
    {
        try
        {
            if (!Scp3114Mods.Singleton.Config.FakeUsableInteractions)
                return;
            if (Config.Dbg) Log.Debug($"Fake Usable Interaction Triggered.");
            Player ply = Player.Get(conn.identity);
            if (ply is null || !(ply.ReferenceHub.inventory.CurInstance is UsableItem usableItem) ||
                usableItem.ItemSerial != msg.ItemSerial)
            {
                return;
            }

            if (ply.Role != RoleTypeId.Scp3114)
                return;
            if (msg.Status == StatusMessage.StatusType.Cancel)
            {
                if (Config.Dbg) Log.Debug("Sending Fake Usable Interaction.");
                new StatusMessage(StatusMessage.StatusType.Start, msg.ItemSerial).SendToAuthenticated();
                if (Scp3114Mods.Singleton.Config.AutohideItemAfterFakeUse)
                {
                    Timing.CallDelayed(usableItem.UseTime, () =>
                    {
                        ply.CurrentItem = null;
                        if (Config.Dbg) Log.Debug("Hiding Item for Fake Use.");
                    });
                }
            }
        }
        catch (Exception e)
        {
            Log.Error("Scp3114Mods has caught an error at Fake Item Use Patch.");
            if (Config.Dbg) Log.Debug($"Exception: \n{e}");
            return;
        }
    }
}