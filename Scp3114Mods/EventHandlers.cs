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
using PlayerRoles.PlayableScps.Subroutines;
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

    // Shows a message to players informing them about the various features they can use as scp3114
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
                    _sendMessage(ev.Player, "You can fake shoot weapons by pressing [T].", 4f);
                    return;
                }

                if (Scp3114Mods.Singleton.Config.FakeUsableInteractions && item is UsableItem)
                {
                    _sendMessage(ev.Player,"You can fake use items by right clicking with your mouse", 4f);
                    return;
                }
            }
        }
        catch (Exception e)
        {
            Logging.Debug("An error has occured.");
        }
    }
    
    [PluginEvent(ServerEventType.RoundStart)]
    internal void OnRoundStart()
    {
    }
    
    // Processes Role Changes.
    [PluginEvent(ServerEventType.PlayerChangeRole)]
    internal void OnRoleChange(PlayerChangeRoleEvent ev)
    {
        if (Scp3114Mods.Singleton.Config.SpectatorHideMode == SpectatorHideMode.Disabled)
            goto skipSpectatorCheck;

        // Spoof player role for spectators if config is enabled.
        if (ev.NewRole == RoleTypeId.Spectator)
        {
            _hide3114ForSpectator(ev.Player);
        }   

        // Un-Spoof player role for spectators if config is enabled.
        if (ev.OldRole.RoleTypeId == RoleTypeId.Spectator)
        {
            _show3114ForPlayer(ev.Player);
        }
        skipSpectatorCheck:
        if (ev.NewRole != RoleTypeId.Scp3114)
            return;
        // Check strangle cooldown.
        try
        {
            Logging.Debug("Scp 3114 processing actions");
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
            Logging.Warning("Could not give player a disguise of themself.");
            Logging.Debug($"Exception: \n{e}");
        }
    }

    /// <summary>
    /// Re-syncs 3114 role info for a player.
    /// </summary>
    private void _show3114ForPlayer(Player updateFor)
    {
        try
        {
            foreach (Player ply in Player.GetPlayers())
            {
                if (ply.Role != RoleTypeId.Scp3114)
                    continue;
                
                ply.ChangeAppearance(RoleTypeId.Scp3114, new List<Player> {updateFor});
            }
        }
        catch (Exception e)
        {
            Logging.Warning($"Could not refresh spoofed info to player {updateFor.Nickname}.");
            Logging.Debug($"Exception: \n{e}");
        }
    }
    
    /// <summary>
    /// Hides scp 3114 to players (if the config setting is enabled.)
    /// </summary>
    private void _hide3114ForSpectator(Player spectator)
    {
        try
        {
            foreach (Player ply in Player.GetPlayers())
            {
                if (ply.Role != RoleTypeId.Scp3114)
                    continue;
                if (ply.RoleBase is not Scp3114Role scpRole)
                    continue;
                if (Scp3114Mods.Singleton.Config.SpectatorHideMode == SpectatorHideMode.TemporaryRoleSpoof && !scpRole.Disguised)
                    continue;
                RoleTypeId lastRole = Scp3114Mods.Singleton.Config.SpectatorRole3114 == RoleTypeId.None ? (scpRole.Disguised ? scpRole.RoleTypeId : RoleTypeId.None) : Scp3114Mods.Singleton.Config.SpectatorRole3114;
                if (lastRole != RoleTypeId.None)
                {
                    ply.ChangeAppearance(lastRole, new List<Player> {spectator});
                    continue;
                }

                ply.ChangeAppearance(RoleTypeId.ClassD,new List<Player> {spectator});
            }
        }
        catch (Exception e)
        {
            Logging.Warning($"Could not send fake spectator info to player {spectator.Nickname}.");
            Logging.Debug($"Exception: \n{e}");
        }
    }


    /// <summary>
    /// Processes Strangle Information.
    /// </summary>
    public void OnStranglingPlayer(StranglingPlayerArgs ev)
    {
        Player target = Player.Get(ev.Target.Target);
        Logging.Debug($"Target Role: {target.Role}");
        if (Scp3114Mods.Singleton.Config.DisableTutorialsStrangling && target.Role == RoleTypeId.Tutorial)
        {
            Logging.Debug("Strangle Disabled. - Tutorial");
            _sendMessage(ev.Attacker, Scp3114Mods.Singleton.Translation.CannotStrangleTutorials, 5f);
            ev.IsAllowed = false;
            return;
        }
        
        // is player innocent?
        if (!Scp3114Mods.Singleton.Config.AllowStranglingInnocents && target.IsPlayerInnocent())
        {
            Logging.Debug("Strangle Disabled. - Innocent");

            _sendMessage(ev.Attacker, Scp3114Mods.Singleton.Translation.CannotStrangleInnocentPlayer, 5f);
            ev.IsAllowed = false;
            return;
        }
        
        // Is the player is not holding an item or the item doesnt count as a strangle blocking item, skip. 
        if (target.CurrentItem is null || Scp3114Mods.Singleton.Config.ItemsThatWontBlockStrangle.Contains(target.CurrentItem.ItemTypeId))
            return;
        
        bool emptyHandAll = Scp3114Mods.Singleton.Config.RequireEmptyHandToStrangleAll;
        bool emptyHandInnocent = Scp3114Mods.Singleton.Config.RequireEmptyHandToStrangleInnocents &&
                                 target.IsPlayerInnocent();
        if (!emptyHandAll && !emptyHandInnocent)
            return;
        
        Logging.Debug("Strangle Disabled. - Empty Hand");

        _sendMessage(ev.Attacker, !emptyHandAll
                ? Scp3114Mods.Singleton.Translation.CannotStrangleInnocentPlayerEmptyHand
                : Scp3114Mods.Singleton.Translation.CannotStranglePlayerEmptyHand, 5f);
        ev.IsAllowed = false;
    }

    /// <summary>
    /// Used to send a message player in respect to the config setting for Broadcasts vs Hints.
    /// </summary>
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
    
    /// <summary>
    /// Processes the fake firing args for dry-firing.
    /// </summary>
    private void _processDryFiring(Firearm firearm, Player sender)
    {
        Logging.Debug("Fake dry firing gun.");
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
    /// <summary>
    /// Processes the fake firing args for normal firing.
    /// </summary>
    private void _processFiring(Firearm firearm)
    {
        Logging.Debug("Fake firing gun.");
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
