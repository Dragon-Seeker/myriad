using System.Collections.Generic;

namespace Myriad; 

public class ExtraSlugcatNames {

    private static List<SlugcatStats.Name> EXTRA_NAMES = new List<SlugcatStats.Name>();
    
    public static SlugcatStats.Name J5 = register("J5");
    public static SlugcatStats.Name J6 = register("J6");
    public static SlugcatStats.Name J7 = register("J7");
    public static SlugcatStats.Name J8 = register("J8");
    public static SlugcatStats.Name J9 = register("J9");
    public static SlugcatStats.Name J10 = register("J10");
    public static SlugcatStats.Name J11 = register("J11");
    public static SlugcatStats.Name J12 = register("J12");
    public static SlugcatStats.Name J13 = register("J13");
    public static SlugcatStats.Name J14 = register("J14");
    public static SlugcatStats.Name J15 = register("J15");
    public static SlugcatStats.Name J16 = register("J16");
    public static SlugcatStats.Name JPlus = register("JPlus");

    private static SlugcatStats.Name register(string nameValue) {
        var name = new SlugcatStats.Name(nameValue, true);
        
        EXTRA_NAMES.Add(name);

        return name;
    }
    
    public static bool isExtraName(SlugcatStats.Name name) {
        return EXTRA_NAMES.Contains(name);
    }
}