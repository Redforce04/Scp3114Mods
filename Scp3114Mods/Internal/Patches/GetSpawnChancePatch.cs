// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         GetSpawnChancePatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/01/2023 4:45 PM
//    Created Date:     11/01/2023 4:45 PM
// -----------------------------------------

using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.PlayableScps;
using PlayerRoles.RoleAssign;
using PluginAPI.Core;
using static HarmonyLib.AccessTools;

namespace Scp3114Mods.Internal.Patches;


[HarmonyPatch(typeof(ScpSpawner),nameof(ScpSpawner.NextScp), MethodType.Getter)]
internal static class GetSpawnChancePatch
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        if (Config.Dbg) Log.Debug($"Patching Transpiler GetSpawnChancePatch");

        /*
            - Change label Operand @ 003a (0070[] -> 006B[])
            - Leave
            - Leave 
            - Leave
            - Remove isinst
            -> ldsfld List<RoletypeId> EnqueuedScps
            -> call Scp3114Mods.Internal.Patches.GetSpawnChancePatch::GetChance()
            - Remove 0043
            - Remove 0048
            - Remove 004d
         */
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
        Dictionary<int, Label> labelIndexes = new Dictionary<int, Label>();
        int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Isinst);
        int changeLabel = index - 4;
        var injectedInstructions = new CodeInstruction[]
        {
            new (OpCodes.Ldsfld, Field(typeof(ScpSpawner), nameof(ScpSpawner.EnqueuedScps))),
            new (OpCodes.Call, Method(typeof(GetSpawnChancePatch), nameof(GetSpawnChancePatch._getChance))),
        };
        
        const bool debug = false;
        for (int z = 0; z < newInstructions.Count; z++)
        {
            if (z == index) // inject code at insert location
            {
                for (int i = 0; i < injectedInstructions.Length; i++)
                {
                    CodeInstruction instruction = injectedInstructions[i];
                    
                    if (debug) Log.Debug(_getOpcodeDebugLabel(instruction, z, i));
                    yield return instruction;
                }

                // Skip this instruction.
                // Skip the next 2 instructions.
                z += 2;
            }
            else // skip the index where we are inserting - we are replacing it anyways.
            {
                var instruction = newInstructions[z];
        
                if(debug) Log.Debug(_getOpcodeDebugLabel(instruction, z));
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

    private static float _getChance(PlayerRoleBase role, List<RoleTypeId> enqueuedRoles)
    {
        float chance;
        if (role is ISpawnableScp spawnableScp)
        {
            chance = spawnableScp.GetSpawnChance(enqueuedRoles);
        }
        else
            chance = Scp3114Mods.Singleton.Config.SpawnFromHumanRoles ? 0 :  Scp3114Mods.Singleton.Config.SpawnChance;

        string enqueued = "";
        foreach (var x in enqueuedRoles)
            enqueued += $" [{x}]";
        
        if(Config.Dbg) Log.Debug($"[{role}] {chance}%     {enqueued}");
        return chance;
    }
}