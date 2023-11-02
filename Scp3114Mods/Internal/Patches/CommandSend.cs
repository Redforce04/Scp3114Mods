// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         CommandSend.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/01/2023 5:51 PM
//    Created Date:     11/01/2023 5:51 PM
// -----------------------------------------

using HarmonyLib;
using Mirror;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.PlayableScps.Subroutines;
using PluginAPI.Core;
using Utils.Networking;

namespace Scp3114Mods.Internal.Patches;

// Only for debugging
[HarmonyPatch(typeof(SubroutineMessage), nameof(SubroutineMessage.Write))]
internal class CommandSend
{
    private static bool Prefix(SubroutineMessage __instance, NetworkWriter writer)
    {
        if (!Config.Dbg)
            return true;
        
        if (__instance._subroutine is not Scp3114Identity)
        {
            return true;
        }

        string msg = "";
        msg += $" [0.byte {(byte)__instance._subroutineIndex}]";
        writer.WriteByte((byte)__instance._subroutineIndex);
        if (__instance._subroutineIndex != 0)
        {
            msg += $" [1.refhub {__instance._target.nicknameSync._firstNickname}]";
            writer.WriteReferenceHub(__instance._target);
            msg += $" [2.role {__instance._role}]";
            writer.WriteRoleType(__instance._role);
            NetworkWriterPooled val = NetworkWriterPool.Get();
            if (__instance._isConfirmation.GetValueOrDefault())
            {
                msg += $" [3. Writing RPC]";
                __instance._subroutine.ServerWriteRpc((NetworkWriter)(object)val);
            }
            else
            {
                msg += $" [4. Writing Cmd]";
                __instance._subroutine.ClientWriteCmd((NetworkWriter)(object)val);
            }
            int num = ((NetworkWriter)(object)val).Position;
            if (num > 65790)
            {
                num = 0;
            }
            msg += $" [5.byte {(byte)Math.Min(num, 255)}]";
            writer.WriteByte((byte)Math.Min(num, 255));
            if (num >= 255)
            {
                msg += $" [6.ushort {(ushort)(num - 255)}]";
                NetworkWriterExtensions.WriteUShort(writer, (ushort)(num - 255));
            }
            msg += $" [7.Write Bytes 0 {num}]";
            writer.WriteBytes(((NetworkWriter)(object)val).buffer, 0, num);
            msg += $" [8.Dispose]";
            val.Dispose();
        }

        Log.Debug($"[3114 RPC] -{msg}");
        return false;
    }
}