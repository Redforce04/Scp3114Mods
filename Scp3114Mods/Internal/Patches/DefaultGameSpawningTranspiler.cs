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
using Scp3114Mods.API;
using static HarmonyLib.AccessTools;
namespace Scp3114Mods.Internal.Patches;

[HarmonyPatch(typeof(ScpSpawner),nameof(ScpSpawner.SpawnableScps), MethodType.Getter)]
internal static class DefaultGameSpawningPatch
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
     
            /*
            og //IL_0028: ldloca.s  keyValuePair
            og //IL_002A: call      instance !1 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<valuetype PlayerRoles.RoleTypeId, class PlayerRoles.PlayerRoleBase>::get_Value()
            og //IL_002F: isinst    PlayerRoles.PlayableScps.ISpawnableScp
            IL_0034: brtrue.s  IL_0044    -> replace og // brfalse.s
                
                
            IL_0036: ldloca.s  keyValuePair
            IL_0038: call      instance !1 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<valuetype PlayerRoles.RoleTypeId, class PlayerRoles.PlayerRoleBase>::get_Value()
            IL_003D: isinst    PlayerRoles.PlayableScps.Scp3114.Scp3114Role
            IL_0042: brfalse.s IL_0051*/
        
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

        Label shortSkipLabel = generator.DefineLabel();
        int index = 01 + newInstructions.FindIndex(x => x.opcode == OpCodes.Isinst);
        int shortSkipLabelIndex = index + 1; // short 
        var injectedInstructions = new CodeInstruction[]
            {
                // if(kvp.value is ISpawnableScp) => skip next if
                new (OpCodes.Brtrue_S, shortSkipLabel),
                // if (kvp.value is Scp3114Role) => skip to after code section
                new (OpCodes.Ldloca_S, 2),
                new (OpCodes.Call, PropertyGetter(typeof(KeyValuePair<RoleTypeId,PlayerRoleBase>), nameof(KeyValuePair<RoleTypeId,PlayerRoleBase>.Value))),
                new (OpCodes.Isinst, typeof(Scp3114Role)),
                new (OpCodes.Brfalse_S), //, fullSkipLabel),
            };

        const bool debug = false;
        if(debug)Logging.Debug($"Patching Transpiler Default GameSpawningPatch");
        for (int z = 0; z < newInstructions.Count; z++)
        {
            if (z == index) // inject code at insert location
            {
                for (int i = 0; i < injectedInstructions.Length; i++)
                {
                    CodeInstruction instruction = injectedInstructions[i];
                    if (i == 04)
                    {
                        // Move the old label on the remove brfalse -> first skip.
                        instruction.operand = newInstructions[z].operand;
                    }
                    if(debug) Logging.Debug(_getOpcodeDebugLabel(instruction, z, i));
                    yield return instruction;
                }
            }
            else // skip the index where we are inserting - we are replacing it anyways.
            {
                var instruction = newInstructions[z];
                
                // add the short skip label
                if (z == shortSkipLabelIndex)
                {
                    instruction = instruction.WithLabels(shortSkipLabel);
                    goto skip;
                }
                skip:
                
                if(debug) Logging.Debug(_getOpcodeDebugLabel(instruction, z));
                yield return instruction;
            }
        }

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }

    private static string _getOpcodeDebugLabel(CodeInstruction instruction, int index, int injectedIndex = -1)
    {
        string labelOperand = "";
        if (instruction.operand is Label label)
            labelOperand += $"-> ({label.GetHashCode()})";

        string notedLabels = instruction.labels.Count > 0 ? "   " : "";
        foreach (var x in instruction.labels)
        {
            notedLabels += $" [{x.GetHashCode()}]";
        }
        string injectedString = injectedIndex < 0 ? "   " : $"+{injectedIndex:00}";
        return $"[{index:000} {injectedString}] {instruction.opcode,-10}{labelOperand,-7}{notedLabels}";
    }
    
}