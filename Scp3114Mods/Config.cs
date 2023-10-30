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

namespace Scp3114Mods;

public class Config
{
    [Description("Determines whether the plugin is enabled or not.")]
    public bool IsEnabled { get; set; } = true;
    
    [Description("Should debug logs be shown.")]
    public bool Debug { get; set; } = false;
    
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
    
    [Description("Can tutorials be strangled.")]
    public bool CanTutorialsBeStrangled { get; set; } = false;

    [Description("Does candy cause a player to lose innocence.")]
    public bool CandyLosesInnocence { get; set; } = true;

    [Description("Should fake firing use ammo. Prevents reloading unless the gun has used ammo.")]
    public bool FakeFiringUsesAmmo { get; set; } = false;
    [Description("Should fake firing be allowed.")]
    public bool FakeFiringAllowed { get; set; } = true;


}
