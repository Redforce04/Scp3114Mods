// <copyright file="Log.cs" company="Redforce04#4091">
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
using PlayerRoles;
using YamlDotNet.Serialization;

namespace Scp3114Mods;

#if !EXILED
public class Config
#else
public class Config : Exiled.API.Interfaces.IConfig
#endif
{
    [YamlIgnore]
    public static bool Dbg
    {
        get => Scp3114Mods.Singleton?.Config?.Debug ?? false;
        
    }
    
    [Description("Determines whether the plugin is enabled or not.")]
    public bool IsEnabled { get; set; } = true;

    [Description("Should debug logs be shown.")]
    public bool Debug { get; set; } = false;

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
    public bool AllowStranglingInnocents { get; set; } = true;
    
    [Description("Items that will not prevent strangline.")]
    public List<ItemType> StrangleNonInnocentItems = new List<ItemType>()
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
        ItemType.SCP330
    };
    [Description("Does the target of the strangle have to have an empty hand.")]
    public bool RequireEmptyHandToStrangleAll { get; set; } = true;
    [Description("Does the target of the strangle have to have an empty hand if non innocent.")]
    public bool RequireEmptyHandToStrangleInnocents { get; set; } = true;

    [Description("A list of items that will be considered \"empty hands\" for the empty hand strangle config.")]
    public List<ItemType> ItemsThatWontBlockStrangle { get; set; } = new List<ItemType>()
    {
        
    };

    [Description("Can tutorials be strangled.")]
    public bool CanTutorialsBeStrangled { get; set; } = false;

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
    
    [Description("How long the cooldown after a failed disguis lasts. -1 is game default.")]
    public float DisguiseFailedCooldown { get; set; } = -1;

    [Description("If set to true, players will be pulled from the human pool (default game). Otherwise, players are pulled from scps.")]
    public bool SpawnFromHumanRoles { get; set; } = true;

    [Description("Percent of Scp3114 spawning in general. Only applies if SpawnFromHumanRoles is enabled. Set to 100 to guarantee at least one scp3114 spawn. if 50/50, half of games won't spawn scp3114.")]
    public int SpawnChance { get; set; } = 50;
    
    [Description("Percent of players to spawn as Scp3114. Ex: 10% of alive human players will become 3114 (if SpawnFromHumanRoles is enabled) or there is a 10% chance of and scp starting as 3114")]
    public int PercentOfPlayers { get; set; } = 10;

    [Description("How many players can be spawned as scp3114.")]
    public int MinimumScp3114Count { get; set; } = 1;

    [Description("How many players can be spawned as scp3114.")]
    public int MaximumScp3114Count { get; set; } = 1;

    [Description("Allows you to prevent spectators from watch 3114's role. Available Options: \n" +
                 "# Disabled - Allows spectators to spectate 3114 normally.\n" +
                 "# PersistentRoleSpoof - 3114 will have a different role visible to spectators, even when out of disguise.\n" +
                 "# TemporaryRoleSpoof - 3114 will have a different role only while in disguise.")]
    public SpectatorHideMode SpectatorHideMode { get; set; } = SpectatorHideMode.Disabled;

    [Description("What role spectators will see when viewing 3114. By default (None) the role will be chosen based off of the last role 3114 had. ")]
    public RoleTypeId SpectatorRole3114 { get; set; } = RoleTypeId.None;

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
