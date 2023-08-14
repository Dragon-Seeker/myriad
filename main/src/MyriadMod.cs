using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using UnityEngine;
using RWCustom;
using BepInEx;
using BepInEx.Logging;
using JetBrains.Annotations;
using JollyCoop.JollyMenu;
using Menu;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using Rewired;
using MonoMod.RuntimeDetour;
using JollyCoop;
using System.Runtime.CompilerServices;
using Logger = BepInEx.Logging.Logger;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ManySlugCats;

[BepInPlugin("myriad", "Myriad of Slug Cats", "0.1.0")]
public class MyriadMod : BaseUnityPlugin {
    
    public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("Myriad");
    public delegate int orig_ShortcutTime(Player self);

    //BindingFlags otherMethodFlags = BindingFlags.Instance | BindingFlags.NonPublic;
    //BindingFlags myMethodFlags = BindingFlags.Static | BindingFlags.Public;

    //public static int plyCnt = 8;
    public static int plyCnt = Reflect_GetPlayerCount(); //SET IN THERE BECAUSE IT LOADS FIRST

    public static int PlyCnt() {
        return plyCnt;
    }

    public static int Reflect_GetPlayerCount() {
        int playerCount = 4;

        Type type = wrapTryCatch(
            () => Type.GetType("Myriad.PreloadPatches.MyriadPreloadPatches, Myriad_PreloadPatcher"), 
            "Unable to [get] the Type [MyriadPreloadPatches] which means the player count will be default of 4"
            );
        
        if (type != null) {
            FieldInfo field = wrapTryCatch(
                () => type.GetField("myCount", BindingFlags.Public | BindingFlags.Static), 
                "Unable to [get] the Field [MyriadPreloadPatches::myCount] which means the player count will be default of 4"
                );

            if (field != null) {
                playerCount = (int) field.GetValue(null);
            } else {
                logger.LogError("Unable to [find] the Field [MyriadPreloadPatches::myCount] which means the player count will be default of 4");
            }
        } else {
            logger.LogError("Unable to [find] the Type [MyriadPreloadPatches] which means the player count will be default of 4");
        }

        return playerCount;
    }
    
    public static T? wrapTryCatch<T>(Func<T> supplier, Func<String> message) where T : class {
        return wrapTryCatch(supplier, message());
    }

    public static T? wrapTryCatch<T>(Func<T> supplier, String message) where T : class {
        return wrapTryCatch(supplier, exception => {
            logger.LogError(message);
            logger.LogError(exception.Message);
        });
    }

    public static T? wrapTryCatch<T>(Func<T> supplier, Action<Exception> logException) where T : class {
        try {
            return supplier();
        } catch (Exception e) {
            logException(e);
        }
        
        return null;
    }
    
    public void OnEnable() {
        try {
            //new MorePlayers().OnEnable();
            On.Menu.InputOptionsMenu.ctor += addMorePlayerOptions;

            On.Options.ctor += addMoreJollyOptions;
            
            // On.JollyCoop.JollyEnums.RegisterAllEnumExtensions += extendJollyEnumData;
            // On.JollyCoop.JollyEnums.UnregisterAllEnumExtensions += removeExtendedJollyEnum;
            // On.SlugcatStats.HiddenOrUnplayableSlugcat += hideExtraJollyEnums;

            On.JollyCoop.JollyMenu.JollySlidingMenu.ctor += adjustPlayerSelectGUI;
            On.JollyCoop.JollyMenu.JollySlidingMenu.NumberPlayersChange += accountForMoreThanFour;
            On.JollyCoop.JollyMenu.JollySlidingMenu.Singal += JollySlidingMenu_Singal;
            
            On.Menu.MenuIllustration.ctor += MenuIllustration_ctor;
            On.Menu.MenuIllustration.LoadFile_string += MenuIllustration_LoadFile_string;
            On.Menu.PlayerJoinButton.GrafUpdate += PlayerJoinButton_GrafUpdate;
            On.Menu.PlayerJoinButton.Update += PlayerJoinButton_Update;
            On.Menu.PlayerResultBox.ctor += PlayerResultBox_ctor;
            On.Menu.PlayerResultBox.GrafUpdate += PlayerResultBox_GrafUpdate;
            On.Menu.PlayerResultBox.IdealPos += PlayerResultBox_IdealPos;

            //-----

            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            //On.Menu.InputOptionsMenu.PlayerButton.ctor += PlayerButton_ctor; //INPUT MENU

            IL.Options.ctor += il => Replace4WithMore(il, true); 									//3
            IL.JollyCoop.JollyMenu.JollySlidingMenu.ctor += Replace4WithMore;   //3
            IL.StoryGameSession.CreateJollySlugStats += Replace4WithMore;       //1
            IL.PlayerGraphics.PopulateJollyColorArray += Replace4WithMore;      //1
            IL.RoomSpecificScript.SU_C04StartUp.ctor += Replace4WithMore;       //2
            IL.ArenaSetup.ctor += Replace4WithMore;                             //2
            //IL.Menu.InputOptionsMenu.ctor += Options_ctor;                      //1  //INPUT MENU
            IL.Menu.MultiplayerMenu.InitiateGameTypeSpecificButtons += il => Replace4WithMore(il, true); //SHOULD GRAB 2
            IL.Menu.MultiplayerMenu.Update += il => Replace4WithMore(il, false, 2); 
			IL.Options.ControlSetup.SaveAllControllerUserdata += Replace4WithMore;  //1
			IL.RainWorld.JoystickConnected += Replace4WithMore; //5-19
            IL.RainWorld.JoystickPreDisconnect += Replace4WithMore;
			IL.RWInput.PlayerUIInput += Replace4WithMore;
			//SOME MORE TO ADD!
			IL.ScavengersWorldAI.Outpost.ctor += Replace4WithMore;
			IL.ArenaGameSession.ctor += Replace4WithMore;
			//IL.World.LoadMapConfig += Replace4WithMore; //PERHAPS? BUT LEAVE OUT UNLESS IT'S DISCOVERED THAT WE NEED IT
            IL.CreatureCommunities.ctor += Replace4WithMore;

            IL.StoryGameSession.ctor += il => Replace4WithMore(il, false, 1);

            RainWorld.PlayerObjectBodyColors = new Color[plyCnt];
            On.Options.ApplyOption += Options_ApplyOption;
            On.Options.ToStringNonSynced += Options_ToStringNonSynced;
            On.Options.ToString += Options_ToString;

            On.RainWorldGame.JollySpawnPlayers += RainWorldGame_JollySpawnPlayers;
            On.JollyCoop.JollyMenu.JollyPlayerSelector.Update += JollyPlayerSelector_Update;
            //LOCK DOWNPOUR STORY CHARACTER SELECTION FOR PLAYER 1
			
            On.JollyCoop.JollyCustom.ForceActivateWithMSC += JollyCustom_ForceActivateWithMSC; //FOR JOLLYCAMPAINGN. JUST RETURN TRUE INSTEAD OF ORIG.

            //ADJUST MENU LAYOUT
            //On.JollyCoop.JollyMenu.JollySlidingMenu.ctor += JollySlidingMenu_ctor;
            On.Menu.MultiplayerMenu.InitiateGameTypeSpecificButtons += MultiplayerMenu_InitiateGameTypeSpecificButtons;
            On.Menu.SandboxSettingsInterface.AddPositionedScoreButton += SandboxSettingsInterface_AddPositionedScoreButton;
            // On.RWInput.PlayerInputLogic += RWInput_PlayerInputLogic;

            // On.Menu.InputOptionsMenu.Update += InputOptionsMenu_Update;
            // On.Menu.InputOptionsMenu.InputSelectButton.ButtonText += InputSelectButton_ButtonText; //INPUT MENU

            //AFTER SWITCHING BACK TO MSOPTIONS WAY
            On.PlayerGraphics.SlugcatColor += PlayerGraphics_SlugcatColor;

            On.ArenaGameSession.SpawnPlayers += ArenaGameSession_SpawnPlayers;
            On.HUD.PlayerSpecificMultiplayerHud.ctor += PlayerSpecificMultiplayerHud_ctor;
            On.SlugcatStats.Name.ArenaColor += Name_ArenaColor;
            On.SlugcatStats.Name.Init += Name_Init;
            On.SlugcatStats.HiddenOrUnplayableSlugcat += SlugcatStats_HiddenOrUnplayableSlugcat;
            On.PlayerGraphics.DefaultSlugcatColor += PlayerGraphics_DefaultSlugcatColor;
            On.ArenaBehaviors.ExitManager.ExitOccupied += ExitManager_ExitOccupied;
            On.Menu.InputTesterHolder.InputTester.ctor += InputTester_ctor;
            On.Menu.InputTesterHolder.InputTester.GetToPos += InputTester_GetToPos;
            On.Menu.InputTesterHolder.InputTester.TestButton.ctor += TestButton_ctor;
            On.Menu.InputTesterHolder.Back.Update += Back_Update;

            On.Player.Update += BPPlayer_Update;
            On.ShelterDoor.Update += ShelterDoor_Update;

            BindingFlags otherMethodFlags = BindingFlags.Instance | BindingFlags.Public;
            BindingFlags myMethodFlags = BindingFlags.Static | BindingFlags.Public;

            //Hook myCustomHook = new Hook(
            //    typeof(Player).GetProperty("InitialShortcutWaitTime", otherMethodFlags).GetGetMethod(), // This gets the getter 
            //    typeof(ManySlugCatsMod).GetMethod("GetIncreasedWaitTime", myMethodFlags) // This gets our hook method
            //);
            //THIS DIDN'T WORK... IT MAY BE THAT IT'S TOO SMALL FOR THE COMPILER TO HANDLE CORRECTLY, LIKE THE OTHER ONES :(

            //-----

            On.ShortcutHandler.ShortCutVessel.ctor += ShortCutVessel_ctor;

            var testArray = new String[300];

            testArray[0] = "test";
            
            logger.LogMessage("Checking Patch");
        } catch (Exception e) {
            logger.LogMessage("ManySlugCats failed to load due to an exception being thrown!");
            logger.LogMessage(e.ToString());
            throw e;
        }

        RainWorld.PlayerObjectBodyColors = new Color[PlyCnt()];
    }

    private Vector2 InputTester_GetToPos(On.Menu.InputTesterHolder.InputTester.orig_GetToPos orig, InputTesterHolder.InputTester self) {

        Vector2 result = orig(self);
        if (PlyCnt() > 8 && self.playerIndex % 2 != 0)
            result -= new Vector2(60, 0);
        return result;
    }

    public void Back_Update(On.Menu.InputTesterHolder.Back.orig_Update orig, InputTesterHolder.Back self) {
        
        orig(self);
        if (PlyCnt() > 8) {
            //self.textLabel.pos.y -= 60f;
            //self.textLabel.pos.x += 55f;
            self.textLabel.pos += new Vector2(-280, 90);
        }
    }

    public void InputTester_ctor(On.Menu.InputTesterHolder.InputTester.orig_ctor orig, InputTesterHolder.InputTester self, Menu.Menu menu, MenuObject owner, int playerIndex) 
    {
        // self.rad = 15; //TOO EARLY! IT DIDN'T WORK
        orig(self, menu, owner, playerIndex);
		self.rad = 22; //THIS SHOULD WORK NOW. AND I BELEIVE GRAFUPDATE() SHOULD HANDLE THE REST
		self.crossSpriteH.scaleX = self.rad * 2f;
		self.crossSpriteV.scaleY = self.rad * 2f;
        self.centerKnobSprite.scale = 0.7f;
        //BASELINE y = -15

        AdjustTestButton(self, 4, 210, -20); //Pickup/Eat
        AdjustTestButton(self, 5, 0, 10); //Jump
        AdjustTestButton(self, 6, 0, 15); //Throw
        AdjustTestButton(self, 7, 210, -5); //Pause

        //THE SYMBOLS DONT MOVE! >:(
        //AdjustTestButton(self, 0, 112 - 15, -0); //left
        //AdjustTestButton(self, 1, 56, -56 + 15); //up
        //AdjustTestButton(self, 2, 15, 0); //right
        //AdjustTestButton(self, 3, 56, 56 + 0); //down
    }

    private void TestButton_ctor(On.Menu.InputTesterHolder.InputTester.TestButton.orig_ctor orig, InputTesterHolder.InputTester.TestButton self, Menu.Menu menu, MenuObject owner, Vector2 pos, string symbolName, int symbolRotat, string labelText, int buttonIndex, int playerIndex) {

        if (symbolName != null && symbolName == "Menu_Symbol_Arrow") {
            pos *= 0.4f; //ARROWS ARE SLIPPERY . SHRINK THEM BEFOREHAND
            if (symbolRotat == 2) //DOWN ARROW
                pos.y = 0;
            //THEN SHIFT THEM ALL OVER
            pos += new Vector2(60, -15);
        }
        //YOU KNOW WHAT, THEY'RE ALL LOW! MOVE THEM ALL UP
        pos += new Vector2(0, 5);

        orig(self, menu, owner, pos, symbolName, symbolRotat, labelText, buttonIndex, playerIndex);

    }


    public static void AdjustTestButton(InputTesterHolder.InputTester self, int index, float xShift, float yShift)
	{
		self.testButtons[index].roundedRect.pos += new Vector2(xShift, yShift);
		
		if (self.testButtons[index].symbolSprite != null) {
            self.testButtons[index].symbolSprite.x += xShift;
            self.testButtons[index].symbolSprite.y += yShift;
            self.testButtons[index].symbolSprite.SetPosition(self.testButtons[index].symbolSprite.x, self.testButtons[index].symbolSprite.y);
            Debug.Log("SYMBOL SPRITE!");
        }
		else if (self.testButtons[index].extraRect != null)
			self.testButtons[index].extraRect.pos += new Vector2(xShift, yShift);
		
		if (self.testButtons[index].labelText != null)
			self.testButtons[index].menuLabel.pos += new Vector2(xShift, yShift);
	}
	

    
    private void ShortCutVessel_ctor(On.ShortcutHandler.ShortCutVessel.orig_ctor orig, ShortcutHandler.ShortCutVessel self, IntVector2 pos, Creature creature, AbstractRoom room, int wait) {
        
        if (MPOptions.longPipeWait.Value && creature is Player && wait > 0) {
            wait *= 1000;
        }
        orig(self, pos, creature, room, wait);
    }

    //public static int GetIncreasedWaitTime(orig_ShortcutTime orig, Player self) {
    //    int result = orig(self);
    //    return result * 1000;
    //}


    private bool ExitManager_ExitOccupied(On.ArenaBehaviors.ExitManager.orig_ExitOccupied orig, ArenaBehaviors.ExitManager self, int exit) {
        
        bool result = orig(self, exit);
        int denSize = Mathf.CeilToInt((self.gameSession.Players.Count + 1) / 4f);
        int inDen = 0;

        for (int i = 0; i < self.playersInDens.Count; i++) {
            if (self.playersInDens[i].entranceNode == exit)
                inDen++;
        }
        if (inDen >= denSize)
            return true;
        return false;

    }
    



    //Needed To fix Template Player being set incorrectly!!!!!
    private void addMoreJollyOptions(On.Options.orig_ctor orig, Options self, RainWorld rainWorld) {
        orig(self, rainWorld);
        
        foreach (Rewired.Player player in ReInput.players.GetPlayers(false)) {
            if (player.id == PlyCnt()) {
                Options.templatePlayer = player;
                break;
            }
        }
    }

    // !!!!!!!!!!!!!!!! --- NOT NEEDED ANYMORE but may be good for compat???? --- !!!!!!!!!!!!!!!!!!!!!!!!!
    /*
    public static SlugcatStats.Name JollyPlayer5;
    public static SlugcatStats.Name JollyPlayer6;
    public static SlugcatStats.Name JollyPlayer7;
    public static SlugcatStats.Name JollyPlayer8;

    private void extendJollyEnumData(On.JollyCoop.JollyEnums.orig_RegisterAllEnumExtensions orig){
        orig();

        JollyPlayer5 = new SlugcatStats.Name("JollyPlayer5", true);
        JollyPlayer6 = new SlugcatStats.Name("JollyPlayer6", true);
        JollyPlayer7 = new SlugcatStats.Name("JollyPlayer7", true);
        JollyPlayer8 = new SlugcatStats.Name("JollyPlayer8", true);
    }

    private void removeExtendedJollyEnum(On.JollyCoop.JollyEnums.orig_UnregisterAllEnumExtensions orig){
        orig();

        if (JollyPlayer5 != null){
            JollyPlayer5.Unregister();
            JollyPlayer5 = null;
        }

        if (JollyPlayer6 != null){
            JollyPlayer6.Unregister();
            JollyPlayer6 = null;
        }

        if (JollyPlayer7 != null){
            JollyPlayer7.Unregister();
            JollyPlayer7 = null;
        }

        if (JollyPlayer8 == null) return;

        JollyPlayer8.Unregister();
        JollyPlayer8 = null;
    }
    
    public static bool hideExtraJollyEnums(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i) => 
        orig(i) || (ModManager.JollyCoop && (i == JollyPlayer5 || i == JollyPlayer6 || i == JollyPlayer7 || i == JollyPlayer8));
    */
    // !!!!!!!!!!!!!!!! --- NOT NEEDED ANYMORE but may be good for compat???? --- !!!!!!!!!!!!!!!!!!!!!!!!!
    

    //WE NEED THIS IN MULTIPLE PLACES BECAUSE IDK WHY RAIN WORLD DOES THIS, PLS
    public static int GetPortraitIndex (string fileName) {
        int result = -1; //NOT A PORTRAIT

        string lowerName = fileName.ToLower();
        if (lowerName.StartsWith("multiplayerportrait")) {
            int pDigits = 1; //ACCOUNT FOR DOUBLE DIGIT PLAYER NUMBERS
            if (fileName.Length == 22 || fileName.IndexOf("-") == 22)
                pDigits = 2; //A DOUBLE DIGIT PLAYER NUMBER
            string substr1 = lowerName.Replace("multiplayerportrait", "").Substring(0, pDigits); //GETS THE PLAYER NUMBER
            string substr2 = lowerName.Replace("multiplayerportrait", "").Substring(pDigits); //THE REST OF THE NUMBERS
            result = Convert.ToInt32(substr1);
        }
        return result;
    }


    public static string AdjustedPortraitFile(string fileName) {

        string newFileName = fileName;
        string lowerName = fileName.ToLower();
        //FILE NAMES ARE NOT CASE SENSITIVE. BUT FSPRITE NAMES ARE :/ SO DON'T CHANGE THE CASE
        //logger.LogMessage("STARTING FILE: " + fileName);

        //We CANNOT do this for player 5 because MSC slugcats use "MultiplayerPortrait40-" for their canon untinted color portraits -_- thanks rainworld
        if (lowerName.StartsWith("multiplayerportrait4") && lowerName.Length > 21) {
            //We'll have to let PlayerJoinButton deal with this because this is a valid portrait file
            //Except for the vanilla slugs because of course it would :/
            if (lowerName.EndsWith("white") || lowerName.EndsWith("yellow") || lowerName.EndsWith("red")) {
                newFileName = fileName.Replace("4", "0");
            }
        }
        else if (fileName.StartsWith("MultiplayerPortrait")) {
            int pDigits = 1; //ACCOUNT FOR DOUBLE DIGIT PLAYER NUMBERS
            if (fileName.Length == 22 || fileName.IndexOf("-") == 22)
                pDigits = 2; //A DOUBLE DIGIT PLAYER NUMBER
            string substr1 = fileName.Replace("MultiplayerPortrait", "").Substring(0, pDigits); //GETS THE PLAYER NUMBER
            string substr2 = fileName.Replace("MultiplayerPortrait", "").Substring(pDigits); //THE REST OF THE NUMBERS
            //IF OUR PLAYER NUM IS HIGHER THAN EXPECTED, RETURN THE 4TH PLAYER IMAGE VERSION
            if (Convert.ToInt32(substr1) > 3)
                substr1 = "0";
            newFileName = "MultiplayerPortrait" + substr1 + substr2; //REBUILD IT
        } else if (lowerName.StartsWith("gamepad") && lowerName.Length == 8) {
            int playNum = int.Parse(lowerName.Replace("gamepad", "")); //GETS THE PLAYER NUMBER
            if (playNum > 4)
                newFileName = "GamepadAny"; //JUST A PLACEHOLDER
        }
        //logger.LogMessage("EDITED FILE: " + newFileName);

        return newFileName;
    }



    //I JUST COPIED THIS IN HERE SO IT RUNS FIRST I GUESS? DO I STILL NEED TO RUN IT IN THE OTHER ONE THEN?
    private void MenuIllustration_ctor(On.Menu.MenuIllustration.orig_ctor orig, MenuIllustration self, Menu.Menu menu, MenuObject owner, string folderName, string fileName, Vector2 pos, bool crispPixels, bool anchorCenter)
    {
        //REMEMBER WHAT PLAYER NUMBER THIS WAS BEFORE WE DO ANYTHING
        int index = GetPortraitIndex(fileName); //PLAYER NUMBER

        //GOOD LORD WE REALLY DO NEED TO DO IT IN BOTH PLACES :/ OTHERWISE FSPRITE TRIES TO CREATE AN INVALID SPRITE
        string newFileName = AdjustedPortraitFile(fileName);

        orig.Invoke(self, menu, owner, folderName, newFileName, pos, crispPixels, anchorCenter);

        if (index > 3 && self.spriteAdded) {
            //logger.LogMessage("TINT OUR PORTRAIT " + SlugcatStats.Name.ArenaColor(index));
            self.sprite.color = PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(index));
        }
    }

    private void MenuIllustration_LoadFile_string(On.Menu.MenuIllustration.orig_LoadFile_string orig, MenuIllustration self, string folder) 
    {
        //REMEMBER WHAT PLAYER NUMBER THIS WAS BEFORE WE DO ANYTHING
        int index = GetPortraitIndex(self.fileName); //PLAYER NUMBER
        //logger.LogMessage("STARTING FILE: " + self.fileName);
        self.fileName = AdjustedPortraitFile(self.fileName); //SHOULD WE SET THI BACK WHEN WE'RE DONE?... NAHHH
        //logger.LogMessage("--ENDING FILE: " + self.fileName);
        orig(self, folder);
        
        //IF WE WERE A SLUGCAT PORTRAIT PAST PLAYER 4, TINT OUR PORTRAIT
        if (index > 3 && self.spriteAdded) {
            //logger.LogMessage("TINT OUR PORTRAIT " + SlugcatStats.Name.ArenaColor(index) + PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(index)) + "  ---   " + self.sprite.color);
            self.sprite.color = PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(index));
        }
    }


    //CONTINUING TO LOSE MY MIND
    private void PlayerJoinButton_GrafUpdate(On.Menu.PlayerJoinButton.orig_GrafUpdate orig, PlayerJoinButton self, float timeStacker) {
        Color origColor = self.portrait.sprite.color;
        
        orig(self, timeStacker);

        //NOT ALL PORTRAITS WILL BE WHITE NOW
        if (self.index > 3 && origColor != Color.white)
        {
            Color newColor = PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(self.index));
            self.portrait.sprite.color = Color.Lerp(newColor, Color.black, Custom.SCurve(Mathf.Lerp(self.lastPortraitBlack, self.portraitBlack, timeStacker), 0.5f) * 0.75f);
        } 
    }

    //BASICALLY THE SAME TREATMENT
    private void PlayerResultBox_GrafUpdate(On.Menu.PlayerResultBox.orig_GrafUpdate orig, PlayerResultBox self, float timeStacker) {
        
        orig(self, timeStacker);

        //NOT ALL PORTRAITS WILL BE WHITE NOW
        int index = self.player.playerNumber;
        if (index > 3) {
            Color newColor = PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(index));
            float num = self.UseWinnerColor(timeStacker);
            float num2 = self.UseTextWhite(timeStacker);
            self.portrait.sprite.color = Color.Lerp(Color.black, newColor, self.showAsAlive ? 1f : (0.25f + Mathf.Max(0.75f * num, 0.25f * num2)));
        }
    }


    private Vector2 PlayerResultBox_IdealPos(On.Menu.PlayerResultBox.orig_IdealPos orig, PlayerResultBox self) {
        
        Vector2 result = orig(self);
        
        //START OVER FROM THE TOP
        result.y = (self.menu as PlayerResultMenu).topMiddle.y; 
        result.y -= (600 / self.menu.manager.arenaSitting.players.Count) * (float) self.index;
        return result;
    }

    //SQUEEZE THE LABELS A BIT CLOSER TOGETHER SO OVERLAPPING BOXES WON'T BE AN ISSUE
    private void PlayerResultBox_ctor(On.Menu.PlayerResultBox.orig_ctor orig, PlayerResultBox self, Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size, ArenaSitting.ArenaPlayer player, int index) {

        orig(self, menu, owner, pos, size, player, index);
        self.playerNameLabel.pos.y -= 20f;
    }



    private void PlayerJoinButton_Update(On.Menu.PlayerJoinButton.orig_Update orig, PlayerJoinButton self) {

        //SPECIFICALY PLAYER 5 NEEDS TO HAVE THEIR PORTRAIT REPLACED BECAUSE OTHER PARTS OF THE GAME USE THAT FILENAME
        if (self.index == 4 && self.portrait.fileName.StartsWith("MultiplayerPortrait4")) {
            self.portrait.fileName = self.portrait.fileName.Replace("4", "0"); //STUPIT...
            self.portrait.LoadFile();
            self.portrait.sprite.SetElementByName(self.portrait.fileName);
        }

        //NON MSC VERSIONS NEED PORTRAITS
        if (!ModManager.MSC && self.index > 3 && self.portrait.fileName != "MultiplayerPortrait01") {
            //OKAY MANUALLY SET AND COLOR OUR MENU I THINK
            self.portrait.fileName = "MultiplayerPortrait01";
            self.portrait.LoadFile();
            self.portrait.sprite.SetElementByName(self.portrait.fileName);
        }

        orig(self);
    }






    public void accountForMoreThanFour(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_NumberPlayersChange orig, JollySlidingMenu self, UIconfig config, string value, string oldvalue) {
        orig(self, config, value, oldvalue);
        
        int result;

        var jollyPlayerOptions = self.Options.jollyPlayerOptionsArray;
        
        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
            if (result > PlyCnt() || result < 4) return;
            
            for (int index = 0; index < jollyPlayerOptions.Length; ++index) jollyPlayerOptions[index].joined = index <= result - 1;
        }
        
        self.UpdatePlayerSlideSelectable(result - 1);
    }
	
	
	public SimpleButton[] jollySwapButtons;
	
    public void adjustPlayerSelectGUI(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_ctor orig, JollySlidingMenu self, JollySetupDialog menu, MenuObject owner, Vector2 pos) {
        orig(self, menu, owner, pos);

        //float num1 = 70;
        //float num2 = (float)((1024.0 - num1 * 8.0) / 5.0);
        //Vector2 pos1 = new Vector2(-25 + num2, 0.0f) + new Vector2(0.0f, menu.manager.rainWorld.screenSize.y * 0.55f);
		
		//TRYING SOMETHING FUNKY
		jollySwapButtons = new SimpleButton[PlyCnt()];

        for (int index = 0; index < PlyCnt(); ++index) {
            JollyPlayerSelector playerSelector = self.playerSelector[index];

            if (PlyCnt() > 8) {
                playerSelector.portraitRectangle.size /= 2;
                playerSelector.portraitRectangle.lastSize /= 2;

                playerSelector.portraitRectangle.pos += new Vector2(18, 4);
                playerSelector.portraitRectangle.lastPos = playerSelector.portraitRectangle.pos;

                var portaitPos = playerSelector.portraitRectangle.pos + new Vector2(25, 25);  //new Vector2((18 * 2) + 8, (12 * 2) + 8);

                playerSelector.RemoveSubObject(playerSelector.portrait);

                playerSelector.portrait = new MenuIllustration((Menu.Menu) menu, playerSelector, "", "MultiplayerPortrait" + index.ToString() + "1",
                    portaitPos, true, true);

                playerSelector.subObjects.Add(playerSelector.portrait);

                playerSelector.portrait.sprite.scale = 0.5f;

                // var portaitSpritPos = new Vector2(18, 12);
                //
                // playerSelector.portrait.pos += portaitSpritPos;
                // playerSelector.portrait.lastPos = playerSelector.portrait.pos;

                playerSelector.classButton.SetSize(playerSelector.classButton.size -= new Vector2(18, 0));

                playerSelector.classButton.menuLabel.label.scale = 0.90f;
                
                //----

                playerSelector.playerLabelSelector.size -= new Vector2(18, 0);
            }
            
            //playerSelector.pos.x -= playerSelector.pos.x - pos1.x;
            //playerSelector.playerLabelSelector._pos.x -= playerSelector.playerLabelSelector._pos.x - pos1.x;
            //pos1 += new Vector2(num2 + num1, 0.0f);

            //LETS TRY SOMETHING SIMPLER...
            float newX = ((menu.manager.rainWorld.screenSize.x) / PlyCnt()) * index;
            newX += 5 + Mathf.Lerp(700f, 0f, (Custom.rainWorld.options.ScreenSize.x / 1360)); //AN ATTEMPT TO FIX THE WEIRD SCREEN SIZE SCALING
            playerSelector.pos.x = newX + 0;
            playerSelector.playerLabelSelector._pos.x = newX + 0;

            if (PlyCnt() > 8) {
                playerSelector.pupButton.pos += new Vector2(-85f, 95f); //-45
                playerSelector.pupButton.roundedRect.size *= 0.8f;
                playerSelector.pupButton.selectRect.size *= 0.8f;
            }
			
            jollySwapButtons[index] = new SimpleButton(self.menu, self, "<->", "JOLLYSWAP" + index.ToString(), playerSelector.pos + new Vector2(-20, 130) , new Vector2(40f, 20f));
            menu.elementDescription.Add(jollySwapButtons[index].signalText, menu.Translate("Swap Player <p_n> and Player <p_n2>").Replace("<p_n>", (index + 0).ToString()).Replace("<p_n2>", (index + 1).ToString()));
            self.subObjects.Add(jollySwapButtons[index]);
            
            if (index > 0) {
                // <0 1^ 2> 3v
                jollySwapButtons[index].nextSelectable[3] = playerSelector.pLabelSelectorWrapper; //DOWN
                jollySwapButtons[index].nextSelectable[1] = self.sliderWrapper; //UP
                jollySwapButtons[index].nextSelectable[0] = jollySwapButtons[index - 1]; //LEFT
                jollySwapButtons[index].nextSelectable[2] = jollySwapButtons[index]; //RIGHT - ASSIGN IT TO OURSELF FOR NOW. IF THERES ANOTHER ONE TO OUR RIGHT, IT'LL CHANGE OURS
                jollySwapButtons[index - 1].nextSelectable[2] = jollySwapButtons[index];

                playerSelector.pLabelSelectorWrapper.nextSelectable[1] = jollySwapButtons[index];
                playerSelector.pLabelSelectorWrapper.nextSelectable[3] = playerSelector.pupButton;
                playerSelector.pLabelSelectorWrapper.nextSelectable[0] = self.playerSelector[index-1].pLabelSelectorWrapper;
                self.playerSelector[index].pLabelSelectorWrapper.nextSelectable[2] = self.playerSelector[index].pLabelSelectorWrapper; //SELF, UNLESS THE ONE NEXT TO US UPDATES IT
                self.playerSelector[index - 1].pLabelSelectorWrapper.nextSelectable[2] = self.playerSelector[index].pLabelSelectorWrapper;
                
                if (PlyCnt() > 8) {
                    self.playerSelector[index].pupButton.nextSelectable[0] = self.playerSelector[index - 1].pupButton;
                    self.playerSelector[index].pupButton.nextSelectable[2] = self.playerSelector[index].pupButton; //SELF, UNLESS THE ONE NEXT TO US UPDATES IT
                    self.playerSelector[index - 1].pupButton.nextSelectable[2] = self.playerSelector[index].pupButton;
                }
                    
            }
            else {
                //JUST PRETEND THIS FIRST ONE DOESN'T EXIST
                jollySwapButtons[0].roundedRect.pos.x -= 1000;
                jollySwapButtons[0].menuLabel.pos.x -= 1000;
                jollySwapButtons[0].selectRect.pos.x -= 1000;

                playerSelector.pLabelSelectorWrapper.nextSelectable[3] = playerSelector.pupButton;
                playerSelector.pLabelSelectorWrapper.nextSelectable[0] = self.playerSelector[index].pLabelSelectorWrapper;
                if (PlyCnt() > 8)
                    playerSelector.pupButton.nextSelectable[0] = self.playerSelector[index].pupButton;
            }

            playerSelector.pupButton.nextSelectable[1] = playerSelector.pLabelSelectorWrapper;
        }

        jollySwapButtons[1].nextSelectable[0] = jollySwapButtons[1];

        //---

        var slider = self.numberPlayersSlider;
        
        var config = menu.oi.config.Bind("_cosmetic", Custom.rainWorld.options.JollyPlayerCount, new ConfigAcceptableRange<int>(1, PlyCnt()));

        var uiConfigType = typeof(UIconfig);
        var flags = BindingFlags.Public | BindingFlags.Instance;

        //self.numberPlayersSlider.cfgEntry = config;
        uiConfigType.GetField("cfgEntry", flags)
            .SetValue(self.numberPlayersSlider, config);
        
        //self.numberPlayersSlider.cosmetic = config.IsCosmetic;
        uiConfigType.GetField("cosmetic", flags)
            .SetValue(self.numberPlayersSlider, config.IsCosmetic);

        slider.defaultValue = config.defaultValue;

        slider.cfgEntry.BoundUIconfig = slider;

        var bl = config.info != null && config.info.acceptable != null;
        
        slider.min = bl ? (int) config.info.acceptable.Clamp(int.MinValue) : 0;
        slider.max = bl ? (int) config.info.acceptable.Clamp(int.MaxValue) : (slider._IsTick ? 15 : 100);

        float num1 = 70;
        slider.pos = self.playerSelector[0].pos + new Vector2((num1 / 2f) + 15, 142f);

        slider._size = new Vector2(Math.Max((int)(self.playerSelector[PlyCnt() - 1].pos - self.playerSelector[0].pos).x, 30), 30f);
        slider.fixedSize = slider._size;
        
        slider.Initialize();
        
        //---
        
        //Rebind buttons is needed to fix navigating sortof... Needs to be better handled I think
        //self.BindButtons(); //THIS DOESN'T WORK! BREAKS TOO MANY SELECTIONS. FIXING UP TOP
    }


    private void JollySlidingMenu_Singal(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_Singal orig, JollySlidingMenu self, MenuObject sender, string message) {

        orig(self, sender, message);
        if (message.Contains("JOLLYSWAP")) {
            for (int i = 0; i < self.playerSelector.Length; ++i){
                if (message == "JOLLYSWAP" + i.ToString()) {
                    JollyPlayerOptions jOptA = Custom.rainWorld.options.jollyPlayerOptionsArray[i];
                    JollyPlayerOptions jOptB = Custom.rainWorld.options.jollyPlayerOptionsArray[i - 1];
                    Custom.rainWorld.options.jollyPlayerOptionsArray[i] = jOptB;
                    Custom.rainWorld.options.jollyPlayerOptionsArray[i - 1] = jOptA;
					bool swapPup = self.JollyOptions(i).isPup != self.JollyOptions(i-1).isPup;
					
                    self.SetPortraitsDirty(); //REFRESH THE PORTRAITS!
                    self.playerSelector[i].dirty = true;
                    self.playerSelector[i-1].dirty = true;
                    
					//IF ONE OF US WAS A PUP AND THE OTHER WASN'T, TOGGLE BOTH PUP BUTTONS
					if (swapPup) {
						self.playerSelector[i].pupButton.Toggle();
						self.playerSelector[i-1].pupButton.Toggle();
					}
					
					//TRY AND UPDATE THE NAMES TOO
					self.playerSelector[i].playerLabelSelector.value = JollyCustom.GetPlayerName(i);
					self.playerSelector[i-1].playerLabelSelector.value = JollyCustom.GetPlayerName(i-1);
					
					//UPDATE WHO IS ACTUALLY ACTIVE
                    self.NumberPlayersChange(self.numberPlayersSlider.cfgEntry.BoundUIconfig, self.numberPlayersSlider.value, self.numberPlayersSlider.value);
                }
            }
        }
    }

    //----

    public static void addMorePlayerOptions(On.Menu.InputOptionsMenu.orig_ctor orig, InputOptionsMenu self, ProcessManager manager) {
        orig(self, manager);

        Vector2 vector2_1 = new Vector2(0.0f, -30f);

        //---

        var newDeviceButton = new InputOptionsMenu.DeviceButton[PlyCnt()+2];

        self.deviceButtons.CopyTo(newDeviceButton, 0);
        
        self.deviceButtons = newDeviceButton;
        
        //---

        var inputTesterIndex = self.pages[0].subObjects.IndexOf(self.inputTesterHolder);
        
        var buttonOffset = 60.0;
        var deviceBtnHeight = 680.0; //620

        if (PlyCnt() > 8) {
            deviceBtnHeight = 740;
            buttonOffset /= (PlyCnt() / 11.0f); //8.5
        }


        for (int index = 0; index < self.deviceButtons.Length; ++index) {
            InputOptionsMenu.DeviceButton deviceButton; 
            
            if(index > 5){
                string str = self.inputDevicedTexts[Math.Min(index, self.inputDevicedTexts.Length - 1)];
                
                if (index > 1) str = Regex.Replace(str, "<X>", (index - 1).ToString());
                
                if (index == 0 && (self.CurrLang != InGameTranslator.LanguageID.English)) str = InGameTranslator.EvenSplit(str, 1);
                
                deviceButton = new InputOptionsMenu.DeviceButton(self, self.pages[0], new Vector2(450f, (float) (deviceBtnHeight - (double) index * buttonOffset)) + vector2_1, str, self.deviceButtons, index);

                self.deviceButtons[index] = deviceButton;
                
                self.pages[0].subObjects.Insert(inputTesterIndex, self.deviceButtons[index]);
            } else {
                deviceButton = self.deviceButtons[index];

                deviceButton.pos = new Vector2(450f, (float)(deviceBtnHeight - (double)index * buttonOffset)) + vector2_1;
                deviceButton.buttonArray = self.deviceButtons;
            }
            
            foreach (var deviceButtonSubObject in deviceButton.subObjects) {
                if (!(deviceButtonSubObject is RectangularMenuObject rectMenuObject)) return;
                
                rectMenuObject.size /= 2;
                rectMenuObject.lastSize /= 2;
            }
            
            deviceButton.size /= 2;
            deviceButton.lastSize /= 2;
            
            deviceButton.menuLabel.pos.y += 30f;

            if(deviceButton.deviceImage != null){
                var padding = 16;
                
                deviceButton.RemoveSubObject(deviceButton.deviceImage);

                deviceButton.deviceImage = new MenuIllustration(self, deviceButton, "", index == 1 ? "KeyboardIcon" : "GamepadIcon", deviceButton.size / 2f, true, true) ;

                deviceButton.subObjects.Add(deviceButton.deviceImage);

                deviceButton.deviceImage.sprite.scale = 0.5f;
            }
            
            if (deviceButton.numberImage != null) {
                deviceButton.Container.RemoveChild(deviceButton.darkFade);
                
                deviceButton.darkFade = new FSprite("Futile_White");
                
                deviceButton.darkFade.shader = self.manager.rainWorld.Shaders["FlatLight"];
                
                deviceButton.darkFade.color = new Color(0.0f, 0.0f, 0.0f);
                
                deviceButton.Container.AddChild(deviceButton.darkFade);
                
                //----
                
                deviceButton.RemoveSubObject(deviceButton.numberImage);

                //var rollOverNum = (index > 5 ? (index % 4 - 1) : index - 1); //This is just to stop errors! Should be replaced with text or something?
                
                deviceButton.numberImage = new MenuIllustration(self, deviceButton, "", index == 0 ? "GamepadAny" : "Gamepad" + (index - 1), deviceButton.size / 2f, true, true);
                
                deviceButton.subObjects.Add(deviceButton.numberImage);
                
                deviceButton.numberImage.sprite.scale = 0.5f;
            }
        }
        
        //---

        var newPlayerButtons = new InputOptionsMenu.PlayerButton[PlyCnt()];
        
        self.playerButtons.CopyTo(newPlayerButtons, 0);

        self.playerButtons = newPlayerButtons;
        
        //----

        self.rememberPlayersSignedIn = new bool[self.playerButtons.Length];
        
        var perInputOffset = 76f;//143.3333282470703;

        var initalYOffset = 665;

        var xOffset = 32;

        if (PlyCnt() > 8) {
            initalYOffset = 740;
            perInputOffset /= (PlyCnt() / 9.8f);
            self.backButton.pos -= new Vector2(85, 0); //COME ON IT LOOKS BETTER THIS WAY...
        }

        for (int index = self.playerButtons.Length - 1; index >= 0; --index) {
            InputOptionsMenu.PlayerButton playerButton;
            
            if (index < 4) {
                playerButton = self.playerButtons[index];
                
                playerButton.pos = new Vector2(200f + xOffset, (float)(initalYOffset - (double)index * perInputOffset)) + vector2_1;
                
                playerButton.originalPos = new Vector2(Mathf.Floor(playerButton.pos.x) + 0.01f, Mathf.Floor(playerButton.pos.y) + 0.01f);
                
                playerButton.buttonArray = self.playerButtons;
            } else {
                playerButton = new InputOptionsMenu.PlayerButton(self, self.pages[0], new Vector2(200f + xOffset, (float) (initalYOffset - (double) index * perInputOffset)) + vector2_1, self.playerButtons, index);

                self.playerButtons[index] = playerButton;
                
                self.pages[0].subObjects.Add(self.playerButtons[index]);
                
                self.rememberPlayersSignedIn[index] = manager.rainWorld.IsPlayerActive(index);
            }
            
            foreach (var playerButtonSubObject in playerButton.subObjects) {
                if (!(playerButtonSubObject is RectangularMenuObject rectMenuObject)) return;
                
                rectMenuObject.size /= 2;
                rectMenuObject.lastSize /= 2;
            }

            playerButton.menuLabel.pos.y += 30f;

            var padding = 16;
            
            playerButton.RemoveSubObject(playerButton.portrait);

            var portraintIndex = index % 4;
            
            playerButton.portrait = new MenuIllustration(self, playerButton, "", $"MultiplayerPortrait{portraintIndex}1", playerButton.size / 4f, true, true);

            playerButton.subObjects.Add(playerButton.portrait);

            playerButton.portrait.sprite.scale = 0.5f;

            playerButton.size /= 2;
            playerButton.lastSize /= 2;

            //INCREASE VISIBILITY
            playerButton.menuLabel.pos.y += 20;
            playerButton.menuLabel.label.MoveToFront();
        }

        for (int playerNumber = 4; playerNumber < manager.rainWorld.options.controls.Length; ++playerNumber) {
            manager.rainWorld.RequestPlayerSignIn(playerNumber, null);
        }

        for (int index = 4; index < self.playerButtons.Length; ++index) {
            self.playerButtons[index].pointPos = self.playerButtons[index].IdealPointHeight();
            self.playerButtons[index].lastPointPos = self.playerButtons[index].pointPos;
        }
        
        //-----

        foreach (var inputTester in self.inputTesterHolder.testers) self.inputTesterHolder.subObjects.Remove(inputTester);

        self.inputTesterHolder.testers = new InputTesterHolder.InputTester[(self.inputTesterHolder.menu as InputOptionsMenu).playerButtons.Length];
        
        for (int playerIndex = 0; playerIndex < self.inputTesterHolder.testers.Length; ++playerIndex) {
            self.inputTesterHolder.testers[playerIndex] = new InputTesterHolder.InputTester(self.inputTesterHolder.menu, self.inputTesterHolder, playerIndex);
            self.inputTesterHolder.subObjects.Add((MenuObject) self.inputTesterHolder.testers[playerIndex]);
        }
        
        //-----
        
        self.inputTesterHolder.Initiate();

        self.UpdateConnectedControllerLabels();
        
        for (int index = 0; index < self.gamePadButtonButtons.Length; ++index) self.gamePadButtonButtons[index].nextSelectable[2] = self.playerButtons[index < self.gamePadButtonButtons.Length / 2 ? 0 : 1];
    }


    //ADDING FAKE CHARACTERS FOR THE ARENA MODE COLORS
    private void Name_Init(On.SlugcatStats.Name.orig_Init orig) {
        orig();
        // ExtEnum<SlugcatStats.Name>.values.AddEntry(SlugcatStats.Name.White.value);
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J5");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J6");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J7");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J8");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J9");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J10");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J11");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J12");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J13");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J14");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J15");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J16");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("JPlus");
    }

    //SO THEY DON'T SHOW UP IN THE SELECT SCREEN
    private bool SlugcatStats_HiddenOrUnplayableSlugcat(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i) {

        bool extraPlayer = (
            i == EnumExt_MyNames.J5 ||
            i == EnumExt_MyNames.J6 ||
            i == EnumExt_MyNames.J7 ||
            i == EnumExt_MyNames.J8 ||
            i == EnumExt_MyNames.J9 ||
            i == EnumExt_MyNames.J10 ||
            i == EnumExt_MyNames.J11 ||
            i == EnumExt_MyNames.J12 ||
            i == EnumExt_MyNames.J13 ||
            i == EnumExt_MyNames.J14 ||
            i == EnumExt_MyNames.J15 ||
            i == EnumExt_MyNames.J16 ||
            i == EnumExt_MyNames.JPlus);
        return orig(i) || extraPlayer;
    }


    private SlugcatStats.Name Name_ArenaColor(On.SlugcatStats.Name.orig_ArenaColor orig, int playerIndex)
    {
        //THIS VERSION WORKS EVEN IF MSC IS NOT ENABLED
		switch(playerIndex)
		{
		case 4:
			return EnumExt_MyNames.J5;
		case 5:
			return EnumExt_MyNames.J6;
		case 6:
			return EnumExt_MyNames.J7;
		case 7:
			return EnumExt_MyNames.J8;
		case 8:
			return EnumExt_MyNames.J9;
		case 9:
			return EnumExt_MyNames.J10;
		case 10:
			return EnumExt_MyNames.J11;
		case 11:
			return EnumExt_MyNames.J12;
		case 12:
			return EnumExt_MyNames.J13;
		case 13:
			return EnumExt_MyNames.J14;
		case 14:
			return EnumExt_MyNames.J15;
		case 15:
			return EnumExt_MyNames.J16;
		}

        if (playerIndex > 15) //MORE THAN 16?
            return EnumExt_MyNames.JPlus; //OR ELSE IT WILL RETURN NULL AND CRASH MOST THINGS

        return orig(playerIndex);
    }



    private Color PlayerGraphics_DefaultSlugcatColor(On.PlayerGraphics.orig_DefaultSlugcatColor orig, SlugcatStats.Name i) {

        Color result = orig(i);

        float dim = 0.8f;

        if (i == EnumExt_MyNames.J10) //Sofanthiel
            return new Color(0.09f, 0.14f, 0.31f);
        if (i == EnumExt_MyNames.J5) //Rivulet
            return new Color(0.56863f, 0.8f, 0.94118f);
        if (i == EnumExt_MyNames.J6) //Artificer
            return new Color(0.43922f, 0.13725f, 0.23529f);
        if (i == EnumExt_MyNames.J7) //Saint
            return new Color(0.66667f, 0.9451f, 0.33725f);
        if (i == EnumExt_MyNames.J8) //Spear
            return new Color(0.31f, 0.18f, 0.41f);
        if (i == EnumExt_MyNames.J9) //Gourmand
            return new Color(0.94118f, 0.75686f, 0.59216f);
        if (i == EnumExt_MyNames.J11) //pup1
            return new Color(0.6f * dim, 0.7f * dim, 0.9f * dim); //TOO CLOSE TO RIVULET! DIM IT A LITTLE...
        if (i == EnumExt_MyNames.J12) //pup2
            return new Color(0.48f, 0.87f, 0.81f);
        if (i == EnumExt_MyNames.J13) //Pebbles
            return new Color(1f, 0.4f, 0.79607844f);
        if (i == EnumExt_MyNames.J14) //Moon
            return new Color(0.13f, 0.53f, 0.69f);
        if (i == EnumExt_MyNames.J15) //NSH
            return new Color(0f, 1f, 0f);
        if (i == EnumExt_MyNames.J16) //Sliver
            return new Color(0.89f * dim, 0.89f * dim, 0.79f * dim); //TOO CLOSE TO SURVIVOR! DIM IT A LITTLE...
        if (i == EnumExt_MyNames.JPlus)
            return new Color(1f, 1f, 1f);

        return result;
    }
	
	



    private void PlayerSpecificMultiplayerHud_ctor(On.HUD.PlayerSpecificMultiplayerHud.orig_ctor orig, HUD.PlayerSpecificMultiplayerHud self, HUD.HUD hud, ArenaGameSession session, AbstractCreature abstractPlayer)
    {
        orig(self, hud, session, abstractPlayer);

        int playNum = (abstractPlayer.state as PlayerState).playerNumber;
        int rank = 0;

        while (playNum > 3)
        {
            playNum = playNum - 4;
            rank += 1;
        }

        //THEY'RE GOING TO NEED TO GO THROUGH THIS AGAIN...
        if (rank > 0)
        {
            switch (playNum)
            {
                case 0:
                    self.cornerPos = new Vector2(hud.rainWorld.options.ScreenSize.x - hud.rainWorld.options.SafeScreenOffset.x, 20f + hud.rainWorld.options.SafeScreenOffset.y);
                    self.flip = -1;
                    break;
                case 1:
                    self.cornerPos = new Vector2(hud.rainWorld.options.SafeScreenOffset.x, 20f + hud.rainWorld.options.SafeScreenOffset.y);
                    self.flip = 1;
                    break;
                case 2:
                    self.cornerPos = new Vector2(hud.rainWorld.options.SafeScreenOffset.x, hud.rainWorld.options.ScreenSize.y - 20f - hud.rainWorld.options.SafeScreenOffset.y);
                    self.flip = 1;
                    break;
                case 3:
                    self.cornerPos = new Vector2(hud.rainWorld.options.ScreenSize.x - hud.rainWorld.options.SafeScreenOffset.x, hud.rainWorld.options.ScreenSize.y - 20f - hud.rainWorld.options.SafeScreenOffset.y);
                    self.flip = -1;
                    break;
            }

            self.cornerPos = self.cornerPos + new Vector2(40 * rank * self.flip, 0);
            self.scoreCounter.pos = new Vector2(self.cornerPos.x + (float) self.flip * 20f + 0.01f, self.cornerPos.y + 0.01f);
        }
    }

    private void ArenaGameSession_SpawnPlayers(On.ArenaGameSession.orig_SpawnPlayers orig, ArenaGameSession self, Room room, List<int> suggestedDens)
    {
        if (!(ModManager.MSC && self.GameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge))
        {
            //SO IT SKIPS CHALLENGES.
            //BUT WHAT IF IT DIDNT...

            //WILL THIS INFINITE LOOP?
            if (suggestedDens != null)
            {
                int initialCount = suggestedDens.Count;
                for (int i = 0; i < initialCount; i++)
                {
                    Logger.LogInfo("DEN " + i);
                    suggestedDens.Add(suggestedDens[i]);
                }
            }

        }

        orig(self, room, suggestedDens);
    }


    private Color PlayerGraphics_SlugcatColor(On.PlayerGraphics.orig_SlugcatColor orig, SlugcatStats.Name i)
    {
        //Logger.LogInfo("SLUGCAT COLOR pt1 " + " - " + i);
        Color result = orig(i);
        //Logger.LogInfo("SLUGCAT COLOR pt2 " + result + " - " + i);

        if (i == null)
            return result;

        string source = i.ToString();
        int pNum = 0;
        if (source.Contains("JollyPlayer"))
        {
            string split = "JollyPlayer";
            pNum = int.Parse(source.Substring(source.IndexOf(split) + split.Length));
        }

        if (pNum > 4)
        {
            //IT'S A BONUS CAT! AND THE GAME IS BAD AT HANDLING US. SO WE GOTTA FIX IT OURSELVES...
            if (ModManager.CoopAvailable)
            {
                if ((Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO && (pNum - 1) > 0) || Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM)
                {
                    return PlayerGraphics.JollyColor(pNum - 1, 0);
                }
                //WHA?? WHAT IS THIS DOING HERE? WHAT DOES IT DO?.. WHATEVER I GUESS I'M KEEPING IT
                i = (Custom.rainWorld.options.jollyPlayerOptionsArray[pNum - 1].playerClass ?? i);
            }
            if (PlayerGraphics.CustomColorsEnabled())
            {
                return PlayerGraphics.CustomColorSafety(0);
            }
            //Debug.Log("DETERMINING CUSTOM COLOR FOR: " + i + " - P" + pNum);
            return PlayerGraphics.DefaultSlugcatColor(i);
        }
        //i
        return result;
    }

    //WHY DO P5 AND 6 STILL LOAD POINTING AT CONTROLLER SLOT 3&4? COULD IT BE THAT THE DATA READ FROM Options_ToString  IS SETTING THEM THIS WAY?
    //TAKE A LOOK AT THE OPTIONS TEXT FILES AND SEE WHAT IS BEING SAVED FOR THOSE VALUES

    //DO CONTROLLERS EVEN WORK?

    //IT STAYS CONNECTED TO THE CORRECT INPUT TYPE UNTIL YOU SELECT A NEW KEY WITH A CONTROLLER
    /*private string InputSelectButton_ButtonText(On.Menu.InputOptionsMenu.InputSelectButton.orig_ButtonText orig, Menu.Menu menu, bool gamePadBool, int player, int button, bool inputTesterDisplay)
    {
        string result = orig(menu, gamePadBool, player, button, inputTesterDisplay);
        if (player > 3)
        {
            
            ActionElementMap actionElementMap = null;
            for (int i = 0; i < (menu as InputOptionsMenu).inputActions[button].Length; i++)
            {
                //Debug.Log("INPUT BUTTON TEXT: " + (menu as InputOptionsMenu).inputActions[button][i] + " - " + button + " - " + (menu as InputOptionsMenu).inputActions[i][0]);
                int inputType = 0; //OKAY THIS DOESN'T WORK
                if (MPOptions.usingKeyboard[player - 4].Value == false)
                    inputType = 1;
    
                inputType = i;
    
                Debug.Log("INPUT TYPE: " + MPOptions.usingKeyboard[player - 4].Value);
                Debug.Log("INPUT BUTTON TEXT: " + (menu as InputOptionsMenu).inputActions[button][inputType] + " - " + (menu as InputOptionsMenu).inputActionCategories[button][inputType] + " - " + (menu as InputOptionsMenu).inputAxesPositive[button]);
    
    
                result = "????";
                switch ((menu as InputOptionsMenu).inputActions[button][inputType])
                {
                    case 0:
                        result = MPOptions.jumpKey[player - 4].Value.ToString();
                        break;
                    case 1:
                        result = ((menu as InputOptionsMenu).inputAxesPositive[button]) ? MPOptions.rightKey[player - 4].Value.ToString() : MPOptions.leftKey[player - 4].Value.ToString();
                        break;
                    case 2:
                        result = ((menu as InputOptionsMenu).inputAxesPositive[button]) ? MPOptions.upKey[player - 4].Value.ToString() : MPOptions.downKey[player - 4].Value.ToString();
                        break;
                    case 3:
                        result = MPOptions.grabKey[player - 4].Value.ToString();
                        break;
                    case 4:
                        result = MPOptions.throwKey[player - 4].Value.ToString();
                        break;
                    case 5:
                        result = "_"; //FORGET IT LOL
                        break;
                    case 6:
                        result = "_A"; 
                        break;
                    case 7:
                        result = "_B";
                        break;
                    case 8:
                        result = "_C";
                        break;
                    case 9:
                        result = "_D";
                        break;
                    case 10:
                        result = "_E";
                        break;
                    case 11:
                        result = MPOptions.mapKey[player - 4].Value.ToString();
                        break;
                }
                //MPOptions.usingKeyboard[k - 4].Value
                //return (menu as InputOptionsMenu).inputActions[button][0].ToString() + " - " + (menu as InputOptionsMenu).inputAxesPositive[button].ToString();
                //Debug.Log("INPUT BUTTON TEXT: " + (menu as InputOptionsMenu).inputActions[button][0] + " - " + (menu as InputOptionsMenu).inputActionCategories[button][0] + " - " + (menu as InputOptionsMenu).inputAxesPositive[button]);
                return result;
                // return (menu as InputOptionsMenu).inputActions[button][inputType] + " - " + (menu as InputOptionsMenu).inputActionCategories[button][inputType] + " - " + (menu as InputOptionsMenu).inputAxesPositive[button];
            }
    
            if (actionElementMap == null)
            {
                return "BORKED";
            }
            return actionElementMap.elementIdentifierName;
        }
        return result;
    }*/

    /*private void InputOptionsMenu_Update(On.Menu.InputOptionsMenu.orig_Update orig, InputOptionsMenu self)
    {
        bool flag = true;
        for (int i = 0; i < self.inputMappers.Length; i++)
        {
            if (self.inputMappers[i].status != InputMapper.Status.Idle)
            {
                flag = false;
            }
        }
    
        if (self.mappersStarted && flag)
        {
            Debug.Log("BLEH: ");
            if (self.selectedObject is InputOptionsMenu.InputSelectButton) //IF WE'VE SELECTED AN INPUT BUTTON
            {
                
                int plr = self.manager.rainWorld.options.playerToSetInputFor;
                int action = (self.selectedObject as InputOptionsMenu.InputSelectButton).index; //THIS DETERMINES WHAT ACTION IT WAS FOR
    
                //CHECK EVERY SINGLE INPUT BTN AND SEE IF ANY OF THEM ARE HELD DOWN.
                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)).OfType<KeyCode>())
                {
                    if (Input.GetKey(keyCode)) // || CustomInputExt.ResolveButtonDown(keyCode, ctrl, self.CurrentControlSetup.GetActivePreset())) {
                    {
                        Debug.Log("CUSTOM KEYBIND DETECTED: " + action + " PLR: " + plr);
    
                        switch (action)
                        {
                            case 0:
                                //MPOptions.mapKey[plr - 4].Value = keyCode;
                                break;
                            case 1:
                                MPOptions.mapKey[plr - 4].Value = keyCode;
                                break;
                            case 2:
                                MPOptions.grabKey[plr - 4].Value = keyCode;
                                break;
                            case 3:
                                MPOptions.jumpKey[plr - 4].Value = keyCode;
                                break;
                            case 4:
                                MPOptions.throwKey[plr - 4].Value = keyCode;
                                break;
                            case 5:
                                MPOptions.leftKey[plr - 4].Value = keyCode;
                                break;
                            case 6:
                                MPOptions.upKey[plr - 4].Value = keyCode;
                                break;
                            case 7:
                                MPOptions.rightKey[plr - 4].Value = keyCode;
                                break;
                            case 8:
                                MPOptions.downKey[plr - 4].Value = keyCode;
                                break;
                        }
                    }
                }
            }
        }
    
        orig(self);
    }*/


	/* GOODNIGHT, MY SWEET PRINCE...
    private Player.InputPackage RWInput_PlayerInputLogic(On.RWInput.orig_PlayerInputLogic orig, int categoryID, int playerNumber, RainWorld rainWorld)
    {
        if (rainWorld.options.controls[playerNumber].player == null)
        {
            Debug.Log("NULLED PLAYER INFO! ");
            //rainWorld.options.controls[playerNumber] = new Options.ControlSetup(playerNumber, false);
			//FILL THIS IN WITH DUMMY INFO COPIED FROM THE PREVIOUS PLAYER, JUST SO THAT THE GAME DOESN'T CRASH. WE'LL BE IGNORING MOST OF THAT INFO PAST PLAYER 4 ANYWAYS
            rainWorld.options.controls[playerNumber].player = rainWorld.options.controls[playerNumber - 1].player;
            Debug.Log("NULLED PLAYER INFO COMPLETE! " + rainWorld.options.controls[playerNumber]);
   
            //HEY DON'T FORGET THE OTHER PARTS OF THEIR CONTROL PACKAGE!
			//intG
            //string myGamePad = Math.Max(int.Parse(MPOptions.gamePadType[playerNumber - 4].Value) - 1, 0).ToString();
            string myGamePad = MPOptions.gamePadType[playerNumber - 4].Value;
            rainWorld.options.controls[playerNumber].gamePadNumber = MPOptions.GetGamepadInt2(myGamePad);
            rainWorld.options.controls[playerNumber].usingGamePadNumber = rainWorld.options.controls[playerNumber].gamePadNumber;
            rainWorld.options.controls[playerNumber].controlPreference = (myGamePad == "Keyboard") ? Options.ControlSetup.ControlToUse.KEYBOARD : Options.ControlSetup.ControlToUse.SPECIFIC_GAMEPAD;
            //rainWorld.options.controls[playerNumber].UpdateControlPreference((myGamePad == "Keyboard") ? Options.ControlSetup.ControlToUse.KEYBOARD : Options.ControlSetup.ControlToUse.SPECIFIC_GAMEPAD, false);
   
   
            //WE CAN'T RELY ON THIS TO CATCH PLAYER 5 BECAUSE THEY HAVE A SLOT ALREADY!
            //OKAY THIS IS KIND OF SILLY BUT SINCE WE DON'T WANT THIS TO RUN EVERY TICK, LET'S JUST RUN IT 3 TIMES. 
            //string p5GamePad = Math.Max(int.Parse(MPOptions.gamePadType[0].Value) - 1, 0).ToString();
            string p5GamePad = MPOptions.gamePadType[0].Value;
            rainWorld.options.controls[4].gamePadNumber = MPOptions.GetGamepadInt2(p5GamePad);
            rainWorld.options.controls[4].usingGamePadNumber = rainWorld.options.controls[4].gamePadNumber;
            rainWorld.options.controls[4].controlPreference = (p5GamePad == "Keyboard") ? Options.ControlSetup.ControlToUse.KEYBOARD : Options.ControlSetup.ControlToUse.SPECIFIC_GAMEPAD;
        }
   
   
        try
        {
            //Debug.Log("NULLED PLAYER INFO! " + playerNumber + " - " + rainWorld.options.controls[playerNumber].gamePadNumber);
            //bool thing = Input.legacy.Input.GetKey(KeyCode.UpArrow); //options.controls[playerNumber].KeyboardLeft
            //OKAY WE GOTTA CATCH IT AND REPLACE IT
            if (playerNumber > 3)
            {
                Player.InputPackage inputPackage = new Player.InputPackage();
                int plr = playerNumber;
				
                if (categoryID == 0)
                {
                    if (Input.GetKey(MPOptions.leftKey[plr - 4].Value))
                    {
                        inputPackage.x--;
                    }
                    if (Input.GetKey(MPOptions.rightKey[plr - 4].Value))
                    {
                        inputPackage.x++;
                    }
                    if (Input.GetKey(MPOptions.downKey[plr - 4].Value))
                    {
                        inputPackage.y--;
                    }
                    if (Input.GetKey(MPOptions.upKey[plr - 4].Value))
                    {
                        inputPackage.y++;
                    }
                    if (inputPackage.y < 0)
                    {
                        inputPackage.downDiagonal = inputPackage.x;
                    }
                    if (Input.GetKey(MPOptions.jumpKey[plr - 4].Value))
                    {
                        inputPackage.jmp = true;
                    }
                    if (Input.GetKey(MPOptions.throwKey[plr - 4].Value))
                    {
                        inputPackage.thrw = true;
                    }
                    if (Input.GetKey(MPOptions.mapKey[plr - 4].Value))
                    {
                        inputPackage.mp = true;
                    }
                    if (Input.GetKey(MPOptions.grabKey[plr - 4].Value))
                    {
                        inputPackage.pckp = true;
                    }
					//WAIT! I THINK I FINALLY GOT IT...
					if (MPOptions.gamePadType[playerNumber - 4].Value == "Keyboard")
					{
						inputPackage.analogueDir = new Vector2(inputPackage.x, inputPackage.y); //UHH... IS THIS GOOD ENOUGH?
					}
					else
					{   //GETTING THE ANOLOG INPUT WITHOUT USING REWIRED.PLAYER!!!!!
                        string port = (rainWorld.options.controls[plr].gamePadNumber + 1).ToString();
                        inputPackage.analogueDir = new Vector2(Input.GetAxisRaw("Horizontal" + port), Input.GetAxisRaw("Vertical" + port));
					}
					
					
					
					inputPackage.analogueDir = Vector2.ClampMagnitude(inputPackage.analogueDir * (ModManager.MMF ? rainWorld.options.analogSensitivity : 1f), 1f);
					if (inputPackage.analogueDir.x < -0.5f)
						inputPackage.x = -1;
					if (inputPackage.analogueDir.x > 0.5f)
						inputPackage.x = 1;
					if (inputPackage.analogueDir.y < -0.5f)
						inputPackage.y = -1;
					if (inputPackage.analogueDir.y > 0.5f)
						inputPackage.y = 1;
					
					if (ModManager.MMF)
					{
						if (inputPackage.analogueDir.y < -0.05f || inputPackage.y < 0)
						{
							if (inputPackage.analogueDir.x < -0.05f || inputPackage.x < 0)
								inputPackage.downDiagonal = -1;
							else if (inputPackage.analogueDir.x > 0.05f || inputPackage.x > 0)
								inputPackage.downDiagonal = 1;
						}
					}
					else if (inputPackage.analogueDir.y < -0.05f)
					{
						if (inputPackage.analogueDir.x < -0.05f)
							inputPackage.downDiagonal = -1;
						else if (inputPackage.analogueDir.x > 0.05f)
							inputPackage.downDiagonal = 1;
					}
                }
				else
				{
                    //AND NOW THE CONTROLLER VERSION... OH BOY...
                    //WAIT NO I DON'T THINK THIS IS CONTROLLER VERSION. I THINK THIS IS JUST... ""OTHER""...
                    //Debug.Log("SOME WEIRD OTHER CONTROL! CATEGORY 1 (NOT 0)");
					
					//ACTUALLY, I THINK IT BECOMES CATEGORY 1 IF IT'S GETTING INPUT FOR UI PURPOSES. BUT IN-GAME JUST USES CATEGORY 0? I THINK
                }
   
                return inputPackage;
            }
        }
        catch (Exception arg)
        {
            Logger.LogError(string.Format("REDUCE! - ", arg));
        }
        
        Player.InputPackage result = orig(categoryID, playerNumber, rainWorld);
        return result;
    }
    */

    private void JollyPlayerSelector_Update(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_Update orig, JollyPlayerSelector self)
    {
        orig(self);
		
		//IDK WHAT I WAS EVEN DOING HERE
        // if (self.index == 0 && SlugcatStats.IsSlugcatFromMSC(self.JollyOptions(self.index).playerClass)
            // && !(ModManager.Expedition && self.menu.manager.rainWorld.ExpeditionMode))
        // {
            // self.classButton.GetButtonBehavior.greyedOut = true;
        // }
		
		//I LIKE THIS ONE :)
        //AAAND UNLOCK P1 EXPEDITION MODE BECAUSE WHY NOT
        if (self.index == 0 && ModManager.Expedition && self.menu.manager.rainWorld.ExpeditionMode)
            self.classButton.GetButtonBehavior.greyedOut = false;
    }
    
	//OKAY WEIRD BUT WE A DEFINITELY DUPLICATING MENU OBJECTS WHEN SWITCHING BETWEEN ARENA MODES WHILE MSC IS DISABLED...
    private void MultiplayerMenu_InitiateGameTypeSpecificButtons(On.Menu.MultiplayerMenu.orig_InitiateGameTypeSpecificButtons orig, MultiplayerMenu self) {
        orig(self);

        if (PlyCnt() <= 4) return;
        
        if (self.playerJoinButtons != null) {
            //foreach (var playerJoinButton in self.playerJoinButtons) playerJoinButton.pos.x -= shift;
            for (int i = 0; i < self.playerJoinButtons.Length; i++) {
                //float shift = 235 + i * 10; //298 //NORMALLY 120
                //float shift = 235 + i * 4.1f * self.playerJoinButtons.Length * Mathf.Lerp((1366 / Custom.rainWorld.options.ScreenSize.x), 1f, 0.4f);
                float shift = 235 + i * (PlyCnt() > 8 ? 4.1f : 1.2f) * self.playerJoinButtons.Length * Mathf.Lerp((1366 / Custom.rainWorld.options.ScreenSize.x), 1f, 0.4f);
                if (PlyCnt() > 8) {
                    //EXTRA SHIFT
                    shift -= 15;

                    //SHRINK THE BUTTONS!!
                    self.playerJoinButtons[i].size /= 2f;
                    self.playerJoinButtons[i].lastSize /= 2f;
                    self.playerJoinButtons[i].portrait.sprite.scale = 0.5f;
                    self.playerJoinButtons[i].portrait.pos -= self.playerJoinButtons[i].size / 2f;
                    foreach (var playerButtonSubObject in self.playerJoinButtons[i].subObjects) {
                        if (!(playerButtonSubObject is RectangularMenuObject rectMenuObject)) return;
                        rectMenuObject.size /= 2;
                        rectMenuObject.lastSize /= 2;
                        //rectMenuObject.pos += rectMenuObject.size;
                    }
                }
                self.playerJoinButtons[i].pos.x -= shift;
                if (ModManager.MSC && self.playerClassButtons != null) {
                    self.playerClassButtons[i].pos.x -= shift;
                    //IF WE ARE USING SHRUNK ICONS, SHIFT EVERY OTHER CLASS BUTTON UP TOP 
                    if (PlyCnt() > 8) {
                        self.playerClassButtons[i].pos.x -= self.playerJoinButtons[i].size.x / 2f;
                        self.playerClassButtons[i].size.y *= 0.75f;
                        if (i % 2 == 0)
                            self.playerClassButtons[i].pos.y += self.playerJoinButtons[i].size.y * 2f;
                    }
                }
            }
        }
        
        if (self.levelSelector != null) {
            self.levelSelector.pos -= new Vector2(165, 0);
            self.levelSelector.lastPos = self.levelSelector.pos;
        }
    }


    private void SandboxSettingsInterface_AddPositionedScoreButton(On.Menu.SandboxSettingsInterface.orig_AddPositionedScoreButton orig, SandboxSettingsInterface self, SandboxSettingsInterface.ScoreController button, ref IntVector2 ps, Vector2 additionalOffset) {

        additionalOffset.x -= 180;
        orig(self, button, ref ps, additionalOffset);
    }

    public static void BPPlayer_Update(On.Player.orig_Update orig, Player self, bool eu) {
        orig(self, eu);

        //BRING BACK THIS CLASSIC JOLLYCOOP FIXES FEATURE THAT IS VERY MUCH NEEDED IN CO-OP
        if (!rotundWorldEnabled && MPOptions.grabRelease.Value && self.input[0].jmp && !self.input[1].jmp && self.grabbedBy?.Count > 0) {
            for (int graspIndex = self.grabbedBy.Count - 1; graspIndex >= 0; graspIndex--) {
                if (self.grabbedBy[graspIndex] is Creature.Grasp grasp && grasp.grabber is Player player_) {
                    if (!self.isNPC || (player_.isNPC)) //PUPS SHOULD LET GO OF OTHER PUPS
                        player_.ReleaseGrasp(grasp.graspUsed); // list is modified
                }
            }
        }

        //MAKE SHELTERS CLOSE EASIER IF AT LEAST 4 PEOPLE ARE READY TO SLEEP
        if (playersReadyToSleep >= 4 && forceShutTimer >= 200) {
            if (self.room != null && self.room.abstractRoom.shelter && self.AI == null && self.room.game.IsStorySession && !self.dead && !self.Sleeping && self.room.shelterDoor != null && !self.room.shelterDoor.Broken) {
                if (self.shortcutDelay < 1 && self.readyForWin) {
                    if (ModManager.CoopAvailable)
                        self.ReadyForWinJolly = true;
                    self.room.shelterDoor.Close(); //THIS JUST CHECKS FOR CLOSE
                }
            }
        }
    }


    public static int playersReadyToSleep = 0;
    public static int forceShutTimer = 0;
    private void ShelterDoor_Update(On.ShelterDoor.orig_Update orig, ShelterDoor self, bool eu) {

        orig(self, eu);

        playersReadyToSleep = 0;
        //COUNT PLAYERS READY TO HIBERNATE
        for (int k = 0; k < self.room.game.Players.Count; k++) {
            if (self.room.game.Players[k].realizedCreature != null && self.room.game.Players[k].realizedCreature is Player player) {
                if (player.ReadyForWinJolly)
                    playersReadyToSleep++;
            }
        }

        //GIVE THE STRAGGLERS 6 SECONDS TO CRAWL INSIDE
        if (playersReadyToSleep >= 4)
            forceShutTimer++;
        else
            forceShutTimer = 0;
    }

    private bool JollyCustom_ForceActivateWithMSC(On.JollyCoop.JollyCustom.orig_ForceActivateWithMSC orig)
    {
        if (MPOptions.downpourCoop.Value)
            return true;
        else
            return orig();
    }
    
    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
	{
		orig(self);
        MachineConnector.SetRegisteredOI("manyslugcats", new MPOptions());
 
        for (int i = 0; i < ModManager.ActiveMods.Count; i++)
		{
			if (ModManager.ActiveMods[i].id == "willowwisp.bellyplus")
            {
				rotundWorldEnabled = true;
				Debug.Log("ROTUND WORLD DETECTED");
			}
        }
		
    }
	
	public static bool rotundWorldEnabled = false;
    
	private bool Options_ApplyOption(On.Options.orig_ApplyOption orig, Options self, string[] splt2)
    {
        
		bool result = false;
        try
        {
            //Logger.LogInfo("APPLY OPTION: " + splt2[0] + " - " +self.controls.Length);
            result = orig.Invoke(self, splt2);
        }
        catch
        {
            Logger.LogError("FAILED TO APPLY OPTION!!");
            result = false;
        }
		
		if (splt2[0] == "InputSetup2")
		{
			result = false; //BECAUSE WE HIT A MATCH
			Logger.LogInfo("---APPLYING EXTRA INPUT OPTION! " + self.controls.Length + " - " + splt2[1] + " - " + splt2[2]);
            //WE SET IT TO CONTROL SO THAT THE NUMBER OF ENTRIES IN THE TABLE MATCHES CONTROL
            int entryNum = int.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
            try
            {
                //Logger.LogInfo("-APPLYING TO ARRAY " + entryNum);
                self.controls[entryNum].FromString(splt2[2]);// = myControlArry[entryNum];
            }
            catch (Exception arg)
            {
                Logger.LogError(string.Format("FAILED READ/APPLY EXTRA INPUT SETTINGS! - ", arg));
            }
        }

        //6-8-23 TRYING AGAIN BUT FOR JOLLY EXTRAS
		if (splt2[0] == "JollySetupPlayersExtra")
		{
			result = false; //BECAUSE WE HIT A MATCH
			if (ModManager.JollyCoop)
			{
				string[] array3 = Regex.Split(splt2[1], "<optC>");
				int num = 4; //START AT 4
				foreach (string text in array3)
				{
					if (!(text == string.Empty) && num < PlyCnt()) //CHECK FOR THIS IN CASE WE SAVED EXTRA VALUES
					{
                        Logger.LogInfo("--- JOLLY PLAYER OPTIONS!(V2) " + text);
                        JollyPlayerOptions jollyPlayerOptions = new JollyPlayerOptions(num);
						jollyPlayerOptions.FromString(text); 
						self.jollyPlayerOptionsArray[jollyPlayerOptions.playerNumber] = jollyPlayerOptions;
						num++;
					}
				}
			}
			else
			{
				result = true;
			}
		}
		
        return result;
    }

   //6-8 WAIT NO WE NEED TO USE THIS THIS BREAKS THE SAVES OTHERWISE
    private string Options_ToStringNonSynced(On.Options.orig_ToStringNonSynced orig, Options self)
    {
		int servMemory = self.playerToSetInputFor;
		self.playerToSetInputFor = Math.Min(self.playerToSetInputFor, 3); //DON'T LET THIS GET SAVED OUTSIDE OF THE NORMAL RANGE
		//AND DON'T LET CONTROLS SAVE FOR MORE THAN 4 PLAYERS
		Options.ControlSetup[] controlsMemory = self.controls;
		self.controls = new Options.ControlSetup[4];
		for (int i = 0; i < self.controls.Length; i++) 
        { 
            self.controls[i] = controlsMemory[i];
		}
		
		string result = orig(self);

        //5-18-23 INSTEAD, MAYBE WE OUTPUT THAT STUFF TO A FILE AS A DIFFERENT NAME?
        for (int k = 4; k < PlyCnt(); k++)
        {
            Logger.LogInfo("---SAVING CUTSOM OPTIONS STRING FOR PLAYER: " + k);
            result += string.Format(CultureInfo.InvariantCulture, "InputSetup2<optB>{0}<optB>{1}<optA>", k, controlsMemory[k]);
            // !!! WARNING!!!! THE GAME SEEMS TO WANT TO SET P5'S SPECIFIC CONTROLLER TO #3, WHICH CRASHES THE GAME IF NOT PLUGGED IN. UNDO THAT!!!
        }

        self.controls = controlsMemory;
		self.playerToSetInputFor = servMemory;
        return result;
	}
    
    //THIS IS FOR SAVING OUR DATA (TO THE TEXT FILES AND STUFF)
	private string Options_ToString(On.Options.orig_ToString orig, Options self)
    {
        JollyPlayerOptions[] optionsMemory = self.jollyPlayerOptionsArray;
        Options.ControlSetup[] controlsMemory = self.controls;

        //REBUILD THE ARRAY WITH A MAX OF 4 PLAYERS
        self.jollyPlayerOptionsArray = new JollyPlayerOptions[4];
        self.controls = new Options.ControlSetup[4]; //WE MAY NEED THE SAME FOR INPUT SETUP TOO...
        for (int j = 0; j < self.jollyPlayerOptionsArray.Length; j++)
        {
            self.jollyPlayerOptionsArray[j] = optionsMemory[j];
        }
        for (int i = 0; i < self.controls.Length; i++)
        {
            self.controls[i] = controlsMemory[i];
        }

        //RUN THE ORIGINAL WITH THE SHORTENED TABLES
        string text = orig(self);
		
        //NOW WE SAVE THE EXTRA VALUES 
		for (int k = 4; k < PlyCnt(); k++)
        {
            Logger.LogInfo("---SAVING CUTSOM OPTIONS STRING FOR PLAYER: " + k);
            text += string.Format(CultureInfo.InvariantCulture, "InputSetup2<optB>{0}<optB>{1}<optA>", k, controlsMemory[k]);
        }
		
		//6-8-23 -OKAY THE MODDED VERSION KINDA STINKS BECAUSE IT DOESN'T ACTUALLY SAVE MOST OF THE TIME
		if (ModManager.JollyCoop)
		{
			text += "JollySetupPlayersExtra<optB>";
			for (int j = 4; j < optionsMemory.Length; j++)
			{
				text = text + optionsMemory[j].ToString() + "<optC>";
			}
			text += "<optA>";
		}

        //THEN RESTORE THE ARRAY TO IT'S FULL PLAYER COUNT
        self.jollyPlayerOptionsArray = optionsMemory;
        self.controls = controlsMemory;
        return text;
    }

    private void RainWorldGame_JollySpawnPlayers(On.RainWorldGame.orig_JollySpawnPlayers orig, RainWorldGame self, WorldCoordinate location)
    {

        Debug.Log("---Number of jolly players: " + self.rainWorld.options.JollyPlayerCount.ToString()); // 
        Debug.Log("---accesing directly: " + Enumerable.Count<JollyPlayerOptions>(self.rainWorld.options.jollyPlayerOptionsArray, (JollyPlayerOptions x) => x.joined).ToString());
        Debug.Log("---BONUS: ARRAY LENGTH" + self.rainWorld.options.jollyPlayerOptionsArray.Length);
        int PlayerArrSize = self.rainWorld.options.jollyPlayerOptionsArray.Length;

        JollyPlayerOptions[] jollyPlayerOptionsArray = self.rainWorld.options.jollyPlayerOptionsArray;
        for (int i = 0; i < jollyPlayerOptionsArray.Length; i++)
        {
            Debug.Log(jollyPlayerOptionsArray[i].ToString());
        }
		
		
        //IF WE'RE PLAYING WITH A NORMAL AMOUNT (LESS THAN 5) RUN THIS
        if (self.rainWorld.options.JollyPlayerCount < PlayerArrSize)
        {
            //WE HAVE TO BRIEFLY PRETEND OUR PLAYEROPTIONSARRAY IS ONLY AS MANY ENTRIES AS WE'VE SELECTED
            JollyPlayerOptions[] optionsMemory = self.rainWorld.options.jollyPlayerOptionsArray;
            self.rainWorld.options.jollyPlayerOptionsArray = new JollyPlayerOptions[self.rainWorld.options.JollyPlayerCount];
            for (int j = 0; j < self.rainWorld.options.jollyPlayerOptionsArray.Length; j++)
            {
                self.rainWorld.options.jollyPlayerOptionsArray[j] = optionsMemory[j];
            }
            //Debug.Log("---CUSTOM JOLLY SPAWN" + self.rainWorld.options.jollyPlayerOptionsArray.Length);
            orig(self, location);

            //THEN RESTORE THE ARRAY TO IT'S FULL PLAYER COUNT
            self.rainWorld.options.jollyPlayerOptionsArray = optionsMemory;
        }
        else
            orig(self, location); //IF WE'RE PLAYING WITH EVERYONE ENABLED, WE'RE GOOD TO RUN THE ORIGINAL FOR SOME REASON

        Debug.Log("----JOLLY SPAWN SUCCESS");
    }

    
    /*
    private void PlayerButton_ctor(On.Menu.InputOptionsMenu.PlayerButton.orig_ctor orig, InputOptionsMenu.PlayerButton self, Menu.Menu menu, MenuObject owner, Vector2 pos, InputOptionsMenu.PlayerButton[] array, int index)
    {
        orig(self, menu, owner, pos, array, index);
    
        //SHIFT BUTTONS UP IF WE NEED ROOM
        if (PlyCnt() == 5)
        {
            self.originalPos.y += 10 + (3 * (index * PlyCnt() - 4));
        }
        else if (PlyCnt() > 5)
        {
            //INCREASE THE HIGHT, AND POSSIBLY BRING THEM CLOSER TOGETHER
            self.originalPos.y += 25 + (8 * (index * PlyCnt() - 4));
            self.menuLabel.pos.y += 25;
            //self.menuLabel.Container.MoveToFront();
            self.menuLabel.label.MoveToFront();
        }
    }
	*/

    private void Replace4WithMore(ILContext il) => Replace4WithMore(il, false);

    private void Replace4WithMore(ILContext il, bool checkLdarg = false, int maxReplace = -1) {
        List<Func<Instruction, bool>> predicates = new List<Func<Instruction, bool>>();
        
        if(checkLdarg) predicates.Add(i => i.MatchLdarg(0));
        
        predicates.Add(i => i.MatchLdcI4(4));

        var cursor = new ILCursor(il);
        var x = 0;
        
        while (cursor.TryGotoNext(MoveType.After, predicates.ToArray())) {
            x++;
            //cursor.Emit(OpCodes.Ldloc, player); //THESE LIKE, BECOME ARGUMENTS WITHIN EMITDELEGATE  I THINK?
            //cursor.Emit(OpCodes.Ldloc, k);

            //cursor.EmitDelegate((float rad, Player player, int k) =>
            cursor.EmitDelegate((int oldNum) => plyCnt);
            
            if(maxReplace != 1 && maxReplace == x) break;
        }

        if (x == 0) {
            Logger.LogWarning($"A method had NONE adjustments made to account for increased player count: [Adjustments #: {x}, Method: {il.Method.Name}]");
        } else {
            Logger.LogInfo($"A method had adjustments made to account for increased player count: [Adjustments #: {x}, Method: {il.Method.Name}]");
        }
    }

    private void Options_ctor(MonoMod.Cil.ILContext il)
    {
        var cursor = new ILCursor(il);
        var x = 0;
        while (cursor.TryGotoNext(MoveType.After, 
            i => i.MatchLdarg(0),
            i => i.MatchLdcI4(4)
        ))
        {
            x++;
            //cursor.Emit(OpCodes.Ldloc, player); //THESE LIKE, BECOME ARGUMENTS WITHIN EMITDELEGATE  I THINK?
            //cursor.Emit(OpCodes.Ldloc, k);

            //cursor.EmitDelegate((float rad, Player player, int k) =>
            cursor.EmitDelegate((int oldNum) =>
            {
                return plyCnt;
            });
        }

        Logger.LogInfo("TESTMYSLUGCAT IL LINES ADDED! " + x);
    }

    //----

    public static class EnumExt_MyNames {
        public static SlugcatStats.Name J5 = new SlugcatStats.Name("J5", true);
        public static SlugcatStats.Name J6 = new SlugcatStats.Name("J6", true);
        public static SlugcatStats.Name J7 = new SlugcatStats.Name("J7", true);
        public static SlugcatStats.Name J8 = new SlugcatStats.Name("J8", true);
        public static SlugcatStats.Name J9 = new SlugcatStats.Name("J9", true);
        public static SlugcatStats.Name J10 = new SlugcatStats.Name("J10", true);
        public static SlugcatStats.Name J11 = new SlugcatStats.Name("J11", true);
        public static SlugcatStats.Name J12 = new SlugcatStats.Name("J12", true);
        public static SlugcatStats.Name J13 = new SlugcatStats.Name("J13", true);
        public static SlugcatStats.Name J14 = new SlugcatStats.Name("J14", true);
        public static SlugcatStats.Name J15 = new SlugcatStats.Name("J15", true);
        public static SlugcatStats.Name J16 = new SlugcatStats.Name("J16", true);
        public static SlugcatStats.Name JPlus = new SlugcatStats.Name("JPlus", true);
    }
}
