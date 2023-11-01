// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         EventHandlers.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/29/2023 5:24 PM
//    Created Date:     10/29/2023 5:24 PM
// -----------------------------------------

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Usables;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using Scp3114Mods.API;
using Scp3114Mods.API.EventArgs;
using UnityEngine;

namespace Scp3114Mods;

public class EventHandlers
{

    [PluginEvent(ServerEventType.PlayerChangeItem)]
    internal void ShowMessage(PlayerChangeItemEvent ev)
    {
        try
        {
            if (ev.Player.Role is RoleTypeId.Scp3114)
            {
                var item = ev.Player.Items.FirstOrDefault(x => x.ItemSerial == ev.NewItem);
                if (item is null)
                    return;
                if (Scp3114Mods.Singleton.Config.FakeFiringAllowed && item is Firearm)
                {
                    ev.Player.ReceiveHint("You can fake shoot weapons by pressing [T].", 4f);
                    return;
                }

                if (Scp3114Mods.Singleton.Config.FakeUsableInteractions && item is UsableItem)
                {
                    ev.Player.ReceiveHint("You can fake use items by right clicking with your mouse", 4f);
                    return;
                }
            }
        }
        catch (Exception e)
        {
            Log.Debug("An error has occured.");
        }
    }
    
    [PluginEvent(ServerEventType.RoundStart)]
    internal void OnRoundStart()
    {
        Scp3114Mods.Singleton._clearCooldownList();
    }
    

    [PluginEvent(ServerEventType.PlayerChangeRole)]
    internal void OnRoleChange(PlayerChangeRoleEvent ev)
    {
        if (ev.NewRole != RoleTypeId.Scp3114)
            return;
        try
        {
            if (Config.Dbg) Log.Debug("Scp 3114 processing actions");
            if (Scp3114Mods.Singleton.Config.StranglePartialCooldown > 0)
            {
                Timing.CallDelayed(.1f, () =>
                {
                    if (!(ev.Player.RoleBase is not Scp3114Role role ||
                          !role.SubroutineModule.TryGetSubroutine<Scp3114Strangle>(out var strangle) ||
                          strangle is null))
                        strangle._postReleaseCooldown = Scp3114Mods.Singleton.Config.StranglePartialCooldown;

                });
            }
            if (Scp3114Mods.Singleton.Config.DisguiseDuration != 0)
            {
                Timing.CallDelayed(.1f, () => ev.Player.SetDisguisePermanentDuration(Scp3114Mods.Singleton.Config.DisguiseDuration == -1 ? float.MaxValue : Scp3114Mods.Singleton.Config.DisguiseDuration));
            }
            if(Scp3114Mods.Singleton.Config.StartInDisguiseOfSelf)
                Timing.CallDelayed(.1f, () => ev.Player.SetDisguise(ev.Player.Nickname, UnityEngine.Random.Range(0, 4) == 1 ? RoleTypeId.Scientist : RoleTypeId.ClassD));
        }
        catch (Exception e)
        {
            Log.Warning("Could not give player a disguise of themself.");
            if(Config.Dbg) Log.Debug($"Exception: \n{e}");
        }
    }


    public void OnStranglingPlayer(StranglingPlayerArgs ev)
    {
        if (Scp3114Mods.Singleton.Config.CanTutorialsBeStrangled && ev.Target.Role == RoleTypeId.Tutorial)
        {
            if (Config.Dbg) Log.Debug("Strangle Disabled. - Tutorial");
            _sendMessage(ev.Attacker, Scp3114Mods.Singleton.Translation.CannotStrangleTutorials, 5f);
            ev.IsAllowed = false;
            return;
        }
        
        // is player innocent?
        if (!Scp3114Mods.Singleton.Config.AllowStranglingInnocents && ev.Target.IsPlayerInnocent())
        {
            if (Config.Dbg) Log.Debug("Strangle Disabled. - Innocent");

            _sendMessage(ev.Attacker, Scp3114Mods.Singleton.Translation.CannotStrangleInnocentPlayer, 5f);
            ev.IsAllowed = false;
            return;
        }
        
        // Is the player is not holding an item or the item doesnt count as a strangle blocking item, skip. 
        if (ev.Target.CurrentItem is null || Scp3114Mods.Singleton.Config.ItemsThatWontBlockStrangle.Contains(ev.Target.CurrentItem.ItemTypeId))
            return;
        
        bool emptyHandAll = Scp3114Mods.Singleton.Config.RequireEmptyHandToStrangleAll;
        bool emptyHandInnocent = Scp3114Mods.Singleton.Config.RequireEmptyHandToStrangleInnocents &&
                                 ev.Target.IsPlayerInnocent();
        if (!emptyHandAll && !emptyHandInnocent)
            return;
        
        if (Config.Dbg) Log.Debug("Strangle Disabled. - Empty Hand");

        _sendMessage(ev.Attacker, !emptyHandAll
                ? Scp3114Mods.Singleton.Translation.CannotStrangleInnocentPlayerEmptyHand
                : Scp3114Mods.Singleton.Translation.CannotStranglePlayerEmptyHand, 5f);
        ev.IsAllowed = false;
    }

    private static void _sendMessage(Player ply, string message, float duration)
    {
        if (Scp3114Mods.Singleton.Config.UseHintsInsteadOfBroadcasts)
        {
            ply.ReceiveHint(message, duration);
        }
        else
        {
            ply.ClearBroadcasts();
            ply.SendBroadcast(message, (ushort)duration);
        }
    }
    public bool OnPlayerThrowItem(Player ply , ushort itemSerial, bool tryThrow)
    {
        if (!Scp3114Mods.Singleton.Config.FakeFiringAllowed)
            return true;
        if (ply.Role == RoleTypeId.Scp3114)
        {
            if (!ply.ReferenceHub.inventory.UserInventory.Items.ContainsKey(itemSerial))
                return true;
            var item = ply.ReferenceHub.inventory.UserInventory.Items[itemSerial];
            if (item is Firearm firearm)
            {
                if (firearm.Status.Ammo == 0)
                    _processDryFiring(firearm, ply);
                else
                    _processFiring(firearm);
                
                return false;
            }
        }

        return true;
    }
    
    
    private void _processDryFiring(Firearm firearm, Player sender)
    {
        if (Config.Dbg) Log.Debug("Fake dry firing gun.");
        // Dry Fire
        switch (firearm.ActionModule)
        {
            case AutomaticAction automatic:
                firearm.ServerSendAudioMessage((byte)(automatic)._dryfireClip);
                firearm.PlayGunshotForOwner(automatic._dryfireClip);

                break;
            case PumpAction pump:
                firearm.ServerSendAudioMessage((byte)(pump)._dryfireClip);
                firearm.PlayGunshotForOwner((byte)pump._dryfireClip);
                break;
            case DoubleAction doubleAction:
                firearm.ServerSendAudioMessage((byte)(doubleAction)._dryfireClip);
                firearm.PlayGunshotForOwner(doubleAction._dryfireClip);
                break;
        }
    }
    private void _processFiring(Firearm firearm)
    {
        if (Config.Dbg) Log.Debug("Fake firing gun.");
        switch (firearm.ActionModule)
        {
            case AutomaticAction:
                firearm.FakeFireAutomatic();
                break;
            case PumpAction:
                firearm.FakeFirePump();
                break;
            case DisruptorAction:
                firearm.FakeFireDisruptor();
                break;
            case DoubleAction:
                firearm.FakeFireDoubleAction();
                break;
        }
    }
}
