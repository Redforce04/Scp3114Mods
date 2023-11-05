// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         3114_Debugging.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/04/2023 5:49 PM
//    Created Date:     11/04/2023 5:49 PM
// -----------------------------------------

using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;

namespace Scp3114Mods.Internal.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Cmd3114Debugging : ICommand, IUsageProvider
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        /*
         * 0 - SendRPC
         * 1 - Resync
         * 2 - ForceRoleIdentity
         * 3 - ForceRoleDisguise
         * 4 - ChangeDisguiseState
         * 5 - ForceCooldownDisguise
         * 6 - ForceClearCooldownDisguise
         * 7 - ForceCooldownIdentity
         * 8 - ForceClearCooldownIdentity
         */
        if (!sender.CheckPermission(PlayerPermissions.SetGroup))
        {
            response = "You do not have permissions to use this command!";
            return false;
        }

        if (arguments.Count < 1)
        {
            response = "You must specify a player.";
            return false;
        }

        if (arguments.Count < 2)
        {
            response = "You must specify a feature.";
            return false;
        }

        Player? ply = Player.GetPlayers().FirstOrDefault(x => x.Nickname.Contains(arguments.At(0)));
        if (ply is null)
        {
            response = $"Could not find player \"{arguments.At(0)}\"";
            return false;
        }

        if (!int.TryParse(arguments.At(1), out int feature))
        {
            response = $"Could not parse feature [int] from \"{arguments.At(1)}\"";
            return false;
        }

        if (ply.RoleBase is not Scp3114Role role)
        {
            response = $"Player {ply.Nickname} is not Scp3114!";
            return false;
        }
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Identity>(out var identity) || identity is null)
        {
            response = $"Player {ply.Nickname} does not have an identity module!";
            return false;
        }
        if (!role.SubroutineModule.TryGetSubroutine<Scp3114Disguise>(out var disguise) || disguise is null)
        {
            response = $"Player {ply.Nickname} does not have an disguise module!";
            return false;
        }

        string featureName = "";
        switch (feature)
        {  
            case 0:
                featureName = "SendRPC";
                break;
            case 1:
                featureName = "Resync";
                break;
            case 2:
                featureName = "ForceRoleIdentity";
                break;
            case 3:
                featureName = "ForceRoleDisguise";
                break;
            case 4:
                featureName = "ChangeDisguiseState";
                break;
            case 5:
                featureName = "ForceCooldownDisguise";
                break;
            case 6:
                featureName = "ForceClearCooldownDisguise";
                break;
            case 7:
                featureName = "ForceCooldownIdentity";
                break;
            case 8:
                featureName = "ForceClearCooldownIdentity";
                break;
            default:
                response = $"Could not find feature {feature}";
                return false;
        }
        response = $"Successfully ran feature {feature} on player {ply.Nickname}.";
        return true;
    }

    public string Command => "3114Debug";
    public string[] Aliases => Array.Empty<string>();
    public string Description => "Allows debugging of Scp 3114 Features.";
    public string[] Usage => new [] { "Player", "SyncAll" };
}