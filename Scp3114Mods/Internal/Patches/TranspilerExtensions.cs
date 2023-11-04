// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         TranspilerExtensions.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/03/2023 8:40 PM
//    Created Date:     11/03/2023 8:40 PM
// -----------------------------------------

using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Scp3114Mods.API;

namespace Scp3114Mods.Internal.Patches;

internal static class TranspilerExtensions
{
    internal static void Log(this CodeInstruction instruction, int index, int injectedIndex = -1, bool enable = true,
        bool debug = false)
    {
        if (!enable)
            return;
        Logging.Debug(GetOpcodeDebugLabel(instruction, index, injectedIndex, debug ? OutputIncludes.Debug : OutputIncludes.Basic));
    } 
    internal static string GetOpcodeDebugLabel(this CodeInstruction instruction, int index, int injectedIndex = -1, OutputIncludes output = OutputIncludes.Basic)
    {
        if (output == OutputIncludes.None)
            return "";
        
        string labelOperand = "";
        if (output.HasFlag(OutputIncludes.LabelOperand))
        {
            // if opcode has a label operand
            if (instruction.operand is Label label)
                labelOperand += $"-> ({label.GetHashCode()})";
        }

        if (output.HasFlag(OutputIncludes.FieldOperand))
        {
            // if opcode has a field operand
            if (instruction.operand is FieldInfo field)
                labelOperand += $"-> (<{field.FieldType.Name}> {field.Name})";
        }


        if (output.HasFlag(OutputIncludes.MethodOperand))
        {
            // if opcode has a method operand
            if (instruction.operand is MethodInfo method)
            {
                string paramList = "";
                foreach (ParameterInfo param in method.GetParameters())
                {
                    paramList += $" [{param.ParameterType.Name}]";
                }

                labelOperand += $"-> (<{method.ReturnType?.Name}> {method.Name}{paramList})";
            }
        }

        if (output.HasFlag(OutputIncludes.ArgOperand))
        {
            // if opcode has a argument operand
            if ((instruction.opcode == OpCodes.Ldarg_S || instruction.opcode == OpCodes.Ldarga_S || instruction.opcode == OpCodes.Starg_S || instruction.opcode == OpCodes.Ldarg || instruction.opcode == OpCodes.Starg_S)
                && instruction.operand is int arg)
            {
                labelOperand += $"-> ({arg})";
            }
        }
        if (output.HasFlag(OutputIncludes.ValueOperand))
        {
            // if opcode has a value operand
            if (instruction.operand?.GetType().IsValueType ?? false && labelOperand != "" )
            {
                if (instruction.operand is not Label)
                    labelOperand += $"-> (<{instruction.operand.GetType().Name}> {instruction.operand})";
            }
        }


        // adds labels
        string notedLabels = "";
        if (output.HasFlag(OutputIncludes.Labels))
        {
            notedLabels = instruction.labels.Count > 0  ? "   " : "";
            foreach (var x in instruction.labels)
            {
                notedLabels += $" [{x.GetHashCode()}]";
            }
        }

        string injectedString = injectedIndex < 0 ? "   " : $"+{injectedIndex:00}";
        string indexes = "";
        if (output.HasFlag(OutputIncludes.Index))
            indexes += $"[{index:000}{(output.HasFlag(OutputIncludes.InjectedIndex) ? $" {injectedString}" : "")}]";
        
        return $"{indexes} {instruction.opcode,-10}{labelOperand,-7}{notedLabels}";
    }

    [Flags]
    internal enum OutputIncludes
    {
        None = 0,
        All = ~0,
        Basic = 1 | 2 | 4 | 8,
        Debug = All,
        Index = 1,
        InjectedIndex = 2,
        Labels = 4,
        AllOperands = 8 | 16 | 32 | 64 | 128,
        LabelOperand = 8,
        FieldOperand = 16,
        MethodOperand = 32,
        ArgOperand = 64,
        ValueOperand = 128,
        
    }
}