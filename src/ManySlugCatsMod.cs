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
using JollyCoop;
using JollyCoop.JollyMenu;
using ManySlugCats.PreloadPatches;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using Rewired;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ManySlugCats;

[BepInPlugin("blodhgarm.manyslugcats", "Many Slug Cats", "1.0.0")]
public class ManySlugCatsMod : BaseUnityPlugin {
    
    public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("ManySlugCats");

    public static Func<Options> getOptions = null;
    
    bool init;

    BindingFlags otherMethodFlags = BindingFlags.Instance | BindingFlags.NonPublic;
    BindingFlags myMethodFlags = BindingFlags.Static | BindingFlags.Public;
    
    public void OnEnable() {
        new MorePlayers().OnEnable();
        
        //controlMapperHook.Apply();

        // ControlMap;

        On.Menu.InputOptionsMenu.ctor += addMorePlayerOptions;
        //On.Menu.InputOptionsMenu.PlayerButton.ctor += adjustPlayerOptionSize;

            // Add hooks here
        // On.RainWorld.OnModsInit += OnModsInit;
        // On.Options.ctor += addMoreJollyOptions;
        //
        // On.JollyCoop.JollyEnums.RegisterAllEnumExtensions += extendJollyEnumData;
        // On.JollyCoop.JollyEnums.UnregisterAllEnumExtensions += removeExtendedJollyEnum;
        // On.SlugcatStats.HiddenOrUnplayableSlugcat += hideExtraJollyEnums;
        
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
        
        logger.LogMessage("Checking Patch");

        //Rewired.Logger.Log();
        
        var loggerType = Type.GetType("Rewired.Logger, Rewired_Core");

        if (loggerType != null) {
            // foreach (var methodInfo in loggerType.GetMethods())
            // {
            //     logger.LogMessage($"Methods:{methodInfo}");
            // }
            
            if (ManySlugCatsPatches.playersHaveBeenInjected) {
                logger.LogMessage("IT HAS WORKED BUT NOT");
            } else {
                logger.LogMessage("Well Time to be sad ):");
            }
        } else {
            logger.LogMessage("WHY DOSE MY LIFE HATE ME");
        }
        
        //RainWorld.PlayerObjectBodyColors = new Color[8];
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
    private void adjustPlayerRecordArray(On.StoryGameSession.orig_ctor orig, StoryGameSession self, 
        SlugcatStats.Name saveStateNumber, RainWorldGame game)
    {
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

    // private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    // {
    //     orig(self);
    //
    //     if (init) return;
    //
    //     init = true;
    //
    //     // Initialize assets, your mod config, and anything that uses RainWorld here
    // }
    //
    // private void addMoreJollyOptions(On.Options.orig_ctor orig, Options self, RainWorld rainWorld)
    // {
    //     //TODO: MAYBE A GOOD IDEA TO THROW OUT ALREADY EXISTING OPTIONS FILES DUE TO FUN STUFF
    //     orig(self, rainWorld);
    //     
    //     foreach (Rewired.Player player in (IEnumerable<Rewired.Player>) ReInput.players.GetPlayers(false))
    //     {
    //         if (player.id == 8)
    //         {
    //             Options.templatePlayer = player;
    //             break;
    //         }
    //     }
    //
    //     int totalNumberOfPlayers = 8;
    //
    //     JollyPlayerOptions[] newOptions = new JollyPlayerOptions[totalNumberOfPlayers];
    //
    //     self.jollyPlayerOptionsArray.CopyTo(newOptions, 0);
    //
    //     for (int i = 4; i < totalNumberOfPlayers; i++) {
    //         newOptions[i] = new JollyPlayerOptions(i);
    //         newOptions[i].joined = false;
    //     }
    //
    //     self.jollyPlayerOptionsArray = newOptions;
    //
    //     Logger.LogMessage("Test");
    //     
    //
    //     //------
    //
    //     BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    //     
    //     //--------
    //
    //     Options.ControlSetup[] newControlSetups = new Options.ControlSetup[totalNumberOfPlayers];
    //     
    //     self.controls.CopyTo(newControlSetups, 0);
    //     
    //     for (int i = 4; i < totalNumberOfPlayers; ++i)
    //         newControlSetups[i] = new Options.ControlSetup(i, i == 0);
    //     
    //     self.controls = newControlSetups;
    //
    //     //------
    //     int index = self.JollyPlayerCount;
    //
    //     string str1 = index.ToString();
    //
    //     index = self.jollyPlayerOptionsArray.Count(x => x.joined);
    //
    //     string str2 = index.ToString();
    //
    //     Action<string> logFunc = (str) => System.IO.File.AppendAllText("consoleLog.txt", str + Environment.NewLine);
    //
    //     logFunc.Invoke("[CUSTOM_OUT]: Number of jolly players: " + str1 + " accesing directly: " + str2);
    //
    //     JollyPlayerOptions[] playerOptionsArray = self.jollyPlayerOptionsArray;
    //
    //     for (index = 0; index < playerOptionsArray.Length; ++index)
    //     {
    //         logFunc.Invoke("[CUSTOM_OUT]: " + playerOptionsArray[index].ToString());
    //     }
    // }

    
    // !!!!!!!!!!!!!!!! --- NOT NEEDED ANYMORE --- !!!!!!!!!!!!!!!!!!!!!!!!!
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
    }*/
    // !!!!!!!!!!!!!!!! --- NOT NEEDED ANYMORE --- !!!!!!!!!!!!!!!!!!!!!!!!!
    
    // public static bool hideExtraJollyEnums(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i) {
    //     return orig(i) || (ModManager.JollyCoop && (i == JollyPlayer5 || i == JollyPlayer6 || i == JollyPlayer7 || i == JollyPlayer8));
    // }

    public void adjustPlayerSelectGUI(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_ctor orig, JollySlidingMenu self, JollySetupDialog menu, MenuObject owner, Vector2 pos) {
        
        orig(self, menu, owner, pos);

        //   self.menu = menu;
        // menu.tabWrapper = new MenuTabWrapper((Menu.Menu) menu, (MenuObject) self);
        // self.subObjects.Add((MenuObject) menu.tabWrapper);
        // self.AddJollyTitle();
        
        int num1 = 70; //100
        float num2 = (float)((/*1024.0*/ 1024.0 - (double)num1 * 8.0) / 5.0);
        Vector2 vector2 = new Vector2(/*171f*/ -50 + num2, 0.0f);
        Vector2 pos1 = vector2 + new Vector2(0.0f, menu.manager.rainWorld.screenSize.y * 0.55f);

        // JollyPlayerSelector[] newSelector = new JollyPlayerSelector[8];
        //
        // self.playerSelector.CopyTo(newSelector, 0);
        //
        // self.playerSelector = newSelector;
        
        logger.LogMessage($"Array Data: {self.playerSelector.ToList()}");
        
        for (int index = 0; index < 8; ++index) {
            JollyPlayerSelector playerSelector;
            
            // if (index < 4) {
            playerSelector = self.playerSelector[index];
            
            playerSelector.pos.x -= playerSelector.pos.x - pos1.x;
            
            playerSelector.playerLabelSelector._pos.x -= playerSelector.playerLabelSelector._pos.x - pos1.x;
            
            //playerSelector.dirty = true;
            // }
            // else {
                // playerSelector = new JollyPlayerSelector(menu, (MenuObject)self, pos1, index);
                // self.playerSelector[index] = playerSelector;
                // self.subObjects.Add(playerSelector);
            // }

            pos1 += new Vector2(num2 + (float)num1, 0.0f);
        }
        
        foreach (var jollyPlayerSelector in self.playerSelector)
        {
            logger.LogMessage($"Data Test: {jollyPlayerSelector.ToString()}");
        }
        
        logger.LogMessage($"Array Data: {self.playerSelector.Length}");

        logger.LogMessage("DEEEEZ");
        
        menu.tabWrapper.wrappers.Remove(self.numberPlayersSlider);
        menu.tabWrapper.subObjects.Remove(self.sliderWrapper);
        
        menu.tabWrapper._tab.RemoveItems(new UIelement[1]
        {
            self.numberPlayersSlider
        });

        logger.LogMessage("WEEEEEE");
        
        logger.LogMessage(Custom.rainWorld.options.JollyPlayerCount);
        
        logger.LogMessage("WAAAAAAAAAAAAAAAAA");

        int offset = 15;
        
        self.numberPlayersSlider = new OpSliderTick(
            menu.oi.config.Bind<int>("_cosmetic", Custom.rainWorld.options.JollyPlayerCount, new ConfigAcceptableRange<int>(1, 8)),
            self.playerSelector[0].pos + new Vector2(((float)num1 / 2f) + offset, 130f),
            ((int)(self.playerSelector[7].pos - self.playerSelector[0].pos).x), 
            false
            );
        
        self.numberPlayersSlider.description = menu.Translate("Adjust the number of players");
        
        self.sliderWrapper = new UIelementWrapper(menu.tabWrapper, (UIelement)self.numberPlayersSlider);

        OnValueChangeHandler method = self.NumberPlayersChange;

        //self.numberPlayersSlider.OnValueUpdate += method;

         var eventInfo = typeof(UIconfig).GetEvent("OnValueUpdate");
         var methodInfo = method.Method.GetBaseDefinition();
        
         Delegate handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, self, methodInfo);
        
         eventInfo.AddEventHandler(self.numberPlayersSlider, handler);


        // self.subObjects.Add((MenuObject) new MenuLabel((Menu.Menu) menu, (MenuObject) self, menu.Translate("Adjust the number of players"), new Vector2(623f, self.numberPlayersSlider.PosY + 25f), new Vector2(120f, 30f), false)
        // {
        //   label = {
        //     alignment = FLabelAlignment.Center
        //   }
        // });
        FTextParams textParams1 = new FTextParams();
        FTextParams textParams2 = new FTextParams();
        if (InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang))
        {
            textParams1.lineHeightOffset = -15f;
            textParams2.lineHeightOffset = -10f;
        }

        /*bool textAboveButton = false;
        if ((ExtEnum<InGameTranslator.LanguageID>)menu.CurrLang ==
            (ExtEnum<InGameTranslator.LanguageID>)InGameTranslator.LanguageID.French ||
            (ExtEnum<InGameTranslator.LanguageID>)menu.CurrLang ==
            (ExtEnum<InGameTranslator.LanguageID>)InGameTranslator.LanguageID.Russian)
            textAboveButton = true;
        Vector2 pos2 = new Vector2((float)((double)self.playerSelector[1].pos.x + 100.0 - 70.0), 160f);

        self.subObjects.Remove(self.friendlyToggle);
        self.friendlyToggle = self.AddSymbolToggleButton(Custom.rainWorld.options.friendlyFire, "friendly_fire", menu.Translate("description_spears_on"), menu.Translate("description_spears_off"), pos2, new Vector2(70f, 70f), "fire", Custom.ReplaceLineDelimeters(menu.Translate("Spears hit")), Custom.ReplaceLineDelimeters(menu.Translate("Spears miss")), textAboveButton, textParams2);
        
        self.subObjects.Remove(self.hudToggle);
        self.hudToggle = self.AddSymbolToggleButton(Custom.rainWorld.options.jollyHud, "hud", menu.Translate("description_hud_on"), menu.Translate("description_hud_off"), pos2 - new Vector2(130f, 0.0f), new Vector2(70f, 70f), "hud", Custom.ReplaceLineDelimeters(menu.Translate("HUD on")), Custom.ReplaceLineDelimeters(menu.Translate("HUD off")), textAboveButton, textParams2);
        
        self.subObjects.Remove(self.cameraCyclesToggle);
        self.cameraCyclesToggle = self.AddSymbolToggleButton(Custom.rainWorld.options.cameraCycling, "camera_cycle", menu.Translate("description_camera_toggle_off"), menu.Translate("description_camera_toggle_on"), pos2 - new Vector2(0.0f, 100f), new Vector2(70f, 70f), "cyclecamera", Custom.ReplaceLineDelimeters(menu.Translate("Camera cycles")), Custom.ReplaceLineDelimeters(menu.Translate("Camera doesn't cycle")), false, textParams2);
        
        self.subObjects.Remove(self.smartShortcutToggle);
        self.smartShortcutToggle = self.AddSymbolToggleButton(Custom.rainWorld.options.smartShortcuts, "smartpipe", menu.Translate("description_smart_shorcuts_off"), menu.Translate("description_smart_shorcuts_on"), pos2 - new Vector2(130f, 100f), new Vector2(70f, 70f), "smartpipe", Custom.ReplaceLineDelimeters(menu.Translate("Smart shortcuts")), Custom.ReplaceLineDelimeters(menu.Translate("Vanilla shortcuts")), false, textParams1);
        
        self.subObjects.Remove(self.friendlyLizardsToggle);
        self.friendlyLizardsToggle = self.AddSymbolToggleButton(Custom.rainWorld.options.friendlyLizards, "friendlylizard", menu.Translate("description_friendlylizards_off"), menu.Translate("description_friendlylizards_on"), pos2 - new Vector2(260f, 0.0f), new Vector2(70f, 70f), "friendlylizard", Custom.ReplaceLineDelimeters(menu.Translate("Friendly lizards")), Custom.ReplaceLineDelimeters(menu.Translate("Vanilla lizards")), textAboveButton, textParams2);
        
        self.subObjects.Remove(self.friendlySteal);
        self.friendlySteal = self.AddSymbolToggleButton(Custom.rainWorld.options.friendlySteal, "friendlysteal", menu.Translate("description_friendlystealing_off"), menu.Translate("description_friendlystealing_on"), pos2 - new Vector2(260f, 100f), new Vector2(70f, 70f), "friendlystealing", Custom.ReplaceLineDelimeters(menu.Translate("Friendly stealing")), Custom.ReplaceLineDelimeters(menu.Translate("No stealing")), false, textParams1);
        */
        
        self.subObjects.Remove((MenuObject)self.controlsButton);
        self.controlsButton = new ControlsButton((Menu.Menu)menu, (MenuObject)self,
            new Vector2(200f, (float)((double)Custom.rainWorld.screenSize.y - 100.0 - 40.0)),
            menu.Translate("Input Settings"));
        self.subObjects.Add((MenuObject)self.controlsButton);

        self.subObjects.Remove((MenuObject)self.manualButton);
        self.manualButton = new SimpleButton((Menu.Menu)menu, (MenuObject)self,
            menu.Translate("JOLLY_MANUAL_BUTTON").ToUpperInvariant(), "JOLLY_MANUAL",
            menu.cancelButton.pos - new Vector2(0.0f, -45f), new Vector2(110f, 30f));
        self.subObjects.Add((MenuObject)self.manualButton);

        //self.GetType().GetMethod("BindButtons").Invoke(self, new object[] { });

        self.BindButtons();
    }

    public void accountForMoreThanFour(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_NumberPlayersChange orig, JollySlidingMenu self, UIconfig config, string value, string oldvalue) {
        orig(self, config, value, oldvalue);
        
        int result;
        
        if (int.TryParse(value, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result))
        {
            if (result > 8 || result < 4) return;
            
            for (int index = 0; index < self.Options.jollyPlayerOptionsArray.Length; ++index)
                self.Options.jollyPlayerOptionsArray[index].joined = index <= result - 1;
        }
        
        self.UpdatePlayerSlideSelectable(result - 1);
    }

    // public void preventOutOfBounds(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_UpdatePlayerSlideSelectable orig, JollySlidingMenu self, int pIndex) {
    //     if(pIndex > self.playerSelector.Length) pIndex = 3;
    //
    //     orig(self, pIndex);
    // }
    
    //----

    public static void addMorePlayerOptions(On.Menu.InputOptionsMenu.orig_ctor orig, InputOptionsMenu self, ProcessManager manager) {
        logger.LogMessage("1");
        
        orig(self, manager);

        Vector2 vector2_1 = new Vector2(0.0f, -30f);
        
        Vector2 vector2_2 = new Vector2(839f, 615f) + new Vector2(82f, 0.0f) + vector2_1;
        
        logger.LogMessage("2");
        
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
            
            foreach (var playerButtonSubObject in deviceButton.subObjects) {
                if (playerButtonSubObject is RectangularMenuObject rectMenuObject) {
                    rectMenuObject.size /= 2;
                    rectMenuObject.lastSize /= 2;
                }
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
                
                deviceButton.numberImage = new MenuIllustration(self, deviceButton, "", index == 0 ? "GamepadAny" : "Gamepad" + (index - 1), deviceButton.size / 2f, true, true);
                
                deviceButton.subObjects.Add(deviceButton.numberImage);
                
                deviceButton.numberImage.sprite.scale = 0.5f;
            }
        }
        
        //---

        logger.LogMessage("3");
        
        //---

        var newPlayerButtons = new InputOptionsMenu.PlayerButton[8];
        
        self.playerButtons.CopyTo(newPlayerButtons, 0);

        self.playerButtons = newPlayerButtons;
        
        //----

        logger.LogMessage("4");
        
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
            }
            else {
                playerButton = new InputOptionsMenu.PlayerButton(self, self.pages[0], new Vector2(200f + xOffset, (float) (initalYOffset - (double) index * perInputOffset)) + vector2_1, self.playerButtons, index);

                self.playerButtons[index] = playerButton;
                
                self.pages[0].subObjects.Add(self.playerButtons[index]);
                self.rememberPlayersSignedIn[index] = manager.rainWorld.IsPlayerActive(index);
            }
            
            foreach (var playerButtonSubObject in playerButton.subObjects) {
                if (playerButtonSubObject is RectangularMenuObject rectMenuObject) {
                    rectMenuObject.size /= 2;
                    rectMenuObject.lastSize /= 2;
                }
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
        
        logger.LogMessage("5");

        for (int playerNumber = 4; playerNumber < manager.rainWorld.options.controls.Length; ++playerNumber) {
            manager.rainWorld.RequestPlayerSignIn(playerNumber, null);
        }
        
        logger.LogMessage("6");
        
        for (int index = 4; index < self.playerButtons.Length; ++index)
        {
            self.playerButtons[index].pointPos = self.playerButtons[index].IdealPointHeight();
            self.playerButtons[index].lastPointPos = self.playerButtons[index].pointPos;
        }
        
        logger.LogMessage("7");
        
        //-----
        
        
        
        //-----

        foreach (var inputTester in self.inputTesterHolder.testers) self.inputTesterHolder.subObjects.Remove(inputTester);

        self.inputTesterHolder.testers = new InputTesterHolder.InputTester[(self.inputTesterHolder.menu as InputOptionsMenu).playerButtons.Length];
        for (int playerIndex = 0; playerIndex < self.inputTesterHolder.testers.Length; ++playerIndex)
        {
            self.inputTesterHolder.testers[playerIndex] = new InputTesterHolder.InputTester(self.inputTesterHolder.menu, self.inputTesterHolder, playerIndex);
            self.inputTesterHolder.subObjects.Add((MenuObject) self.inputTesterHolder.testers[playerIndex]);
        }
        
        //-----
        self.inputTesterHolder.Initiate();

        self.UpdateConnectedControllerLabels();
        
        for (int index = 0; index < self.gamePadButtonButtons.Length; ++index) self.gamePadButtonButtons[index].nextSelectable[2] = self.playerButtons[index < self.gamePadButtonButtons.Length / 2 ? 0 : 1];
    }

    //----

    // public void increasePlayerColorArray(On.PlayerGraphics.orig_PopulateJollyColorArray orig, SlugcatStats.Name reference) {
    //     PlayerGraphics.jollyColors = new Color?[8][];
    //     JollyCustom.Log("Initializing colors... reference " + reference?.ToString());
    //     for (int playerNumber = 0; playerNumber < PlayerGraphics.jollyColors.Length; ++playerNumber)
    //     {
    //         PlayerGraphics.jollyColors[playerNumber] = new Color?[3];
    //         if ((ExtEnum<Options.JollyColorMode>)Custom.rainWorld.options.jollyColorMode ==
    //             (ExtEnum<Options.JollyColorMode>)Options.JollyColorMode.CUSTOM)
    //             PlayerGraphics.LoadJollyColorsFromOptions(playerNumber);
    //         else if ((ExtEnum<Options.JollyColorMode>)Custom.rainWorld.options.jollyColorMode ==
    //                  (ExtEnum<Options.JollyColorMode>)Options.JollyColorMode.AUTO)
    //         {
    //             JollyCustom.Log("Need to generate colors for player " + playerNumber.ToString());
    //             if (playerNumber == 0)
    //             {
    //                 List<string> stringList = PlayerGraphics.DefaultBodyPartColorHex(reference);
    //                 PlayerGraphics.jollyColors[0][0] = new Color?(Color.white);
    //                 PlayerGraphics.jollyColors[0][1] = new Color?(Color.black);
    //                 PlayerGraphics.jollyColors[0][2] = new Color?(Color.green);
    //                 if (stringList.Count >= 1)
    //                     PlayerGraphics.jollyColors[0][0] = new Color?(Custom.hexToColor(stringList[0]));
    //                 if (stringList.Count >= 2)
    //                     PlayerGraphics.jollyColors[0][1] = new Color?(Custom.hexToColor(stringList[1]));
    //                 if (stringList.Count >= 3)
    //                     PlayerGraphics.jollyColors[0][2] = new Color?(Custom.hexToColor(stringList[2]));
    //             }
    //             else
    //             {
    //                 Color complementaryColor =
    //                     JollyCustom.GenerateComplementaryColor(PlayerGraphics.JollyColor(playerNumber - 1, 0));
    //                 PlayerGraphics.jollyColors[playerNumber][0] = new Color?(complementaryColor);
    //                 HSLColor hslColor1 =
    //                     JollyCustom.RGB2HSL(JollyCustom.GenerateClippedInverseColor(complementaryColor));
    //                 float num = hslColor1.lightness + 0.45f;
    //                 hslColor1.lightness *= num;
    //                 hslColor1.saturation *= num;
    //                 PlayerGraphics.jollyColors[playerNumber][1] = new Color?(hslColor1.rgb);
    //                 HSLColor hslColor2 = JollyCustom.RGB2HSL(JollyCustom.GenerateComplementaryColor(hslColor1.rgb));
    //                 hslColor2.saturation = Mathf.Lerp(hslColor2.saturation, 1f, 0.8f);
    //                 hslColor2.lightness = Mathf.Lerp(hslColor2.lightness, 1f, 0.8f);
    //                 PlayerGraphics.jollyColors[playerNumber][2] = new Color?(hslColor2.rgb);
    //                 JollyCustom.Log("Generating auto color for player " + playerNumber.ToString());
    //             }
    //         }
    //     }
    // }

    // public static Color adjustPlayerColor(On.PlayerGraphics.orig_SlugcatColor orig, SlugcatStats.Name i) {
    //     orig(i);
    //     
    //     if (ModManager.CoopAvailable)
    //     {
    //         int playerNumber = 0;
    //         
    //         if (i == JollyEnums.Name.JollyPlayer2) playerNumber = 1;
    //         else if(i == JollyEnums.Name.JollyPlayer3) playerNumber = 2;
    //         else if(i == JollyEnums.Name.JollyPlayer4) playerNumber = 3;
    //         else if(i == JollyPlayer5) playerNumber = 4;
    //         else if(i == JollyPlayer6) playerNumber = 5;
    //         else if(i == JollyPlayer7) playerNumber = 6;
    //         else if(i == JollyPlayer8) playerNumber = 7;
    //
    //         if (Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO && playerNumber > 0
    //             || Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM) {
    //             return PlayerGraphics.JollyColor(playerNumber, 0);
    //         }
    //             
    //         
    //         i = Custom.rainWorld.options.jollyPlayerOptionsArray[playerNumber].playerClass ?? i;
    //     }
    //     
    //     return PlayerGraphics.CustomColorsEnabled() ? PlayerGraphics.CustomColorSafety(0) : PlayerGraphics.DefaultSlugcatColor(i);
    // }

    #region Helper Methods

    private void ClearMemory() {
        //If you have any collections (lists, dictionaries, etc.)
        //Clear them here to prevent a memory leak
        //YourList.Clear();
    }

    #endregion
}
