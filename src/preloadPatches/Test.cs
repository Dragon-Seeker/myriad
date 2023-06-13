using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using Rewired;
using Rewired.Data;
using Rewired.Utils;

namespace ManySlugCats.PreloadPatches;

public class Test
{
    private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("PlayerInjection");
    
    public static void addStuff()
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        
        logger.LogMessage("ATTEMPTING TO ADD MORE PLAYERS!!!!");
        
        var reinputType = typeof(ReInput);
        
        var usrDataProp = reinputType.GetProperty("UserData", flags); 
        
        UserData userData = (UserData) usrDataProp.GetValue(null);//rainWorld.rewiredInputManager.userData; 
        
        if (userData == null)
        {
            logger.LogMessage("WELL IDK HOW THIS HAPPEND!");
        }
        else
        {
        
            FieldInfo info = userData.GetType().GetField("players", BindingFlags.Instance | BindingFlags.NonPublic);
        
            if (info == null) throw new Exception("SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS");
        
            List<Player_Editor> playerList = (List<Player_Editor>)info.GetValue(userData);
        
            var template = playerList[5];
        
            playerList[5] = null;
        
            playerList.Add(null);
            playerList.Add(null);
            playerList.Add(null);
        
            logger.LogMessage("Array Size = " + playerList.ToArray().Length);
            
            Action<Player_Editor, int> adjustId = (editor, id) =>
            {
                var baseType = editor.GetType();
        
                if (baseType == null) throw new Exception(" FUCK MY LIFE ");
                
                baseType.GetProperty("id", flags).SetValue(editor, id);
            };
        
            adjustId(template, 9);
            
            playerList.Add(template);
        
            Func<string[]> GetPlayerNames = () =>
            {
                List<String> playerNames = new List<string>();
        
                for (int index = 0; index < playerList.Count; ++index)
                {
                    var player = playerList[index];
        
                    if (player != null)
                    {
                        playerNames.Add(player.name);
                    }
                }
        
        
                return playerNames.ToArray();
            };
        
            Func<int, int, Player_Editor> create = (index, id) =>
            {
                Player_Editor playerEditor = playerList[index].Clone();
        
                var baseType = playerEditor.GetType();
        
                if (baseType == null) throw new Exception(" FUCK MY LIFE ");
        
                logger.LogMessage("Player " + id + " has been added!");
        
                userData.GetNewPlayerId();
                
                baseType.GetProperty("id", flags).SetValue(playerEditor, id); //playerEditor.id = userData.GetNewPlayerId();
                baseType.GetProperty("name", flags).SetValue(playerEditor,
                    StringTools.IterateName(playerEditor.name, names: GetPlayerNames())); //playerEditor.name = StringTools.IterateName(playerEditor.name, names: userData.GetPlayerNames());
                baseType.GetProperty("assignMouseOnStart", flags).SetValue(playerEditor, false); //playerEditor.assignMouseOnStart = false;
        
                return playerEditor;
            
            };
        
            playerList[5] = create(4, 5);
            playerList[6] = create(4, 6);
            playerList[7] = create(4, 7); 
            playerList[8] = create(4, 8);
        }
        
        logger.LogMessage("PLAYERS HAVE BEEN ADDED!!!!!@#");
    }
}