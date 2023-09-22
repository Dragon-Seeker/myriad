namespace Myriad.hooks; 

public class SlugcatStatsMixin {
    public static SlugcatStatsMixin INSTANCE = new SlugcatStatsMixin();

    public void init() {
        On.SlugcatStats.Name.ArenaColor += Name_ArenaColor;
        On.SlugcatStats.Name.Init += Name_Init;
        On.SlugcatStats.HiddenOrUnplayableSlugcat += SlugcatStats_HiddenOrUnplayableSlugcat;
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
        
        return orig(i) || extraPlayer;
    }
    
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
            > 15 => ExtraSlugcatNames.JPlus, //OR ELSE IT WILL RETURN NULL AND CRASH MOST THINGS
            _ => null
        };

        return name ?? orig(playerIndex);
    }
}