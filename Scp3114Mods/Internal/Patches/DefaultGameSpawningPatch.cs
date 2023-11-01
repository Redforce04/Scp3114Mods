// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         DefaultGameSpawningPatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/01/2023 12:20 PM
//    Created Date:     11/01/2023 12:20 PM
// -----------------------------------------

using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.RoleAssign;
using PluginAPI.Core;
using static HarmonyLib.AccessTools;
namespace Scp3114Mods.Internal.Patches;

//[HarmonyPatch(typeof(ScpSpawner),nameof(ScpSpawner.SpawnableScps), MethodType.Getter)]
internal static class DefaultGameSpawningPatch
{
    //[HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        PlayerRoleBase ba = null!;
        if (ba is Scp3114Role)
        {/*
            //IL_0028: ldloca.s  keyValuePair
            //IL_002A: call      instance !1 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<valuetype PlayerRoles.RoleTypeId, class PlayerRoles.PlayerRoleBase>::get_Value()
            //IL_002F: isinst    PlayerRoles.PlayableScps.ISpawnableScp
            IL_0034: brtrue.s  IL_0044
                
                
            IL_0036: ldloca.s  keyValuePair
            IL_0038: call      instance !1 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<valuetype PlayerRoles.RoleTypeId, class PlayerRoles.PlayerRoleBase>::get_Value()
            IL_003D: isinst    PlayerRoles.PlayableScps.Scp3114.Scp3114Role
            IL_0042: brfalse.s IL_0051*/
        }
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

        Label shortSkipLabel = generator.DefineLabel();
        Label fullSkipLabel = generator.DefineLabel();
        int index = 01 + newInstructions.FindIndex(x => x.opcode == OpCodes.Isinst);
        int shortSkipLabelIndex = index + 1; // short 
        int fullSkipLabelIndex = index + 5; // full skip
        var injectedInstructions = new CodeInstruction[]
            {
                // if(kvp.value is ISpawnableScp) => skip next if
                new (OpCodes.Brtrue_S, shortSkipLabel),
                // if (kvp.value is Scp3114Role) => skip to after code section
                new (OpCodes.Ldloc_S, 2),
                new (OpCodes.Call, PropertyGetter(typeof(KeyValuePair<RoleTypeId,PlayerRoleBase>), nameof(KeyValuePair<RoleTypeId,PlayerRoleBase>.Value))),
                new (OpCodes.Isinst, typeof(Scp3114Role)),
                new (OpCodes.Brfalse_S, fullSkipLabel),
            };

        //newInstructions[newInstructions.Count - 1].WithLabels();
        const bool debug = true;
        for (int z = 0; z < newInstructions.Count; z++)
        {
            if (z == index) // inject code at insert location
            {
                for (int i = 0; i < injectedInstructions.Length; i++)
                {
                    if (debug) Log.Debug($"[{z:000} +{i:00}] {injectedInstructions[i].opcode}");
                    yield return injectedInstructions[i];
                    
                }
            }
            else // skip the index where we are inserting - we are replacing it anyways.
            {
                var instruction = newInstructions[z];
                string debugString = $"[{z:000}    ] {(instruction.opcode.ToString()).PadRight(13)}";
                // add the short skip label
                if (z == shortSkipLabelIndex)
                {
                    debugString += $" [Short Skip]";
                    instruction = instruction.WithLabels(shortSkipLabel);
                    goto skip;
                }
                // add the full skip label
                if (z == fullSkipLabelIndex)
                {
                    debugString += $" [Full Skip]";
                    instruction = instruction.WithLabels(shortSkipLabel);
                }
                skip:
                if(debug) Log.Debug(debugString);
                yield return instruction;
            }
        }

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }
    
}