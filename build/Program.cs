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
    
    public static List<String> PRELOAD_PATCH_BLACKLIST = new List<String>() {
        "Rewired_Windows",
        "Assembly-CSharp",
        "Assembly-CSharp-firstpass",
        "HOOKS-Assembly-CSharp",
        "UnityEngine.CoreModule",
        "UnityEngine.ImageConversionModule",
        "UnityEngine.InputLegacyModule",
        "com.rlabrecque.steamworks.net"
    };

    public static String RainWorldDir = Environment.GetEnvironmentVariable("RainWorldDir");

    public static String ASSETS_DIR = "/assets/";

    public static String MOD_COPY_TO_PATH = RainWorldDir + "/RainWorld_Data/StreamingAssets/mods/" + PROJECT_NAME;
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

    public CustomProjectParserResult projectData;
    public string MsBuildConfiguration { get; set; }

    public BuildContext(ICakeContext context) : base(context) {
        projectPath = context.MakeAbsolute(new DirectoryPath("../main"));
        outputPath = projectPath.Combine("bin/Debug");

        projectData = context.ParseProject("../main/ManySlugCats.csproj", "Debug");

        MsBuildConfiguration = context.Argument("configuration", "Debug");
    }
}

/*
 <Target Name="PostBuild-CopyToRainWorldDir" AfterTargets="PostBuildEvent" Condition="Exists('$(RainWorldDir)')">
    <!-- { Create Asset object for transfer later } -->
    <ItemGroup>
      <Assets Include="$(ProjectDir)/assets/**//*.*" />
    </ItemGroup>

    <!-- { Transfer main plugin portion of the Mod } -->
    <Copy SourceFiles="@(Assets)" DestinationFiles="$(RainWorldDir)/RainWorld_Data/StreamingAssets/mods/$(ProjectName)/%(RecursiveDir)%(Filename)%(Extension)" />
    <Copy SourceFiles="$(TargetPath)" DestinationFolder="$(RainWorldDir)/RainWorld_Data/StreamingAssets/mods/$(ProjectName)/plugins" />

    <!-- { Transfer prelaunch patchers of the Mod } -->
    <Copy SourceFiles="$(OutputPatchPath)" DestinationFolder="$(RainWorldDir)/RainWorld_Data/StreamingAssets/mods/$(ProjectName)/patchers" />
    </Target>
*/

[TaskName("CopyToRainworldDir")]
[IsDependentOn(typeof(PreloadPatchBuildTask))]
public sealed class CopyToRainworldDir : FrostingTask<BuildContext> {
    public override void Run(BuildContext context) {

        var copyDir = new DirectoryPath(Settings.MOD_COPY_TO_PATH);
        
        // **/*.*
        context.CopyDirectory(context.projectPath.Combine(new DirectoryPath(Settings.ASSETS_DIR)), new DirectoryPath(Settings.MOD_COPY_TO_PATH));
        context.CopyFileToDirectory(context.projectData.OutputPath.FullPath + context.projectData.AssemblyName, copyDir.Combine($"{Settings.PROJECT_NAME}/plugins"));
        
        String preloadPatchName = $"{Settings.PROJECT_NAME}_PreloadPatch";
        
        context.CopyFileToDirectory(context.outputPath.CombineWithFilePath($"{preloadPatchName}.dll"), copyDir.Combine($"{Settings.PROJECT_NAME}/patchers"));
    }
}

/*
  <Target Name="BuildPreloadPatches" AfterTargets="PostBuildEvent">
    <!-- { Getting the patch files to be included with the Patch DLL } -->
    <ItemGroup>
      <PreloadPatches Include="$(ProjectDir)/src/preloadPatches/*.cs" />
    </ItemGroup>

    <!-- { OutputPath combinded with the DLL name } -->
    <PropertyGroup>
      <OutputPatchPath>$(OutputPath)/$(ProjectName)_PreloadPatch.dll</OutputPatchPath>
    </PropertyGroup>
    
    <Message Text="Building Preload Patches" />
    <CSC Sources="@(PreloadPatches)" References="@(PatchReference)" TargetType="library" OutputAssembly="$(OutputPatchPath)" EmitDebugInformation="true" />
  </Target>
 */

[TaskName("PreloadPatchBuild")]
[IsDependeeOf(typeof(Default))]
public sealed class PreloadPatchBuildTask : FrostingTask<BuildContext> {
    
    public override void Run(BuildContext context) {
        String preloadPatchName = $"{Settings.PROJECT_NAME}_PreloadPatch";
        
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
        }
    }
}

[TaskName("Build")]
[IsDependentOn(typeof(CleanTask))]
public sealed class BuildTask : FrostingTask<BuildContext> {
    public override void Run(BuildContext context) {
        var buildSettings = new DotNetBuildSettings();

        buildSettings.Configuration = context.MsBuildConfiguration;

        context.DotNetBuild("../main/ManySlugCats.csproj", buildSettings);
    }
}

[TaskName("Clean")]
public sealed class CleanTask : FrostingTask<BuildContext> {
    public override void Run(BuildContext context) {
        context.CleanDirectory($"../main/bin/{context.MsBuildConfiguration}");
    }
}

[IsDependentOn(typeof(BuildTask))]
public sealed class Default : FrostingTask {}