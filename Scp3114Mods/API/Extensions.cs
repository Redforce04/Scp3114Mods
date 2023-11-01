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

using Mirror;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.Ragdolls;
using PlayerStatsSystem;
using PluginAPI.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scp3114Mods.API;

public static class Extensions
{
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
        if (Config.Dbg) Log.Debug("Ragdoll created");
        return component;
    }
    public static bool SetDisguise(this Player ply, string name, RoleTypeId role) => SetDisguise(ply, role.GetRagdoll(name));
    public static bool SetDisguise(this Player ply, BasicRagdoll ragdoll)
    {
        if (ragdoll is null)
        {
            if (Config.Dbg) Log.Debug("Ragdoll is null.");
            return false;
        }
        if (ply.RoleBase is not Scp3114Role role)
            return false;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
            return false;
        //disguise._identity.RemainingDuration.NextUse = NetworkTime.time - 1;
        //disguise._identity.CurIdentity.Status = Scp3114Identity.DisguiseStatus.Active;
        disguise._identity.CurIdentity.Ragdoll = ragdoll;
        role.Ragdoll = ragdoll;
        disguise._identity.RemainingDuration.NextUse = NetworkTime.time - 1;
        disguise._identity.CurIdentity.Status = Scp3114Identity.DisguiseStatus.Active;
        // disguise._identity.Update();
        disguise._identity.ServerResendIdentity();

        if (Config.Dbg) Log.Debug("Setting disguise");

        return true;
    }

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
        
        if (disguise.CurRagdoll is null)
        {
            var ragdoll = newRole.GetRagdoll(ply.Nickname);
            disguise.CurRagdoll = ragdoll;
            role.Ragdoll = ragdoll;
            disguise._identity.RemainingDuration.NextUse = NetworkTime.time - 1;
            disguise._identity.CurIdentity.Status = Scp3114Identity.DisguiseStatus.Active;
        }
        else
        {
            var data = (disguise.CurRagdoll.NetworkInfo);
            var ragdollData = new RagdollData(data.OwnerHub, data.Handler, newRole, data.StartPosition, data.StartRotation, data.Nickname, data.CreationTime);
            disguise.CurRagdoll.NetworkInfo = ragdollData;
            role.Ragdoll.NetworkInfo = ragdollData;
            disguise.CurRagdoll.Info = ragdollData;
            role.Ragdoll.Info = ragdollData;
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

        if (disguise.CurRagdoll is null)
        {
            var ragdoll = ply.Role.GetRagdoll(name);
            disguise.CurRagdoll = ragdoll;
            role.Ragdoll = ragdoll;
            disguise._identity.RemainingDuration.NextUse = NetworkTime.time - 1;
            disguise._identity.CurIdentity.Status = Scp3114Identity.DisguiseStatus.Active;
        }
        else
        {
            var data = (disguise.CurRagdoll.NetworkInfo);
            var info = new RagdollData(data.OwnerHub, data.Handler, data.RoleType, data.StartPosition, data.StartRotation, name, data.CreationTime);
            disguise.CurRagdoll.NetworkInfo = info;
            role.Ragdoll.NetworkInfo = info;
            disguise.CurRagdoll.Info = info;
            role.Ragdoll.Info = info;
        }
        
        disguise._identity.ServerResendIdentity();
        return true;
    }

    public static bool SetDisguiseDuration(this Player ply, float duration)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return false;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
            return false;
        disguise.Cooldown.Trigger(duration);
        return true;
    }
    public static bool SetDisguisePermanentDuration(this Player ply, float duration)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return false;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Identity>(out var identity) || identity is null)
            return false;
        if (Config.Dbg) Log.Debug("Setting Scp3114 Permanent Disguise");
        identity._disguiseDurationSeconds = duration;
        return true;
    }

    public static void TriggerDisguiseRemoval(this Player ply)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
            return;
        if (Config.Dbg) Log.Debug("Removing Scp 3114 Disguise");
        disguise._identity.OnIdentityStatusChanged();
    }

    public static void TriggerDisguiseCooldown(this Player ply, float cooldown)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
            return;
        disguise.Cooldown.Trigger(cooldown);
        if (Config.Dbg) Log.Debug("Triggering Cooldown for Disguise");
    }
    public static void TriggerStrangleCooldown(this Player ply, float cooldown)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Strangle>(out var strangle) || strangle is null)
            return;
        strangle.Cooldown.Trigger(cooldown);
        if (Config.Dbg) Log.Debug("Triggering Cooldown for Strangle");
    }
    public static void TriggerSlapCooldown(this Player ply, float cooldown)
    {
        if (ply.RoleBase is not Scp3114Role role)
            return;
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Slap>(out var slap) || slap is null)
            return;
        slap.Cooldown.Trigger(cooldown);
        if (Config.Dbg) Log.Debug("Triggering Cooldown for Slap");
    }
    
    
}
