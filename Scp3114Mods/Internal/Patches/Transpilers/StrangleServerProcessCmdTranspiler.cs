// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         StrangleTranspilerPatch.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/02/2023 2:49 PM
//    Created Date:     11/02/2023 2:49 PM
// -----------------------------------------

using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.PlayableScps.Scp3114;
using Scp3114Mods.API;
using Scp3114Mods.API.EventArgs;
using static HarmonyLib.AccessTools;

namespace Scp3114Mods.Internal.Patches.Transpilers;

[HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.ServerProcessCmd))]
internal static class StrangleServerProcessCmdTranspiler
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
        Label skipLabel = generator.DefineLabel();
        int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Ldfld && (FieldInfo)x.operand == Field(typeof(Scp3114Strangle), nameof(Scp3114Strangle.ServerOnBegin))) - 1;
        var injectedInstructions = new CodeInstruction[]
        {
            // ev = new StranglingPlayerArgs(this, syncTarget (new one), 1)
            new (OpCodes.Ldarg_0),
            new (OpCodes.Ldloca_S, 0), // syncTarget - NOT SyncTarget
            new (OpCodes.Call, PropertyGetter(typeof(Nullable<Scp3114Strangle.StrangleTarget>), nameof(Nullable<Scp3114Strangle.StrangleTarget>.Value))),
            new (OpCodes.Ldc_I4_1),
            new (OpCodes.Newobj, GetDeclaredConstructors(typeof(global::Scp3114Mods.API.EventArgs.StranglingPlayerArgs))[0]),
            new (OpCodes.Dup),
            
            // if(!ev.IsAllowed)
            new (OpCodes.Call, Method(typeof(Events), nameof(Events.OnPlayerStrangling))),
            new (OpCodes.Callvirt, PropertyGetter(typeof(StranglingPlayerArgs), nameof(StranglingPlayerArgs.IsAllowed))),
            // return
            new (OpCodes.Brtrue_S, skipLabel),
            new (OpCodes.Ret),
        };
        
        const bool debug = false;
        if(debug) Logging.Debug($"Patching Transpiler StrangleTranspilerPatch");
        for (int z = 0; z < newInstructions.Count; z++)
        {
            CodeInstruction instruction;
            if (z == index) // inject code at insert location
            {
                for (int i = 0; i < injectedInstructions.Length; i++)
                {
                    instruction = injectedInstructions[i];
                    
                    if(debug) Logging.Debug(_getOpcodeDebugLabel(instruction, z, i));
                    yield return instruction;
                }
                
                // Add the skip label.
                instruction = newInstructions[z].WithLabels(skipLabel);
        
                if(debug) Logging.Debug(_getOpcodeDebugLabel(instruction, z));
                yield return instruction;
            }
            else // skip the index where we are inserting - we are replacing it anyways.
            {
                instruction = newInstructions[z];
        
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
[HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.ServerProcessCmd))]
internal static class StrangleServerProcessCmdFinishedTranspiler
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
        LocalBuilder ev = generator.DeclareLocal(typeof(StrangleFinishedEventArgs));
        int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Ldfld && (FieldInfo)x.operand == Field(typeof(Scp3114Strangle), nameof(Scp3114Strangle.Cooldown)));
        var injectedInstructions = new CodeInstruction[]
        {
            // ev = new StrangleFinishedPlayerArgs(this, SyncTarget (old one), 1)
            new (OpCodes.Ldarg_0),
            new (OpCodes.Call, PropertyGetter(typeof(Scp3114Strangle), nameof(Scp3114Strangle.SyncTarget))),
            new (OpCodes.Stloc_2),
            new (OpCodes.Ldloca_S, 2),
            new (OpCodes.Call, PropertyGetter(typeof(Nullable<Scp3114Strangle.StrangleTarget>), nameof(Nullable<Scp3114Strangle.StrangleTarget>.Value))),
            //new (OpCodes.Box),
            new (OpCodes.Ldarg_0),
            new (OpCodes.Ldfld, Field(typeof(Scp3114Strangle), nameof(Scp3114Strangle._postReleaseCooldown))),
            new (OpCodes.Newobj, GetDeclaredConstructors(typeof(global::Scp3114Mods.API.EventArgs.StrangleFinishedEventArgs))[0]),
            new (OpCodes.Stloc_S, ev.LocalIndex),
            new (OpCodes.Ldloc_S, ev.LocalIndex),
            new (OpCodes.Call,  Method(typeof(Events), nameof(Events.OnStrangleFinished))),
            new (OpCodes.Ldarg_0),
            new (OpCodes.Ldfld, Field(typeof(Scp3114Strangle), nameof(Scp3114Strangle.Cooldown))),
            new (OpCodes.Ldloc_S,ev.LocalIndex),
            new (OpCodes.Callvirt, PropertyGetter(typeof(StrangleFinishedEventArgs), nameof(StrangleFinishedEventArgs.StrangleCooldown))),
        };
        
        const bool debug = false;
        if(debug) Logging.Debug($"Patching Transpiler StrangleFinishedTranspilerPatch");
        for (int z = 0; z < newInstructions.Count; z++)
        {
            CodeInstruction instruction;
            if (z == index) // inject code at insert location
            {
                for (int i = 0; i < injectedInstructions.Length; i++)
                {
                    instruction = injectedInstructions[i];
                    
                    if(debug) Logging.Debug(_getOpcodeDebugLabel(instruction, z, i));
                    yield return instruction;
                }
                z += 2;
            }
            else // skip the index where we are inserting - we are replacing it anyways.
            {
                instruction = newInstructions[z];
        
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