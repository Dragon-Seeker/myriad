using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using UnityEngine;
using RWCustom;
using BepInEx;
using BepInEx.Logging;
using JollyCoop.JollyMenu;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Rewired;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ManySlugCats;

[BepInPlugin("manyslugcats", "Many Slug Cats", "1.0.0")]
public class ManySlugCatsMod : BaseUnityPlugin {
    
    public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("ManySlugCats");

    bool init;

    BindingFlags otherMethodFlags = BindingFlags.Instance | BindingFlags.NonPublic;
    BindingFlags myMethodFlags = BindingFlags.Static | BindingFlags.Public;
    
    public void OnEnable() {
        try {
            new MorePlayers().OnEnable();

            On.Menu.InputOptionsMenu.ctor += addMorePlayerOptions;
            //On.Menu.InputOptionsMenu.PlayerButton.ctor += adjustPlayerOptionSize;

            // Add hooks here
            // On.RainWorld.OnModsInit += OnModsInit;
            On.Options.ctor += addMoreJollyOptions;
            //
            // On.JollyCoop.JollyEnums.RegisterAllEnumExtensions += extendJollyEnumData;
            // On.JollyCoop.JollyEnums.UnregisterAllEnumExtensions += removeExtendedJollyEnum;
            // On.SlugcatStats.HiddenOrUnplayableSlugcat += hideExtraJollyEnums;

            //IL.JollyCoop.JollyMenu.JollySlidingMenu.ctor += IL_adjustPlayerSelectGUI;

            On.JollyCoop.JollyMenu.JollySlidingMenu.ctor += adjustPlayerSelectGUI;
            On.JollyCoop.JollyMenu.JollySlidingMenu.NumberPlayersChange += accountForMoreThanFour;
            // On.JollyCoop.JollyMenu.JollySlidingMenu.UpdatePlayerSlideSelectable += preventOutOfBounds;

            // On.PlayerGraphics.PopulateJollyColorArray += increasePlayerColorArray;
            // On.PlayerGraphics.SlugcatColor += adjustPlayerColor;
            //
            // On.JollyCoop.JollyCustom.Log += forceInstantLogging;

            //On.RainWorldGame.JollySpawnPlayers += rewriteSpawnMethod;

            On.StoryGameSession.ctor += adjustPlayerRecordArray;
            // On.StoryGameSession.CreateJollySlugStats += override_CreateJollySlugStats;

            //On.PlayerGraphics.ApplyPalette += redirectPlayerRendering;
            On.Menu.MenuIllustration.ctor += MenuIllustration_ctor;

            logger.LogMessage("Checking Patch");
        } catch (Exception e) {
            logger.LogMessage("ManySlugCats failed to load due to an exception being thrown!");
            logger.LogMessage(e.ToString());
            throw e;
        }

        RainWorld.PlayerObjectBodyColors = new Color[8];
    }


    //I JUST COPIED THIS IN HERE SO IT RUNS FIRST I GUESS? DO I STILL NEED TO RUN IT IN THE OTHER ONE THEN?
    private void MenuIllustration_ctor(On.Menu.MenuIllustration.orig_ctor orig, MenuIllustration self, Menu.Menu menu, MenuObject owner, string folderName, string fileName, Vector2 pos, bool crispPixels, bool anchorCenter)
    {
        string newFileName = fileName;
        string lowerName = fileName.ToLower();
        if (lowerName.StartsWith("multiplayerportrait"))
        {
            string substr1 = lowerName.Replace("multiplayerportrait", "").Substring(0, 1); //GETS THE PLAYER NUMBER
            string substr2 = lowerName.Replace("multiplayerportrait", "").Substring(1); //THE REST OF THE NUMBERS
            //IF OUR PLAYER NUM IS HIGHER THAN EXPECTED, RETURN THE 4TH PLAYER IMAGE VERSION
            if (Convert.ToInt32(substr1) > 3)
                substr1 = "3";

            //REBUILD IT
            newFileName = "MultiplayerPortrait" + substr1 + substr2;
            logger.LogMessage("FINAL FILE: " + substr1 + substr2 + "  -  " + newFileName);
        }
        else if (lowerName.StartsWith("gamepad") && lowerName.Length == 8)
        {
            int playNum = int.Parse(lowerName.Replace("gamepad", "")); //GETS THE PLAYER NUMBER
            if (playNum > 4)
                newFileName = "GamepadAny"; //JUST A PLACEHOLDER
            logger.LogMessage("FINAL FILE: " + playNum + "  -  " + newFileName);
        }
        orig.Invoke(self, menu, owner, folderName, newFileName, pos, crispPixels, anchorCenter);
    }

    //----

    // private void forceInstantLogging(On.JollyCoop.JollyCustom.orig_Log orig, string logText, bool throwException = false) {
    //     orig(logText, throwException);
    //
    //     JollyCustom.WriteToLog();
    // }

    // private void rewriteSpawnMethod(On.RainWorldGame.orig_JollySpawnPlayers orig, RainWorldGame self, WorldCoordinate location) {
    //     int index = self.rainWorld.options.JollyPlayerCount;
    //     string str1 = index.ToString();
    //     index = (self.rainWorld.options.jollyPlayerOptionsArray)
    //         .Count((x => x.joined));
    //     string str2 = index.ToString();
    //     JollyCustom.Log("Number of jolly players: " + str1 + " accesing directly: " + str2);
    //     JollyPlayerOptions[] playerOptionsArray = self.rainWorld.options.jollyPlayerOptionsArray;
    //     for (index = 0; index < playerOptionsArray.Length; ++index) 
    //         JollyCustom.Log(playerOptionsArray[index].ToString());
    //     for (int number = 1; number < self.rainWorld.options.jollyPlayerOptionsArray.Length; ++number)
    //     {
    //         if (!self.rainWorld.options.jollyPlayerOptionsArray[number].joined)
    //         {
    //             System.Type type = self.rainWorld.setup.GetType();
    //             index = number + 1;
    //             string name = "player" + index.ToString();
    //
    //             var field = type.GetField(name);
    //
    //             if (field != null && !(bool)field.GetValue((object)self.rainWorld.setup)) continue;
    //         }
    //
    //         JollyCustom.Log("[JOLLY] Spawning player: " + number.ToString());
    //         AbstractCreature abstractCreature = new AbstractCreature(self.world,
    //         StaticWorld.GetCreatureTemplate("Slugcat"), (Creature)null, location, new EntityID(-1, number));
    //         AbstractCreature crit = abstractCreature;
    //         int playerNumber = number;
    //         index = number + 1;
    //         SlugcatStats.Name slugcatCharacter = new SlugcatStats.Name("JollyPlayer" + index.ToString());
    //         PlayerState playerState = new PlayerState(crit, playerNumber, slugcatCharacter, false)
    //         {
    //             isPup = self.rainWorld.options.jollyPlayerOptionsArray[number].isPup,
    //             swallowedItem = (string)null
    //         };
    //         abstractCreature.state = (CreatureState)playerState;
    //         self.world.GetAbstractRoom(abstractCreature.pos.room).AddEntity((AbstractWorldEntity)abstractCreature);
    //         JollyCustom.Log("Adding player: " + (abstractCreature.state as PlayerState).playerNumber.ToString());
    //         index = (self.session as StoryGameSession).playerSessionRecords.Length;
    //         JollyCustom.Log("Player session records: " + index.ToString());
    //         self.session.AddPlayer(abstractCreature);
    //     }
    // }

    //Test if needed still??
    private void adjustPlayerRecordArray(On.StoryGameSession.orig_ctor orig, StoryGameSession self, SlugcatStats.Name saveStateNumber, RainWorldGame game) {
        orig(self, saveStateNumber, game);

        self.playerSessionRecords = new PlayerSessionRecord[8];
    }

    // private void override_CreateJollySlugStats(On.StoryGameSession.orig_CreateJollySlugStats orig,
    //     StoryGameSession self, bool m)
    // {
    //     if (self.game.Players.Count == 0) {
    //         JollyCustom.Log("[JOLLY] NO PLAYERS IN SESSION!!");
    //     } else {
    //         self.characterStatsJollyplayer = new SlugcatStats[8]; //CHANGE IS HERE
    //         PlayerState state1 = self.game.Players[0].state as PlayerState;
    //         SlugcatStats slugcatStats = new SlugcatStats(self.saveState.saveStateNumber, m);
    //         for (int index = 0; index < self.game.world.game.Players.Count; ++index)
    //         {
    //             PlayerState state2 = self.game.Players[index].state as PlayerState;
    //             SlugcatStats.Name slugcat = self.game.rainWorld.options.jollyPlayerOptionsArray[state2.playerNumber]
    //                 .playerClass;
    //             if ((ExtEnum<SlugcatStats.Name>)slugcat == (ExtEnum<SlugcatStats.Name>)null)
    //             {
    //                 slugcat = self.saveState.saveStateNumber;
    //                 JollyCustom.Log(string.Format("Using savelot stats for p [{0}]: {1} ...", (object)index,
    //                     (object)slugcat));
    //             }
    //
    //             self.characterStatsJollyplayer[state2.playerNumber] = new SlugcatStats(slugcat, m);
    //             self.characterStatsJollyplayer[state2.playerNumber].foodToHibernate = slugcatStats.foodToHibernate;
    //             self.characterStatsJollyplayer[state2.playerNumber].maxFood = slugcatStats.maxFood;
    //             self.characterStatsJollyplayer[state2.playerNumber].bodyWeightFac = slugcatStats.bodyWeightFac;
    //         }
    //     }
    // }

    /*private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
    
        if (init) return;
    
        init = true;
    
        // Initialize assets, your mod config, and anything that uses RainWorld here
    }*/
    
    //Needed To fix Template Player being set incorrectly!!!!!
    private void addMoreJollyOptions(On.Options.orig_ctor orig, Options self, RainWorld rainWorld) {
        orig(self, rainWorld);
        
        foreach (Rewired.Player player in ReInput.players.GetPlayers(false)) {
            if (player.id == 8) {
                Options.templatePlayer = player;
                break;
            }
        }
    
        // int totalNumberOfPlayers = 8;
        //
        // JollyPlayerOptions[] newOptions = new JollyPlayerOptions[totalNumberOfPlayers];
        //
        // self.jollyPlayerOptionsArray.CopyTo(newOptions, 0);
        //
        // for (int i = 4; i < totalNumberOfPlayers; i++) {
        //     newOptions[i] = new JollyPlayerOptions(i);
        //     newOptions[i].joined = false;
        // }
        //
        // self.jollyPlayerOptionsArray = newOptions;
        //
        // Logger.LogMessage("Test");
        //
        //
        // //------
        //
        // BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        //
        // //--------
        //
        // Options.ControlSetup[] newControlSetups = new Options.ControlSetup[totalNumberOfPlayers];
        //
        // self.controls.CopyTo(newControlSetups, 0);
        //
        // for (int i = 4; i < totalNumberOfPlayers; ++i)
        //     newControlSetups[i] = new Options.ControlSetup(i, i == 0);
        //
        // self.controls = newControlSetups;
        //
        // //------
        // int index = self.JollyPlayerCount;
        //
        // string str1 = index.ToString();
        //
        // index = self.jollyPlayerOptionsArray.Count(x => x.joined);
        //
        // string str2 = index.ToString();
        //
        // Action<string> logFunc = (str) => System.IO.File.AppendAllText("consoleLog.txt", str + Environment.NewLine);
        //
        // logFunc.Invoke("[CUSTOM_OUT]: Number of jolly players: " + str1 + " accesing directly: " + str2);
        //
        // JollyPlayerOptions[] playerOptionsArray = self.jollyPlayerOptionsArray;
        //
        // for (index = 0; index < playerOptionsArray.Length; ++index)
        // {
        //     logFunc.Invoke("[CUSTOM_OUT]: " + playerOptionsArray[index].ToString());
        // }
    }
    
    // !!!!!!!!!!!!!!!! --- NOT NEEDED ANYMORE but may be good for compat???? --- !!!!!!!!!!!!!!!!!!!!!!!!!
    /*public static SlugcatStats.Name JollyPlayer5;
    public static SlugcatStats.Name JollyPlayer6;
    public static SlugcatStats.Name JollyPlayer7;
    public static SlugcatStats.Name JollyPlayer8;

    private void extendJollyEnumData(On.JollyCoop.JollyEnums.orig_RegisterAllEnumExtensions orig)
    {
        orig();

        JollyPlayer5 = new SlugcatStats.Name("JollyPlayer5", true);
        JollyPlayer6 = new SlugcatStats.Name("JollyPlayer6", true);
        JollyPlayer7 = new SlugcatStats.Name("JollyPlayer7", true);
        JollyPlayer8 = new SlugcatStats.Name("JollyPlayer8", true);
    }


    private void removeExtendedJollyEnum(On.JollyCoop.JollyEnums.orig_UnregisterAllEnumExtensions orig)
    {
        orig();

        if (JollyPlayer5 != null)
        {
            JollyPlayer5.Unregister();
            JollyPlayer5 = null;
        }

        if (JollyPlayer6 != null)
        {
            JollyPlayer6.Unregister();
            JollyPlayer6 = null;
        }

        if (JollyPlayer7 != null)
        {
            JollyPlayer7.Unregister();
            JollyPlayer7 = null;
        }

        if (JollyPlayer8 == null) return;

        JollyPlayer8.Unregister();
        JollyPlayer8 = null;
    }
    
    public static bool hideExtraJollyEnums(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i) {
        return orig(i) || (ModManager.JollyCoop && (i == JollyPlayer5 || i == JollyPlayer6 || i == JollyPlayer7 || i == JollyPlayer8));
    }*/
    // !!!!!!!!!!!!!!!! --- NOT NEEDED ANYMORE but may be good for compat???? --- !!!!!!!!!!!!!!!!!!!!!!!!!

    // public void IL_adjustPlayerSelectGUI(ILContext il) {
    //     //-------
    //
    //     ILCursor cursor = new ILCursor(il);
    //
    //     if (cursor.TryGotoNext(instruction => { return instruction.MatchLdcI4(100); })) {
    //         cursor.Next.Operand = (sbyte)70;
    //     }
    //
    //     //---
    //
    //     if (cursor.TryGotoNext(instruction => { return instruction.MatchLdcR4(4); })) {
    //         cursor.Next.Operand = 8f;
    //     }
    //
    //     //---
    //
    //     if (cursor.TryGotoNext(instruction => { return instruction.MatchLdcR4(171); })) {
    //         cursor.Next.Operand = -50f;
    //     }
    //     
    //     //--
    //
    //     cursor = new ILCursor(il);
    //     
    //     int num1 = 100;
    //     float num2 = (float) ((1024.0 - (double) num1 * 4.0) / 5.0);
    //     Vector2 vector2 = new Vector2(171f + num2, 0.0f);
    //
    //     //This is slightly risky so maybe more checks on instructions preceding these like checking for the button String Labels
    //     while (cursor.TryGotoNext(instruction => instruction.MatchLdloc(3))) {
    //         logger.LogMessage(cursor.Next.ToString());
    //         cursor.Next.Operand = vector2;
    //     }
    //     //----
    //
    // }
    
    public void adjustPlayerSelectGUI(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_ctor orig, JollySlidingMenu self, JollySetupDialog menu, MenuObject owner, Vector2 pos) {
        orig(self, menu, owner, pos);

        int num1 = 70;
        float num2 = (float)((1024.0 - num1 * 8.0) / 5.0);
        Vector2 pos1 = new Vector2(-50 + num2, 0.0f) + new Vector2(0.0f, menu.manager.rainWorld.screenSize.y * 0.55f);

        for (int index = 0; index < 8; ++index) {
            JollyPlayerSelector playerSelector = self.playerSelector[index];
        
            playerSelector.pos.x -= playerSelector.pos.x - pos1.x;
            
            playerSelector.playerLabelSelector._pos.x -= playerSelector.playerLabelSelector._pos.x - pos1.x;
            
            pos1 += new Vector2(num2 + num1, 0.0f);
        }

        //---

        var slider = self.numberPlayersSlider;
        
        var config = menu.oi.config.Bind("_cosmetic", Custom.rainWorld.options.JollyPlayerCount, new ConfigAcceptableRange<int>(1, 8));

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

        slider.pos = self.playerSelector[0].pos + new Vector2((num1 / 2f) + 15, 130f);

        slider._size = new Vector2(Math.Max((int)(self.playerSelector[7].pos - self.playerSelector[0].pos).x, 30), 30f);
        slider.fixedSize = slider._size;
        
        slider.Initialize();
        
        //---
        
        //Rebind buttons is needed to fix navigating sortof... Needs to be better handled I think
        self.BindButtons();
    }

    public void accountForMoreThanFour(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_NumberPlayersChange orig, JollySlidingMenu self, UIconfig config, string value, string oldvalue) {
        orig(self, config, value, oldvalue);
        
        int result;

        var jollyPlayerOptions = self.Options.jollyPlayerOptionsArray;
        
        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
            if (result > 8 || result < 4) return;
            
            for (int index = 0; index < jollyPlayerOptions.Length; ++index) jollyPlayerOptions[index].joined = index <= result - 1;
        }
        
        self.UpdatePlayerSlideSelectable(result - 1);
    }
    
    //----

    public static void addMorePlayerOptions(On.Menu.InputOptionsMenu.orig_ctor orig, InputOptionsMenu self, ProcessManager manager) {
        orig(self, manager);

        Vector2 vector2_1 = new Vector2(0.0f, -30f);

        //---

        var newDeviceButton = new InputOptionsMenu.DeviceButton[10];

        self.deviceButtons.CopyTo(newDeviceButton, 0);
        
        self.deviceButtons = newDeviceButton;
        
        //---

        var inputTesterIndex = self.pages[0].subObjects.IndexOf(self.inputTesterHolder);
        
        var buttonOffset = 60.0;
        
        for (int index = 0; index < self.deviceButtons.Length; ++index) {
            InputOptionsMenu.DeviceButton deviceButton; 
            
            if(index > 5){
                string str = self.inputDevicedTexts[Math.Min(index, self.inputDevicedTexts.Length - 1)];
                
                if (index > 1) str = Regex.Replace(str, "<X>", (index - 1).ToString());
                
                if (index == 0 && (self.CurrLang != InGameTranslator.LanguageID.English)) str = InGameTranslator.EvenSplit(str, 1);
                
                deviceButton = new InputOptionsMenu.DeviceButton(self, self.pages[0], new Vector2(450f, (float) (620.0 - (double) index * buttonOffset)) + vector2_1, str, self.deviceButtons, index);

                self.deviceButtons[index] = deviceButton;
                
                self.pages[0].subObjects.Insert(inputTesterIndex, self.deviceButtons[index]);
            } else {
                deviceButton = self.deviceButtons[index];

                deviceButton.pos = new Vector2(450f, (float)(620.0 - (double)index * buttonOffset)) + vector2_1;
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

        var newPlayerButtons = new InputOptionsMenu.PlayerButton[8];
        
        self.playerButtons.CopyTo(newPlayerButtons, 0);

        self.playerButtons = newPlayerButtons;
        
        //----
        
        
        self.rememberPlayersSignedIn = new bool[self.playerButtons.Length];
        
        var perInputOffset = 76;//143.3333282470703;

        var initalYOffset = 672;

        var xOffset = 32;
        
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

    //----

    /*public static Color adjustPlayerColor(On.PlayerGraphics.orig_SlugcatColor orig, SlugcatStats.Name i) {
        orig(i);
        
        if (ModManager.CoopAvailable)
        {
            int playerNumber = 0;
            
            if (i == JollyEnums.Name.JollyPlayer2) playerNumber = 1;
            else if(i == JollyEnums.Name.JollyPlayer3) playerNumber = 2;
            else if(i == JollyEnums.Name.JollyPlayer4) playerNumber = 3;
            else if(i == JollyPlayer5) playerNumber = 4;
            else if(i == JollyPlayer6) playerNumber = 5;
            else if(i == JollyPlayer7) playerNumber = 6;
            else if(i == JollyPlayer8) playerNumber = 7;
    
            if (Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO && playerNumber > 0
                || Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM) {
                return PlayerGraphics.JollyColor(playerNumber, 0);
            }
                
            
            i = Custom.rainWorld.options.jollyPlayerOptionsArray[playerNumber].playerClass ?? i;
        }
        
        return PlayerGraphics.CustomColorsEnabled() ? PlayerGraphics.CustomColorSafety(0) : PlayerGraphics.DefaultSlugcatColor(i);
    }
    */

    #region Helper Methods

    private void ClearMemory() {
        //If you have any collections (lists, dictionaries, etc.)
        //Clear them here to prevent a memory leak
        //YourList.Clear();
    }

    #endregion
}
