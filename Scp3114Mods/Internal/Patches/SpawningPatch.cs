// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         SpawningPatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/01/2023 11:23 AM
//    Created Date:     11/01/2023 11:23 AM
// -----------------------------------------

using PlayerRoles;
using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp3114;

namespace Scp3114Mods.Internal.Patches;
public class Scp3114Spawn : Scp3114Role, ISpawnableScp
{
    public float GetSpawnChance(List<RoleTypeId> alreadySpawned)
    {
        return 1f;
    }
}