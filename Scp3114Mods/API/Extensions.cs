// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         Extensions.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/31/2023 3:20 PM
//    Created Date:     10/31/2023 3:20 PM
// -----------------------------------------

using InventorySystem.Items.Firearms;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.Ragdolls;
using PlayerStatsSystem;
using PluginAPI.Core;
using RelativePositioning;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scp3114Mods.API;

public static class Extensions
{
    /// <summary>
    /// Checks to see if a player is "innocent".
    /// </summary>
    /// <param name="ply">The <see cref="Player"/> to check.</param>
    /// <returns>True if the player is innocent, False if the player is not innocent.</returns>
    public static bool IsPlayerInnocent(this Player ply)
    {

        if (ply.Role != RoleTypeId.Scientist && ply.Role != RoleTypeId.ClassD)
        {
            Logging.Debug($"IsPlayerInnocent {ply.Nickname} - false [role]");
            return false;
        }
        if (ply.Items.Any(x =>
            {
                if (Scp3114Mods.Singleton.Config.CandyLosesInnocence && x.ItemTypeId == ItemType.SCP330)
                {
                    Logging.Debug($"IsPlayerInnocent {ply.Nickname} - false [Candy]");
                    return true;
                }
                
                return Scp3114Mods.Singleton.Config.StrangleNonInnocentItems.Contains(x.ItemTypeId);
                // obsolete - makes it more configurable 
                if (x is Firearm)
                {
                    Logging.Debug($"IsPlayerInnocent {ply.Nickname} - false [Firearm]");
                    return true;
                }

                if (x.ItemTypeId is ItemType.GrenadeFlash or ItemType.GrenadeHE or ItemType.SCP018)
                {
                    Logging.Debug($"IsPlayerInnocent {ply.Nickname} - false [Throwable]");
                    return true;
                }

			    
                return false;
            }))
        {
            Logging.Debug($"IsPlayerInnocent {ply.Nickname} - false [items]");
            return false;
        }
        Logging.Debug($"IsPlayerInnocent {ply.Nickname} - true");
        return true;
    }
    
    public static BasicRagdoll GetRagdoll(this RoleTypeId role, string name, bool spawn = true, Vector3? pos = null, Vector3? rot = null, ReferenceHub? hub = null)
    {
        if (!PlayerRoleLoader.TryGetRoleTemplate(role, out PlayerRoleBase rolebase) || rolebase is not IRagdollRole ragdoll)
            return null;
        
        GameObject gameObject = Object.Instantiate(ragdoll.Ragdoll.gameObject, pos ?? Vector3.zero,  Quaternion.Euler(rot ?? Vector3.zero));
        if (gameObject.TryGetComponent(out BasicRagdoll component))
        {
            component.NetworkInfo = new RagdollData(hub, new UniversalDamageHandler(0.0f, DeathTranslations.Unknown), role, pos ?? Vector3.zero , Quaternion.Euler(rot ?? Vector3.zero), name, NetworkTime.time);
        }

        if(spawn)
            NetworkServer.Spawn(gameObject);
        Logging.Debug("Ragdoll created");
        return component;
    }
    public static bool SetDisguise(this Player ply, string name, RoleTypeId role, bool shouldUpdateCooldown = false) => SetDisguise(ply, role.GetRagdoll(name), shouldUpdateCooldown);
    public static bool SetDisguise(this Player ply, BasicRagdoll ragdoll, bool shouldUpdateCooldown = false)
    {
        if (ragdoll is null)
        {
            Logging.Debug("Ragdoll is null.");
            return false;
        }
        if (ply.RoleBase is not Scp3114Role role)
            return false;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
            return false;

        role.Ragdoll = ragdoll;

        // Update Disguise Component
        disguise.CurRagdoll = ragdoll;
        
        // Update Identity Component
        // These 2 will change the disguise duration.
        if (shouldUpdateCooldown)
        {
            float duration = Scp3114Mods.Singleton.Config.DisguiseDuration;
            if (duration == 0)
                duration = disguise._identity._disguiseDurationSeconds;

            if (duration < 0)
            {
                duration = float.MaxValue;
                disguise._identity.RemainingDuration.Remaining = float.MaxValue;
            }
            
            disguise._identity.RemainingDuration.Trigger(duration);
            // Resets it - disguise._identity.RemainingDuration.NextUse = NetworkTime.time - 1;
        } 
        disguise._identity._wasDisguised = true;
        disguise._identity.CurIdentity.Ragdoll = ragdoll;
        disguise.ScpRole.Disguised = true;
        // disguise._identity.ServerResendIdentity();
        // disguise._identity.OnIdentityStatusChanged();

        _getDebuggingInfo(disguise, $"Set Disguise");
        return true;
    }

    private static void _getDebuggingInfo(Scp3114Disguise disguise, string prefix)
    {
        Timing.CallDelayed(1f, () =>
        {
            Logging.Debug($"{prefix}\n=============================" +
                          $"\nIdentity Stolen Role: {disguise._identity.CurIdentity.StolenRole}" +
                          $"\n3114 Stolen Role: {disguise.ScpRole.CurIdentity.StolenRole}" +
                          $"\nIdentity Status: {disguise._identity.CurIdentity._status}" +
                          $"\n3114 Identity Status: {disguise.ScpRole.CurIdentity._status}" +
                          $"\n3114 Disguised: {disguise.ScpRole.Disguised}" +
                          $"\nIdentity Was Disguised: {disguise._identity._wasDisguised}" +
                          "\n-----------------------------" +
                          $"\nDisguise Info: {_getRagdollInfo(disguise!.CurRagdoll.Info)}" +
                          $"\nDisguise Network Info: {_getRagdollInfo(disguise!.CurRagdoll.NetworkInfo)}" +
                          $"\nIdentity Info: {_getRagdollInfo(disguise._identity.CurIdentity.Ragdoll.Info)}" +
                          $"\nIdentity Network Info: {_getRagdollInfo(disguise._identity!.CurIdentity.Ragdoll.NetworkInfo)}" +
                          $"\n3114 Info: {_getRagdollInfo(disguise.ScpRole.CurIdentity.Ragdoll.Info)}" +
                          $"\n3114 Network Info: {_getRagdollInfo(disguise.ScpRole.CurIdentity.Ragdoll.NetworkInfo)}" +
                          $"\n=============================");
        });
    }
    private static string _getRagdollInfo(RagdollData data)
    {
        return $"[Nick: {data.Nickname}, Role: {data.RoleType}]";
    }
    // Untested
    public static bool SetDisguiseUnitId(this Player ply, byte unitId)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return false;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
            return false;
        
        disguise._identity.CurIdentity.UnitNameId = unitId;
        disguise._identity.ServerResendIdentity();
        return true;
    }
    
    public static bool SetDisguiseRole(this Player ply, RoleTypeId newRole)
    {
        
        if (ply.RoleBase is not Scp3114Role role)
            return false;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
            return false;

        //SetDisguise(ply, role._identity.CurIdentity?.Ragdoll.Info.Nickname ?? ply.Nickname, newRole);
        //return true;

        if (disguise.CurRagdoll is null || disguise._identity.CurIdentity.Status == Scp3114Identity.DisguiseStatus.None)
        {
            return ply.SetDisguise(ply.Nickname, newRole, false);
        }

        if (newRole is RoleTypeId.None or RoleTypeId.Spectator or RoleTypeId.Scp3114 or RoleTypeId.Overwatch)
        {
            role.Disguised = false;
            return true;
        }
        
        // This Doesnt Work!
        
        var data = (disguise.CurRagdoll.NetworkInfo);
        var ragdollData = new RagdollData(data.OwnerHub, data.Handler, newRole, data.StartPosition, data.StartRotation, data.Nickname, data.CreationTime);
        
        // Change Role Ragdoll
        role.Ragdoll.NetworkInfo = ragdollData;
        role.Ragdoll.Info = ragdollData;
        
        // Change Disguise Ragdoll
        disguise.CurRagdoll.NetworkInfo = ragdollData;
        disguise.CurRagdoll.Info = ragdollData;
        
        // Change Identity Ragdoll
        disguise._identity.CurIdentity.Ragdoll.Info = ragdollData;
        disguise._identity.CurIdentity.Ragdoll.NetworkInfo = ragdollData;
            
        disguise._identity.ServerResendIdentity();
        _getDebuggingInfo(disguise, "Set Role");

        return true;
    }

    public static bool SetDisguiseName(this Player ply, string name)
    {
        if (ply.RoleBase is not Scp3114Role role || !role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
        {
            return false;
        }
        
        if (disguise.CurRagdoll is null || disguise._identity.CurIdentity.Status == Scp3114Identity.DisguiseStatus.None || !role.Disguised)
        {
            // we may want to return false here, because they should be creating a new ragdoll - who knows.
            return SetDisguise(ply, name, (UnityEngine.Random.Range(0, 4) == 1 ? RoleTypeId.Scientist : RoleTypeId.ClassD));
        }
        
        // This Confirmed Works!
        var data = (disguise.CurRagdoll.NetworkInfo);
        var info = new RagdollData(data.OwnerHub, data.Handler, data.RoleType, data.StartPosition, data.StartRotation, name, data.CreationTime);
        
        // Change Role Ragdoll
        role.Ragdoll.NetworkInfo = info;
        role.Ragdoll.Info = info;
        
        // Change Disguise Ragdoll
        disguise.CurRagdoll.NetworkInfo = info;
        disguise.CurRagdoll.Info = info;
        
        // Change Identity Ragdoll
        disguise._identity.CurIdentity.Ragdoll.Info = info;
        disguise._identity.CurIdentity.Ragdoll.NetworkInfo = info;
            
        disguise._identity.ServerResendIdentity();
        _getDebuggingInfo(disguise, "Set Disguise Name");

        return true;
    }

    public static bool SetDisguiseDuration(this Player ply, float duration)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return false;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
            return false;
        
        disguise._identity.RemainingDuration.Remaining = duration;
        //disguise._identity.RemainingDuration.Trigger(duration);
        return true;
    }

    public static void TriggerDisguiseRemoval(this Player ply)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
            return;
        role.Disguised = false;
        Logging.Debug("Removing Scp 3114 Disguise");
        disguise._identity.OnIdentityStatusChanged();
    }

    public static void TriggerDisguiseCooldown(this Player ply, float cooldown)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
            return;
        disguise.Cooldown.Clear();
        disguise.Cooldown.Trigger(cooldown);
        Logging.Debug("Triggering Cooldown for Disguise");
    }
    public static void TriggerStrangleCooldown(this Player ply, float cooldown)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Strangle>(out var strangle) || strangle is null)
            return;
        strangle.Cooldown.Clear();
        strangle.Cooldown.Trigger(cooldown);
        Logging.Debug("Triggering Cooldown for Strangle");
    }
    public static void TriggerSlapCooldown(this Player ply, float cooldown)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Slap>(out var slap) || slap is null)
            return;
        slap.Cooldown.Clear();
        slap.Cooldown.Trigger(cooldown);
        Logging.Debug("Triggering Cooldown for Slap");
    }

    /// <summary>
    /// Plays a gun sound that only the <paramref name="player" /> can hear.
    /// </summary>
    /// <param name="player">Target to play.</param>
    /// <param name="position">Position to play on.</param>
    /// <param name="itemType">Weapon' sound to play.</param>
    /// <param name="volume">Sound's volume to set.</param>
    /// <param name="audioClipId">GunAudioMessage's audioClipId to set (default = 0).</param>
    public static void PlayGunSound(
        this Player player,
        ItemType itemType,
        byte volume,
        Vector3? position = null,
        byte audioClipId = 0)
    {
        GunAudioMessage message = new GunAudioMessage()
        {
            Weapon = itemType,
            AudioClipId = audioClipId,
            MaxDistance = volume,
            ShooterHub = player.ReferenceHub,
            ShooterPosition = new RelativePosition(position ?? player.Position)
        };
        player.Connection.Send<GunAudioMessage>(message);
    }
    
}
