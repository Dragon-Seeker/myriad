using Rewired.UI.ControlMapper;

namespace ManySlugCats;

public delegate void orig_Initilize(ControlMapper self);

public class AddMorePlayers
{
    
    public static void AddMorePlayerControls(orig_Initilize orig, ControlMapper self)
    {
        orig(self);

        ManySlugCatsMod.logger.LogMessage("ATTEMPTING TO ADD MORE PLAYERS!!!!");
        
        var manager = self.rewiredInputManager;

        manager.userData.AddPlayer();
        manager.userData.AddPlayer();
        manager.userData.AddPlayer(); 
        manager.userData.AddPlayer();
        
    }
    
    
    
}