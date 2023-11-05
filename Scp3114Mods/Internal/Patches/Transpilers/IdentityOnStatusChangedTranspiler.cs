// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         IdentityOnStatusChangedTranspiler.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/03/2023 7:05 PM
//    Created Date:     11/03/2023 7:05 PM
// -----------------------------------------

using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PluginAPI.Core;
using Scp3114Mods.API;
using static HarmonyLib.AccessTools;

namespace Scp3114Mods.Internal.Patches.Transpilers;

[HarmonyPatch(typeof(Scp3114Identity),nameof(Scp3114Identity.OnIdentityStatusChanged))]
internal static class IdentityOnStatusChangedTranspiler
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {

        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
        LocalBuilder local = generator.DeclareLocal(typeof(double));
        Label label = generator.DefineLabel();
        Label label2 = generator.DefineLabel();
        int index1 = 0;
        int index2 = 0;
        int occurence = 0;
        for (int i = 0; i < newInstructions.Count; i++)
        {
            // find first and second field call, and assign index 1 and 2 to it respectively.
            if (newInstructions[i].opcode == OpCodes.Ldfld && (FieldInfo)newInstructions[i].operand == Field(typeof(Scp3114Identity), nameof(Scp3114Identity.RemainingDuration)))
            {
                occurence++;
                if (occurence == 1)
                {
                    index1 = i;
                    continue;
                }
                index2 = i - 1;
                break;
            }
        }
        var injectedInstructions1 = new CodeInstruction[]
        {
            // if (scp == RoleTypeId.Scp3114)
            // {
            //     return GetPlayerPreferencePatch.GetPlayerPreferenceOf3114(ply);
            // }
            new(OpCodes.Call, Method(typeof(IdentityOnStatusChangedTranspiler), nameof(GetIdentityDuration))),
            new(OpCodes.Stloc_S, local.LocalIndex),
            new(OpCodes.Ldloc_S, local.LocalIndex),
            new(OpCodes.Ldc_R8, 0d),
            new(OpCodes.Bgt_Un_S, label),
            new(OpCodes.Ret),
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldfld, Field(typeof(Scp3114Identity), nameof(Scp3114Identity.RemainingDuration))),
            new(OpCodes.Ldloc_S, local.LocalIndex),
        };
        var injectedInstructions2 = new CodeInstruction[]
        {
            new(OpCodes.Call, Method(typeof(IdentityOnStatusChangedTranspiler), nameof(CanClearIdentity))),
            new(OpCodes.Brfalse, label2),
            new(OpCodes.Ldarg_0)
        };

        // Debug shows output ilcode and info about it.
        // Patch1 & Patch2 can disable one patch or the other for testing purposes 
        const bool patch1 = true;
        const bool patch2 = true;
        const bool debug = false;
        if (debug) Logging.Debug($"Patching Transpiler StrangleFinishedTranspilerPatch");
        for (int z = 0; z < newInstructions.Count; z++)
        {
            CodeInstruction instruction;
            if (z == index1 && patch1) // inject code at insert location
            {
                for (int i = 0; i < injectedInstructions1.Length; i++)
                {
                    instruction = (i != 6) ? injectedInstructions1[i] : injectedInstructions1[i].WithLabels(label);
                    instruction.Log(z, i, debug);
                    yield return instruction;
                }
                z += 3;
            }
            if (z == index2 && patch2) // inject code at insert location
            {
                // return original instruction and inject code afterwards
                newInstructions[z].Log(z, -1, debug);
                yield return newInstructions[z];
                for (int i = 0; i < injectedInstructions2.Length; i++)
                {
                    instruction = injectedInstructions2[i];
                    instruction.Log(z, i, debug);
                    yield return instruction;
                }
                
            }
            else // skip the index where we are inserting - we are replacing it anyways.
            {
                //instruction = newInstructions[z];
                instruction = (z != index2 + 3 || !patch2) ?  newInstructions[z] : newInstructions[z].WithLabels(label2);
                instruction.Log(z, -1, debug);
                yield return instruction;
            }
        }

        ListPool<CodeInstruction>.Shared.Return(newInstructions);

    }

    

    private static double GetIdentityDuration(Scp3114Identity _identity)
    {

        float value = Scp3114Mods.Singleton.Config.DisguiseDuration;
        if (value == 0)
        {
            value = _identity._disguiseDurationSeconds;
        }

        if (value < 0)
        {
            string role = "";
            if (Scp3114Mods.Singleton.Translation.RoleNames.ContainsKey(_identity.CurIdentity.StolenRole))
                role = Scp3114Mods.Singleton.Translation.RoleNames[_identity.CurIdentity.StolenRole];
            else
                role = _identity.CurIdentity.StolenRole.ToString();
            Scp3114Mods.Singleton.Handlers.ShowInfiniteDisguiseMessage(Player.Get(_identity.Owner), role);
            return -1;
        }
        return value;
    }

    private static bool CanClearIdentity(Scp3114Identity _identity)
    {
        return true;
        // prevent infinite disguises from clearing.
        //return Scp3114Mods.Singleton.Config.DisguiseDuration == -1;
    }
}