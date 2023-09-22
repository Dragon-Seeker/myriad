using Menu;
using UnityEngine;

namespace Myriad.hooks.menu; 

public class PlayerResultBoxMixin {

    public static PlayerResultBoxMixin INSTANCE = new PlayerResultBoxMixin();
    
    public void init() {
        On.Menu.PlayerResultBox.ctor += PlayerResultBox_ctor;
        On.Menu.PlayerResultBox.GrafUpdate += PlayerResultBox_GrafUpdate;
        On.Menu.PlayerResultBox.IdealPos += PlayerResultBox_IdealPos;
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
}