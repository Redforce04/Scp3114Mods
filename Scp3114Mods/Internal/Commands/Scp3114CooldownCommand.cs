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
using PluginAPI.Core;
using Scp3114Mods.API;

namespace Scp3114Mods.Internal.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
public class Scp3114CooldownCommand : ICommand, IUsageProvider
{
    public string Command => "3114_cooldown";
    public string[] Aliases => Array.Empty<string>();
    public string Description => "sets the cooldown for various features.";
    public string[] Usage => new[] { "%player%", "Strangle / Slap / Disguise", "Cooldown" };

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
            response = "Usage: 3114_cooldown [Player] [Strangle/Slap/Disguise] [Cooldown]";
            return false;
        }

        if (!float.TryParse(arguments.At(2), out float cooldown))
        {
            response = $"Could not parse cooldown from \"{arguments.At(2)}\".";
            return false;
        }

        switch (arguments.At(1).ToLower())
        {
            case "strangle":
                ply.TriggerStrangleCooldown(cooldown);
                response = $"Set a new strangle cooldown for for player {ply.Nickname}.";
                return true;
            case "disguise":
                ply.TriggerDisguiseCooldown(cooldown);
                response = $"Set a new disguise cooldown for player {ply.Nickname}.";
                return true;
            case "slap":
                ply.TriggerSlapCooldown(cooldown);
                response = $"Set a new slap cooldown for player {ply.Nickname}.";
                return true;
            default:
                response = $"Could not find option \"{arguments.At(1)}\". Available Options: \"Strangle\", \"disguise\", \"slap\".";
                return false;
        }
    }
}
