using BepInEx.Logging;
using Kittehface.Framework20;
using Menu;
using Myriad.hooks.jollycoop;
using Myriad.utils;
using RWCustom;
using UnityEngine;
using Kittehface.Build;
using MoreSlugcats;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using System.Text.RegularExpressions;
using Rewired;
using Menu.Remix;

namespace Myriad.hooks.menu; 

[Mixin(typeof(MultiplayerMenu))]
public class MultiplayerMenuMixin {
    public static MultiplayerMenuMixin INSTANCE = new MultiplayerMenuMixin();

    private ManualLogSource Logger;
    public static bool[] arenaPlrsMemory; //AT SOME POINT MAYBE WE'LL WRITE AN ACTUAL IL HOOK BUT FOR NOW, THIS WILL DO

    public void init(ManualLogSource logger) {
        this.Logger = logger;
        //ADJUST MENU LAYOUT
        On.Menu.MultiplayerMenu.InitiateGameTypeSpecificButtons += MultiplayerMenu_InitiateGameTypeSpecificButtons;
        On.Menu.MultiplayerMenu.Update += MultiplayerMenu_Update;
        On.Menu.Menu.Update += Menu_Update; //I AM NOT ABOVE RESORTING TO MILITARY GRADE SHENANGIANS TO AVOID WRITING IL HOOKS
        //IL.Menu.MultiplayerMenu.Update += MultiplayerMenu_Update1;
    }

    //WE'LL COME BACK TO THIS... MAYBE..
    //private void MultiplayerMenu_Update1(MonoMod.Cil.ILContext il) {
    private void MultiplayerMenu_Update1(MonoMod.Cil.ILContext context) {

        //EMERALD'S SECOND ATTEMPT. STILL DIDN'T SEEM TO WORK? GAVE TABLE BOUNDS ERROR
        /*try {
            ILCursor c = new ILCursor(context);
            ILLabel label1;
            ILLabel label2;

            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<MainLoopProcess>(nameof(MainLoopProcess.manager)),
                x => x.MatchLdfld<ProcessManager>(nameof(ProcessManager.arenaSetup)),
                x => x.MatchLdfld<ArenaSetup>(nameof(ArenaSetup.playersJoined)),
                x => x.MatchLdloc(4),
                x => x.MatchLdloc(6)
            );
            label1 = c.DefineLabel();
            c.Index += 9;
            label2 = (ILLabel) c.Next.Operand;
            c.GotoLabel(label2);
            c.Index += 2;
            label2 = c.DefineLabel();
            c.Emit(OpCodes.Ldarg, 0);
            c.Emit(OpCodes.Ldloc, 4);
            c.EmitDelegate<Action<MainLoopProcess, int>>((self, i) =>
            {
                self.manager.arenaSetup.playersJoined[i] = true;
            });
            c.GotoLabel(label1);
            c.Emit(OpCodes.Br_S, label2);
        } catch (Exception ex) {
            Logger.LogInfo(ex);
        }
        */

        /*
        // EMERALDS FIRST ATTEMPT "JUST ADDS 'TRUE' TO THE END"
        try {
            ILCursor c = new ILCursor(context);
            ILLabel label;

            c.GotoNext(
                x => x.MatchLdloc(4),
                x => x.MatchLdloc(6)
            );
            c.Index += 5;
            label = (ILLabel) c.Next.Operand;
            c.GotoLabel(label);
            c.Index += 2;
            c.Emit(OpCodes.Ldarg, 0);
            c.Emit(OpCodes.Ldloc, 4);
            c.EmitDelegate<Action<MainLoopProcess, int>>((self, i) =>
            {
                self.manager.arenaSetup.playersJoined[i] = true;
            });
        } catch (Exception ex) {
            Logger.LogInfo(ex);
        }
        */


        /*var cursor = new ILCursor(il);
        var x = 0;

        if (!cursor.TryGotoNext(MoveType.After,
            //i => i.MatchLdarg(0),
            //i => i.MatchLdcI4(4)
            i => i.MatchStloc(4)
        )) {
            throw new Exception("Failed to match IL for MENU UPDATE!");
        }

        cursor.EmitDelegate((int oldNum) => {
            return 4;
        });


        while (cursor.TryGotoNext(MoveType.After,
            //i => i.MatchLdarg(0),
            //i => i.MatchLdcI4(4)
            i => i.MatchStloc(4)
        )) {
            x++;
            //cursor.Emit(OpCodes.Ldloc, player); //THESE LIKE, BECOME ARGUMENTS WITHIN EMITDELEGATE  I THINK?
            //cursor.Emit(OpCodes.Ldloc, k);

            //cursor.EmitDelegate((float rad, Player player, int k) =>
            cursor.EmitDelegate((int oldNum) => {
                return 4;
            });
            break;
        }

        Logger.LogInfo("TESTMYSLUGCAT IL LINES ADDED! " + x);
        */


        /*
        var cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(MoveType.After,
            i => i.Matchldloc(6),



            i => i.MatchLdarg(0),
            i => i.MatchCallOrCallvirt<PhysicalObject>("get_bodyChunks"),
            i => i.MatchLdcI4(0),
            i => i.MatchLdelemRef(),
            i => i.MatchLdflda<BodyChunk>(nameof(BodyChunk.vel)),
            i => i.MatchLdflda<Vector2>(nameof(Vector2.y)),
            i => i.MatchDup(),
            i => i.MatchLdindR4(),
            i => i.MatchLdarg(0),
            i => i.MatchCallOrCallvirt<PhysicalObject>("get_EffectiveRoomGravity"),
            i => i.MatchSub(),
            i => i.MatchStindR4())) {
            throw new Exception("Couldn't match in whatever hook this is");
        }

        var label = il.DefineLabel();
        cursor.MarkLabel(label);

        if (!cursor.TryGotoPrev(MoveType.Before,
            i => i.MatchLdarg(0),
            i => i.MatchLdsfld<Player.AnimationIndex>(nameof(Player.AnimationIndex.None)),
            i => i.MatchStfld<Player>(nameof(Player.animation)))) {
            throw new Exception("Couldn't match whatever bla bla bla error you can recognize later");
        } else
            BellyPlus.Logger.LogInfo("PB PLAYERMOVEMENT IL ADDED! ");

        cursor.Emit(OpCodes.Br, label);

        */

        /*
		we get a label to the end
		then move back to the start
		and emit a br
		which tells the code to skip to that label no matter what 
		(br stands for branch)
		this is the same as putting an if (false) { } around the code
		br is the same as a goto in C#
		*/
    }

    private void Menu_Update(On.Menu.Menu.orig_Update orig, Menu.Menu self) {
        if (self.manager?.arenaSetup?.playersJoined != null && self.manager?.arenaSetup?.playersJoined?.Length != MyriadMod.plyCnt && arenaPlrsMemory!= null)
            self.manager.arenaSetup.playersJoined = arenaPlrsMemory;
        orig(self);
    }

    bool btnHeld = false;
    private void MultiplayerMenu_Update(On.Menu.MultiplayerMenu.orig_Update orig, MultiplayerMenu self) {
        /*
        if (!self.requestingControllerConnections && !self.exiting) {
            for (int i = 1; i < self.manager.arenaSetup.playersJoined.Length; i++) {
                PlayerHandler playerHandler = self.manager.rainWorld.GetPlayerHandler(i);
                if (playerHandler != null) {
                    Rewired.Player rewiredPlayer = UserInput.GetRewiredPlayer(playerHandler.profile, i);
                    self.manager.arenaSetup.playersJoined[i] = true; // (rewiredPlayer.controllers.joystickCount > 0 || rewiredPlayer.controllers.hasKeyboard);
                } else {
                    self.manager.arenaSetup.playersJoined[i] = false;
                }
                self.manager.rainWorld.GetPlayerSigningIn(i);
            }
        }
        
        //TEMPORARILY ADJUST THE TABLE SIZE SO REWIRED DOESN'T TRY AND READ CONTROL SETTINGS 5+
        arenaPlrsMemory = self.manager.arenaSetup.playersJoined;
        self.manager.arenaSetup.playersJoined = new bool[4];
        for (int i = 0; i < self.manager.arenaSetup.playersJoined.Length; i++) {
            self.manager.arenaSetup.playersJoined[i] = arenaPlrsMemory[i];
        }
        orig(self);
        //self.manager.arenaSetup.playersJoined = arenaPlrsMemory; //WE NEED THIS BEFORE THEN! BUT WE'LL CATCH IT IN THE BASE.UPDATE...
        */

        if (!self.requestingControllerConnections && !self.exiting) {
            for (int i = 1; i < self.manager.arenaSetup.playersJoined.Length; i++) {
                PlayerHandler playerHandler = self.manager.rainWorld.GetPlayerHandler(i);
                if (playerHandler != null) {
                    //Rewired.Player rewiredPlayer = UserInput.GetRewiredPlayer(playerHandler.profile, i);
                    self.manager.arenaSetup.playersJoined[i] = true; // (rewiredPlayer.controllers.joystickCount > 0 || rewiredPlayer.controllers.hasKeyboard);
                } else {
                    self.manager.arenaSetup.playersJoined[i] = false;
                }
                self.manager.rainWorld.GetPlayerSigningIn(i);
            }
        }
        //base.Update(); NEED TO GO DEEPER
        if (!self.init) {
            self.Init();
            self.init = true;
        }
        if (self.manager.menuesMouseMode) {
            self.floatScrollWheel += Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.RoundToInt(self.floatScrollWheel * 15f) != self.lastScrollWheel) {
                self.mouseScrollWheelMovement = self.lastScrollWheel - Mathf.RoundToInt(self.floatScrollWheel * 10f);
                self.lastScrollWheel = Mathf.RoundToInt(self.floatScrollWheel * 10f);
            } else {
                self.mouseScrollWheelMovement = 0;
            }
        } else {
            self.floatScrollWheel = 0f;
            self.lastScrollWheel = 0;
            self.mouseScrollWheelMovement = 0;
        }
        //self.Update(); //GOING DEEPER

        // 
        self.allowSelectMove = true;
        if ((self.manager.rainWorld.setup.devToolsActive || ModManager.DevTools) && (self.manager.currentMainLoop is RainWorldGame || self.manager.currentMainLoop is SlideShow || self.manager.currentMainLoop is EndCredits || self.manager.currentMainLoop is SleepAndDeathScreen || self.manager.currentMainLoop is GhostEncounterScreen || self.manager.currentMainLoop is DreamScreen)) {
            if (Input.GetKey("s")) {
                self.framesPerSecond = 400;
            } else if (Input.GetKey("a")) {
                self.framesPerSecond = 10;
            } else {
                self.framesPerSecond = 40;
            }
        }
        self.lastMousePos = self.mousePosition;
        self.mousePosition = Futile.mousePosition;
        self.lastInput = self.input;
        self.input = RWInput.PlayerUIInput(-1);
        self.lastInfoLabelFade = self.infoLabelFade;
        if (self.selectedObject != null) {
            self.infoLabelFade = 1f;
        } else {
            self.infoLabelFade = Mathf.Max(0f, self.infoLabelFade - 1f / Mathf.Lerp(1f, 100f, self.infoLabelFade));
        }
        self.infoLabelSin += self.infoLabelFade;
        for (int i = 0; i < self.pages.Count; i++) {
            self.pages[i].Update();
        }
        self.lastHoldButton = self.holdButton;
        self.lastMouseDown = self.mouseDown;
        self.mouseDown = (Input.GetMouseButton(0) && self.Active);
        if (self.FreezeMenuFunctions) {
            if (self.mouseDown && !self.lastMouseDown) {
                self.manager.menuesMouseMode = true;
                self.modeSwitch = true;
                self.holdButton = true;
            }
            return;
        }
        if (self.manager.menuesMouseMode) {
            if (self.ForceNoMouseMode) {
                self.selectedObject = null;
            } else {
                MenuObject menuObject = self.selectedObject;
                self.selectedObject = null;
                int j = 0;
                while (j < self.pages[self.currentPage].selectables.Count) {
                    if (self.pages[self.currentPage].selectables[j].CurrentlySelectableMouse && self.pages[self.currentPage].selectables[j].IsMouseOverMe) {
                        self.selectedObject = (self.pages[self.currentPage].selectables[j] as MenuObject);
                        if (self.selectedObject == menuObject || self.modeSwitch || !(self.selectedObject is UIelementWrapper)) {
                            break;
                        }
                        if (!(self.selectedObject as UIelementWrapper).thisElement.mute) {
                            self.PlaySound((!(self.selectedObject as UIelementWrapper).GreyedOut) ? SoundID.MENU_Button_Select_Mouse : SoundID.MENU_Greyed_Out_Button_Select_Mouse);
                            break;
                        }
                        self.PlaySound((self.selectedObject is ButtonMenuObject && !(self.selectedObject as ButtonMenuObject).GetButtonBehavior.greyedOut) ? SoundID.MENU_Button_Select_Mouse : SoundID.MENU_Greyed_Out_Button_Select_Mouse);
                        break;
                    } else {
                        j++;
                    }
                }
                self.holdButton = self.mouseDown;
                bool anyButton = ReInput.controllers.Mouse.GetAnyButton();
                if ((self.input.x != 0 || self.input.y != 0 || self.input.jmp) && !anyButton) {
                    self.manager.menuesMouseMode = false;
                    self.modeSwitch = true;
                    self.holdButton = self.input.jmp;
                }
            }
        } else {
            self.holdButton = self.input.jmp;
            if (self.mouseDown && !self.lastMouseDown) {
                self.manager.menuesMouseMode = true;
                self.modeSwitch = true;
                self.holdButton = true;
            }
            if (self.input.y != 0 && self.lastInput.y != self.input.y) {
                self.SelectNewObject(new IntVector2(0, self.input.y));
            } else if (self.input.x != 0 && self.lastInput.x != self.input.x) {
                self.SelectNewObject(new IntVector2(self.input.x, 0));
            }
            if (self.input.y != 0 && self.lastInput.y == self.input.y && self.input.x == 0) {
                self.scrollInitDelay++;
            } else if (self.input.x != 0 && self.lastInput.x == self.input.x && self.input.y == 0) {
                self.scrollInitDelay++;
            } else {
                self.scrollInitDelay = 0;
            }
            if (self.scrollInitDelay > 20) {
                self.scrollDelay++;
                if (self.scrollDelay > 6) {
                    self.scrollDelay = 0;
                    if (self.input.y != 0 && self.lastInput.y == self.input.y) {
                        self.SelectNewObject(new IntVector2(0, self.input.y));
                    } else if (self.input.x != 0 && self.lastInput.x == self.input.x) {
                        self.SelectNewObject(new IntVector2(self.input.x, 0));
                    }
                }
            } else {
                self.scrollDelay = 0;
            }
            if (self.allowSelectMove && self.backObject != null && self.input.thrw && !self.lastInput.thrw && !self.input.jmp && self.selectedObject != self.backObject) {
                self.selectedObject = self.backObject;
                if (self.selectedObject is UIelementWrapper) {
                    if (!(self.selectedObject as UIelementWrapper).thisElement.mute) {
                        self.PlaySound((!(self.selectedObject as UIelementWrapper).GreyedOut) ? SoundID.MENU_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard);
                    } else {
                        self.PlaySound((self.selectedObject is ButtonMenuObject && !(self.selectedObject as ButtonMenuObject).GetButtonBehavior.greyedOut) ? SoundID.MENU_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard);
                    }
                }
            }
        }
        if (self.selectedObject != null) {
            if (self.infoLabel != null && (self.pages[self.currentPage].lastSelectedObject != self.selectedObject || self.infolabelDirty)) {
                self.infoLabel.text = self.UpdateInfoText();
                self.infolabelDirty = false;
                self.infoLabelSin = 0f;
            }
            self.pages[self.currentPage].lastSelectedObject = self.selectedObject;
            if (self.selectedObject is UIelementWrapper) {
                self.holdButton = (self.selectedObject as UIelementWrapper).tabWrapper.holdElement;
            }
        }
        if (self.modeSwitch) {
            if (!self.holdButton) {
                self.modeSwitch = false;
            }
            self.holdButton = false;
            self.pressButton = false;
            self.selectedObject = null;
        } else {
            self.pressButton = (self.holdButton && !self.lastHoldButton);
        }
        if (self.pressButton && self.selectedObject != null && self.selectedObject is ButtonTemplate && (self.selectedObject as ButtonTemplate).buttonBehav.greyedOut) {
            (self.selectedObject as ButtonTemplate).buttonBehav.extraSizeBump = 0f;
            self.PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
        }
        if (self.holdButton != self.lastHoldButton) {
            self.infolabelDirty = true;
        }
        if (self.soundLoop != null) {
            self.soundLoop.loopVolume = Custom.LerpAndTick(self.soundLoop.loopVolume, 1f, 0.02f, 0.05f);
        } else if (self.manager.menuMic != null) {
            bool flag7 = false;
            for (int k = 0; k < self.manager.menuMic.soundObjects.Count; k++) {
                if (self.manager.menuMic.soundObjects[k] is MenuMicrophone.MenuSoundLoop && (self.manager.menuMic.soundObjects[k] as MenuMicrophone.MenuSoundLoop).isBkgLoop && ((self.mySoundLoopID != SoundID.None && self.manager.menuMic.soundObjects[k].soundData.soundID == self.mySoundLoopID) || (self.mySoundLoopName != "" && self.manager.menuMic.soundObjects[k].soundData.soundName == self.mySoundLoopName))) {
                    self.soundLoop = (self.manager.menuMic.soundObjects[k] as MenuMicrophone.MenuSoundLoop);
                    flag7 = true;
                    break;
                }
            }
            if (!flag7 && self.mySoundLoopName != "") {
                self.soundLoop = self.PlayLoopCustom(self.mySoundLoopName, 0f, 1f, 1f, true);
            } else if (!flag7 && self.mySoundLoopID != SoundID.None) {
                self.soundLoop = self.PlayLoop(self.mySoundLoopID, 0f, 1f, 1f, true);
            }
        }
        if (!self.allAlienSoundLoopsAreGone && self.manager.menuMic != null) {
            self.allAlienSoundLoopsAreGone = true;
            for (int l = 0; l < self.manager.menuMic.soundObjects.Count; l++) {
                if (self.manager.menuMic.soundObjects[l] is MenuMicrophone.MenuSoundLoop && (self.manager.menuMic.soundObjects[l] as MenuMicrophone.MenuSoundLoop).isBkgLoop && (self.manager.menuMic.soundObjects[l].soundData.soundID != self.mySoundLoopID || self.manager.menuMic.soundObjects[l].soundData.soundName != self.mySoundLoopName)) {
                    (self.manager.menuMic.soundObjects[l] as MenuMicrophone.MenuSoundLoop).loopVolume = Mathf.Max(0f, (self.manager.menuMic.soundObjects[l] as MenuMicrophone.MenuSoundLoop).loopVolume - 0.025f);
                    if ((self.manager.menuMic.soundObjects[l] as MenuMicrophone.MenuSoundLoop).loopVolume <= 0f) {
                        (self.manager.menuMic.soundObjects[l] as MenuMicrophone.MenuSoundLoop).Destroy();
                    }
                    self.allAlienSoundLoopsAreGone = false;
                }
            }
        }
        //
        bool flag = RWInput.CheckPauseButton(0);
        if (flag && !self.lastPauseButton && self.manager.dialog == null) {
            self.OnExit();
        }
        self.lastPauseButton = flag;
        self.lastBlackFade = self.blackFade;
        float num = 0f;
        if (self.nextGameType != self.currentGameType) {
            num = 1f;
            if (self.blackFade == 1f && self.lastBlackFade == 1f) {
                self.ClearGameTypeSpecificButtons();
                self.currentGameType = self.nextGameType;
                self.InitiateGameTypeSpecificButtons();
            }
        }
        if (ModManager.MSC && self.currentGameType == MoreSlugcatsEnums.GameTypeID.Challenge && self.levelSelector != null && self.thumbsToBeLoaded.Count > 0) {
            self.levelSelector.Update();
        }
        if (self.blackFade < num) {
            self.blackFade = Custom.LerpAndTick(self.blackFade, num, 0.05f, 0.06666667f);
        } else {
            self.blackFade = Custom.LerpAndTick(self.blackFade, num, 0.05f, 0.125f);
        }
        bool flag2 = false;
        int num2 = 0;
        for (int j = 0; j < self.GetArenaSetup.playersJoined.Length; j++) {
            if (self.GetArenaSetup.playersJoined[j]) {
                num2++;
            }
        }
        if (self.currentGameType == ArenaSetup.GameTypeID.Sandbox) {
            self.abovePlayButtonLabel.text = ((num2 == 0) ? self.Translate("No players joined!") : "");
            flag2 = true;
        } else if (num2 == 0) {
            self.abovePlayButtonLabel.text = self.Translate("No players joined!");
        } else if (self.currentGameType == ArenaSetup.GameTypeID.Competitive) {
            if (self.levelSelector.levelsPlaylist != null && self.levelSelector.levelsPlaylist.mismatchCounter > 20) {
                self.abovePlayButtonLabel.text = self.Translate("ERROR");
            } else {
                int num3 = self.GetGameTypeSetup.playList.Count * self.GetGameTypeSetup.levelRepeats;
                if (num3 == 0) {
                    self.abovePlayButtonLabel.text = Regex.Replace(self.Translate("Select which levels to play<LINE>in the level selector"), "<LINE>", "\r\n");
                } else {
                    int num4 = self.ApproximatePlayTime();
                    string text;
                    if (num3 == 1) {
                        text = self.Translate("ROUND SESSION");
                    } else if (num3 >= 2 && num3 <= 4) {
                        text = self.Translate("ROUNDS SESSION-ru2");
                    } else {
                        text = self.Translate("ROUNDS SESSION");
                    }
                    if (text.Contains("#")) {
                        text = text.Replace("#", num3.ToString());
                    } else {
                        text = num3.ToString() + " " + text;
                    }
                    self.abovePlayButtonLabel.text = text + ((num4 > 0) ? string.Concat(new string[]
                    {
                        "\r\n",
                        self.Translate("Approximately"),
                        " ",
                        num4.ToString(),
                        " ",
                        (num4 == 1) ? self.Translate("minute") : self.Translate("minutes")
                    }) : "");
                    flag2 = true;
                }
            }
        }
        self.APBLLastSin = self.APBLSin;
        self.APBLLastPulse = self.APBLPulse;
        if (!ModManager.MSC || self.currentGameType == MoreSlugcatsEnums.GameTypeID.Challenge || self.currentGameType == MoreSlugcatsEnums.GameTypeID.Safari) {
            flag2 = true;
        }
        if (ModManager.MSC && self.currentGameType == MoreSlugcatsEnums.GameTypeID.Safari && !self.manager.rainWorld.progression.miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.SafariUnlockID(ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.GetEntry(self.GetGameTypeSetup.safariID), false)) && !MultiplayerUnlocks.CheckUnlockSafari()) {
            flag2 = false;
        }
        if (!flag2) {
            self.APBLSin += 1f;
            self.APBLPulse = Custom.LerpAndTick(self.APBLPulse, 1f, 0.04f, 0.025f);
            self.playButton.buttonBehav.greyedOut = true;
            return;
        }
        self.APBLPulse = Custom.LerpAndTick(self.APBLPulse, 0f, 0.04f, 0.025f);
        self.playButton.buttonBehav.greyedOut = false;
    }

    //OKAY WEIRD BUT WE A DEFINITELY DUPLICATING MENU OBJECTS WHEN SWITCHING BETWEEN ARENA MODES WHILE MSC IS DISABLED...
    private void MultiplayerMenu_InitiateGameTypeSpecificButtons(On.Menu.MultiplayerMenu.orig_InitiateGameTypeSpecificButtons orig, MultiplayerMenu self) {
        
        //TEMPORARY DETOUR UNTIL WE FIX THE COMPETITIVE MODE...
        /*
        if (self.currentGameType == ArenaSetup.GameTypeID.Competitive) {
            self.currentGameType = self.GetArenaSetup.CycleGameType(1);
            self.nextGameType = self.currentGameType;
        }

        if (self.nextGameType == ArenaSetup.GameTypeID.Competitive) {
            if (self.currentGameType == ArenaSetup.GameTypeID.Sandbox)
                self.nextGameType = self.GetArenaSetup.CycleGameType(-1);
            else
                self.nextGameType = self.GetArenaSetup.CycleGameType(1);
        }
        */
        orig(self);

        var plyCnt = MyriadMod.PlyCntDisplay();
        
        if (plyCnt <= 4) return;

        var playerJoinBtns = self.playerJoinButtons;
        
        if (playerJoinBtns != null) {
            //foreach (var playerJoinButton in playerJoinBtns) playerJoinButton.pos.x -= shift;
            var btnPos = playerJoinBtns[0].pos;

            var perBtnOffset = (Custom.rainWorld.options.ScreenSize.x - btnPos.x + 210) / plyCnt; // playerJoinBtns.Length;

            var startingX = btnPos.x - 210;
            
            for (int i = 0; i < playerJoinBtns.Length; i++) {
                //float shift = 235 + i * 10; //298 //NORMALLY 120
                //float shift = 235 + i * 4.1f * playerJoinBtns.Length * Mathf.Lerp((1366 / Custom.rainWorld.options.ScreenSize.x), 1f, 0.4f);
                float shift = i * perBtnOffset;//((i * (buttonDistance / plyCnt)) / mul); //i /** (plyCnt > 8 ? 4.1f : 1.2f)*/ /** playerJoinBtns.Length*/ * Mathf.Lerp((1366 / Custom.rainWorld.options.ScreenSize.x), 1f, 0.4f);

                var playerJoinBtn = playerJoinBtns[i];
                
                if (plyCnt > 8) {
                    //EXTRA SHIFT
                    shift -= 15;

                    //SHRINK THE BUTTONS!!
                    playerJoinBtn.size /= 2f;
                    playerJoinBtn.lastSize /= 2f;
                    playerJoinBtn.portrait.sprite.scale = 0.5f;
                    playerJoinBtn.portrait.pos -= playerJoinBtn.size / 2f;
                    
                    foreach (var playerButtonSubObject in playerJoinBtn.subObjects) {
                        if (!(playerButtonSubObject is RectangularMenuObject rectMenuObject)) return;
                        
                        rectMenuObject.size /= 2;
                        rectMenuObject.lastSize /= 2;
                        //rectMenuObject.pos += rectMenuObject.size;
                    }
                }

                playerJoinBtn.pos.x = startingX + shift;
                
                //Logger.LogWarning($"Shift:{shift}, X:{playerJoinBtn.pos.x}");
                
                if (self.playerClassButtons != null) {
                    var playerClassBtn = self.playerClassButtons[i];
                    //IF WE ARE USING SHRUNK ICONS, SHIFT EVERY OTHER CLASS BUTTON UP TOP 
                    
                    if (plyCnt > 8) {
                        float xMultiply = 0.85f;
                        float yMultiply = 0.85f;
                        
                        float xDiff = (playerClassBtn.size.x * xMultiply) - playerClassBtn.size.x;
                        float yDiff = (playerClassBtn.size.y * 0.85f) - playerClassBtn.size.y;
                        
                        playerClassBtn.size.x *= xMultiply;
                        playerClassBtn.size.y *= yMultiply;
                        
                        playerClassBtn.roundedRect.size.x *= xMultiply;
                        playerClassBtn.selectRect.size.x *= yMultiply;
                        
                        playerClassBtn.roundedRect.size.y *= xMultiply;
                        playerClassBtn.selectRect.size.y *= yMultiply;

                        playerClassBtn.menuLabel.pos.x += xDiff / 2;
                        playerClassBtn.menuLabel.pos.y += yDiff / 2;
                        
                        
                        if (i % 2 == 0) {
                            playerClassBtn.pos.y += playerJoinBtn.size.y * 2f;
                        }
                    }
                    
                    playerClassBtn.pos.x = (playerJoinBtn.pos.x + (playerJoinBtn.size.x / 2f)) - (playerClassBtn.size.x / 2f);  // /*+ (playerJoinBtn.size.x / 2f)*/;
                }
            }
        }
        
        if (self.levelSelector != null) {
            self.levelSelector.pos -= new Vector2(165, 0);
            self.levelSelector.lastPos = self.levelSelector.pos;
        }
    }
}