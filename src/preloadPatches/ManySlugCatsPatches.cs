using System.Collections.Generic;
using Mono.Cecil;
using System;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace ManySlugCats.PreloadPatches;

public class ManySlugCatsPatches
{
    private static ManualLogSource logger = Logger.CreateLogSource("ManySlugCats.PreloadPatch");
    
    public static bool playersHaveBeenInjected = false;
    
    // List of assemblies to patch
    public static IEnumerable<string> TargetDLLs  { get; } = new[] {"Rewired_Core.dll"};

    // Patches the assemblies
    
    //class: [ BBctKivJxEjGKRzyEkHSRjJoJqnW ], Method: [ zQQfvDZMmpVqPPLYlLuSJXXpwJcI ]
    public static void Patch(AssemblyDefinition assembly)
    {
        // Patcher code here

        foreach (var module in assembly.Modules)
        {
            
            // foreach (var type in module.Types)
            // {
            //     Console.WriteLine($"Class: {type}");
            // }
                
            // {
            //     TypeDefinition loggerDef =
            //         module.Types.First(t => t.FullName == "Rewired.Logger");
            //
            //     MethodDefinition methodDef = loggerDef.Methods.First(m => m.Name == "Log" && m.Parameters.Capacity == 2);
            //
            //     var instructions = methodDef.Body.Instructions;
            //
            //     var methodProcessor = methodDef.Body.GetILProcessor();
            //     
            //     methodProcessor.InsertBefore(
            //         instructions[0], 
            //         methodProcessor.Create(
            //             OpCodes.Call,
            //             module.ImportReference(typeof(ManySlugCatsPatches).GetMethod("InjectedMethod"))
            //         )
            //     );
            //     
            //     // for (int i = 0; i < instructions.Count; i++)
            //     // {
            //     //     Console.WriteLine($"Instruction {i}: {instructions[i]}");
            //     // }
            // }
            {
                TypeDefinition classDef =
                    module.Types.First(t => t.FullName == "BBctKivJxEjGKRzyEkHSRjJoJqnW");
            
                Console.WriteLine($"Class: {classDef}");
            
                MethodDefinition methodDef = classDef.Methods.First(m => m.Name == "zQQfvDZMmpVqPPLYlLuSJXXpwJcI");
            
                Collection<Instruction> instructions = methodDef.Body.Instructions;
            
                // for (int i = 0; i < instructions.Count; i++)
                // {
                //     Console.WriteLine($"Instruction {i}: {instructions[i]}");
                // }
            
                var instructionBeforeTarget = instructions.First(i => i.OpCode == OpCodes.Ret);
            
                int indexOfRet = instructions.IndexOf(instructionBeforeTarget);

                var targetInstruction = instructions[0];//instructions[indexOfRet + 1];
            
                var methodProcess = methodDef.Body.GetILProcessor();
            
                // new MethodDefinition(
                //     "injectedMethod", 
                //     MethodAttributes.Public | MethodAttributes.Static, 
                //     module.ImportReference(typeof(void))
                // )
            
                methodProcess.InsertBefore(
                    targetInstruction,
                    Instruction.Create(
                        OpCodes.Call,
                        module.ImportReference(typeof(ManySlugCatsPatches).GetMethod("AddToRewiredPlayerEditorList"))
                    )
                );
            
                // for (int i = 0; i < indexOfRet + 7; i++)
                // {
                //     Console.WriteLine($"Instruction {i}: {instructions[i]}");
                // }
            
                logger.LogMessage("Finished Patching!!!");
            }
        }
        
    }

    //Method that adds the needed players by calling a helper method
    public static void AddToRewiredPlayerEditorList()
    {
        playersHaveBeenInjected = true;
        
        // Console.WriteLine("I HAVE BEEN INJECTED!!!!!!");
        try
        {
            Type.GetType("ManySlugCats.PreloadPatches.Test")
                .GetMethod("addStuff")
                .Invoke(null, Array.Empty<object>());
        }
        catch (Exception e)
        {
            logger.LogError("The Player Injection method has gone wrong meaning things will be broken!");
        }
        
        

        
    }
}