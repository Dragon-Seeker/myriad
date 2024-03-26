using BepInEx.Logging;
using Kittehface.Framework20;
using Menu;
using Myriad.hooks.jollycoop;
using Myriad.utils;
using RWCustom;
using UnityEngine;
using Kittehface.Build;

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
    }

    private void Menu_Update(On.Menu.Menu.orig_Update orig, Menu.Menu self) {
        if (self.manager?.arenaSetup?.playersJoined?.Length != MyriadMod.plyCnt && arenaPlrsMemory!= null)
            self.manager.arenaSetup.playersJoined = arenaPlrsMemory;
        orig(self);
    }

    bool btnHeld = false;
    private void MultiplayerMenu_Update(On.Menu.MultiplayerMenu.orig_Update orig, MultiplayerMenu self) {

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



        bool pressedBtn = false;
        //Profiles.Profile profile = self.manager.rainWorld.playerHandler.profile;
        Profiles.Profile profile = self.manager.rainWorld.GetPlayerHandler(0).profile; //PRETTY SURE WE WANT PLAYER 1
        //if (Input.GetKey((KeyCode) 324) || (profile != null && UserInput.GetButton(profile, "UICancel"))) { //"Take"
        if (Input.GetKey((KeyCode) 324) || (profile != null && UserInput.GetRewiredPlayer(profile, 0).GetButton(9))) { //9 = UICancel
            if (!btnHeld)
                pressedBtn = true;
            btnHeld = true;
        } else {
            btnHeld = false;
        }

        if (self.playerClassButtons != null) {
            for (int k = 0; k < self.playerClassButtons.Length; k++) {
                if (self.playerClassButtons[k].Selected && pressedBtn) {
                    self.GetArenaSetup.playerClass[k] = JollySlidingMenuMixin.PrevClass(self.GetArenaSetup.playerClass[k], "arena", self.manager.rainWorld);
                    self.GetArenaSetup.playerClass[k] = JollySlidingMenuMixin.PrevClass(self.GetArenaSetup.playerClass[k], "arena", self.manager.rainWorld);
                    self.Singal(self.playerClassButtons[k], "CLASSCHANGE" + k.ToString());
                }
            }
        }
    }

    //OKAY WEIRD BUT WE A DEFINITELY DUPLICATING MENU OBJECTS WHEN SWITCHING BETWEEN ARENA MODES WHILE MSC IS DISABLED...
    private void MultiplayerMenu_InitiateGameTypeSpecificButtons(On.Menu.MultiplayerMenu.orig_InitiateGameTypeSpecificButtons orig, MultiplayerMenu self) {
        orig(self);

        var plyCnt = MyriadMod.PlyCnt();
        
        if (plyCnt <= 4) return;

        var playerJoinBtns = self.playerJoinButtons;
        
        if (playerJoinBtns != null) {
            //foreach (var playerJoinButton in playerJoinBtns) playerJoinButton.pos.x -= shift;
            var btnPos = playerJoinBtns[0].pos;
            
            var perBtnOffset = (Custom.rainWorld.options.ScreenSize.x - btnPos.x + 210) / playerJoinBtns.Length;

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
                
                if (ModManager.MSC && self.playerClassButtons != null) {
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