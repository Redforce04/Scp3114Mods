// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         Command.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/29/2023 8:52 PM
//    Created Date:     10/29/2023 8:52 PM
// -----------------------------------------


using CommandSystem;
using PluginAPI.Core;

namespace Scp3114Mods.Internal.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
internal class Better3114Command : ICommand, IUsageProvider
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission(PlayerPermissions.FacilityManagement))
        {
            response = "You need the Facility Management permission to run this command.";
            return false;
        }
        if (arguments.Count <= 0)
        {
            response = "You must specify Enable, Disable or Reload for this command.";
            return false;
        }

        if (arguments.At(0).ToLower() is "true" or "enable" or "1" or "enabled")
        {
            if (Scp3114Mods.Singleton.EventsRegistered)
            {
                response = "Features were already enabled";
                return false;
            }

            Scp3114Mods.Singleton.UnregisterEvents();
            response = "Features have been enabled";
            return true;
        }
        
        if (arguments.At(0).ToLower() is "false" or "disable" or "disabled" or "0")
        {
            if (!Scp3114Mods.Singleton.EventsRegistered)
            {
                response = "Features were already disabled";
                return false;
            }

            Scp3114Mods.Singleton.RegisterEvents();
            response = "Features have been disabled";
            return true;
        }
        if(arguments.At(0).ToLower() is "reload" or "reloadconfigs" or "rc")
        {
            PluginHandler.Get(Scp3114Mods.Singleton).ReloadConfig(Scp3114Mods.Singleton);
            response = "Reloading configs.";
            return true;
        }

        response = $"Could not find option \"{arguments.At(0)}\"";
        return false;
    }

    public string Command => "Better3114";
    public string[] Aliases => Array.Empty<string>();
    public string Description => "Enables or disables better 3114 functionality.";
    public string[] Usage => new[] { "Enabled / Disable / Reload" };
}