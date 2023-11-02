// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         StrangleProcessCommandPatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/01/2023 8:54 PM
//    Created Date:     11/01/2023 8:54 PM
// -----------------------------------------

using Mirror;
using PlayerRoles.PlayableScps.Scp3114;

namespace Scp3114Mods.Internal.Patches;

internal static class StrangleProcessCommandPatch
{
    private static bool Prefix(Scp3114Strangle __instance, NetworkReader reader)
    {
        //base.ServerProcessCmd(reader);
        Scp3114Strangle.StrangleTarget? nullable = __instance.ProcessAttackRequest(reader);
        bool hasValue = nullable.HasValue;
        if (hasValue != __instance.SyncTarget.HasValue)
        {
            if (hasValue)
            {
                return true;
            }
            else
            {
                if (Scp3114Mods.Singleton.Config.StranglePartialCooldown <= 0)
                    return false;
                
                /*
                 * Timing.CallDelayed(Scp3114Mods.Singleton.Config.StranglePartialCooldown, () => {
                 *      if(__instance is not null) __instance.Cooldown.Clear();
                 * });
                 */
                __instance.Cooldown.Trigger((double) Scp3114Mods.Singleton.Config.StranglePartialCooldown);
                return false;
            }
        }
        __instance.SyncTarget = nullable;
        __instance.ServerSendRpc(true);
        return false;
    }
}