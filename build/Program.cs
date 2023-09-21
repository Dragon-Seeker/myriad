using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Solution.Project;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.Incubator.Project;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using JsonSerializer = System.Text.Json.JsonSerializer;


public class ProjectSettings {

    public static ProjectSettings INSTANCE = null;

    public String? PROJECT_NAME = null;
    //public String? GAME_VER = null; 
    public String? MOD_VER = null;

    //--

    public String ASSETS_DIR = "assets/";
    
    public String PLUGINS_DIR = "plugins/";
    public String PATCHER_DIR = "patchers/";
    
    public String? RainWorldDir = Environment.GetEnvironmentVariable("RainWorldDir");

    public String? MOD_COPY_TO_PATH;
    
    public ProjectSettings(DirectoryPath path) {
        string fileName = path.Combine(ASSETS_DIR).GetFilePath("modinfo.json").FullPath;

        string stuff = File.ReadAllText(fileName);
        
        Console.WriteLine(stuff);
        
        JsonObject? jsonObject = JsonSerializer.Deserialize<JsonObject>(stuff, options: null);
        
        if (jsonObject != null) {
            if (jsonObject.ContainsKey("id")) {
                PROJECT_NAME = jsonObject["id"].GetValue<String>();
            } 
            if (jsonObject.ContainsKey("version")) {
                MOD_VER = jsonObject["version"].GetValue<String>();
            }
        }
        
        MOD_COPY_TO_PATH = RainWorldDir != null ? $"{RainWorldDir}/RainWorld_Data/StreamingAssets/mods/{PROJECT_NAME}" : null;
    }

    public static ProjectSettings create(DirectoryPath path) {
        INSTANCE = new ProjectSettings(path);

        return INSTANCE;
    }
    
   
}

public class ModInfo {
    
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

        ProjectSettings.create(new DirectoryPath("../main"));
    }
}

[IsDependentOn(typeof(ZipMod))]
public sealed class Default : FrostingTask {}

[TaskName("ZipMod")]
[IsDependentOn(typeof(CopyToDirectories))]
public sealed class ZipMod : FrostingTask<BuildContext> {
    public override void Run(BuildContext context) {
        string zipFileLocation =
            $"../output/versions/{ProjectSettings.INSTANCE.PROJECT_NAME}-{ProjectSettings.INSTANCE.MOD_VER}.zip";
        
        if (context.FileExists(zipFileLocation)) {
            context.DeleteFile(zipFileLocation);
        }
        
        context.Zip("../output/temp", zipFileLocation);
    }
}

[TaskName("CopyToRainworldDir")]
[IsDependentOn(typeof(BuildTask))]
public sealed class CopyToDirectories : FrostingTask<BuildContext> {
    public override void Run(BuildContext context) {
        if(ProjectSettings.INSTANCE.MOD_COPY_TO_PATH != null) copyToPath(context, new DirectoryPath(ProjectSettings.INSTANCE.MOD_COPY_TO_PATH), false);
        copyToPath(context, new DirectoryPath($"../output/temp/{ProjectSettings.INSTANCE.PROJECT_NAME}"), true);
    }

    private void copyToPath(BuildContext context, DirectoryPath output, bool cleanOutput) {
        if(cleanOutput) context.CleanDirectory(output);
        
        context.CreateDirectory(output);
        
        // **/*.*
        context.CopyDirectory(
            context.projectPath.Combine(new DirectoryPath(ProjectSettings.INSTANCE.ASSETS_DIR)),
            output
        );
        
        context.CreateDirectory(output.Combine(ProjectSettings.INSTANCE.PLUGINS_DIR));
        
        context.CopyFileToDirectory(
            context.mainProjectData.OutputPaths[0].FullPath + $"/{context.mainProjectData.AssemblyName}.dll", 
            output.Combine(ProjectSettings.INSTANCE.PLUGINS_DIR)
        );

        context.CreateDirectory(output.Combine(ProjectSettings.INSTANCE.PATCHER_DIR));
        
        context.CopyFileToDirectory(
            context.preloadProjectData.OutputPaths[0].FullPath + $"/{context.preloadProjectData.AssemblyName}.dll",
            output.Combine(ProjectSettings.INSTANCE.PATCHER_DIR)
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

