using System.Collections.Generic;
using Mono.Cecil;
using System;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using MonoMod.Cil;
using MonoMod.Utils;
using MonoMod.Utils.Cil;

namespace ManySlugCats.PreloadPatches;

public class ManySlugCatsPatches {
    
    private static ManualLogSource logger = Logger.CreateLogSource("ManySlugCats.PreloadPatch");
    public static int myCount = 16;

    // List of assemblies to patch
    public static IEnumerable<string> TargetDLLs  { get; } = new[] {"Rewired_Core.dll", "Rewired_Windows.dll"};

    private static bool patched_Rewired_Core = false;
    private static bool patched_Rewired_Windows = false;
    
    // Patches the assemblies
    public static void Patch(AssemblyDefinition assembly) {
        // Patcher code here

        // logger.LogMessage(assembly.FullName);
        // logger.LogMessage(assembly.ToString());

        // if (Type.GetType("QhRUnWbULvmdFTzNHeLFXfWeuhyi, Rewired_Windows") != null) {
        //     logger.LogError("FUCK");
        //     logger.LogError("FUCK");
        //     logger.LogError("FUCK");
        //     logger.LogError("FUCK");
        //     logger.LogError("FUCK");
        //     logger.LogError("FUCK");
        //     logger.LogError("FUCK");
        //     logger.LogError("FUCK");
        //     logger.LogError("FUCK");
        // }
        
        try {
            if (assembly.FullName.Contains("Rewired_Core") && !patched_Rewired_Core) {
                foreach (var module in assembly.Modules) {
                    patch_BBctKivJxEjGKRzyEkHSRjJoJqnW_zQQfvDZMmpVqPPLYlLuSJXXpwJcI(module);
                }

                patched_Rewired_Core = true;
            }

            if (assembly.FullName.Contains("Rewired_Windows") && !patched_Rewired_Windows) {
                foreach (var module in assembly.Modules) {
                    patch_QhRUnWbULvmdFTzNHeLFXfWeuhyi(module);
                }

                patched_Rewired_Windows = true;
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

    private static int controllerCount = 16;
    
    //class: [ QhRUnWbULvmdFTzNHeLFXfWeuhyi ], Method: [  ]
    public static void patch_QhRUnWbULvmdFTzNHeLFXfWeuhyi(ModuleDefinition module) {
        /*
         * ctor : 2
         * deviceCount : 1 *****
         * Initialize : 2
         * Update : 1
         * OnDestroy : 1
         * mgNyFCYOmaDhuDlmPtVuuEkxCLGY : 1
         * CUkyANOuFqmdjVxOuUXHFiMmRdqm : 1
         * OThhbPDYAeefCPgboVjnGYlZfAHF : 1
         * mOwoiTszqUSvvufezLHkDmyvQZyn : 3
         */

        List<String> patchableMethods = new (new []{".ctor", "deviceCount", "Initialize", "Update", "OnDestroy", 
            "mgNyFCYOmaDhuDlmPtVuuEkxCLGY", "CUkyANOuFqmdjVxOuUXHFiMmRdqm", "OThhbPDYAeefCPgboVjnGYlZfAHF", "mOwoiTszqUSvvufezLHkDmyvQZyn"});
        
        TypeDefinition classDef = module.Types.First(t => t.FullName == "QhRUnWbULvmdFTzNHeLFXfWeuhyi");

        // foreach (MethodDefinition methodDefinition in classDef.Methods) {
        //     logger.LogMessage(methodDefinition);
        // }
        
        foreach (string patchableMethod in patchableMethods) {
            try {
                MethodDefinition methodDef;

                if (patchableMethod == "deviceCount") {
                    var propertyDef = classDef.Properties.First(p => p.Name == "deviceCount");

                    methodDef = propertyDef.GetMethod;
                } else {
                    methodDef = classDef.Methods.First(m => m.Name == patchableMethod);
                }

                Replace4WithMore(methodDef);
            } catch (Exception e) {
                logger.LogError($"An attempt at patching [{patchableMethod}] has failed and threw an exception!");
                logger.LogError(e.ToString());
            }
        }

        try {
            TypeDefinition classDef2 = classDef.NestedTypes.First(t => t.FullName.Contains("hTqbXbSaCGbbKiuhgXToQLOuaIxwA"));

            MethodDefinition methodDef2 = classDef2.Methods.First(m => m.Name == ".ctor");
            
            ReplaceNumWithNum(5, 9, methodDef2);
        } catch (Exception e) {
            logger.LogError("An attempt at patching [hTqbXbSaCGbbKiuhgXToQLOuaIxwA::ctor] has failed and threw an exception!");
            logger.LogError(e.ToString());
        }
    }

    
    private static void Replace4WithMore(MethodDefinition methodDef) {
        ReplaceNumWithNum(4, myCount, methodDef);
    }

    private static void ReplaceNumWithNum(int targetNum, int outputNum, MethodDefinition methodDef) {
        methodDef.Body.SimplifyMacros(); //Call to fix issues with offset or something idk, allows for the replacement of any int now but idk why!!!!

        var x = 0;

        var methodProcess = methodDef.Body.GetILProcessor();
        
        Collection<Instruction> methodInstructions = methodDef.Body.Instructions;
        
        bool stillChangingInstructions = true;

        while (stillChangingInstructions) {
            Instruction changingInstruction = null;
            
            for (int i = 0; i < methodInstructions.Count; i++) {
                var methodInstruction = methodInstructions[i];

                if (methodInstruction.MatchLdcI4(targetNum)) {
                    changingInstruction = methodInstruction;
                    
                    break;
                }
            }

            if (changingInstruction != null) {
                x++;

                methodProcess.Replace(changingInstruction, Instruction.Create(OpCodes.Ldc_I4, outputNum));
            } else {
                stillChangingInstructions = false;
            }
        }

        if (x == 0) {
            logger.LogWarning($"A method had NONE adjustments made to account for increased XInput Capabilities: [Adjustments #: {x}, Method: {methodDef.DeclaringType.Name}::{methodDef.Name}]");
        } else {
            logger.LogInfo($"A method had adjustments made to account for increased XInput Capabilities: [Adjustments #: {x}, Method: {methodDef.DeclaringType.Name}::{methodDef.Name}]");
        }
        
        methodDef.Body.OptimizeMacros(); // Just incase
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

    public static void idk() {
        logger.LogError("WWWWWWWWWWWWWWWWWWWWWWEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEWWWWWWWWWWWWWWWWWWWWWWWWWWWWWOOOOOOOOOOOOOOOOOOO");
    }
}