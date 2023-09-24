using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Myriad; 

/// <summary>
/// This class acts as a gate to getting the current max player settings once, which will then be used within the Preload Patch first.
///
/// If not the value will be accessed only once and then saved forever... so to prevent mismatching data with loading the file more than once.
///
/// Should be fine to class load such if need be due to it containing no game depended files meaning its safe for a preload patch.
/// </summary>
public class PreloadMaxPlayerSettings {
    public const int defMaxCap = 16; // Change within future to be a lower number again?

    //public static readonly string SavePath = Path.Combine(Application.dataPath, "Myriad", "Myriad.json");
    public static readonly string SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Videocult", "Rain World", "Myriad", "Myriad.json");
    
    public class MyData {
        public int pCap;
    }

    [CanBeNull] 
    private static MyData playerSettings = null;
    
    public static MyData LoadSettings() {
        if (playerSettings == null) {
            playerSettings = File.Exists(SavePath) 
                ? JsonConvert.DeserializeObject<MyData>(File.ReadAllText(SavePath)) 
                : new MyData() { pCap = defMaxCap };
        }

        return playerSettings;
    }

    public static int getPlayerCap() {
        return LoadSettings().pCap;
    }

    public static void SaveSettings(int playerCount) {
        var data = new MyData() { pCap = playerCount };

        Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!); //CREATE DIRECTORY IF IT DOESNT EXIST
        
        File.WriteAllText(SavePath, JsonConvert.SerializeObject(data));
    }
}