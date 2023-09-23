using Menu;
using MonoMod.Utils;
using Myriad.utils;
using UnityEngine;

namespace Myriad.hooks.menu; 

[Mixin(typeof(MenuIllustration))]
public class MenuIllustrationMixin {

    public static MenuIllustrationMixin INSTANCE = new MenuIllustrationMixin();
    
    public void init() {
        On.Menu.MenuIllustration.ctor += MenuIllustration_ctor;
        On.Menu.MenuIllustration.LoadFile_string += MenuIllustration_LoadFile_string;
    }
    
    //I JUST COPIED THIS IN HERE SO IT RUNS FIRST I GUESS? DO I STILL NEED TO RUN IT IN THE OTHER ONE THEN?
    private void MenuIllustration_ctor(On.Menu.MenuIllustration.orig_ctor orig, MenuIllustration self, Menu.Menu menu, MenuObject owner, string folderName, string fileName, Vector2 pos, bool crispPixels, bool anchorCenter)
    {
        //REMEMBER WHAT PLAYER NUMBER THIS WAS BEFORE WE DO ANYTHING
        int index = PortraitUtils.GetPortraitIndex(fileName); //PLAYER NUMBER

        //GOOD LORD WE REALLY DO NEED TO DO IT IN BOTH PLACES :/ OTHERWISE FSPRITE TRIES TO CREATE AN INVALID SPRITE
        string newFileName = PortraitUtils.AdjustedPortraitFile(fileName);

        orig.Invoke(self, menu, owner, folderName, newFileName, pos, crispPixels, anchorCenter);

        if (index > 3 && self.spriteAdded) {
            //logger.LogMessage("TINT OUR PORTRAIT " + SlugcatStats.Name.ArenaColor(index));
            self.sprite.color = PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(index));
        }
    }

    private void MenuIllustration_LoadFile_string(On.Menu.MenuIllustration.orig_LoadFile_string orig, MenuIllustration self, string folder) 
    {
        //REMEMBER WHAT PLAYER NUMBER THIS WAS BEFORE WE DO ANYTHING
        int index = PortraitUtils.GetPortraitIndex(self.fileName); //PLAYER NUMBER
        //logger.LogMessage("STARTING FILE: " + self.fileName);
        self.fileName = PortraitUtils.AdjustedPortraitFile(self.fileName); //SHOULD WE SET THI BACK WHEN WE'RE DONE?... NAHHH
        //logger.LogMessage("--ENDING FILE: " + self.fileName);
        orig(self, folder);
        
        //IF WE WERE A SLUGCAT PORTRAIT PAST PLAYER 4, TINT OUR PORTRAIT
        if (index > 3 && self.spriteAdded) {
            //logger.LogMessage("TINT OUR PORTRAIT " + SlugcatStats.Name.ArenaColor(index) + PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(index)) + "  ---   " + self.sprite.color);
            self.sprite.color = PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(index));
        }
    }

}