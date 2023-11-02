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
public class TalkCommand : ICommand, IUsageProvider
{
    public string Command => "talk";
    public string[] Aliases => new [] { "t", "say" };
    public string Description => "Makes a player say a voiceline.";
    public string[] Usage => new[] { "Player*", "Voiceline Number*" };
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        bool isSelf = true;
        var ply = Player.Get(sender);
        int voiceNum = 0;
        Scp3114VoiceLines.VoiceLinesName voiceLine = Scp3114VoiceLines.VoiceLinesName.RandomIdle;
        if (arguments.Count > 1)
        {
            if (!int.TryParse(arguments.At(1), out voiceNum))
            {
                if (!Enum.TryParse<Scp3114VoiceLines.VoiceLinesName>(arguments.At(1), true, out voiceLine))
                {
                    response = $"Could not parse voice line from \"{arguments.At(1)}\".";
                    return false;
                }
            }
            else
                voiceLine = (Scp3114VoiceLines.VoiceLinesName)voiceNum;
            
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
            if (arguments.Count > 0)
            {
                if (!int.TryParse(arguments.At(0), out voiceNum))
                {
                    if (!Enum.TryParse<Scp3114VoiceLines.VoiceLinesName>(arguments.At(0), true, out voiceLine))
                    {
                        response = $"Could not parse voice line from \"{arguments.At(0)}\".";
                        return false;
                    }
                }
                else
                    voiceLine = (Scp3114VoiceLines.VoiceLinesName)voiceNum;
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
                Logging.Warning("[Talk] Player was not Scp3114Role!");
                response = $"Could not make {(isSelf ? "you" : $"player {ply.Nickname}")} talk due to an error.";
                return false;
            }

            if (!role.SubroutineModule.TryGetSubroutine<Scp3114VoiceLines>(out var voice) || voice is null)
            {
                Logging.Warning("[Talk] Talk Module was not found!");
                response = $"Could not make {(isSelf ? "you" : $"player {ply.Nickname}")} talk due to an error.";
                return false;
            }
            
            if (voice._voiceLines.Length < voiceNum)
            {
                response = "Could not find that voiceline.";
                return false;
            }
            voice.ServerPlayConditionally(voiceLine);
            response = $"{(isSelf ? "You" : $"Player {ply.Nickname}")} is now talking.";
        }
        catch (Exception e)
        {
            response = $"Could not make {(isSelf ? "you" : $"player {ply.Nickname}")} talk due to an error.";
            Logging.Warning($"The talk command has caught an error.");
            Logging.Debug($"Exception: \n{e}");
            return false;
        }

        response = $"{(isSelf ? $"You" : $"Player {ply.Nickname}")} is now talking.";
        return true;
    }

}