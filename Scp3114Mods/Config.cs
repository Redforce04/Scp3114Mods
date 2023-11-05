﻿// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         Config.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/29/2023 6:02 PM
//    Created Date:     10/29/2023 6:02 PM
// -----------------------------------------

using System.ComponentModel;
using MEC;
using PlayerRoles;
using PluginAPI.Helpers;
using Scp3114Mods.API;
using YamlDotNet.Serialization;

namespace Scp3114Mods;

#if !EXILED
public class Config
#else
public class Config : Exiled.API.Interfaces.IConfig
#endif
{
    public Config()
    {
#if !EXILED
        string basePath = Path.Combine(Paths.GlobalPlugins.Plugins, "Scp3114Mods");
#else
        string basePath = Path.Combine(Exiled.API.Features.Paths.Configs, "Scp3114Mods");
#endif
        try
        {
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
            this.PlayerPreferenceFileLocation = Path.Combine(basePath, "Scp3114PlayerPreferences.txt");
            if (!File.Exists(PlayerPreferenceFileLocation))
                File.Create(PlayerPreferenceFileLocation);
        }
        catch (Exception e)
        {
            string message =
                $"Cannot load basepath \nBase Path: \"{basePath}\"\n File Location:\"{PlayerPreferenceFileLocation}\"\nExceptions: \n{e}";
            Timing.CallDelayed(1f, () =>
            {
                Logging.Debug(message);
            });
            // this exception doesnt matter, as it is a pre-serialization value. IE. player configs havent loaded, so they may fix the problem outright.
        }
        

    }
    [YamlIgnore]
    public static bool Dbg
    {
        get => Scp3114Mods.Singleton?.Config?.Debug ?? false;
        set { } // Exiled aint got no yamlignore or equiv for the copy properties method :clueless:
    }
    
    [Description("Determines whether the plugin is enabled or not.")]
    public bool IsEnabled { get; set; } = true;

    [Description("Should debug logs be shown.")]
    public bool Debug { get; set; } = false;
    
    [Description("Where the player preference file should be located.")]
    public string PlayerPreferenceFileLocation { get; set; } = "";

    [Description("Allows players with DNT to opt in to having thier scp 3114 preference stored. Player with DNT will have their userid hashed then stored.")]
    public bool AllowExplicitDntOptIn { get; set; } = true;

    [Description("Should 049 be able to resurrect players who have had their skin stolen. It may remove immersion, and expose Scp 3114.")]
    public bool Allow049ResurrectStolenSkinPlayer { get; set; } = true;

    [Description("The default player preference for users who haven't opted in, or have DNT enabled.")]
    public int DefaultPlayerPreference { get; set; } = 5;
    
    [Description("If true, players will be notified of gameplay mechanics with hints. If false, broadcasts will be used.")]
    public bool UseHintsInsteadOfBroadcasts { get; set; } = true;

    [Description("Plays a fake sound and visual effect to all players when scp3114 \"uses\" an item.")]
    public bool FakeUsableInteractions { get; set; } = true;

    
    [Description("After an item's fake \"use\" is done, should the item automatically be hidden.")]
    public bool AutohideItemAfterFakeUse { get; set; } = true;
    
    [Description("How long the cooldown is for scp3114 \"grab\" attack. -1 to disable. This applies if the attack kills a player.")]
    public float StrangleCooldown { get; set; } = 30f;

    [Description("How long the cooldown is for scp3114 \"grab\" attack. This only applies if the attack doesn't kill players.")]
    public float StranglePartialCooldown { get; set; } = 10f;
    
    [Description("Can Scp3114 strangle innocents / Class D or Scientists without a weapon.")]
    public bool AllowStranglingInnocents { get; set; } = false;

    [Description("Each role, and how much ahp to give 3114 when they slap the role. Roles not mentioned will use default ahp.")]
    public Dictionary<RoleTypeId, float> AhpToGiveOnSlap { get; set; } = new Dictionary<RoleTypeId, float>()
    {
        { RoleTypeId.ClassD, 5 },
        { RoleTypeId.Scientist, 5 },
        { RoleTypeId.ChaosRepressor, 35 },
        { RoleTypeId.ChaosMarauder, 30 },
        { RoleTypeId.ChaosConscript, 25 },
        { RoleTypeId.ChaosRifleman, 25 },
        { RoleTypeId.NtfCaptain, 35 },
        { RoleTypeId.NtfSergeant, 30 },
        { RoleTypeId.NtfSpecialist, 30 },
        { RoleTypeId.NtfPrivate, 25 },
        { RoleTypeId.FacilityGuard, 15 },
    };
    
    [Description("The damage that the slap deals to different roles. Roles not mentioned will deal default damage.")]
    public Dictionary<RoleTypeId, float> DamageSlapDeals { get; set; } = new Dictionary<RoleTypeId, float>()
    {
        { RoleTypeId.ClassD, 10 },
        { RoleTypeId.Scientist, 10 },
        { RoleTypeId.ChaosRepressor, 35 },
        { RoleTypeId.ChaosMarauder, 30 },
        { RoleTypeId.ChaosConscript, 25 },
        { RoleTypeId.ChaosRifleman, 25 },
        { RoleTypeId.NtfCaptain, 35 },
        { RoleTypeId.NtfSergeant, 30 },
        { RoleTypeId.NtfSpecialist, 30 },
        { RoleTypeId.NtfPrivate, 25 },
        { RoleTypeId.FacilityGuard, 15 },
    };
    
    [Description("Items that will mark a player as \"Non-Innocent\".")]
    public List<ItemType> StrangleNonInnocentItems { get; set; } = new List<ItemType>()
    {
        ItemType.GrenadeFlash,
        ItemType.GrenadeHE,
        ItemType.SCP018,
        ItemType.GunA7,
        ItemType.GunCom45,
        ItemType.GunCrossvec,
        ItemType.GunLogicer,
        ItemType.GunRevolver,
        ItemType.GunShotgun,
        ItemType.GunAK,
        ItemType.GunCom45,
        ItemType.GunCOM15,
        ItemType.GunCOM18,
        ItemType.GunE11SR,
        ItemType.GunFSP9,
        ItemType.GunFRMG0,
        ItemType.ParticleDisruptor,
        ItemType.MicroHID,
        ItemType.SCP330
    };
    [Description("Does the target of the strangle have to have an empty hand.")]
    public bool RequireEmptyHandToStrangleAll { get; set; } = false;
    [Description("Does the target of the strangle have to have an empty hand if non innocent.")]
    public bool RequireEmptyHandToStrangleInnocents { get; set; } = false;

    [Description("A list of items that will be considered \"empty hands\" for the empty hand strangle config.")]
    public List<ItemType> ItemsThatWontBlockStrangle { get; set; } = new List<ItemType>()
    {
        ItemType.KeycardO5,
        ItemType.KeycardContainmentEngineer,
        ItemType.KeycardChaosInsurgency,
        ItemType.KeycardMTFCaptain,
        ItemType.KeycardMTFOperative,
        ItemType.KeycardMTFPrivate,
        ItemType.KeycardFacilityManager,
        ItemType.KeycardGuard,
        ItemType.KeycardZoneManager,
        ItemType.KeycardResearchCoordinator,
        ItemType.KeycardScientist,
        ItemType.KeycardJanitor,
        ItemType.Coin,
        ItemType.Radio,
    };

    [Description("Can tutorials be strangled.")]
    public bool DisableTutorialsStrangling { get; set; } = true;

    [Description("Does candy cause a player to lose innocence.")]
    public bool CandyLosesInnocence { get; set; } = true;

    [Description("Should fake firing use ammo. Prevents reloading unless the gun has used ammo.")]
    public bool FakeFiringUsesAmmo { get; set; } = false;
    [Description("Should fake firing be allowed.")]
    public bool FakeFiringAllowed { get; set; } = true;

    [Description("Should fake firing be allowed.")]
    public bool StartInDisguiseOfSelf { get; set; } = true;

    [Description("How long shoud disguises last before being destroyed. -1 is infinite, and 0 is game default.")]
    public float DisguiseDuration { get; set; } = 0;

    [Description("How long the cooldown between new disguises lasts. 0 disabled.")]
    public float DisguiseCooldown { get; set; } = -1;
    
    [Description("How long the cooldown after a failed disguise lasts. -1 is game default.")]
    public float DisguiseFailedCooldown { get; set; } = -1;

    [Description("If set to true, players will be spawned from the default system, (using non-scp lives). Otherwise, players are pulled from scps. This must be set to false to use the player preference system.")]
    public bool SpawnFromHumanRoles { get; set; } = false;

    [Description("Percent of Scp3114 spawning in general.")]
    public int SpawnChance { get; set; } = 100;
    
    /*[Description("Percent of players to spawn as Scp3114. Ex: 10% of alive human players will become 3114 (if SpawnFromHumanRoles is enabled) or there is a 10% chance of and scp starting as 3114")]
    public int PercentOfPlayers { get; set; } = 10;

    [Description("How many players can be spawned as scp3114.")]
    public int MinimumScp3114Count { get; set; } = 1;

    [Description("How many players can be spawned as scp3114.")]
    public int MaximumScp3114Count { get; set; } = 1;*/

    /*[Description("Allows you to prevent spectators from watch 3114's role. Available Options: \n" +
                 "# Disabled - Allows spectators to spectate 3114 normally.\n" +
                 "# PersistentRoleSpoof - 3114 will have a different role visible to spectators, even when out of disguise.\n" +
                 "# TemporaryRoleSpoof - 3114 will have a different role only while in disguise.")]
    public SpectatorHideMode SpectatorHideMode { get; set; } = SpectatorHideMode.Disabled;

    [Description("What role spectators will see when viewing 3114. By default (None) the role will be chosen based off of the last role 3114 had. ")]
    public RoleTypeId SpectatorRole3114 { get; set; } = RoleTypeId.None;
    */
}

public enum SpectatorHideMode
{
    [Description("# Disabled - Allows spectators to spectate 3114 normally.")]
    Disabled = 0,
    [Description("# PersistentRoleSpoof - 3114 will have a different role visible to spectators, even when out of disguise.")]
    PersistentRoleSpoof = 1,
    [Description("# TemporaryRoleSpoof - 3114 will have a different role only while in disguise.")]
    TemporaryRoleSpoof = 1,
    
}
