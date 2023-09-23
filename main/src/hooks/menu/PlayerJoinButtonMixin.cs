using Menu;
using Myriad.utils;
using RWCustom;
using UnityEngine;

namespace Myriad.hooks.menu; 

[Mixin(typeof(PlayerJoinButton))]
public class PlayerJoinButtonMixin {

    public static PlayerJoinButtonMixin INSTANCE = new PlayerJoinButtonMixin();

    public void init() {
        On.Menu.PlayerJoinButton.GrafUpdate += PlayerJoinButton_GrafUpdate;
        On.Menu.PlayerJoinButton.Update += PlayerJoinButton_Update;
    }
    
    //CONTINUING TO LOSE MY MIND
    private void PlayerJoinButton_GrafUpdate(On.Menu.PlayerJoinButton.orig_GrafUpdate orig, PlayerJoinButton self, float timeStacker) {
        Color origColor = self.portrait.sprite.color;
        
        orig(self, timeStacker);

        //NOT ALL PORTRAITS WILL BE WHITE NOW
        if (self.index > 3 && origColor != Color.white){
            Color newColor = PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(self.index));
            
            self.portrait.sprite.color = Color.Lerp(newColor, Color.black, Custom.SCurve(Mathf.Lerp(self.lastPortraitBlack, self.portraitBlack, timeStacker), 0.5f) * 0.75f);
        } 
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
}