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
using InventorySystem.Items;
using InventorySystem.Items.Usables;
using MEC;
using Mirror;
using PlayerRoles;
using PluginAPI.Core;
using Scp3114Mods.API;
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
                else
                {
                    var item = ply.CurrentItem;
                    //MirrorExtensions.SetDirtyBitsMethodInfo;
                    ////this.GeneratedSyncVarSetter<ItemIdentifier>(value, ref this.CurItem, 1UL, new Action<ItemIdentifier, ItemIdentifier>(this.OnItemUpdated)
                    //ply.SendFakeSyncVar(ply.ReferenceHub.networkIdentity, typeof(InventorySystem.Inventory), nameof(InventorySystem.Inventory.CurItem),new ItemIdentifier(ply.CurrentItem.ItemTypeId, ply.CurrentItem.ItemSerial));
                    Timing.CallDelayed(usableItem.UseTime, () =>
                    {
                        ply.SendFakeSyncVar(ply.ReferenceHub.networkIdentity, typeof(InventorySystem.Inventory), nameof(InventorySystem.Inventory.CurItem),new ItemIdentifier(ply.CurrentItem.ItemTypeId, ply.CurrentItem.ItemSerial));
                        // ply.CurrentItem = item;
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