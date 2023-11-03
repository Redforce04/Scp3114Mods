// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         GetPlayerPreferencePatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/02/2023 4:08 PM
//    Created Date:     11/02/2023 4:08 PM
// -----------------------------------------

using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.PlayableScps.Scp3114;
using Scp3114Mods.API;
using static HarmonyLib.AccessTools;

namespace Scp3114Mods.Internal.Patches;

[HarmonyPatch(typeof(PlayerRoles.RoleAssign.ScpSpawner), nameof(PlayerRoles.RoleAssign.ScpSpawner.GetPreferenceOfPlayer))]
public static class GetPlayerPreferencePatch
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        Label label = generator.DefineLabel();

        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
        var injectedInstructions = new CodeInstruction[]
        {
            // if (scp == RoleTypeId.Scp3114)
            // {
            //     return GetPlayerPreferencePatch.GetPlayerPreferenceOf3114(ply);
            // }
            new (OpCodes.Ldarg_1),
            new (OpCodes.Ldc_I4_S, 23),
            new (OpCodes.Bne_Un_S, label),
            new (OpCodes.Ldarg_0),
            new (OpCodes.Call, Method(typeof(GetPlayerPreferencePatch),nameof(GetPlayerPreferencePatch.GetPlayerPreferenceOf3114))),
            new (OpCodes.Ret),
        };
        
        const bool debug = false;
        if(debug) Logging.Debug($"Patching Transpiler GetPlayerPreferencePatch");
        for (int i = 0; i < injectedInstructions.Length; i++)
        {
            CodeInstruction injectedInstruction = injectedInstructions[i];
                    
            if(debug) Logging.Debug(_getOpcodeDebugLabel(injectedInstruction, 0, i));
            yield return injectedInstruction;
        }

        //skip the first instruction, as we inject the skip label here.
        var instruction = newInstructions[0].WithLabels(label);
        if(debug) Logging.Debug(_getOpcodeDebugLabel(instruction, 0));
        yield return instruction;
        
        for (int z = 1; z < newInstructions.Count; z++)
        {
            instruction = newInstructions[z];

            if(debug) Logging.Debug(_getOpcodeDebugLabel(instruction, z));
            yield return instruction;
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

    public static int GetPlayerPreferenceOf3114(ReferenceHub hub)
    {
        return 1;
    }
}