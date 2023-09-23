using HUD;
using Myriad.utils;
using UnityEngine;

namespace Myriad.hooks.hud; 

[Mixin(typeof(PlayerSpecificMultiplayerHud))]
public class PlayerSpecificMultiplayerHudMixin {
    public static PlayerSpecificMultiplayerHudMixin INSTANCE = new PlayerSpecificMultiplayerHudMixin();

    public void init() {
        On.HUD.PlayerSpecificMultiplayerHud.ctor += PlayerSpecificMultiplayerHud_ctor;
    }
    
    private void PlayerSpecificMultiplayerHud_ctor(On.HUD.PlayerSpecificMultiplayerHud.orig_ctor orig, HUD.PlayerSpecificMultiplayerHud self, HUD.HUD hud, ArenaGameSession session, AbstractCreature abstractPlayer) {
        orig(self, hud, session, abstractPlayer);

        int playNum = (abstractPlayer.state as PlayerState).playerNumber;
        int rank = 0;

        while (playNum > 3) {
            playNum = playNum - 4;
            rank += 1;
        }

        //THEY'RE GOING TO NEED TO GO THROUGH THIS AGAIN...
        if (rank > 0) {
            switch (playNum) {
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
}