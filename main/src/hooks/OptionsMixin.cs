using BepInEx.Logging;
using JollyCoop.JollyMenu;
using Rewired;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Myriad.hooks; 

public class OptionsMixin {

    public ManualLogSource Logger = null;
    
    public static OptionsMixin INSTANCE = new OptionsMixin();

    public void init(ManualLogSource logger) {
        this.Logger = logger;
        
        On.Options.ctor += addMoreJollyOptions;
        
        On.Options.ApplyOption += Options_ApplyOption;
        On.Options.ToStringNonSynced += Options_ToStringNonSynced;
        On.Options.ToString += Options_ToString;
    }
    
    //Needed To fix Template Player being set incorrectly!!!!!
    private void addMoreJollyOptions(On.Options.orig_ctor orig, Options self, RainWorld rainWorld) {
        orig(self, rainWorld);
        
        foreach (Rewired.Player player in ReInput.players.GetPlayers(false)) {
            if (player.id == MyriadMod.PlyCnt()) {
                Options.templatePlayer = player;
                break;
            }
        }
    }
    
    private bool Options_ApplyOption(On.Options.orig_ApplyOption orig, Options self, string[] splt2){
        bool result = false;
        
        try {
            //Logger.LogInfo("APPLY OPTION: " + splt2[0] + " - " +self.controls.Length);
            result = orig.Invoke(self, splt2);
        } catch(Exception e) {
            Logger.LogError($"Was unable to apply options! [Exception: {e.Message}]");
            Logger.LogError(e.StackTrace);
            
            result = false;
        }
        
        if (splt2[0] == "InputSetup2") {
            result = false; //BECAUSE WE HIT A MATCH
            
            Logger.LogInfo("---APPLYING EXTRA INPUT OPTION! " + self.controls.Length + " - " + splt2[1] + " - " + splt2[2]);
            
            //WE SET IT TO CONTROL SO THAT THE NUMBER OF ENTRIES IN THE TABLE MATCHES CONTROL
            int entryNum = int.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
            
            try {
                //Logger.LogInfo("-APPLYING TO ARRAY " + entryNum);
                self.controls[entryNum].FromString(splt2[2]);// = myControlArry[entryNum];
            } catch (Exception e) {
                Logger.LogError($"FAILED READ/APPLY EXTRA INPUT SETTINGS! - [Exception: {e.Message}]");
                Logger.LogError(e.StackTrace);
            }
        }

        //6-8-23 TRYING AGAIN BUT FOR JOLLY EXTRAS
        if (splt2[0] == "JollySetupPlayersExtra"){
            result = false; //BECAUSE WE HIT A MATCH
            
            if (ModManager.JollyCoop){
                string[] array3 = Regex.Split(splt2[1], "<optC>");
                int num = 4; //START AT 4
                
                foreach (string text in array3) {
                    if (!(text == string.Empty) && num < MyriadMod.PlyCnt()) { //CHECK FOR THIS IN CASE WE SAVED EXTRA VALUES
                        Logger.LogInfo("--- JOLLY PLAYER OPTIONS!(V2) " + text);
                        
                        JollyPlayerOptions jollyPlayerOptions = new JollyPlayerOptions(num);
                        jollyPlayerOptions.FromString(text);
                        
                        self.jollyPlayerOptionsArray[jollyPlayerOptions.playerNumber] = jollyPlayerOptions;
                        
                        num++;
                    }
                }
            } else {
                result = true;
            }
        }
        
        return result;
    }
    
    //6-8 WAIT NO WE NEED TO USE THIS THIS BREAKS THE SAVES OTHERWISE
    private string Options_ToStringNonSynced(On.Options.orig_ToStringNonSynced orig, Options self) {
        int servMemory = self.playerToSetInputFor;
        self.playerToSetInputFor = Math.Min(self.playerToSetInputFor, 3); //DON'T LET THIS GET SAVED OUTSIDE OF THE NORMAL RANGE
        //AND DON'T LET CONTROLS SAVE FOR MORE THAN 4 PLAYERS
        Options.ControlSetup[] controlsMemory = self.controls;
        
        self.controls = new Options.ControlSetup[4];
        for (int i = 0; i < self.controls.Length; i++) { 
            self.controls[i] = controlsMemory[i];
        }
        
        string result = orig(self);

        //5-18-23 INSTEAD, MAYBE WE OUTPUT THAT STUFF TO A FILE AS A DIFFERENT NAME?
        for (int k = 4; k < MyriadMod.PlyCnt(); k++) {
            Logger.LogInfo("---SAVING CUTSOM OPTIONS STRING FOR PLAYER: " + k);
            result += string.Format(CultureInfo.InvariantCulture, "InputSetup2<optB>{0}<optB>{1}<optA>", k, controlsMemory[k]);
            // !!! WARNING!!!! THE GAME SEEMS TO WANT TO SET P5'S SPECIFIC CONTROLLER TO #3, WHICH CRASHES THE GAME IF NOT PLUGGED IN. UNDO THAT!!!
        }

        self.controls = controlsMemory;
        self.playerToSetInputFor = servMemory;
        
        return result;
    }
    
    //THIS IS FOR SAVING OUR DATA (TO THE TEXT FILES AND STUFF)
    private string Options_ToString(On.Options.orig_ToString orig, Options self) {
        JollyPlayerOptions[] optionsMemory = self.jollyPlayerOptionsArray;
        Options.ControlSetup[] controlsMemory = self.controls;

        //REBUILD THE ARRAY WITH A MAX OF 4 PLAYERS
        self.jollyPlayerOptionsArray = new JollyPlayerOptions[4];
        self.controls = new Options.ControlSetup[4]; //WE MAY NEED THE SAME FOR INPUT SETUP TOO...
        for (int j = 0; j < self.jollyPlayerOptionsArray.Length; j++) {
            self.jollyPlayerOptionsArray[j] = optionsMemory[j];
        }
        
        for (int i = 0; i < self.controls.Length; i++) {
            self.controls[i] = controlsMemory[i];
        }

        //RUN THE ORIGINAL WITH THE SHORTENED TABLES
        string text = orig(self);
        
        //NOW WE SAVE THE EXTRA VALUES 
        for (int k = 4; k < MyriadMod.PlyCnt(); k++){
            Logger.LogInfo("---SAVING CUTSOM OPTIONS STRING FOR PLAYER: " + k);
            text += string.Format(CultureInfo.InvariantCulture, "InputSetup2<optB>{0}<optB>{1}<optA>", k, controlsMemory[k]);
        }
        
        //6-8-23 -OKAY THE MODDED VERSION KINDA STINKS BECAUSE IT DOESN'T ACTUALLY SAVE MOST OF THE TIME
        if (ModManager.JollyCoop){
            text += "JollySetupPlayersExtra<optB>";
            
            for (int j = 4; j < optionsMemory.Length; j++){
                text = text + optionsMemory[j].ToString() + "<optC>";
            }
            
            text += "<optA>";
        }

        //THEN RESTORE THE ARRAY TO IT'S FULL PLAYER COUNT
        self.jollyPlayerOptionsArray = optionsMemory;
        self.controls = controlsMemory;
        
        return text;
    }
}