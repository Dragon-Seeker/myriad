using Menu;
using RWCustom;
using UnityEngine;

namespace Myriad.hooks.menu; 

public class MultiplayerMenuMixin {
    public static MultiplayerMenuMixin INSTANCE = new MultiplayerMenuMixin();

    public void init() {
        //ADJUST MENU LAYOUT
        On.Menu.MultiplayerMenu.InitiateGameTypeSpecificButtons += MultiplayerMenu_InitiateGameTypeSpecificButtons;
    }
    
    //OKAY WEIRD BUT WE A DEFINITELY DUPLICATING MENU OBJECTS WHEN SWITCHING BETWEEN ARENA MODES WHILE MSC IS DISABLED...
    private void MultiplayerMenu_InitiateGameTypeSpecificButtons(On.Menu.MultiplayerMenu.orig_InitiateGameTypeSpecificButtons orig, MultiplayerMenu self) {
        orig(self);

        var plyCnt = MyriadMod.PlyCnt();
        
        if (plyCnt <= 4) return;
        
        if (self.playerJoinButtons != null) {
            //foreach (var playerJoinButton in self.playerJoinButtons) playerJoinButton.pos.x -= shift;
            for (int i = 0; i < self.playerJoinButtons.Length; i++) {
                //float shift = 235 + i * 10; //298 //NORMALLY 120
                //float shift = 235 + i * 4.1f * self.playerJoinButtons.Length * Mathf.Lerp((1366 / Custom.rainWorld.options.ScreenSize.x), 1f, 0.4f);
                float shift = 235 + i * (plyCnt > 8 ? 4.1f : 1.2f) * self.playerJoinButtons.Length * Mathf.Lerp((1366 / Custom.rainWorld.options.ScreenSize.x), 1f, 0.4f);
                
                if (plyCnt > 8) {
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
                    
                    if (plyCnt > 8) {
                        self.playerClassButtons[i].pos.x -= self.playerJoinButtons[i].size.x / 2f;
                        self.playerClassButtons[i].size.y *= 0.75f;
                        
                        if (i % 2 == 0) {
                            self.playerClassButtons[i].pos.y += self.playerJoinButtons[i].size.y * 2f;
                        }
                    }
                }
            }
        }
        
        if (self.levelSelector != null) {
            self.levelSelector.pos -= new Vector2(165, 0);
            self.levelSelector.lastPos = self.levelSelector.pos;
        }
    }
}