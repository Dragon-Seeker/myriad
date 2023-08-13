using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Solution.Project;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.Incubator.Project;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Settings {
    public static String PROJECT_NAME = "myriad";
    public static String GAME_VER = ""; //+15.6
    public static String MOD_VER = "0.1.0";

    //--
    
    public static String RainWorldDir = Environment.GetEnvironmentVariable("RainWorldDir");

    public static String ASSETS_DIR = "assets/";
    
    public static String PLUGINS_DIR = "plugins/";
    public static String PATCHER_DIR = "patchers/";

    public static String MOD_COPY_TO_PATH = $"{RainWorldDir}/RainWorld_Data/StreamingAssets/mods/{PROJECT_NAME}";
}

public static class Program {
    
    public static int Main(string[] args) {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

public class BuildContext : FrostingContext {
    public DirectoryPath projectPath;
    public DirectoryPath outputPath;

    public CustomProjectParserResult mainProjectData;
    public CustomProjectParserResult preloadProjectData;
    public string MsBuildConfiguration { get; set; }

    public BuildContext(ICakeContext context) : base(context) {
        projectPath = context.MakeAbsolute(new DirectoryPath("../main"));
        outputPath = projectPath.Combine("bin/Debug");

        mainProjectData = context.ParseProject("../main/Myriad.csproj", "Debug");
        preloadProjectData = context.ParseProject("../preload_patch/Myriad_PreloadPatcher.csproj", "Debug");

        MsBuildConfiguration = context.Argument("configuration", "Debug");
    }
}

[IsDependentOn(typeof(ZipMod))]
public sealed class Default : FrostingTask {}

[TaskName("ZipMod")]
[IsDependentOn(typeof(CopyToDirectories))]
public sealed class ZipMod : FrostingTask<BuildContext> {
    public override void Run(BuildContext context) {
        context.DeleteFile($"../output/versions/{Settings.PROJECT_NAME}-{Settings.MOD_VER}.zip");
        
        context.Zip("../output/temp", $"../output/versions/{Settings.PROJECT_NAME}-{Settings.MOD_VER}.zip");
    }
}

[TaskName("CopyToRainworldDir")]
[IsDependentOn(typeof(BuildTask))]
public sealed class CopyToDirectories : FrostingTask<BuildContext> {
    public override void Run(BuildContext context) {
        copyToPath(context, new DirectoryPath(Settings.MOD_COPY_TO_PATH), false);
        copyToPath(context, new DirectoryPath($"../output/temp/{Settings.PROJECT_NAME}"), true);
    }

    private void copyToPath(BuildContext context, DirectoryPath output, bool cleanOutput) {
        if(cleanOutput) context.CleanDirectory(output);
        
        context.CreateDirectory(output);
        
        // **/*.*
        context.CopyDirectory(
            context.projectPath.Combine(new DirectoryPath(Settings.ASSETS_DIR)),
            output
        );
        
        context.CreateDirectory(output.Combine(Settings.PLUGINS_DIR));
        
        context.CopyFileToDirectory(
            context.mainProjectData.OutputPaths[0].FullPath + $"/{context.mainProjectData.AssemblyName}.dll", 
            output.Combine(Settings.PLUGINS_DIR)
        );

        context.CreateDirectory(output.Combine(Settings.PATCHER_DIR));
        
        context.CopyFileToDirectory(
            context.preloadProjectData.OutputPaths[0].FullPath + $"/{context.preloadProjectData.AssemblyName}.dll",
            output.Combine(Settings.PATCHER_DIR)
        );
    }
}

[TaskName("Build")]
[IsDependentOn(typeof(CleanTask))]
public sealed class BuildTask : FrostingTask<BuildContext> {
    public override void Run(BuildContext context) {
        var mainBuildSettings = new DotNetBuildSettings();

        mainBuildSettings.Configuration = context.MsBuildConfiguration;

        context.DotNetBuild("../main/Myriad.csproj", mainBuildSettings);
        
        //---
        
        /*String preloadPatchName = $"{Settings.PROJECT_NAME}_PreloadPatch";
        
        FilePath asssemblyPath = context.outputPath.CombineWithFilePath($"{preloadPatchName}.dll");
        FilePath symbolsPath = context.outputPath.CombineWithFilePath($"{preloadPatchName}.pdb");
        
        var arguments = new ProcessArgumentBuilder()
            .Append("/debug")
            .AppendSwitch("/target", ":", "library") // exe, winexe...
            .AppendSwitchQuoted("/out",":", asssemblyPath.FullPath)
            .AppendSwitchQuoted("/pdb",":", symbolsPath.FullPath)
            .Append("/recurse:*.cs");
        
        List<ProjectAssemblyReference> assemblyReferences = context.projectData.References.ToList();
        
        assemblyReferences.RemoveAll(reference => Settings.PRELOAD_PATCH_BLACKLIST.Contains(reference.Name));
        
        foreach (ProjectAssemblyReference refence in assemblyReferences) {
            if (refence.HintPath == null) {
                Console.WriteLine(refence.Name);
                
                continue;
            }
        
            String pathResolved = Settings.RainWorldDir + refence.HintPath.ToString().Split("$(RainWorldDir)")[1];
            
            Console.WriteLine(pathResolved);
            
            arguments.AppendSwitchQuoted("/reference", ":", pathResolved);
        }
        
        var result = context.StartProcess(
            "C:/Windows/Microsoft.NET/Framework/v4.0.30319/csc.exe", // needs to be full path if not in path
            new ProcessSettings {
                WorkingDirectory = context.projectPath.Combine("src/preloadPatches"),
                Arguments = arguments
            }
        );
        
        if (result != 0) {
            throw new Exception(string.Format("csc.exe exited with {0}", result));
        }*/
        
        var preloadBuildSettings = new DotNetBuildSettings();

        preloadBuildSettings.Configuration = context.MsBuildConfiguration;

        context.DotNetBuild("../preload_patch/Myriad_PreloadPatcher.csproj", preloadBuildSettings);
    }
}

[TaskName("Clean")]
public sealed class CleanTask : FrostingTask<BuildContext> {
    public override void Run(BuildContext context) {
        context.CleanDirectory($"../preload_patcher/bin/{context.MsBuildConfiguration}");
        context.CleanDirectory($"../main/bin/{context.MsBuildConfiguration}");
    }
}

