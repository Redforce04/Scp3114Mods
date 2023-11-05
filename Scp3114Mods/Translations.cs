// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         Translations.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/01/2023 12:42 PM
//    Created Date:     11/01/2023 12:42 PM
// -----------------------------------------

using System.ComponentModel;
using PlayerRoles;

namespace Scp3114Mods;

#if EXILED
public class Translations : Exiled.API.Interfaces.ITranslation
#else
public class Translations
#endif
{
    [Description("Many of these messages will only be shown when the relevant config is applied.\n" +
                 "# The message showed when a player tries to strangle an innocent player with an empty hand.")]
    public string CannotStrangleInnocentPlayer { get; set; } = $"Cannot strangle innocent players. They must have a weapon or candy.";
    [Description("The message showed when a player tries to strangle an innocent player with an empty hand.")]
    public string CannotStrangleInnocentPlayerEmptyHand { get; set; } = $"Innocent players must have an empty hand to be strangled.";
    [Description("The message showed when a player tries to strangle another player with an empty hand.")]
    public string CannotStranglePlayerEmptyHand { get; set; } = $"Players must have an empty hand to be strangled.";
    [Description("The message showed when a player tries to strangle a tutorial, and tutorial strangling is disabled.")]
    public string CannotStrangleTutorials { get; set; } = "Cannot strangle tutorials.";

    [Description("The message to show a player when they select a weapon that can be faked.")]
    public string FakeFiringInform { get; set; } = "You can fake shoot weapons by pressing [T].";

    [Description("The message to show a player when they select a usable item that can be faked.")]
    public string FakeUsableInform { get; set; } = "You can fake use items by right clicking with your mouse";

    public string InfiniteDisguisesEnabled { get; set; } = "Infinite disguises are enabled. A cooldown will not be shown.\nCurrent Disguise: {role}";
    
    [Description("The message showed when a player tries to use the preference command, but has DNT enabled." +
                 "# If AllowExplicitDNTOptIn is not enabled it will show the PlayerPreferenceDNTDisabled translation instead." +
                 "# {pref} will be replaced with the default player preference, or the player's current preference (if already opted in)")]
    public string PlayerPreferenceCmdDNTOptInMessage { get; set; } = "You have Do-Not-Track On! You can still opt into this feature! If not, the default preference is {pref}." +
                                                                     "\n- When opting in, your userid will be secured in a unreversible manner (hashing)." +
                                                                     "\n- This means server staff and administration won't be able to track your user id." +
                                                                     "\n\n- It is still possible to find your preference, if they already know your user id." +
                                                                     "\n\n- The ONLY DATA STORED includes:" +
                                                                     "\n- UserId" +
                                                                     "\n- Scp 3114 Preference\n" +
                                                                     "\nType \".3114preference [Scp 3114 Preference(0-10)] DNTCONFIRM\" to confirm this message and opt in.";

    [Description("The message showed when a player with DNT tries to use the preference command but AllowExplicitDNTOptIn is disabled.")]
    public string PlayerPreferenceDNTDisabled { get; set; } = "You have DNT on! Players with DNT cannot use this feature! Consider turning off DNT to use this feature.";

    [Description("The result for the preference command when the player doesnt specify a new preference. {pref} will be replaced with their prior preference.")]
    public string PlayerPreferenceCurrentPreference { get; set; } = "Current preference level: {pref}. \nTo set a new preference, please specify the new preference level.";

    [Description("The result for the preference command when the new preference cannot be determined. {arg} will be replaced with the failed input they tried to select.")]
    public string PlayerPreferenceCouldntDetermineNewPreference { get; set; } = "Could not determine the desired preference from \"{arg}\". Ensure it is a number with no decimals.";

    
    [Description("The result for the preference command when the new preference was successfully set. {newPref} will be replaced with the new preference.")]
    public string PlayerPreferenceSuccess { get; set; } = "Successfully set your 3114 preference to {newPref}";

    [Description("This message will be added on to the response whenever a player incorrectly uses the player preference command.")]
    public string PlayerPreferenceUsage { get; set; } = "\nCommand Usage: \n.3114preference [Scp 3114 Preference (0 - 10)]. Ie: .3114preference 8";

    [Description("This message will be shown when an error occurs while using the player preference command.")]
    public string PlayerPreferenceErrorMessage { get; set; } = "An error has occured and we could not set your player preference. (You must be a player to use this command).";

    [Description("A list of roles, and their color / names. This is used to show a player what role they are disguised at, if infinite disguise is enabled")]
    public Dictionary<RoleTypeId, string> RoleNames { get; set; } = new Dictionary<RoleTypeId, string>()
    {
        { RoleTypeId.Scp079, "<color=#ff0000>Scp 079</color>" },
        { RoleTypeId.Scp049, "<color=#ff0000>Scp 049</color>" },
        { RoleTypeId.Scp0492, "<color=#ff0000>Scp 049-2</color>" },
        { RoleTypeId.Scp096, "<color=#ff0000>Scp 096</color>" },
        { RoleTypeId.Scp173, "<color=#ff0000>Scp 173</color>" },
        { RoleTypeId.Scp106, "<color=#ff0000>Scp 106</color>" },
        { RoleTypeId.Scp939, "<color=#ff0000>Scp 939</color>" },
        { RoleTypeId.Scp3114, "<color=#ff0000>Scp 3114</color>" },
        { RoleTypeId.ChaosMarauder, "<color=#008f1c>Chaos Marauder</color>" },
        { RoleTypeId.ChaosRepressor, "<color=#008f1c>Chaos Repressor</color>" },
        { RoleTypeId.ChaosConscript, "<color=#008f1c>Chaos Conscript</color>" },
        { RoleTypeId.ChaosRifleman, "<color=#008f1c>Chaos Rifleman</color>" },
        { RoleTypeId.NtfCaptain, "<color=#003dca>Ntf Captain</color>" },
        { RoleTypeId.NtfSpecialist, "<color=#0096ff>Ntf Specialist</color>" },
        { RoleTypeId.NtfSergeant, "<color=#0096ff>Ntf Sergeant</color>" },
        { RoleTypeId.NtfPrivate, "<color=#70c3ff>Ntf Private</color>" },
        { RoleTypeId.FacilityGuard, "<color=#556278>Facility Guard</color>" },
        { RoleTypeId.Scientist, "<color=#ffff7c>Scientist</color>" },
        { RoleTypeId.ClassD, "<color=#ff8000>ClassD</color>" },
        { RoleTypeId.Spectator, "<color=#ffffff>Spectator</color>" },
        { RoleTypeId.Overwatch, "<color=#00ffff>Overwatch</color>" },
        { RoleTypeId.Filmmaker, "<color=#010101>Filmmaker</color>" },
        { RoleTypeId.Tutorial, "<color=#ff00b0>Tutorial</color>" },
    };
}