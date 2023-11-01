// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         Scp3114RoleName.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/31/2023 9:38 PM
//    Created Date:     10/31/2023 9:38 PM
// -----------------------------------------

using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;
using Scp3114Mods.API;

namespace Scp3114Mods.Internal.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
public class Scp3114RoleType : ICommand, IUsageProvider
{
    public string Command => "3114_role_type";
    public string[] Aliases => Array.Empty<string>();
    public string Description => "Sets the disguise type for a player who is scp 3114.";
    public string[] Usage => new[] { "%player%", "Role" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        response = "This command is still WIP and disabled.";
        if (!Config.Dbg)
            return false;
        if (!sender.CheckPermission(PlayerPermissions.SetGroup))
        {
            response = "You don't have permission to use this command.";
            return false;
        }
        if (arguments.Count < 1 )
        {
            response = "You must specify the name of the player ";
            return false;
        }
        
        Player? ply = Player.GetPlayers().FirstOrDefault(x => x.Nickname.ToLower().Contains(arguments.At(0).ToLower()));

        if (ply is null)
        {
            response = $"Could not find player \"{arguments.At(0)}\"";
            return false;
        }
        if (arguments.Count < 2 )
        {
            response = "You must specify the new role for the disguise.";
            return false;
        }

        if (!Enum.TryParse<RoleTypeId>(arguments.At(1), true, out RoleTypeId role))
        {
            response = $"Could not find role \"{arguments.At(1)}\".";
            return false;
        }
        return _changeDisguiseRole(ply, role, out response);
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

}