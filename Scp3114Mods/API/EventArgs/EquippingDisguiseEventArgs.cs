// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         EquippingDisguiseEventArgs.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/03/2023 6:44 PM
//    Created Date:     11/03/2023 6:44 PM
// -----------------------------------------

using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.Ragdolls;
using PluginAPI.Core;

namespace Scp3114Mods.API.EventArgs;

public class EquippingDisguiseEventArgs
{
    public EquippingDisguiseEventArgs()
    {
        
    }
    public Player Player { get; }
    public Scp3114Disguise Disguise { get; }
    public BasicRagdoll Ragdoll { get; }
    public bool IsAllowed { get; set; }
}