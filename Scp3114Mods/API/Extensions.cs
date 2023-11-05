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
    public static bool SetDisguise(this Player ply, string name, RoleTypeId role) => SetDisguise(ply, role.GetRagdoll(name));
    public static bool SetDisguise(this Player ply, BasicRagdoll ragdoll)
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

        disguise._identity.CurIdentity.Ragdoll = ragdoll;
        role.Ragdoll = ragdoll;
        disguise._identity.RemainingDuration.NextUse = NetworkTime.time - 1;
        disguise._identity.CurIdentity.Status = Scp3114Identity.DisguiseStatus.Active;
        disguise._identity.ServerResendIdentity();
        //disguise._identity.RemainingDuration.Trigger(Scp3114Mods.Singleton.Config.DisguiseDuration );
        disguise._identity._wasDisguised = true;
        
        disguise._identity.OnIdentityStatusChanged();

        
        Logging.Debug("Setting disguise");

        return true;
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
    
    // This only kinda works...
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
            // try this but with a twist????????
            Logging.Debug("Creating new ragdoll.");
            var ragdoll = newRole.GetRagdoll(ply.Nickname);
            disguise.CurRagdoll = ragdoll;
            role.Ragdoll = ragdoll;
            disguise._identity.RemainingDuration.NextUse = NetworkTime.time - 1;
            disguise._identity.CurIdentity.Status = Scp3114Identity.DisguiseStatus.Active;
            disguise._identity.OnIdentityStatusChanged();
        }
        else
        {
            disguise._identity.CurIdentity.Status = Scp3114Identity.DisguiseStatus.None;

            Logging.Debug("Reusing ragdoll.");
            var data = (disguise.CurRagdoll.NetworkInfo);
            var ragdollData = new RagdollData(data.OwnerHub, data.Handler, newRole, data.StartPosition, data.StartRotation, data.Nickname, data.CreationTime);
            disguise.CurRagdoll.NetworkInfo = ragdollData;
            role.Ragdoll.NetworkInfo = ragdollData;
            disguise.CurRagdoll.Info = ragdollData;
            role.Ragdoll.Info = ragdollData;
            disguise._identity.CurIdentity.Status = Scp3114Identity.DisguiseStatus.Active;
        }
        
        disguise._identity.ServerResendIdentity();
        return true;
    }

    public static bool SetDisguiseName(this Player ply, string name)
    {
        if (ply.RoleBase is not Scp3114Role role ||
            !role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
        {
            return false;
        }
        SetDisguise(ply, name, role._identity.CurIdentity?.Ragdoll.Info.RoleType ?? (UnityEngine.Random.Range(0, 4) == 1 ? RoleTypeId.Scientist : RoleTypeId.ClassD));
        return true;
        //if (disguise.CurRagdoll is null || disguise._identity.CurIdentity.Status == Scp3114Identity.DisguiseStatus.None || !role.Disguised)
        //{
            Logging.Debug("Creating new ragdoll.");
            var ragdoll = ply.Role.GetRagdoll(name);
            disguise.CurRagdoll = ragdoll;
            role.Ragdoll = ragdoll;
            disguise._identity.RemainingDuration.NextUse = NetworkTime.time - 1;
            disguise._identity.CurIdentity.Status = Scp3114Identity.DisguiseStatus.Active;
        /*}
        else
        {
            Logging.Debug("Reusing ragdoll.");
            var data = (disguise.CurRagdoll.NetworkInfo);
            var info = new RagdollData(data.OwnerHub, data.Handler, data.RoleType, data.StartPosition, data.StartRotation, name, data.CreationTime);
            disguise.CurRagdoll.NetworkInfo = info;
            role.Ragdoll.NetworkInfo = info;
            disguise.CurRagdoll.Info = info;
            role.Ragdoll.Info = info;
            disguise._identity.OnIdentityStatusChanged();
        }*/
        
        disguise._identity.ServerResendIdentity();
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
    public static bool SetDisguisePermanentDuration(this Player ply, float duration)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return false;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Identity>(out var identity) || identity is null)
            return false;
        Logging.Debug("Setting Scp3114 Permanent Disguise");
        identity._disguiseDurationSeconds = duration;
        identity.ServerResendIdentity();
        return true;
    }

    public static void TriggerDisguiseRemoval(this Player ply)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
            return;
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
