// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         Scp3114Mods
//    Project:          Scp3114Mods
//    FileName:         3114SlapOnDamagingTranspiler.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/04/2023 10:13 PM
//    Created Date:     11/04/2023 10:13 PM
// -----------------------------------------

using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp3114;
using Scp3114Mods.API;
using Scp3114Mods.API.EventArgs;

using static HarmonyLib.AccessTools;
namespace Scp3114Mods.Internal.Patches.Transpilers;

[HarmonyPatch(typeof(Scp3114Slap), nameof(Scp3114Slap.DamagePlayers))]
public static class Scp3114SlapOnDamagingTranspiler 
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
        Label skipLabel = generator.DefineLabel();
        var local = generator.DeclareLocal(typeof(SlappingPlayerEventArgs));
        int index = newInstructions.FindIndex(x => x.Calls(PropertyGetter(typeof(ScpAttackAbilityBase<Scp3114Role>), nameof(ScpAttackAbilityBase<Scp3114Role>.DamageAmount))));
        int index2 = newInstructions.FindIndex(x => x.opcode == OpCodes.Ldfld && (FieldInfo)x.operand == Field(typeof(Scp3114Slap), nameof(Scp3114Slap._humeShield))) + 2;
        var injectedInstructions = new CodeInstruction[]
        {
            // ev = new SlappingPlayerArgs(this, targethub, damage amount, hume to regen, true)
            new (OpCodes.Ldc_R4, 25f),
            new (OpCodes.Ldc_I4_1),
            new (OpCodes.Newobj, GetDeclaredConstructors(typeof(SlappingPlayerEventArgs))[0]),
            new (OpCodes.Stloc_S, local),
            
            // Events.OnSlappingPlayer(ev);
            new (OpCodes.Ldloc_S, local),
            new (OpCodes.Call, Method(typeof(Events),nameof(Events.OnSlappingPlayerEventArgs))),
            new (OpCodes.Ldloc_S, local),
            
            // if (!ev.IsAllowed) return;
            new (OpCodes.Callvirt, PropertyGetter(typeof(SlappingPlayerEventArgs), nameof(SlappingPlayerEventArgs.IsAllowed))),
            new (OpCodes.Brtrue_S, skipLabel),
            new (OpCodes.Ret),
            
            // DamagePlayer(primaryTarget, ev.DamageAmount);
            new (OpCodes.Ldarg_0),
            new (OpCodes.Ldloc_1),
            new (OpCodes.Ldloc_S, local),
            new (OpCodes.Callvirt, PropertyGetter(typeof(SlappingPlayerEventArgs), nameof(SlappingPlayerEventArgs.DamageAmount)))
        };

        CodeInstruction[] injectedInstructions2 = new CodeInstruction[]
        {
            new(OpCodes.Ldloc_S, local),
            new(OpCodes.Callvirt, PropertyGetter(typeof(SlappingPlayerEventArgs),nameof(SlappingPlayerEventArgs.HumeShieldToGive)))
        };
        
        const bool debug = false;
        if(debug) Logging.Debug($"Patching Scp3114SlapOnDamagingTranspiler [Index 1: {index}], [Index 2: {index2}]");
        for (int z = 0; z < newInstructions.Count; z++)
        {
            if (z == index) // inject code at insert location
            {
                // return original then inject for the first index.
                yield return newInstructions[z].Log(z, -1, debug, false);
                for (int i = 0; i < injectedInstructions.Length; i++)
                {
                    var instruction = (i != 10) ? injectedInstructions[i] : injectedInstructions[i].WithLabels(skipLabel);
                    yield return instruction.Log(z, i, debug, false);
                }
            }
            else if (z == index2)
            {
                // skip the original instruction
                for (int i = 0; i < injectedInstructions2.Length; i++)
                {
                    yield return injectedInstructions2[i].Log(z, i, debug, false);
                }
            }
            else // skip the index where we are inserting - we are replacing it anyways.
            {
                yield return newInstructions[z].Log(z, -1, debug, false);
            }
        }

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }
}