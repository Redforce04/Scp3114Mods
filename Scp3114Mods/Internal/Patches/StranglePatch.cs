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
using Scp3114Mods.API;
using Scp3114Mods.API.EventArgs;
using UnityEngine;
using Utils.Networking;

namespace Scp3114Mods.Internal.Patches;
/// <summary>
/// 
/// </summary>

[HarmonyPatch(typeof(PlayerRoles.PlayableScps.Scp3114.Scp3114Strangle), nameof(PlayerRoles.PlayableScps.Scp3114.Scp3114Strangle.ProcessAttackRequest))]
internal static class StranglePatch
{
	/// <summary>
	/// Adds logic for tutorial safety, Empty hand safety, and 
	/// </summary>
	private static bool Prefix(Scp3114Strangle __instance, ref Scp3114Strangle.StrangleTarget? __result, NetworkReader reader)
	{
		try
		{
			__result = null;

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

					Player targetPly = Player.Get(hub);
					Player ply = Player.Get(__instance.Owner);
					var ev = new StranglingPlayerArgs(ply, targetPly, true);
					API.Events.SafeInvoke(() => { API.Events.OnPlayerStrangling(ev); });
					if (!ev.IsAllowed)
					{
						return false;
					}
					
					hub.playerEffectsController.EnableEffect<Strangled>();
					value = new Scp3114Strangle.StrangleTarget(hub, __instance.GetStranglePosition(target), __instance.ScpRole.FpcModule.Position);
					__result = value; 
					Logging.Debug($"Player [{ply.Nickname}] Targeting Player {targetPly.Nickname}");
				}
			}

			__instance.ScpRole.FpcModule.Position = __instance._validatedPosition;
			__result = value;
			return false;
		}
		catch (Exception e)
		{
			Logging.Error("Scp3114Mods has caught an error at Strangle Patch.");
			Logging.Debug($"Exception: \n{e}");

			return true;
		}
	}

	
}
