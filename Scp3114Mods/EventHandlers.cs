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
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using Scp3114Mods.API;
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
            if(Scp3114Mods.Singleton.Config.DisguiseDuration != 0)
                Timing.CallDelayed(.1f, () => ev.Player.SetDisguisePermanentDuration(Scp3114Mods.Singleton.Config.DisguiseDuration == -1 ? float.MaxValue : Scp3114Mods.Singleton.Config.DisguiseDuration));
            if(Scp3114Mods.Singleton.Config.StartInDisguiseOfSelf)
                Timing.CallDelayed(.1f, () => ev.Player.SetDisguise(ev.Player.Nickname, UnityEngine.Random.Range(0, 4) == 1 ? RoleTypeId.Scientist : RoleTypeId.ClassD));
        }
        catch (Exception e)
        {
            Log.Warning("Could not give player a disguise of themself.");
            if(Config.Dbg) Log.Debug($"Exception: \n{e}");
        }
    }

    public void OnPlayerThrowItem(Player ply , ushort itemSerial, bool tryThrow)
    {
        if (!Scp3114Mods.Singleton.Config.FakeFiringAllowed)
            return;
        if (ply.Role == RoleTypeId.Scp3114)
        {
            if (!ply.ReferenceHub.inventory.UserInventory.Items.ContainsKey(itemSerial))
                return;
            var item = ply.ReferenceHub.inventory.UserInventory.Items[itemSerial];
            if (item is Firearm firearm)
            {
                if (firearm.Status.Ammo == 0)
                    processDryFiring(firearm, ply);
                else
                    processFiring(firearm);
            }
            
            return;
        }

        return;
    }

    private void processDryFiring(Firearm firearm, Player sender)
    {
        if (Config.Dbg)
            Log.Debug("Fake dry firing gun.");
        // Dry Fire
        switch (firearm.ActionModule)
        {
            case AutomaticAction automatic:
                sender.ReferenceHub.networkIdentity.connectionToClient.Send<GunAudioMessage>(new GunAudioMessage(ReferenceHub._hostHub, automatic._dryfireClip, 10, sender.ReferenceHub));
                firearm.ServerSendAudioMessage((byte)(automatic)._dryfireClip);
                break;
            case PumpAction pump:
                sender.ReferenceHub.networkIdentity.connectionToClient.Send<GunAudioMessage>(new GunAudioMessage(ReferenceHub._hostHub, (byte)pump._dryfireClip, 10, sender.ReferenceHub));
                firearm.ServerSendAudioMessage((byte)(pump)._dryfireClip);
                break;
            case DoubleAction doubleAction:
                sender.ReferenceHub.networkIdentity.connectionToClient.Send<GunAudioMessage>(new GunAudioMessage(ReferenceHub._hostHub, doubleAction._dryfireClip, 10, sender.ReferenceHub));
                firearm.ServerSendAudioMessage((byte)(doubleAction)._dryfireClip);
                break;
        }
    }
    private void processFiring(Firearm firearm)
    {
        if (Config.Dbg)
            Log.Debug("Fake firing gun.");
        switch (firearm.ActionModule)
        {
            case AutomaticAction automatic:
                _fakeFireAutomatic(firearm, automatic);
                break;
            case PumpAction pump:
                _fakeFirePump(firearm, pump);
                break;
            case DisruptorAction disruptor:
                _fakeFireDisruptor(firearm, disruptor);
                break;
            case DoubleAction doubleAction:
                _fakeFireDoubleAction(firearm, doubleAction);
                break;
        }
    }

    private void _fakeFirePump(Firearm firearm, PumpAction pump)
    {
        if (firearm.Owner.HasBlock(BlockedInteraction.ItemPrimaryAction))
        {
            return;
        }
        if (pump.ChamberedRounds == 0 || firearm.Status.Ammo == 0)
        {
            pump.ServerResync();
            return;
        }
        if (pump._lastShotStopwatch.Elapsed.TotalSeconds < (double)pump.TimeBetweenShots || pump._pumpStopwatch.Elapsed.TotalSeconds < (double)pump.PumpingTime)
        {
            return;
        }
        pump.LastFiredAmount = 0;
        int num = pump.AmmoUsage;
        while (num > 0 && pump.ChamberedRounds > 0 && pump._firearm.Status.Ammo > 0)
        {
            num--;
            pump.ChamberedRounds--;
            pump.CockedHammers--;
            pump.LastFiredAmount++;
            if (pump.ChamberedRounds > 0)
            {
                pump._lastShotStopwatch.Restart();
            }
            firearm.Status = new FirearmStatus((byte)(firearm.Status.Ammo - 1), firearm.Status.Flags, firearm.Status.Attachments);
            firearm.Owner.networkIdentity.connectionToClient.Send<GunAudioMessage>(new GunAudioMessage(ReferenceHub._hostHub, (byte) (pump.ShotSoundId + pump.ChamberedRounds), 10, firearm.Owner));
            firearm.ServerSendAudioMessage((byte)(pump.ShotSoundId + pump.ChamberedRounds));
            firearm.ServerSideAnimator.Play(FirearmAnimatorHashes.Fire);
            if (pump.ChamberedRounds == 0 && firearm.Status.Ammo > 0 && !firearm.IsLocalPlayer)
            {
                pump._pumpStopwatch.Restart();
                firearm.AnimSetTrigger(pump._pumpAnimHash);
                break;
            }
        }
    }
    private void _fakeFireDoubleAction(Firearm firearm, DoubleAction doubleA)
    {
        if (firearm.Owner.HasBlock(BlockedInteraction.ItemPrimaryAction))
        {
            return;
        }
        if ((doubleA.ServerTriggerReady || firearm.IsLocalPlayer) && firearm.Status.Ammo > 0)
        {
            if (Scp3114Mods.Singleton.Config.FakeFiringUsesAmmo)
            {
                firearm.Status = new FirearmStatus((byte)(firearm.Status.Ammo - 1), firearm.Status.Flags, firearm.Status.Attachments);
            } 
            doubleA._nextAllowedShot = Time.timeSinceLevelLoad + doubleA._cooldownAfterShot;
            firearm.ServerSendAudioMessage((byte)firearm.AttachmentsValue(AttachmentParam.ShotClipIdOverride));
            firearm.Owner.networkIdentity.connectionToClient.Send<GunAudioMessage>(new GunAudioMessage(ReferenceHub._hostHub, (byte)firearm.AttachmentsValue(AttachmentParam.ShotClipIdOverride), 10, firearm.Owner));
            firearm.ServerSideAnimator.Play(FirearmAnimatorHashes.Fire);
            return;
        }
    }

    private void _fakeFireAutomatic(Firearm firearm, AutomaticAction automatic)
    {
        if (firearm.Owner.HasBlock(BlockedInteraction.ItemPrimaryAction))
            return;
        if (firearm.Status.Ammo < automatic._ammoConsumption)
            return;
        if (!automatic.ServerCheckFirerate())
            return;
        if (!automatic.ModulesReady)
        {
            firearm.Owner.gameConsoleTransmission.SendToClient(
                $"Shot rejected, ammoManager={firearm.AmmoManagerModule.Standby}, equipperModule={firearm.EquipperModule.Standby}, adsModule={firearm.AdsModule.Standby}",
                "gray");
            return;
        }

        FirearmStatusFlags firearmStatusFlags = firearm.Status.Flags;
        if (Scp3114Mods.Singleton.Config.FakeFiringUsesAmmo)
        {
            if (firearm.Status.Ammo - automatic._ammoConsumption < automatic._ammoConsumption &&
                automatic._boltTravelTime == 0f)
                firearmStatusFlags &= ~FirearmStatusFlags.Chambered;
            firearm.Status = new FirearmStatus((byte)(firearm.Status.Ammo - automatic._ammoConsumption), firearmStatusFlags, firearm.Status.Attachments);
        }

        firearm.ServerSendAudioMessage(automatic.ShotClipId);
            firearm.Owner.networkIdentity.connectionToClient.Send<GunAudioMessage>(new GunAudioMessage(ReferenceHub._hostHub, (byte)automatic.ShotClipId, 10, firearm.Owner));
        firearm.ServerSideAnimator.Play(FirearmAnimatorHashes.Fire);
        return;
    }

    private void _fakeFireDisruptor(Firearm firearm, DisruptorAction disruptor)
    {
        if (!firearm.IsLocalPlayer && disruptor.TimeSinceLastShot < 1.5f)
            return;
        if (!disruptor.ModulesReady)
        {
            firearm.Owner.gameConsoleTransmission.SendToClient(
                $"Shot rejected, ammoManager={firearm.AmmoManagerModule.Standby}, equipperModule={firearm.EquipperModule.Standby}, adsModule={firearm.AdsModule.Standby}",
                "gray");
            return;
        }

        firearm.Status = new FirearmStatus((byte)(firearm.Status.Ammo - 1), firearm.Status.Flags,
            firearm.Status.Attachments);
        firearm.ServerSendAudioMessage(0);
            firearm.Owner.networkIdentity.connectionToClient.Send<GunAudioMessage>(new GunAudioMessage(ReferenceHub._hostHub, (byte)0, 10, firearm.Owner));
        if (!firearm.IsLocalPlayer)
            disruptor._lastShotTime = disruptor.CurTime;
        firearm.ServerSideAnimator.Play(FirearmAnimatorHashes.Fire, 0, disruptor.ShotDelay / 2.2667f);
    }
}
