// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         FakeFiringExtensions.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/01/2023 1:13 PM
//    Created Date:     11/01/2023 1:13 PM
// -----------------------------------------

using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using PluginAPI.Core;
using UnityEngine;

namespace Scp3114Mods.API;

public static class FakeFiringExtensions
{
    public static void FakeFirePump(this Firearm firearm)
    {
        if (firearm.ActionModule is not PumpAction pump)
            return;
        
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

            if (Scp3114Mods.Singleton.Config.FakeFiringUsesAmmo)
            {
                firearm.Status = new FirearmStatus((byte)(firearm.Status.Ammo - 1), firearm.Status.Flags, firearm.Status.Attachments);
            }
                
            firearm.PlayGunshotForOwner((byte)(pump.ShotSoundId + pump.ChamberedRounds));
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
    public static void FakeFireDoubleAction(this Firearm firearm)
    {
        if (firearm.ActionModule is not DoubleAction doubleA)
            return;
        
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
            
            firearm.PlayGunshotForOwner((byte)firearm.AttachmentsValue(AttachmentParam.ShotClipIdOverride));
            firearm.ServerSendAudioMessage((byte)firearm.AttachmentsValue(AttachmentParam.ShotClipIdOverride));
            firearm.ServerSideAnimator.Play(FirearmAnimatorHashes.Fire);
            return;
        }
    }

    public static void FakeFireAutomatic(this Firearm firearm)
    {
        if (firearm.ActionModule is not AutomaticAction automatic)
            return;
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

        firearm.PlayGunshotForOwner();
        firearm.ServerSendAudioMessage(automatic.ShotClipId);
        firearm.ServerSideAnimator.Play(FirearmAnimatorHashes.Fire);
        return;
    }

    public static void FakeFireDisruptor(this Firearm firearm)
    {
        if (firearm.ActionModule is not DisruptorAction disruptor)
            return;
        if (!firearm.IsLocalPlayer && disruptor.TimeSinceLastShot < 1.5f)
            return;
        if (!disruptor.ModulesReady)
        {
            firearm.Owner.gameConsoleTransmission.SendToClient(
                $"Shot rejected, ammoManager={firearm.AmmoManagerModule.Standby}, equipperModule={firearm.EquipperModule.Standby}, adsModule={firearm.AdsModule.Standby}",
                "gray");
            return;
        }

        if (Scp3114Mods.Singleton.Config.FakeFiringUsesAmmo)
        {
            firearm.Status = new FirearmStatus((byte)(firearm.Status.Ammo - 1), firearm.Status.Flags, firearm.Status.Attachments);
        }
        firearm.ServerSendAudioMessage(0);
        firearm.PlayGunshotForOwner();
        if (!firearm.IsLocalPlayer)
            disruptor._lastShotTime = disruptor.CurTime;
        firearm.ServerSideAnimator.Play(FirearmAnimatorHashes.Fire, 0, disruptor.ShotDelay / 2.2667f);
    }

    public static void PlayGunshotForOwner(this Firearm firearm, byte clipId = 0)
    {
        float num1 = firearm.AudioClips[clipId].HasFlag(FirearmAudioFlags.ScaleDistance) ? firearm.AudioClips[clipId].MaxDistance * firearm.AttachmentsValue(AttachmentParam.GunshotLoudnessMultiplier) : firearm.AudioClips[clipId].MaxDistance;
        if (firearm.AudioClips[clipId].HasFlag(FirearmAudioFlags.IsGunshot) && (double) firearm.Owner.transform.position.y > 900.0)
            num1 *= 2.3f;
        float num2 = num1 * num1;
        Player ply = Player.Get(firearm.Owner);
        ply.PlayGunSound(firearm.ItemTypeId, (byte)(num2/255));
    }
}