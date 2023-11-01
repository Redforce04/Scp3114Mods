// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         Scp3114CooldownCommand.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/31/2023 9:35 PM
//    Created Date:     10/31/2023 9:35 PM
// -----------------------------------------

using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using Mirror;
using PluginAPI.Core;
using Scp3114Mods.API;

namespace Scp3114Mods.Internal.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
public class Scp3114DisguiseCommand : ICommand, IUsageProvider
{
    public string Command => "3114_disguise_duration";
    public string[] Aliases => Array.Empty<string>();
    public string Description => "Sets the duration for the disguise.";
    public string[] Usage => new[] { "%player%", "Duration" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckPermission(PlayerPermissions.SetGroup))
        {
            response = "You don't have permission to use this command.";
            return false;
        }
        if (arguments.Count < 1)
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
        if (arguments.Count < 2)
        {
            response = "You must specify the new duration.";
            return false;
        }

        if (!float.TryParse(arguments.At(1), out float disguiseDuration))
        {
            response = $"Could not parse disguise duration from \"{arguments.At(1)}\".";
            return false;
        }

        if (disguiseDuration < 0)
            disguiseDuration = float.MaxValue;
                
        ply.SetDisguiseDuration(disguiseDuration);
        response = $"Set the disguise duration to {disguiseDuration} for player {ply.Nickname}";
        return true;
    }
}
