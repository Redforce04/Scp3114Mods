// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         Scp3114UseItemPatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/29/2023 6:09 PM
//    Created Date:     10/29/2023 6:09 PM
// -----------------------------------------

using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;
using RelativePositioning;
using UnityEngine;
using Utils.Networking;

namespace Scp3114Mods.Internal.Patches;

[HarmonyPatch(typeof(PlayerRoles.PlayableScps.Scp3114.Scp3114Strangle), nameof(PlayerRoles.PlayableScps.Scp3114.Scp3114Strangle.ProcessAttackRequest))]
internal static class StranglePatch
{
	private static bool Debug => Scp3114Mods.Singleton.Config.Debug;
	internal static bool Prefix(Scp3114Strangle __instance, ref Scp3114Strangle.StrangleTarget? __result, NetworkReader reader)
	{
		try
		{

			if (Scp3114Mods.Singleton.Config.StrangleCooldown < 0 &&
			    Scp3114Mods.Singleton.Config.AllowStranglingInnocents &&
			    !Scp3114Mods.Singleton.Config.RequireEmptyHandToStrangleAll &&
			    !Scp3114Mods.Singleton.Config.RequireEmptyHandToStrangleInnocents)
				return true;
			__result = null;
			Player ply = Player.Get(__instance.Owner);
			if (Scp3114Mods.Singleton.Config.StrangleCooldown > 0 &&
			    !Scp3114Mods.Singleton.PlayerCanStrangle(ply, out float cooldownRemaining))
			{
				ply.ReceiveHint(
					$"You cannot strangle - your strangle is on cooldown.\nTry slashing instead.\nCooldown remaining: <b>{cooldownRemaining:0} seconds</b>",
					5f);
				if (Debug)
					Log.Debug("Strangle Disabled. - Cooldown");
				return false;
			}

			if (reader.Remaining == 0)
			{
				return false;
			}

			if (__instance.SyncTarget.HasValue || !__instance.Cooldown.TolerantIsReady)
			{
				return false;
			}

			if (!reader.TryReadReferenceHub(out var hub))
			{
				return false;
			}

			Vector3 position = reader.ReadRelativePosition().Position;
			Vector3 position2 = reader.ReadRelativePosition().Position;
			Scp3114Strangle.StrangleTarget value;
			using (new FpcBacktracker(hub, position))
			{
				using (new FpcBacktracker(__instance.Owner, position2, Quaternion.identity))
				{
					if (!__instance.ValidateTarget(hub))
					{
						return false;
					}

					HumanRole target = hub.roleManager.CurrentRole as HumanRole;

					Player targetPly = Player.Get(target._lastOwner);
					if (Scp3114Mods.Singleton.Config.CanTutorialsBeStrangled && targetPly.Role == RoleTypeId.Tutorial)
					{
						if (Debug)
							Log.Debug("Strangle Disabled. - Tutorial");
						ply.ReceiveHint("Cannot strangle tutorials.", 5f);
						return false;
					}

					bool strangleInnocent = Scp3114Mods.Singleton.Config.RequireEmptyHandToStrangleInnocents && isPlayerInnocent(ply);
					if ((Scp3114Mods.Singleton.Config.RequireEmptyHandToStrangleAll ||
					     (strangleInnocent)) && targetPly.CurrentItem is not null)
					{
						if (Debug)
							Log.Debug("Strangle Disabled. - Empty Hand");
						ply.ReceiveHint($"{(strangleInnocent ? "Innocent p" : "P")}layers must have an empty hand to be strangled.", 5f);
						return false;
					}

					if (Scp3114Mods.Singleton.Config.AllowStranglingInnocents && isPlayerInnocent(targetPly))
					{
						if (Debug)
							Log.Debug("Strangle Disabled. - Innocent");
						ply.ReceiveHint($"Cannot strangle innocent players. They must have a weapon{(Scp3114Mods.Singleton.Config.CandyLosesInnocence ? " or candy." : "")}.", 5f);
						return false;
					}

					hub.playerEffectsController.EnableEffect<Strangled>();
					value = new Scp3114Strangle.StrangleTarget(hub, __instance.GetStranglePosition(target),
						__instance.ScpRole.FpcModule.Position);
					__result = value;
					Scp3114Mods.Singleton.AddCooldownForPlayer(ply);
				}
			}

			__instance.ScpRole.FpcModule.Position = __instance._validatedPosition;
			__result = value;
			return false;
		}
		catch (Exception e)
		{
			Log.Error("Scp3114Mods has caught an error at Strangle Patch.");
			if (Scp3114Mods.Singleton.Config.Debug)
			{
				Log.Debug($"Exception: \n{e}");
			}

			return true;
		}
	}

	private static bool isPlayerInnocent(Player ply)
	{
		if (ply.Role is not RoleTypeId.Scientist or RoleTypeId.ClassD)
			return false;
		if (ply.Items.Any(x =>
		    {
			    if (x is Firearm)
				    return true;
			    if (x.ItemTypeId is ItemType.GrenadeFlash or ItemType.GrenadeHE or ItemType.SCP018)
				    return true;
			    if (Scp3114Mods.Singleton.Config.CandyLosesInnocence && x.ItemTypeId == ItemType.SCP330)
				    return true;
			    return false;
		    }))
		{
			return false;
		}

		return true;
	}
}