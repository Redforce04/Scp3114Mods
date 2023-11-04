// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         PlayerPreferencePostfix.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/04/2023 1:17 AM
//    Created Date:     11/04/2023 1:17 AM
// -----------------------------------------

using System.Diagnostics;
using HarmonyLib;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using Scp3114Mods.API;

namespace Scp3114Mods.Internal.Patches.DebuggingPatches;

#if false
[HarmonyPatch(typeof(ScpSpawner), nameof(ScpSpawner.GetPreferenceOfPlayer))]
internal static class PlayerPreferencePostfix
{
    private static void Postfix(ref int __result, ReferenceHub ply, RoleTypeId scp)
    {
        Logging.Debug($"[Pref: {ply.nicknameSync._firstNickname}@{scp} ({__result})]");
        return;
    }
}
#endif