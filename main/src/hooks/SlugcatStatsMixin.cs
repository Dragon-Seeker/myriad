using Myriad.utils;
using UnityEngine;

namespace Myriad.hooks; 

[Mixin(typeof(SlugcatStats))]
public class SlugcatStatsMixin {
    public static SlugcatStatsMixin INSTANCE = new SlugcatStatsMixin();

    public void init() {
        On.SlugcatStats.Name.ArenaColor += Name_ArenaColor;
        On.SlugcatStats.Name.Init += Name_Init;
        On.SlugcatStats.HiddenOrUnplayableSlugcat += SlugcatStats_HiddenOrUnplayableSlugcat;
        On.MoreSlugcats.MSCRoomSpecificScript.SH_GOR02.ctor += SH_GOR02_ctor; //FIX THE GOURM INTRO NOT HAVING ENOUGH SPAWN SPOTS
    }

    private void SH_GOR02_ctor(On.MoreSlugcats.MSCRoomSpecificScript.SH_GOR02.orig_ctor orig, MoreSlugcats.MSCRoomSpecificScript.SH_GOR02 self, Room room) {
        orig(self, room);
        self.spawnSpots = new Vector2[]
        {
            new Vector2(450f, 170f),
            new Vector2(690f, 230f),
            new Vector2(730f, 230f),
            new Vector2(230f, 310f),
            //JUST LOOP IT A BUNCH MORE
            new Vector2(450f, 170f),
            new Vector2(690f, 230f),
            new Vector2(730f, 230f),
            new Vector2(230f, 310f),
            new Vector2(450f, 170f),
            new Vector2(690f, 230f),
            new Vector2(730f, 230f),
            new Vector2(230f, 310f),
            new Vector2(450f, 170f),
            new Vector2(690f, 230f),
            new Vector2(730f, 230f),
            new Vector2(230f, 310f)
        };
    }

    //ADDING FAKE CHARACTERS FOR THE ARENA MODE COLORS
    private void Name_Init(On.SlugcatStats.Name.orig_Init orig) {
        orig();
        // ExtEnum<SlugcatStats.Name>.values.AddEntry(SlugcatStats.Name.White.value);
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J5");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J6");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J7");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J8");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J9");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J10");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J11");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J12");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J13");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J14");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J15");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("J16");
        ExtEnum<SlugcatStats.Name>.values.AddEntry("JPlus");
    }

    //SO THEY DON'T SHOW UP IN THE SELECT SCREEN
    private bool SlugcatStats_HiddenOrUnplayableSlugcat(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i) {
        bool extraPlayer = ExtraSlugcatNames.isExtraName(i);
        
        //MAYBE ANOTHER TIME
        //if (i == Watcher.WatcherEnums.SlugcatStatsName.Watcher || (ModManager.MSC && i == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)) {
        //    return false;
        //}
        return orig(i) || extraPlayer;
    }

    //MAYBE ANOTHER TIME...
    /*private bool SlugcatStats_SlugcatUnlocked(On.SlugcatStats.orig_SlugcatUnlocked orig, SlugcatStats.Name i, RainWorld rainWorld) {
        if (i == Watcher.WatcherEnums.SlugcatStatsName.Watcher || (ModManager.MSC && i == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)) {
            return true;
        }
        return orig(i, rainWorld);
    }*/

    private SlugcatStats.Name Name_ArenaColor(On.SlugcatStats.Name.orig_ArenaColor orig, int playerIndex) {
        //THIS VERSION WORKS EVEN IF MSC IS NOT ENABLED
        SlugcatStats.Name? name = playerIndex switch {
            4 => ExtraSlugcatNames.J5,
            5 => ExtraSlugcatNames.J6,
            6 => ExtraSlugcatNames.J7,
            7 => ExtraSlugcatNames.J8,
            8 => ExtraSlugcatNames.J9,
            9 => ExtraSlugcatNames.J10,
            10 => ExtraSlugcatNames.J11,
            11 => ExtraSlugcatNames.J12,
            12 => ExtraSlugcatNames.J13,
            13 => ExtraSlugcatNames.J14,
            14 => ExtraSlugcatNames.J15,
            15 => ExtraSlugcatNames.J16,
            //MORE THAN 16?
            > 15 => ExtraSlugcatNames.getName(playerIndex), //OR ELSE IT WILL RETURN NULL AND CRASH MOST THINGS
            _ => null
        };

        return name ?? orig(playerIndex);
    }
}