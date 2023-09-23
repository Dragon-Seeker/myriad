using Myriad.utils;
using RWCustom;

namespace Myriad.hooks; 

[Mixin(typeof(ShelterDoor))]
public class ShelterDoorMixin {

    public const int forceTimeLimit = 120;
    
    public static ShelterDoorMixin INSTANCE = new ShelterDoorMixin();

    public int playersReadyToSleep = 0;
    public int forceShutTimer = 0;
    
    public void init() {
        On.ShelterDoor.Update += ShelterDoor_Update;
    }
    
    private void ShelterDoor_Update(On.ShelterDoor.orig_Update orig, ShelterDoor self, bool eu) {
        orig(self, eu);

        if (self.isAncient) return; //SKIP ALL THIS FOR ANCIENT SHELTERS. THEY'VE GOT PLENTY OF ROOM

        playersReadyToSleep = 0;
        //COUNT PLAYERS READY TO HIBERNATE
        for (int k = 0; k < self.room.game.Players.Count; k++) {
            if (self.room.game.Players[k].realizedCreature != null && self.room.game.Players[k].realizedCreature is Player player && player.ReadyForWinJolly) {
                playersReadyToSleep++;
            }
        }

        //GIVE THE STRAGGLERS A FEW SECONDS TO CRAWL INSIDE
        if (playersReadyToSleep >= 4) {
            forceShutTimer++;
        } else {
            forceShutTimer = 0;
        }

        //IF SHELTER DOORS ARE CLOSING, TELEPORT ANY PLAYERS IN A CORRIDOR INTO THE MAIN ROOM.
        if (self.closeSpeed > 0f) {
            //FIND THE FIRST SLUG WHO ISN'T IN A COORIDOR
            Player host = null;
            
            for (int i = 0; i < self.room.game.Players.Count; i++) {
                var abstractCreature = self.room.game.Players[i];
                
                if (abstractCreature.realizedCreature != null 
                    && abstractCreature.realizedCreature is Player player
                    && player.room == self.room
                    && player.bodyMode != Player.BodyModeIndex.CorridorClimb) {
                    host = player; //THIS WILL BE THE GUY WE TELEPORT TO
                    
                    break;
                }
            }

            //TAKE EVERYONE WHO IS STILL IN A CORRIDOR AND TP THEM TO THE HOST
            for (int i = 0; i < self.room.game.Players.Count; i++) {
                var abstractCreature = self.room.game.Players[i];
                
                if (host != null && abstractCreature.realizedCreature != null
                    && abstractCreature.realizedCreature is Player player
                    && player.room == self.room
                    && player.bodyMode == Player.BodyModeIndex.CorridorClimb) {
                    
                    for (int j = 0; j < player.bodyChunks.Length; j++) {
                        player.bodyChunks[j].vel = Custom.DegToVec(UnityEngine.Random.value * 360f) * 4f;
                        player.bodyChunks[j].pos = host.bodyChunks[j].pos;
                        player.bodyChunks[j].lastPos = host.bodyChunks[j].pos;
                    }
                    
                    player.shortcutDelay = 40;
                }
            }
        }
    }
}