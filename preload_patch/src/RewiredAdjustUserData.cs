using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using Rewired;
using Rewired.Data;
using Rewired.Utils;

namespace Myriad.PreloadPatches;

public class RewiredAdjustUserData {
    private static ManualLogSource logger = Logger.CreateLogSource("Myriad.PlayerInjection");
    
    private static readonly BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    
    public static int totalPlayerCount = PreloadMaxPlayerSettings.getPlayerCap();

    public static void adjustData() {
        if (totalPlayerCount <= 4) {
            logger.LogMessage("No adjustments will be applied!");
            return;
        }
        
        logger.LogMessage("Attempting to adjust Player Data!");
        logger.LogMessage(totalPlayerCount);

        var usrDataProp = typeof(ReInput).GetProperty("UserData", flags); 
        
        UserData userData = (UserData) usrDataProp.GetValue(null);//rainWorld.rewiredInputManager.userData; 

        if (userData == null) {
            logger.LogError("Attempted to adjust the userData but found it to be null somehow! Players will not be injected leading to issues");
        
            throw new NullReferenceException("[Myriad] Could not find [UserData] thru reflection or it is null!");
        }

        FieldInfo info = userData.GetType().GetField("players", BindingFlags.Instance | BindingFlags.NonPublic);

        if (info == null) {
            logger.LogError("Attempted to adjust the players but found it to be null somehow! Players will not be injected leading to issues");
            
            throw new NullReferenceException("[Myriad] Could not find [players] thru reflection or it is null!");
        }
    
        List<Player_Editor> playerList = (List<Player_Editor>)info.GetValue(userData);
    
        logger.LogMessage("Obtained PlayerList, attempting to adjust list to proper size!");
        
        Player_Editor templatePlayerEditor = playerList[5]; //OKAY THIS REALLY NEEDS TO BE 5
    
        playerList[5] = null;
        
        for (int i = 5; i < totalPlayerCount; i++) playerList.Add(null);
        
        adjustPlayerID(templatePlayerEditor, totalPlayerCount + 1); //9
        
        playerList.Add(templatePlayerEditor);
        
        logger.LogMessage("Add needed null space within the main list!");
        
        //
        
        for (int i = 5; i <= totalPlayerCount; i++) {
            userData.GetNewPlayerId();

            var template = playerList[4];
            
            var playerName = StringTools.IterateName(template.name, names: getPlayerNames(playerList));
            
            playerList[i] = cloneNewEditor(template, playerName, i);
        }

        logger.LogMessage("Player data has been injected without issue!");
    }

    public static void adjustPlayerID(Player_Editor editor, int id) {
        var baseType = editor.GetType();

        if (baseType == null) throw new Exception("Something has gone very wrong!");
        
        baseType.GetProperty("id", flags).SetValue(editor, id);
    }

    private static Player_Editor cloneNewEditor(Player_Editor basePlayerEditor, String playerName, int id) {
        Player_Editor playerEditor = basePlayerEditor.Clone();
    
        var baseType = playerEditor.GetType();
    
        if (baseType == null) throw new NullReferenceException("Unable to clone the [Player_Editor] to add more Players!");
    
        logger.LogMessage("Player " + id + " has been added!");
            
        baseType.GetProperty("id", flags).SetValue(playerEditor, id); //playerEditor.id = userData.GetNewPlayerId();
        baseType.GetProperty("name", flags).SetValue(playerEditor, playerName);
        baseType.GetProperty("descriptiveName", flags).SetValue(playerEditor, playerName);
        baseType.GetProperty("assignMouseOnStart", flags).SetValue(playerEditor, false); //playerEditor.assignMouseOnStart = false;
    
        return playerEditor;
    }
    
    private static string[] getPlayerNames(List<Player_Editor> playerList) {
        List<String> playerNames = new List<string>();
    
        for (int index = 0; index < playerList.Count; ++index) {
            var player = playerList[index];
    
            if (player != null) playerNames.Add(player.name);
        }

        return playerNames.ToArray();
    }
    
    private static void logList<T>(List<T> objects) {
        foreach (T o in objects) {
            if (o is Player_Editor.Mapping mapping) {
                logger.LogMessage(mapping.ToString());
            } else if (o is Player_Editor.CreateControllerInfo info) {
                logger.LogMessage($"Id:{info.sourceId}, Tag:{info.tag}");
            }
        }
    }
}