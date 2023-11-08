// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         VoiceModuleValidateSend.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/05/2023 2:05 PM
//    Created Date:     11/05/2023 2:05 PM
// -----------------------------------------

using HarmonyLib;
using InventorySystem.Items.Radio;
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;
using Scp3114Mods.API;
using VoiceChat;

namespace Scp3114Mods.Internal.Patches.Prefixes;

[HarmonyPatch(typeof(Scp3114VoiceModule), nameof(Scp3114VoiceModule.ValidateSend))]
internal static class VoiceModuleValidateSendPrefix
{
    [HarmonyPrefix]
    private static bool Prefix(Scp3114VoiceModule __instance, ref VoiceChatChannel __result, VoiceChatChannel channel)
    {
        // If Scp 3114 cannot use the radio.
        if (!Scp3114Mods.Singleton.Config.DisguisedPlayersCanUseRadio)
            return true;
        
        // Allow Proximity Chat
        if (channel == VoiceChatChannel.Proximity)
        {
            __result = __instance.IsDisguised ? VoiceChatChannel.Proximity : VoiceChatChannel.None;
            return false;
        }
        
        // Allow Scp Chat 
        if (channel == __instance.PrimaryChannel)
        {
            __result = channel;
            return false;
        }

        // Allow Radio Usage If Enabled
        if (channel == VoiceChatChannel.Radio)
        {
            Logging.Debug($"[Radio Usage Validate {__instance.Owner.nicknameSync._firstNickname}]");

            __result = VoiceChatChannel.None;

            // If Scp 3114 is not disguised.
            if (!__instance.IsDisguised)
                return false;

            Player ply = Player.Get(__instance.Owner);
            // If Scp 3114 is not holding a radio.
            if(ply.CurrentItem is not RadioItem radio)
                return false;

            // If radio is turned off
            if (!radio._enabled)
                return false;
                
            // If the radio's battery is dead.
            if (radio.BatteryPercent == 0)
                return false;
            
            Logging.Debug($"[Radio Usage Success {__instance.Owner.nicknameSync._firstNickname}]");
            __result = VoiceChatChannel.Radio;
        }
        
        
        // Block all other channels.
        __result = VoiceChatChannel.None;
        return false;
    }
}