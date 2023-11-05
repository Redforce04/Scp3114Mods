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
using MEC;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.Ragdolls;
using PluginAPI.Core;
using Scp3114Mods.API;

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
            response = "You must specify a feature.\n" +
                       "AvailableFeatures:\n" +
                       "0 - Debug Output \n" +
                       "1 - Resync \n" +
                       "2 - Force Status Change \n" +
                       "3 - Force _wasDisguised Change \n" +
                       "4 - Quiet Status Change \n" +
                       "5 - Quiet Status Change Equipping \n" +
                       "6 -  \n" +
                       "7 -  \n" +
                       "8 -  \n";
            return false;
        }

        Player? ply = Player.GetPlayers().FirstOrDefault(x => x.Nickname.ToLower().Contains(arguments.At(0).ToLower()));
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
                _getDebuggingInfo(disguise, "Command Debugging Info");
                featureName = "Debug Output";
                break;
            case 1:
                featureName = "Resync";
                disguise._identity.ServerResendIdentity();
                break;
            case 2:
                featureName = "Force Status Change";
                role.Disguised = !role.Disguised;
                break;
            case 3:
                featureName = "Force _wasDisguised Change";
                disguise._identity._wasDisguised = !disguise._identity._wasDisguised;
                break;
            case 4:
                featureName = "Quiet Status Change";
                disguise.ScpRole.CurIdentity._status = Scp3114Identity.DisguiseStatus.None;
                disguise._identity.CurIdentity._status = Scp3114Identity.DisguiseStatus.None;
                break;
            case 5:
                featureName = "Quiet Status Change Equipping";
                disguise.ScpRole.CurIdentity._status = Scp3114Identity.DisguiseStatus.Equipping;
                disguise._identity.CurIdentity._status = Scp3114Identity.DisguiseStatus.Equipping;
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
    public string[] Usage => new[] { "Player", "SyncAll" };

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
}