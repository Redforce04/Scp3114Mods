// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         VoiceModuleProcessInputsPostfix.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/05/2023 1:01 PM
//    Created Date:     11/05/2023 1:01 PM
// -----------------------------------------

using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;
using Scp3114Mods.API;
using VoiceChat;

namespace Scp3114Mods.Internal.Patches.Postfixes;

[HarmonyPatch(typeof(Scp3114VoiceModule), nameof(Scp3114VoiceModule.ProcessInputs))]
internal static class VoiceModuleProcessInputsPostfix
{
    [HarmonyPostfix]
    private static void Postfix(Scp3114VoiceModule __instance, ref VoiceChatChannel __result, bool primary, bool alt)
    {
        try
        {
            Logging.Debug($"[3114Radio {__instance.Owner.nicknameSync._firstNickname}]");
            if (!Scp3114Mods.Singleton.Config.DisguisedPlayersCanUseRadio)
                return;
            if (!alt || !__instance.IsDisguised)
                return;
            if (Player.Get(__instance.Owner).CurrentItem.ItemTypeId != ItemType.Radio)
                return;
            Logging.Debug($"[Scp 3114 Radio Usage {__instance.Owner.nicknameSync._firstNickname}]");
            __result = VoiceChatChannel.Radio;
        }
        catch (Exception e)
        {
            Logging.Error("Scp3114Mods has caught an error at VoiceModuleProcessInputsPostfix.");
            Logging.Debug($"Exception: \n{e}");
        }
    }
}