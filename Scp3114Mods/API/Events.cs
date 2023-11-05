// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         Events.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/01/2023 1:04 PM
//    Created Date:     11/01/2023 1:04 PM
// -----------------------------------------

using PluginAPI.Core;
using Scp3114Mods.API.EventArgs;
using Scp3114Mods.Internal.Patches;

namespace Scp3114Mods.API;

public class Events
{
    #region SafeInvocationNWAPI
    public static void SafeInvoke(Action action)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception e)
        {
            Logging.Warning($"An error has occured while invoking {action?.Method.Name}.");
            Logging.Debug($"Exceptions: \n{e}");
        }
    }
#endregion

    #region Events
    public static event Action<StranglingPlayerArgs> StranglingPlayer;
    public static event Action<StrangleFinishedEventArgs> StrangleFinished;
    public static event Action<SlappingPlayerEventArgs> SlappingPlayer;
    #endregion
    
    #region EventInvocations
    public static void OnPlayerStrangling(StranglingPlayerArgs ev) => StranglingPlayer.Invoke(ev);
    public static void OnStrangleFinished(StrangleFinishedEventArgs ev) => StrangleFinished.Invoke(ev);
    public static void OnSlappingPlayerEventArgs(SlappingPlayerEventArgs ev) => SlappingPlayer.Invoke(ev);
    #endregion
}