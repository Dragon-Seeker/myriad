using Myriad.utils;

namespace Myriad.hooks; 

[Mixin(typeof(Player))]
public class PlayerMixin {

    public static PlayerMixin INSTANCE = new PlayerMixin();

    public void init() {
        On.Player.Update += BPPlayer_Update;
    }
    
    public static void BPPlayer_Update(On.Player.orig_Update orig, Player self, bool eu) {
        orig(self, eu);
        
        bool check1 = !MyriadMod.rotundWorldEnabled
            && MyriadMod.options.grabRelease.Value
            && self.input[0].jmp
            && !self.input[1].jmp &&
            self.grabbedBy?.Count > 0;
        
        //BRING BACK THIS CLASSIC JOLLYCOOP FIXES FEATURE THAT IS VERY MUCH NEEDED IN CO-OP
        if (check1) {
            for (int graspIndex = self.grabbedBy.Count - 1; graspIndex >= 0; graspIndex--) {
                if (self.grabbedBy[graspIndex] is Creature.Grasp grasp && grasp.grabber is Player player_ &&
                    (!self.isNPC || (player_.isNPC))) {
                    //PUPS SHOULD LET GO OF OTHER PUPS
                    player_.ReleaseGrasp(grasp.graspUsed); // list is modified
                }
            }
        }
        
        var shelterDoorData = ShelterDoorMixin.INSTANCE;

        bool check2 = shelterDoorData.playersReadyToSleep >= 4 
            && shelterDoorData.forceShutTimer >= ShelterDoorMixin.forceTimeLimit
            && self.room != null 
            && self.room.abstractRoom.shelter 
            && self.AI == null
            && self.room.game.IsStorySession 
            && !self.dead 
            && !self.Sleeping 
            && self.room.shelterDoor != null 
            && !self.room.shelterDoor.Broken 
            && self.shortcutDelay < 1 
            && self.readyForWin;

        //MAKE SHELTERS CLOSE EASIER IF AT LEAST 4 PEOPLE ARE READY TO SLEEP
        if (check2) {
            if (ModManager.CoopAvailable) self.ReadyForWinJolly = true;

            self.room.shelterDoor.Close(); //THIS JUST CHECKS FOR CLOSE
        }
    }
}