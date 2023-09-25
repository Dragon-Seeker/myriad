using Myriad.utils;
using RWCustom;
using UnityEngine;

namespace Myriad.hooks; 

[Mixin(typeof(PlayerGraphics))]
public class PlayerGraphicsMixin {

    public static PlayerGraphicsMixin INSTANCE = new PlayerGraphicsMixin();
    
    public void init() {
        On.PlayerGraphics.DefaultSlugcatColor += PlayerGraphics_DefaultSlugcatColor;
        
        //AFTER SWITCHING BACK TO MSOPTIONS WAY
        On.PlayerGraphics.SlugcatColor += PlayerGraphics_SlugcatColor;
    }
    
    private Color PlayerGraphics_DefaultSlugcatColor(On.PlayerGraphics.orig_DefaultSlugcatColor orig, SlugcatStats.Name name) {
        Color result = orig(name);

        float dim = 0.8f;

        Color? extraResult = null;
        
        if (name == ExtraSlugcatNames.J10)   extraResult = new Color(0.09f, 0.14f, 0.31f); //Sofanthiel
        if (name == ExtraSlugcatNames.J5)    extraResult = new Color(0.56863f, 0.8f, 0.94118f); //Rivulet
        if (name == ExtraSlugcatNames.J6)    extraResult = new Color(0.43922f, 0.13725f, 0.23529f); //Artificer
        if (name == ExtraSlugcatNames.J7)    extraResult = new Color(0.66667f, 0.9451f, 0.33725f); //Saint
        if (name == ExtraSlugcatNames.J8)    extraResult = new Color(0.31f, 0.18f, 0.41f); //Spear
        if (name == ExtraSlugcatNames.J9)    extraResult = new Color(0.94118f, 0.75686f, 0.59216f); //Gourmand
        if (name == ExtraSlugcatNames.J11)   extraResult = new Color(1f, 0.4f, 0.79607844f); //Pebbles
        if (name == ExtraSlugcatNames.J12)   extraResult = new Color(0.13f, 0.53f, 0.69f); //Moon
        if (name == ExtraSlugcatNames.J13)   extraResult = new Color(0f, 1f, 0f); //NSH
        if (name == ExtraSlugcatNames.J14)   extraResult = new Color(0.89f * dim, 0.89f * dim, 0.79f * dim); //Sliver - TOO CLOSE TO SURVIVOR! DIM IT A LITTLE...
        if (name == ExtraSlugcatNames.J15)   extraResult = new Color(1f, 0.6f, 0f); //Orange Liz
        if (name == ExtraSlugcatNames.J16)   extraResult = new Color(1f, 0f, 0f); //Red Liz
        if (ExtraSlugcatNames.isAbove16(name)) extraResult = new Color(1f, 1f, 1f);

        return extraResult ?? result;
    }
    
    private Color PlayerGraphics_SlugcatColor(On.PlayerGraphics.orig_SlugcatColor orig, SlugcatStats.Name i) {
        //Logger.LogInfo("SLUGCAT COLOR pt1 " + " - " + i);
        Color result = orig(i);
        //Logger.LogInfo("SLUGCAT COLOR pt2 " + result + " - " + i);

        if (i == null) return result;

        string source = i.ToString();
        int pNum = 0;
        
        if (source.Contains("JollyPlayer")) {
            string split = "JollyPlayer";
            
            pNum = int.Parse(source.Substring(source.IndexOf(split) + split.Length));
        }

        if (pNum > 4) {
            //IT'S A BONUS CAT! AND THE GAME IS BAD AT HANDLING US. SO WE GOTTA FIX IT OURSELVES...
            if (ModManager.CoopAvailable) {
                if ((Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO && (pNum - 1) > 0) || Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM) {
                    return PlayerGraphics.JollyColor(pNum - 1, 0);
                }
                
                //WHA?? WHAT IS THIS DOING HERE? WHAT DOES IT DO?.. WHATEVER I GUESS I'M KEEPING IT
                i = (Custom.rainWorld.options.jollyPlayerOptionsArray[pNum - 1].playerClass ?? i);
            }
            
            if (PlayerGraphics.CustomColorsEnabled()) {
                return PlayerGraphics.CustomColorSafety(0);
            }
            
            //Debug.Log("DETERMINING CUSTOM COLOR FOR: " + i + " - P" + pNum);
            return PlayerGraphics.DefaultSlugcatColor(i);
        }
        
        //i
        return result;
    }
}