﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using RWCustom;
using BepInEx;
using BepInEx.Logging;
using JollyCoop.JollyMenu;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using Myriad.hooks;
using Myriad.hooks.hud;
using Myriad.hooks.jollycoop;
using Myriad.hooks.menu;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Myriad;

[BepInPlugin("myriad", "Myriad of Slug Cats", "1.0.1")]
public class MyriadMod : BaseUnityPlugin {

    public static MPOptions options;
    
    public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("Myriad");
    public delegate int orig_ShortcutTime(Player self);

    //BindingFlags otherMethodFlags = BindingFlags.Instance | BindingFlags.NonPublic;
    //BindingFlags myMethodFlags = BindingFlags.Static | BindingFlags.Public;

    //public static int plyCnt = 8;
    public static int plyCnt = PreloadMaxPlayerSettings.getPlayerCap(); //SET IN THERE BECAUSE IT LOADS FIRST

    public static int PlyCnt() {
        return plyCnt;
    }

    public static int Reflect_GetPlayerCount() {
        int playerCount = 4;

        Type type = TryCatchUtils.wrapTryCatch(
            () => Type.GetType("Myriad.PreloadPatches.RewiredAdjustUserData, Myriad_PreloadPatcher"), 
            "Unable to [get] the Type [RewiredAdjustUserData] which means the player count will be default of 4",
            logger
            );
        
        if (type != null) {
            FieldInfo field = TryCatchUtils.wrapTryCatch(
                () => type.GetField("totalPlayerCount", BindingFlags.Public | BindingFlags.Static), 
                "Unable to [get] the Field [RewiredAdjustUserData::myCount] which means the player count will be default of 4",
                logger
                );

            if (field != null) {
                playerCount = (int) field.GetValue(null);
            } else {
                logger.LogError("Unable to [find] the Field [RewiredAdjustUserData::totalPlayerCount] which means the player count will be default of 4");
            }
        } else {
            logger.LogError("Unable to [find] the Type [RewiredAdjustUserData] which means the player count will be default of 4");
        }

        return playerCount;
    }
    
    public static bool rotundWorldEnabled = false;
    public static bool coopLeashEnabled = false;
    public static string incompatibleMod = "";
    public bool shownIncompatWarning = false;

    public void OnEnable() {
        try {
            
            On.RainWorld.OnModsInit += RainWorld_OnModsInit; //Compat Checks
            On.RainWorld.PostModsInit += RainWorld_PostModsInit; //Compat Checks
            
            RainWorldGameMixin.INSTANCE.init();
            OptionsMixin.INSTANCE.init(Logger);
            
            InputOptionsMenuMixin.INSTANCE.init();
            JollySlidingMenuMixin.INSTANCE.init();
            MenuIllustrationMixin.INSTANCE.init();
            PlayerJoinButtonMixin.INSTANCE.init();
            PlayerResultBoxMixin.INSTANCE.init();
            MultiplayerMenuMixin.INSTANCE.init(Logger);
            InputTesterHolderMixin.INTANCE.init();

            PlayerSpecificMultiplayerHudMixin.INSTANCE.init();
            
            SlugcatStatsMixin.INSTANCE.init();
            PlayerGraphicsMixin.INSTANCE.init();
            PlayerMixin.INSTANCE.init();
            ShelterDoorMixin.INSTANCE.init();

            //-----
            // Section of IL hooks to modify arrays to higher size and various values corresponding to there size
            
            //On.Menu.InputOptionsMenu.PlayerButton.ctor += PlayerButton_ctor; //INPUT MENU

            IL.Options.ctor += il => Replace4WithMore(il, true); 									//3
            IL.JollyCoop.JollyMenu.JollySlidingMenu.ctor += Replace4WithMore;   //3
            IL.StoryGameSession.CreateJollySlugStats += Replace4WithMore;       //1
            IL.PlayerGraphics.PopulateJollyColorArray += Replace4WithMore;      //1
            IL.RoomSpecificScript.SU_C04StartUp.ctor += Replace4WithMore;       //2
            IL.ArenaSetup.ctor += Replace4WithMore;                             //2
            IL.Menu.MultiplayerMenu.InitiateGameTypeSpecificButtons += il => Replace4WithMore(il, true); //SHOULD GRAB 2
            IL.Menu.MultiplayerMenu.Update += il => Replace4WithMore(il, false, 2);
            IL.Options.ControlSetup.SaveAllControllerUserdata += Replace4WithMore;  //1
            IL.RainWorld.JoystickConnected += Replace4WithMore; //5-19
            IL.RainWorld.JoystickPreDisconnect += Replace4WithMore;
            //IL.RWInput.PlayerUIInput += Replace4WithMore;
            IL.RWInput.PlayerUIInput_int += Replace4WithMore;
            //SOME MORE TO ADD!
            IL.ScavengersWorldAI.Outpost.ctor += Replace4WithMore;
            IL.ArenaGameSession.ctor += Replace4WithMore;
            //IL.World.LoadMapConfig += Replace4WithMore; //PERHAPS? BUT LEAVE OUT UNLESS IT'S DISCOVERED THAT WE NEED IT
            IL.CreatureCommunities.ctor += Replace4WithMore;
            IL.StoryGameSession.ctor += il => Replace4WithMore(il, false, 1);
            IL.ArenaSetup.ctor += Replace4WithMore;

            //-----

            RainWorld.PlayerObjectBodyColors = new Color[plyCnt];
            
            On.JollyCoop.JollyMenu.JollyPlayerSelector.Update += JollyPlayerSelector_Update;
            //LOCK DOWNPOUR STORY CHARACTER SELECTION FOR PLAYER 1
            On.JollyCoop.JollyCustom.ForceActivateWithMSC += JollyCustom_ForceActivateWithMSC; //FOR JOLLYCAMPAINGN. JUST RETURN TRUE INSTEAD OF ORIG.
            
            On.Menu.SandboxSettingsInterface.AddPositionedScoreButton += SandboxSettingsInterface_AddPositionedScoreButton;
            On.ArenaGameSession.SpawnPlayers += ArenaGameSession_SpawnPlayers;
            On.ArenaBehaviors.ExitManager.ExitOccupied += ExitManager_ExitOccupied;

            On.Menu.MainMenu.ctor += MainMenu_ctor;
            On.JollyCoop.JollyHUD.JollyPlayerSpecificHud.JollyPlayerArrow.Draw += JollyPlayerArrow_Draw;

            //-----

            On.ShortcutHandler.ShortCutVessel.ctor += ShortCutVessel_ctor;

            On.Menu.SandboxEditorSelector.ctor += (orig, self, menu, owner, overlayOwner) => {
                if (plyCnt > 16) {
                    //Better solution is required to add more support of 16 and this is kinda just a hack... that sort dosn't work
                    SandboxEditorSelector.Width = 64;
                }

                orig(self, menu, owner, overlayOwner);
            };
            
            logger.LogMessage("Checking Patch");
        } catch (Exception e) {
            logger.LogMessage("ManySlugCats failed to load due to an exception being thrown!");
            logger.LogMessage(e.ToString());
            throw e;
        }

        RainWorld.PlayerObjectBodyColors = new Color[PlyCnt()];
    }

    private void JollyPlayerArrow_Draw(On.JollyCoop.JollyHUD.JollyPlayerSpecificHud.JollyPlayerArrow.orig_Draw orig, JollyCoop.JollyHUD.JollyPlayerSpecificHud.JollyPlayerArrow self, float timeStacker) {
        orig(self, timeStacker);
        
        //HIDE STACKS OF PLAYER NAMES WAITING IN PIPES SO IT'S EASIER TO TELL WHEN SOMEONE IS NOT ALL THE WAY IN
        if (options.displayNametags.Value && self.jollyHud?.RealizedPlayer != null && !self.jollyHud.RealizedPlayer.inShortcut && !self.hide) {
            self.label.alpha = 1;
        }
    }

    private void MainMenu_ctor(On.Menu.MainMenu.orig_ctor orig, MainMenu self, ProcessManager manager, bool showRegionSpecificBkg) {

        orig(self, manager, showRegionSpecificBkg);

        if (incompatibleMod != "" && !shownIncompatWarning) {
            self.popupAlert = new DialogBoxNotify(self, self.pages[0], "Myriad " + self.Translate("incompatible mod detected") + ": \n" + incompatibleMod, "ALERT", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 284f), new Vector2(480f, 180f), false);
            self.pages[0].subObjects.Add(self.popupAlert);
            shownIncompatWarning = true;
            return;
        }
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self) {
        orig(self);

        options ??= new MPOptions(logger);
        
        MachineConnector.SetRegisteredOI("myriad", options);
 
        foreach (ModManager.Mod activeMod in ModManager.ActiveMods){
            if (activeMod.id == "willowwisp.bellyplus") rotundWorldEnabled = true;
            if (activeMod.id == "WillowWisp.CoopLeash") coopLeashEnabled = true;
            //INCOMPATIBLE MOD WARNING
            if (activeMod.id == "bettergrab") incompatibleMod = activeMod.id;
            if (activeMod.id == "pkuyo.customfood") incompatibleMod = activeMod.id;
            if (activeMod.id == "IndividualKarma") incompatibleMod = activeMod.id;
        }
    }
    
    private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self) {
        orig(self);

        //Required to make sure that the main config takes priorty to the actual value used by the PreloadMaxPlayerSettings
        options.validateAndSetup();
    }
    
    private void ShortCutVessel_ctor(On.ShortcutHandler.ShortCutVessel.orig_ctor orig, ShortcutHandler.ShortCutVessel self, IntVector2 pos, Creature creature, AbstractRoom room, int wait) {
        //DISABLING IF CO-OP LEASH IS ENABLED, SINCE THE CHANGE IS JARRING
        if (options.longPipeWait.Value && !coopLeashEnabled && creature is Player && wait > 0) {
            wait *= 1000;
        }
        
        orig(self, pos, creature, room, wait);
    }
    
    private bool ExitManager_ExitOccupied(On.ArenaBehaviors.ExitManager.orig_ExitOccupied orig, ArenaBehaviors.ExitManager self, int exit) {
        bool result = orig(self, exit);
        
        int denSize = Mathf.CeilToInt((self.gameSession.Players.Count + 1) / 4f);
        int inDen = 0;

        for (int i = 0; i < self.playersInDens.Count; i++) {
            if (self.playersInDens[i].entranceNode == exit) inDen++;
        }
        
        return inDen >= denSize;
    }
    
    //----

    private void ArenaGameSession_SpawnPlayers(On.ArenaGameSession.orig_SpawnPlayers orig, ArenaGameSession self, Room room, List<int> suggestedDens) {
        if (!(ModManager.MSC && self.GameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)) {
            //SO IT SKIPS CHALLENGES.
            //BUT WHAT IF IT DIDNT...

            //WILL THIS INFINITE LOOP?
            if (suggestedDens != null) {
                int initialCount = suggestedDens.Count;
                
                for (int i = 0; i < initialCount; i++){
                    Logger.LogInfo("DEN " + i);
                    
                    suggestedDens.Add(suggestedDens[i]);
                }
            }
        }

        orig(self, room, suggestedDens);
    }

    private void JollyPlayerSelector_Update(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_Update orig, JollyPlayerSelector self){
        orig(self);
        
        //I LIKE THIS ONE :)
        //AAAND UNLOCK P1 EXPEDITION MODE BECAUSE WHY NOT
        if (self.index == 0 && ModManager.Expedition && self.menu.manager.rainWorld.ExpeditionMode) {
            self.classButton.GetButtonBehavior.greyedOut = false;
        }
    }
    
    private void SandboxSettingsInterface_AddPositionedScoreButton(On.Menu.SandboxSettingsInterface.orig_AddPositionedScoreButton orig, SandboxSettingsInterface self, SandboxSettingsInterface.ScoreController button, ref IntVector2 ps, Vector2 additionalOffset) {
        additionalOffset.x -= 180;
        
        orig(self, button, ref ps, additionalOffset);
    }
    
    private bool JollyCustom_ForceActivateWithMSC(On.JollyCoop.JollyCustom.orig_ForceActivateWithMSC orig) {
        return options.downpourCoop.Value || orig();
    }
    
    //---

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
            
            if(maxReplace == x) break;
        }

        if (x == 0) {
            Logger.LogWarning($"A method had NONE adjustments made to account for increased player count: [Adjustments #: {x}, Method: {il.Method.Name}]");
        } else {
            Logger.LogInfo($"A method had adjustments made to account for increased player count: [Adjustments #: {x}, Method: {il.Method.Name}]");
        }
    }
}
