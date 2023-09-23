using System.Collections.Generic;
using Mono.Cecil;
using System;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Myriad.PreloadPatches;

public class MyriadPreloadPatches {

    private static ManualLogSource logger = Logger.CreateLogSource("Myriad.PreloadPatch");
    
    // List of assemblies to patch
    public static IEnumerable<string> TargetDLLs { get; } = new[] { "Rewired_Core.dll", "Rewired_Windows.dll" };

    private static bool patched_Rewired_Core = false;
    private static bool patched_Rewired_Windows = false;
    
    // Patches the assemblies
    public static void Patch(AssemblyDefinition assembly) {
        LoadSDL2DependenciesEarly();
        
        try {
            if (assembly.FullName.Contains("Rewired_Core") && !patched_Rewired_Core) {
                foreach (var module in assembly.Modules) {
                    patch_BBctKivJxEjGKRzyEkHSRjJoJqnW_zQQfvDZMmpVqPPLYlLuSJXXpwJcI(module);
                    
                    patch_ConfigVars_DoesPlatformUseSDL2(module);
                }

                patched_Rewired_Core = true;
            }

            if (assembly.FullName.Contains("Rewired_Windows") && !patched_Rewired_Windows) {
                foreach (var module in assembly.Modules) {
                    //patch_QhRUnWbULvmdFTzNHeLFXfWeuhyi(module);
                }

                patched_Rewired_Windows = true;
            }

            logger.LogMessage("Patching has been finished");
        } catch (Exception e) {
            logger.LogError("It seems something has gone wrong with preload patching! Things will be broken!");
            logger.LogError(e);
        }
        
        if (!patched_Rewired_Core) {
            logger.LogWarning("Unable to patch [Rewired_Core], something must have gone wrong or they could not be located!");
        }
        
        if (!patched_Rewired_Windows) {
            logger.LogWarning("Unable to patch [Rewired_Core], something must have gone wrong or they could not be located!");
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------
    
    // Patch to make ReInput forcefully use SDL2 instead of any other Input Manager
    //
    // Class: [ ConfigVars ], Method: [ DoesPlatformUseSDL2 ]
    public static void patch_ConfigVars_DoesPlatformUseSDL2(ModuleDefinition module) {
        try {
            TypeDefinition classDef = module.Types
                .First(t => t.FullName.Contains("ConfigVars"));
            
            MethodDefinition methodDef = classDef.Methods
                .First(m => m.Name == "DoesPlatformUseSDL2");

            methodDef.Body
                .SimplifyMacros(); //Call to fix issues with offset or something idk, allows for the replacement of any int now but idk why!!!!

            Collection<Instruction> instructions = methodDef.Body.Instructions;

            var methodProcess = methodDef.Body.GetILProcessor();
                        
            methodProcess.InsertBefore(instructions[0], Instruction.Create(OpCodes.Ldc_I4_1));
            methodProcess.InsertBefore(instructions[1], Instruction.Create(OpCodes.Ret));

            methodDef.Body.OptimizeMacros(); // Just incase
        } catch (Exception e) {
            logger.LogError("It seems something has gone wrong with preload patching! Things will be broken!");
            logger.LogError(e);
        }
    }

    /// <summary>
    /// Method used to force load SDL2 from myriad files to allow for such to be found within Rewired
    /// </summary>
    /// <exception cref="IOException"></exception>
    private static void LoadSDL2DependenciesEarly() {
        
        // Location 1: Base Rainworld Folder
        string executionBasePath = AppDomain.CurrentDomain.BaseDirectory;
        // Location 2: Internal Mod location
        string internalSDL2Path = Path.Combine(executionBasePath, Path.Combine(new [] { "RainWorld_Data", "StreamingAssets", "mods", "myriad" }));
        
        logger.LogWarning(executionBasePath);
        
        List<string> listPathParts = new List<string>(executionBasePath.Split(Path.DirectorySeparatorChar));

        // Remove the empty element at the start and then Backtracks two directories by two to get to the base 'steamapps' folder
        listPathParts.RemoveRange(listPathParts.Count - 3, 3);
        
        //Saves the Drive letter due to combine weird issues and such.
        string driveLetter = listPathParts[0];
        listPathParts.RemoveAt(0);
        
        //..\{steam_folder}\steamapps\
        string baseSteamLibPath = driveLetter + Path.DirectorySeparatorChar + Path.Combine(listPathParts.ToArray());
        
        // Location 3: Workshop location
        string workshopSDL2Path = Path.Combine(baseSteamLibPath, Path.Combine(new [] { "workshop", "content", "312520", "3029456904" }));
        
        List<String> possiblePaths = new List<string>() {
            executionBasePath,       //..{steam_folder}\steamapps\common\Rain World\
            internalSDL2Path + "\\", //..{steam_folder}\steamapps\common\Rain World\RainWorld_Data\StreamingAssets\mods\myriad\
            workshopSDL2Path + "\\"  //..{steam_folder}\steamapps\workshop\content\312520\3029456904\
        };
        
        String? validPath = null;
        
        foreach (string possiblePath in possiblePaths){
            var pathOfFile = Path.Combine(possiblePath + "SDL2.dll");

            logger.LogMessage(pathOfFile);
            
            if (File.Exists(pathOfFile)) {
                validPath = pathOfFile;
                
                //break;
            }
        }
        
        if (validPath != null) {
            logger.LogMessage("Was able to locate SDL2 Control Library");
        } else {
            throw new IOException("Unable to find the Required SDL2.dll! It is required for the mod to function properly at all!");
        }

        if (LoadLibrary(validPath) == IntPtr.Zero) {
            logger.LogError(new Win32Exception(Marshal.GetLastWin32Error()).Message);
        
            logger.LogError($"Failed to load {validPath}, verify that the file exists and is not corrupted.");
            logger.LogError("Make sure you downloaded the correct version of SDL2 for the games Execution Environment i.e 32bit");
        } else {
            logger.LogMessage("Loaded SDL2 Properly within the Game!");
        }
    }
    
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);
    
    //---------------------------------------------------------------------------------------------------------------------------------

    // Patch Used to adjust the amount of players by injecting a method to handle such
    //
    // Class: [ BBctKivJxEjGKRzyEkHSRjJoJqnW ], Method: [ zQQfvDZMmpVqPPLYlLuSJXXpwJcI ]
    public static void patch_BBctKivJxEjGKRzyEkHSRjJoJqnW_zQQfvDZMmpVqPPLYlLuSJXXpwJcI(ModuleDefinition module) {
        try {
            TypeDefinition classDef = module.Types.First(t => t.FullName == "BBctKivJxEjGKRzyEkHSRjJoJqnW");

            logger.LogMessage($"Class: {classDef}");

            MethodDefinition methodDef = classDef.Methods.First(m => m.Name == "zQQfvDZMmpVqPPLYlLuSJXXpwJcI");

            Collection<Instruction> instructions = methodDef.Body.Instructions;

            Instruction instructionBeforeTarget = instructions.First(i => i.OpCode == OpCodes.Ret);

            int indexOfRet = instructions.IndexOf(instructionBeforeTarget);

            Instruction targetInstruction = instructions[indexOfRet + 2]; //instructions[0];

            ILProcessor methodProcess = methodDef.Body.GetILProcessor();

            methodProcess.InsertBefore(
                targetInstruction,
                Instruction.Create(
                    OpCodes.Call,
                    module.ImportReference(typeof(MyriadPreloadPatches).GetMethod("hook_BeforePlayerListGeneration"))
                )
            );
        } catch (Exception e) {
            logger.LogError("Patch [patch_BBctKivJxEjGKRzyEkHSRjJoJqnW_zQQfvDZMmpVqPPLYlLuSJXXpwJcI] has seemingly thrown an exception!");
            logger.LogError(e);
        }
    }
    
    /* Method that is patched into [Rewired_Core] within the [BBctKivJxEjGKRzyEkHSRjJoJqnW] class within the [zQQfvDZMmpVqPPLYlLuSJXXpwJcI] method
     *
     * Such method seems to be where the players are initialized after some other stuff happens
     */
    public static void hook_BeforePlayerListGeneration() {
        try {
            Type.GetType("Myriad.PreloadPatches.RewiredAdjustUserData")
                .GetMethod("adjustData")
                .Invoke(null, Array.Empty<object>());

            logger.LogMessage("The BeforePlayerListGeneration hook has worked, Rewired should now have more players!");
        } catch (Exception e) {
            logger.LogError("The BeforePlayerListGeneration has gone wrong meaning things will be broken!");
            logger.LogError(e.ToString());
        }
    }
    
    //---------------------------------------------------------------------------------------------------------------------------------

    //Old XInput Adjustment code
    
    /*private static int controllerCount = 16;

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
         #1#

        List<String> patchableMethods = new(new[] {
            ".ctor", "deviceCount", "Initialize", "Update", "OnDestroy", "mgNyFCYOmaDhuDlmPtVuuEkxCLGY", "CUkyANOuFqmdjVxOuUXHFiMmRdqm",
            "OThhbPDYAeefCPgboVjnGYlZfAHF", "mOwoiTszqUSvvufezLHkDmyvQZyn"
        });

        TypeDefinition classDef = module.Types.First(t => t.FullName == "QhRUnWbULvmdFTzNHeLFXfWeuhyi");

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
            logger.LogError(e);
        }
    }


    private static void Replace4WithMore(MethodDefinition methodDef) {
        ReplaceNumWithNum(4, playerCount, methodDef);
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
            logger.LogWarning(
                $"A method had NONE adjustments made to account for increased XInput Capabilities: [Adjustments #: {x}, Method: {methodDef.DeclaringType.Name}::{methodDef.Name}]");
        } else {
            logger.LogInfo(
                $"A method had adjustments made to account for increased XInput Capabilities: [Adjustments #: {x}, Method: {methodDef.DeclaringType.Name}::{methodDef.Name}]");
        }

        methodDef.Body.OptimizeMacros(); // Just incase
    }*/
    
    //---------------------------------------------------------------------------------------------------------------------------------

    
}