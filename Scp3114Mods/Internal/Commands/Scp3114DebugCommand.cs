// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         3114debug.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/31/2023 7:53 PM
//    Created Date:     10/31/2023 7:53 PM
// -----------------------------------------

using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using Mirror;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.Ragdolls;
using PlayerStatsSystem;
using PluginAPI.Core;
using Scp3114Mods.API;

namespace Scp3114Mods.Internal.Commands;


[CommandHandler(typeof(ClientCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Scp3114DebugCommand : ICommand
{
    public string Command => "3114debug";
    public string[] Aliases => Array.Empty<string>();
    public string Description => "Provides debug functions for Scp 3114";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckPermission(PlayerPermissions.SetGroup))
        {
            response = "You don't have permission to use this command.";
            return false;
        }

        if (arguments.Count < 1)
        {
            response = "You must specify a subcommand to use.";
            return false;
        }

        if (arguments.Count < 1 + 1)
        {
            response = "You must specify the name of the player ";
            return false;
        }

        Player? ply = Player.GetPlayers().FirstOrDefault(x => x.Nickname.ToLower().Contains(arguments.At(1).ToLower()));

        if (ply is null)
        {
            response = $"Could not find player \"{arguments.At(1)}\"";
            return false;
        }
        switch (arguments.At(0).ToLower())
        {
            case "setname" or "name":
                if (arguments.Count < 2 + 1)
                {
                    response = "You must specify the new name of the disguise.";
                    return false;
                }

                return _changeDisguiseName(ply, arguments.At(1), out response);
            case "setrole" or "role":
                if (arguments.Count < 2 + 1)
                {
                    response = "You must specify the new role for the disguise.";
                    return false;
                }

                if (!Enum.TryParse<RoleTypeId>(arguments.At(2), true, out RoleTypeId role))
                {
                    response = $"Could not find role \"{arguments.At(2)}\".";
                    return false;
                }
                return _changeDisguiseRole(ply, role, out response);
            case "unitid" or "unit" or "id":
                if (arguments.Count < 2 + 1)
                {
                    response = "You must specify the new role for the disguise.";
                    return false;
                }

                if (!byte.TryParse(arguments.At(2), out byte unitId))
                {
                    response = $"Could not find unit id (byte) \"{arguments.At(2)}\".";
                    return false;
                }

                ply.SetDisguiseUnitId(unitId);
                response = $"Successfully set the unit id to {unitId} for player {ply.Nickname}";
                return true;
            case "cooldown":
                if (arguments.Count < 2 + 2)
                {
                    response = "You must specify the new duration and type of cooldown for the cooldown.";
                    return false;
                }

                if (!float.TryParse(arguments.At(3), out float cooldown))
                {
                    response = $"Could not parse cooldown from \"{arguments.At(3)}\".";
                    return false;
                }

                switch (arguments.At(2).ToLower())
                {
                    case "strangle":
                        ply.TriggerStrangleCooldown(cooldown);
                        break;
                    case "disguise":
                        ply.TriggerDisguiseCooldown(cooldown);
                        break;
                    case "slap":
                        ply.TriggerSlapCooldown(cooldown);
                        break;
                }
                response = "Set a new cooldown.";
                return true;
            case "duration" or "disguise":
                if (arguments.Count < 2 + 1)
                {
                    response = "You must specify the new duration.";
                    return false;
                }

                if (!float.TryParse(arguments.At(2), out float disguiseDuration))
                {
                    response = $"Could not parse disguise duration from \"{arguments.At(2)}\".";
                    return false;
                }

                if (disguiseDuration < 0)
                    disguiseDuration = float.MaxValue;
                
                ply.SetDisguiseDuration(disguiseDuration);
                response = $"Set the disguise duration to {disguiseDuration} for player {ply.Nickname}";
                return true;
        }

        response = $"Could not find the command \"{arguments.At(0)}\".";
        return false;
    }

    private static bool _changeDisguiseRole(Player ply, RoleTypeId newRole, out string response)
    {
        if (ply.RoleBase is not Scp3114Role role ||
            !role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
        {
            response = "Player is not Scp 3114.";
            return false;
        }

        
        response = "Role changed.";
        return ply.SetDisguiseRole(newRole);
    }
    private static bool _changeDisguiseName(Player ply, string newName, out string response)
    {
        if (ply.RoleBase is not Scp3114Role role ||
            !role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
        {
            response = "Player is not Scp 3114.";
            return false;
        }

        response = "Name changed.";
        return ply.SetDisguiseName(newName);
    }
}
