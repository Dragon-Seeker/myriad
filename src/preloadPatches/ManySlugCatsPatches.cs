using System.Collections.Generic;
using Mono.Cecil;
using System;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace ManySlugCats.PreloadPatches;

public class ManySlugCatsPatches {
    
    private static ManualLogSource logger = Logger.CreateLogSource("ManySlugCats.PreloadPatch");

    // List of assemblies to patch
    public static IEnumerable<string> TargetDLLs  { get; } = new[] {"Rewired_Core.dll"};

    // Patches the assemblies
    public static void Patch(AssemblyDefinition assembly) {
        // Patcher code here

        try {
            foreach (var module in assembly.Modules) {
                patch_BBctKivJxEjGKRzyEkHSRjJoJqnW_zQQfvDZMmpVqPPLYlLuSJXXpwJcI(module);
            }
            
            logger.LogMessage("Patching has been finished");
        } catch (Exception e) {
            logger.LogError("It seems something has gone wrong with preload patching! Things will be broken!");
            throw e;
        }
    }

    //class: [ BBctKivJxEjGKRzyEkHSRjJoJqnW ], Method: [ zQQfvDZMmpVqPPLYlLuSJXXpwJcI ]
    public static void patch_BBctKivJxEjGKRzyEkHSRjJoJqnW_zQQfvDZMmpVqPPLYlLuSJXXpwJcI(ModuleDefinition module) {
        try {
            TypeDefinition classDef = module.Types.First(t => t.FullName == "BBctKivJxEjGKRzyEkHSRjJoJqnW");
        
            logger.LogMessage($"Class: {classDef}");
        
            MethodDefinition methodDef = classDef.Methods.First(m => m.Name == "zQQfvDZMmpVqPPLYlLuSJXXpwJcI");
        
            Collection<Instruction> instructions = methodDef.Body.Instructions;
        
            // for (int i = 0; i < instructions.Count; i++)
            // {
            //     logger.LogMessage($"Instruction {i}: {instructions[i]}");
            // }
        
            var instructionBeforeTarget = instructions.First(i => i.OpCode == OpCodes.Ret);
        
            int indexOfRet = instructions.IndexOf(instructionBeforeTarget);

            var targetInstruction = instructions[indexOfRet + 2]; //instructions[0];
        
            var methodProcess = methodDef.Body.GetILProcessor();

            methodProcess.InsertBefore(
                targetInstruction,
                Instruction.Create(
                    OpCodes.Call,
                    module.ImportReference(typeof(ManySlugCatsPatches).GetMethod("hook_BeforePlayerListGeneration"))
                )
            );
        
            // for (int i = 0; i < indexOfRet + 7; i++)
            // {
            //     Console.WriteLine($"Instruction {i}: {instructions[i]}");
            // }
        } catch (Exception e) {
            logger.LogError("Patch [patch_BBctKivJxEjGKRzyEkHSRjJoJqnW_zQQfvDZMmpVqPPLYlLuSJXXpwJcI] has seemingly thrown an exception!");

            throw e;
        }
    }

    /* Method that is patched into [Rewired_Core] within the [BBctKivJxEjGKRzyEkHSRjJoJqnW] class within the [zQQfvDZMmpVqPPLYlLuSJXXpwJcI] method
     *
     * Such method seems to be where the players are initialized after some other stuff happens
     */
    public static void hook_BeforePlayerListGeneration() {
        try {
            Type.GetType("ManySlugCats.PreloadPatches.RewiredAdjustUserData")
                .GetMethod("adjustData")
                .Invoke(null, Array.Empty<object>());
            
            logger.LogMessage("The BeforePlayerListGeneration hook has worked, Rewired should now have more players!");
        } catch (Exception e) {
            logger.LogError("The BeforePlayerListGeneration has gone wrong meaning things will be broken!");
            logger.LogError(e.ToString());
        }
    }
}