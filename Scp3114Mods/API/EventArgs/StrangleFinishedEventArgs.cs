// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         StrangleCompleteEventArgs.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/02/2023 2:56 PM
//    Created Date:     11/02/2023 2:56 PM
// -----------------------------------------

using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;

namespace Scp3114Mods.API.EventArgs;

public class StrangleFinishedEventArgs
{
    public StrangleFinishedEventArgs(Scp3114Strangle instance, Scp3114Strangle.StrangleTarget oldTarget, float cooldown)
    {
        Attacker = Player.Get(instance.Owner);
        OldTarget = oldTarget;
        StrangleCooldown = cooldown;
    }
    public Player Attacker { get; set; }
    public Scp3114Strangle.StrangleTarget OldTarget { get; }
    public float StrangleCooldown { get; set; }
}