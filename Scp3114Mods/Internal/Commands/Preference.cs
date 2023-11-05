// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         Preference.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/03/2023 10:16 PM
//    Created Date:     11/03/2023 10:16 PM
// -----------------------------------------

using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using PluginAPI.Core;
using UnityEngine;

namespace Scp3114Mods.Internal.Commands;

[CommandHandler(typeof(ClientCommandHandler))]
internal class Preference : ICommand, IUsageProvider
{
    public string Command => "3114preference";
    public string[] Aliases => new[] { "3114pref" };
    public string Description => "Allows you to specify a preference for how often you get Scp3114.";
    public string[] Usage => new[] { "Scp 3114 Preference (0-10)" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        Player ply = Player.Get(sender);
        if (ply is null)
        {
            response = Scp3114Mods.Singleton.Translation.PlayerPreferenceErrorMessage;
            Log.Warning($"An error occured while trying to set the user preference of a player using the command. The player was null. If you believe this is a bug, please contact the developer Redforce04.");
            return false;
        }

        if (ply.DoNotTrack && (!Scp3114Mods.Singleton.Config.AllowExplicitDntOptIn || arguments.Count < 2 || arguments.At(1).ToLower() != "dntconfirm"))
        {
            var pref = PlayerPreferenceManager.Singleton.Get3114Preference(ply);
            pref += 5;
            if (!Scp3114Mods.Singleton.Config.AllowExplicitDntOptIn)
            {
                response = Scp3114Mods.Singleton.Translation.PlayerPreferenceDntDisabled.Replace("{pref}", pref.ToString());
                return false;
            }

            response = Scp3114Mods.Singleton.Translation.PlayerPreferenceCmdDntOptInMessage.Replace("{pref}", pref.ToString());
            return false;
        }
        if (arguments.Count < 1)
        {
            var pref = PlayerPreferenceManager.Singleton.Get3114Preference(ply) + 5;
            response = Scp3114Mods.Singleton.Translation.PlayerPreferenceCurrentPreference.Replace("{pref}", $"{pref}");
            goto usage;
        }

        if (!int.TryParse(arguments.At(0), out int preference))
        {
            response = Scp3114Mods.Singleton.Translation.PlayerPreferenceCouldntDetermineNewPreference.Replace("{arg}", arguments.At(0));
            goto usage;
        }
        preference = Mathf.Clamp(preference + 5, 0, 10);

        // The preference manager stores the value from [-5] - [5], but we do [0] - [10] for simplicity.
        // This is why we subtract 5.
        PlayerPreferenceManager.Singleton.Set3114Preference(ply, preference - 5);
        response = Scp3114Mods.Singleton.Translation.PlayerPreferenceSuccess.Replace("{newPref}", preference.ToString());
        return true;
        usage:
        response += Scp3114Mods.Singleton.Translation.PlayerPreferenceUsage;
        return false;
    }
}
