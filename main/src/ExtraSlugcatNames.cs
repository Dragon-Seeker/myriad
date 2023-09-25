using System.Collections.Generic;

namespace Myriad; 

public class ExtraSlugcatNames {

    private static Dictionary<string, SlugcatStats.Name> EXTRA_NAMES = new Dictionary<string, SlugcatStats.Name>();
    private static Dictionary<string, SlugcatStats.Name> BASE_EXTRA_NAMES = new Dictionary<string, SlugcatStats.Name>();
    
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
    //public static SlugcatStats.Name JPlus = register("JPlus");

    private static SlugcatStats.Name register(string nameValue, bool baseValue = true) {
        var name = new SlugcatStats.Name(nameValue, true);
        
        EXTRA_NAMES.Add(nameValue, name);

        if(baseValue) BASE_EXTRA_NAMES.Add(nameValue, name);
        
        return name;
    }

    public static SlugcatStats.Name getName(int playerNumber) {
        string nameValue = $"J{playerNumber}";
        
        if (EXTRA_NAMES.ContainsKey(nameValue)) {
            return EXTRA_NAMES[nameValue];
        }

        return register(nameValue, baseValue: false);
    }
    
    public static bool isExtraName(SlugcatStats.Name name) {
        return EXTRA_NAMES.ContainsValue(name);
    }

    public static bool isAbove16(SlugcatStats.Name name) {
        return EXTRA_NAMES.ContainsValue(name) && !BASE_EXTRA_NAMES.ContainsValue(name);
    }
}