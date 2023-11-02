// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         Dance.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/30/2023 1:31 PM
//    Created Date:     10/30/2023 1:31 PM
// -----------------------------------------

using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;
using RemoteAdmin;
using Scp3114Mods.API;

namespace Scp3114Mods.Internal.Commands;

[CommandHandler(typeof(ClientCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Dance : ICommand, IUsageProvider
{
    public string Command => "dance";
    public string[] Aliases => new [] { "d" };
    public string Description => "Makes a player dance.";
    public string[] Usage => new[] { "Player*", "Dance Number*" };
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        bool isSelf = true;
        var ply = Player.Get(sender);
        int danceNum = 0;
        if (arguments.Count > 1)
        {
            if (arguments.Count > 1 && !int.TryParse(arguments.At(1), out danceNum))
            {
                response = $"Could not parse dance number from \"{arguments.At(1)}\".";
                return false;
            }
            if (!sender.CheckPermission(PlayerPermissions.GivingItems))
            {
                response = "You don't have permission to do this. (GivingItems).";
                return false;
            }

            ply = Player.GetPlayers().FirstOrDefault(x => x.Nickname.ToLower().Contains(arguments.At(0).ToLower()));
            if (ply is null)
            {
                response = $"Cannot find player \"{arguments.At(0)}\". Make sure to use the actual name of the player (they might be someone else).";
                return false;
            }

            if (ply.Role != RoleTypeId.Scp3114)
            {
                response = $"Player \"{arguments.At(0)}\" is not scp3114! Make sure to use the actual name of the player playing scp3114 (they might be someone else).";
                return false;
            }

            isSelf = false;
        }
        else
        {
            if (arguments.Count > 0 && !int.TryParse(arguments.At(0), out danceNum))
            {
                response = $"Could not parse dance number from \"{arguments.At(0)}\".";
                return false;
            }
        }
        if (ply is null || ply.Role != RoleTypeId.Scp3114)
        {
            response = "You must be Scp3114 to use this command!";
            return false;
        }

        try
        {
            if (ply.RoleBase is not Scp3114Role role)
            {
                Logging.Warning("[Dance] Player was not Scp3114Role!");
                response = $"Could not make {(isSelf ? "you" : $"player {ply.Nickname}")} dance due to an error.";
                return false;
            }

            if (!role.SubroutineModule.TryGetSubroutine<Scp3114Dance>(out var dance) || dance is null)
            {
                Logging.Warning("[Dance] Dance Module was not found!");
                response = $"Could not make {(isSelf ? "you" : $"player {ply.Nickname}")} dance due to an error.";
                return false;
            }

            if (dance.IsDancing)
            {
                response = $"{(isSelf ? "You" : $"Player {ply.Nickname}")} is already dancing.";
                return false;
            }

            dance.DanceVariant = danceNum;
            dance.IsDancing = true;
            dance._serverStartPos = new(ply.Position);
            dance.ServerSendRpc(true);
            response = $"{(isSelf ? "You" : $"Player {ply.Nickname}")} is now dancing.";
        }
        catch (Exception e)
        {
            response = $"Could not make {(isSelf ? "you" : $"player {ply.Nickname}")} dance due to an error.";
            Logging.Warning($"The dance command has caught an error.");
            Logging.Debug($"Exception: \n{e}");
            return false;
        }

        response = $"{(isSelf ? $"You" : $"Player {ply.Nickname}")} is now dancing.";
        return true;
    }

}