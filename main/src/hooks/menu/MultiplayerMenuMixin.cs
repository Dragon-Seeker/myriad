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
        //IL.Menu.MultiplayerMenu.Update += MultiplayerMenu_Update1;
    }

    //WE'LL COME BACK TO THIS... MAYBE..
    private void MultiplayerMenu_Update1(MonoMod.Cil.ILContext il) {
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
        
    }

    //OKAY WEIRD BUT WE A DEFINITELY DUPLICATING MENU OBJECTS WHEN SWITCHING BETWEEN ARENA MODES WHILE MSC IS DISABLED...
    private void MultiplayerMenu_InitiateGameTypeSpecificButtons(On.Menu.MultiplayerMenu.orig_InitiateGameTypeSpecificButtons orig, MultiplayerMenu self) {
        
        //TEMPORARY DETOUR UNTIL WE FIX THE COMPETITIVE MODE...
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