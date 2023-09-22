using JollyCoop.JollyMenu;
using System.Linq;
using UnityEngine;

namespace Myriad.hooks; 

public class RainWorldGameMixin {
    public static RainWorldGameMixin INSTANCE = new RainWorldGameMixin();

    public void init() {
        On.RainWorldGame.JollySpawnPlayers += RainWorldGame_JollySpawnPlayers;
    }
    
    private void RainWorldGame_JollySpawnPlayers(On.RainWorldGame.orig_JollySpawnPlayers orig, RainWorldGame self, WorldCoordinate location) {
        Debug.Log("---Number of jolly players: " + self.rainWorld.options.JollyPlayerCount.ToString()); // 
        Debug.Log("---accesing directly: " + Enumerable.Count<JollyPlayerOptions>(self.rainWorld.options.jollyPlayerOptionsArray, (JollyPlayerOptions x) => x.joined).ToString());
        Debug.Log("---BONUS: ARRAY LENGTH" + self.rainWorld.options.jollyPlayerOptionsArray.Length);
        
        int PlayerArrSize = self.rainWorld.options.jollyPlayerOptionsArray.Length;

        JollyPlayerOptions[] jollyPlayerOptionsArray = self.rainWorld.options.jollyPlayerOptionsArray;
        
        for (int i = 0; i < jollyPlayerOptionsArray.Length; i++) {
            Debug.Log(jollyPlayerOptionsArray[i].ToString());
        }
        
        //IF WE'RE PLAYING WITH A NORMAL AMOUNT (LESS THAN 5) RUN THIS
        if (self.rainWorld.options.JollyPlayerCount < PlayerArrSize) {
            //WE HAVE TO BRIEFLY PRETEND OUR PLAYEROPTIONSARRAY IS ONLY AS MANY ENTRIES AS WE'VE SELECTED
            JollyPlayerOptions[] optionsMemory = self.rainWorld.options.jollyPlayerOptionsArray;
            
            self.rainWorld.options.jollyPlayerOptionsArray = new JollyPlayerOptions[self.rainWorld.options.JollyPlayerCount];
            
            for (int j = 0; j < self.rainWorld.options.jollyPlayerOptionsArray.Length; j++) {
                self.rainWorld.options.jollyPlayerOptionsArray[j] = optionsMemory[j];
            }
            
            //Debug.Log("---CUSTOM JOLLY SPAWN" + self.rainWorld.options.jollyPlayerOptionsArray.Length);
            orig(self, location);

            //THEN RESTORE THE ARRAY TO IT'S FULL PLAYER COUNT
            self.rainWorld.options.jollyPlayerOptionsArray = optionsMemory;
        } else {
            orig(self, location); //IF WE'RE PLAYING WITH EVERYONE ENABLED, WE'RE GOOD TO RUN THE ORIGINAL FOR SOME REASON
        }

        Debug.Log("----JOLLY SPAWN SUCCESS");
    }
}