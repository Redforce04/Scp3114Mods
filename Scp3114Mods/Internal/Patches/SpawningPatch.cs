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

using HarmonyLib;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;
using UnityEngine;
using Random = System.Random;

namespace Scp3114Mods.Internal.Patches;

/// <summary>
/// Changes the mechanics of the scp3114 spawning system.
/// </summary>
[HarmonyPatch(typeof(Scp3114Spawner), nameof(Scp3114Spawner.OnPlayersSpawned))]
internal static class SpawningPatch
{
    /// <summary>
    /// Plenty of debug, as this is a new and untested feature.
    /// Also it could prevent 3114 from spawning in general.
    /// This patch replaces the default spawn mechanics.
    /// </summary>
    /// <returns></returns>
    private static bool Prefix()
    {
        // Lots of debug because I don't want the spawn system to break - it would be very bad.
        if (!Scp3114Mods.Singleton.Config.SpawnFromHumanRoles)
            return false;
        string text = $"Scp3114 Spawn Stats: \n" +
                      $"[Chance: {Scp3114Mods.Singleton.Config.SpawnChance}], \n" +
                      $"[Minimum: {Scp3114Mods.Singleton.Config.MinimumScp3114Count}], \n" +
                      $"[Maximum: {Scp3114Mods.Singleton.Config.MaximumScp3114Count}], \n" +
                      $"[Percent: {Scp3114Mods.Singleton.Config.PercentOfPlayers}], \n";
        if (UnityEngine.Random.value > Scp3114Mods.Singleton.Config.SpawnChance * .01)
        {
            if (Config.Dbg) Log.Debug(text + "Denied Spawn due to spawn chance.");
            return false;
        }
        
        Scp3114Spawner._ragdollsSpawned = false;
        int amount = (int) (Player.Count * (Scp3114Mods.Singleton.Config.PercentOfPlayers / 100f));
        text += $"$[Chosen %: {amount}],\n";
        amount = Mathf.Clamp(amount, Scp3114Mods.Singleton.Config.MinimumScp3114Count, Scp3114Mods.Singleton.Config.MaximumScp3114Count);
        text += $"$[Clamped Count: {amount}],\n";
        text += $"$[Initial Human Count: {Scp3114Spawner.SpawnCandidates.Count}],\n";
        if(Config.Dbg) Log.Debug(text);
        Scp3114Spawner.SpawnCandidates.Clear();
        PlayerRolesUtils.ForEachRole<HumanRole>(Scp3114Spawner.SpawnCandidates.Add);
        List<ReferenceHub> chosenPlayers = new List<ReferenceHub>();
        for (int i = 0; i < amount; i++)
        {
            // Need to leave at least one human player.
            if (Scp3114Spawner.SpawnCandidates.Count < 2) 
            {
                break;
            }
            ReferenceHub ply = Scp3114Spawner.SpawnCandidates.RandomItem();
            chosenPlayers.Add(ply);
            Scp3114Spawner.SpawnCandidates.Remove(ply);
        }
        
        foreach (ReferenceHub ply in chosenPlayers)
        {
            ply.roleManager.ServerSetRole(RoleTypeId.Scp3114, RoleChangeReason.RoundStart);
        }
        /*if (!(Random.value >= 0.5f))
        {
            PlayerRolesUtils.ForEachRole<HumanRole>(Scp3114Spawner.SpawnCandidates.Add);
            if (Scp3114Spawner.SpawnCandidates.Count >= 2)
            {
                Scp3114Spawner.SpawnCandidates.RandomItem().roleManager.ServerSetRole(RoleTypeId.Scp3114, RoleChangeReason.RoundStart);
            }
        }*/
        return false;
    }
}