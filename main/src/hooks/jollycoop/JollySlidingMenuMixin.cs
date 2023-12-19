using JollyCoop;
using JollyCoop.JollyMenu;
using Kittehface.Framework20;
using Menu;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Myriad.utils;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using Expedition;

namespace Myriad.hooks.jollycoop; 

[Mixin(typeof(JollySlidingMenu))]
public class JollySlidingMenuMixin {

    public static JollySlidingMenuMixin INSTANCE = new JollySlidingMenuMixin();

    public void init() {
        On.JollyCoop.JollyMenu.JollySlidingMenu.ctor += adjustPlayerSelectGUI;
        On.JollyCoop.JollyMenu.JollySlidingMenu.NumberPlayersChange += accountForMoreThanFour;
        On.JollyCoop.JollyMenu.JollySlidingMenu.Singal += JollySlidingMenu_Singal;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.Update += JollyPlayerSelector_Update;

        //Needed to prevent the cap of only 31 as a span...
        IL.Menu.Remix.MixedUI.OpSliderTick.ctor_ConfigurableBase_Vector2_int_bool += il => {
            List<Func<Instruction, bool>> predicates = new List<Func<Instruction, bool>>();

            predicates.Add(i => i.MatchLdcI4(31));

            var cursor = new ILCursor(il);
            var x = 0;

            while (cursor.TryGotoNext(MoveType.After, predicates.ToArray())) {
                x++;
                cursor.EmitDelegate((int oldNum) => 128);
            }
        };
    }

    bool btnHeld = false;
    public void JollyPlayerSelector_Update(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_Update orig, JollyPlayerSelector self) {
        orig(self);

        bool pressedBtn = false;
        if (self.classButton.Selected) {
            
            Profiles.Profile profile = self.menu.manager.rainWorld.playerHandler.profile;
            if (Input.GetKey((KeyCode) 324) || (profile != null && UserInput.GetButton(profile, "UICancel"))) {
                if (!btnHeld)
                    pressedBtn = true;
                btnHeld = true;
            }
            else {
                btnHeld = false;
            }

            if (pressedBtn) {
                //FOR MOD COMPATIBILITY SAKE, SHIFT BACK 2 AND THEN RUN THE NORMAL CLASS CHANGE
                string mode = self.menu.manager.rainWorld.ExpeditionMode ? "expedition" : "story";
                self.slugName = PrevClass(self.slugName, mode, self.menu.manager.rainWorld);
                self.slugName = PrevClass(self.slugName, mode, self.menu.manager.rainWorld);
                self.Singal(self, "CLASSCHANGE" + self.index.ToString());
            }
        }
    }

    public static SlugcatStats.Name PrevClass(SlugcatStats.Name curClass, string menuMode, RainWorld rainWorld) {
        if (ModManager.Expedition && menuMode == "expedition") //self.menu.manager.rainWorld.ExpeditionMode
        {
            int num = ExpeditionGame.unlockedExpeditionSlugcats.IndexOf(curClass) - 1;
            if (num < 0) {
                return ExpeditionGame.unlockedExpeditionSlugcats[ExpeditionGame.unlockedExpeditionSlugcats.Count - 1];
            }
            return ExpeditionGame.unlockedExpeditionSlugcats[num];
        } else {
            SlugcatStats.Name name;
            if (curClass == null) {
                int lastEntry = ExtEnum<SlugcatStats.Name>.values.Count - 1;
                name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(lastEntry), false);
            } else {
                if (curClass.Index <= 0 || curClass.Index > ExtEnum<SlugcatStats.Name>.values.Count - 1) {
                    return PrevClass(null, menuMode, rainWorld);
                }
                name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(curClass.Index - 1), false);
            }
            if (SlugcatStats.HiddenOrUnplayableSlugcat(name)) {
                return PrevClass(name, menuMode, rainWorld);
            }
            if (menuMode == "story" && !SlugcatStats.SlugcatUnlocked(name, rainWorld)) {
                return PrevClass(name, menuMode, rainWorld);
            }
            if (menuMode == "arena" && name != SlugcatStats.Name.White && name != SlugcatStats.Name.Yellow && new MultiplayerUnlocks(rainWorld.progression, new List<string>()).ClassUnlocked(name) == false) {
                return PrevClass(name, menuMode, rainWorld);
            }
            //Debug.Log("Next class: " + ((name != null) ? name.ToString() : null));
            return name;
        }
    }

    public void accountForMoreThanFour(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_NumberPlayersChange orig, JollySlidingMenu self, UIconfig config, string value, string oldvalue) {
        orig(self, config, value, oldvalue);
        
        var plyCnt = MyriadMod.PlyCnt();
        
        if(plyCnt <= 4) return;
        
        int result;

        var jollyPlayerOptions = self.Options.jollyPlayerOptionsArray;
        
        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
            if (result > plyCnt || result < 4) return;
            
            for (int index = 0; index < jollyPlayerOptions.Length; ++index) jollyPlayerOptions[index].joined = index <= result - 1;
        }
        
        self.UpdatePlayerSlideSelectable(result - 1);
    }
    
	public SimpleButton[] jollySwapButtons;
	
    public void adjustPlayerSelectGUI(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_ctor orig, JollySlidingMenu self, JollySetupDialog menu, MenuObject owner, Vector2 pos) {
        orig(self, menu, owner, pos);

        var plyCnt = MyriadMod.PlyCnt();
        
        //if(plyCnt <= 4) return;

        //float num1 = 70;
        //float num2 = (float)((1024.0 - num1 * 8.0) / 5.0);
        //Vector2 pos1 = new Vector2(-25 + num2, 0.0f) + new Vector2(0.0f, menu.manager.rainWorld.screenSize.y * 0.55f);
		
		//TRYING SOMETHING FUNKY
		jollySwapButtons = new SimpleButton[MyriadMod.PlyCnt()];
        float swapOffsetY = (plyCnt > 8 ? 5 : 0);

        for (int index = 0; index < MyriadMod.PlyCnt(); ++index) {
            JollyPlayerSelector playerSelector = self.playerSelector[index];

            if (plyCnt > 8) {
                playerSelector.portraitRectangle.size /= 2;
                playerSelector.portraitRectangle.lastSize /= 2;

                playerSelector.portraitRectangle.pos += new Vector2(18, 4);
                playerSelector.portraitRectangle.lastPos = playerSelector.portraitRectangle.pos;

                var portaitPos = playerSelector.portraitRectangle.pos + new Vector2(25, 25);  //new Vector2((18 * 2) + 8, (12 * 2) + 8);

                playerSelector.RemoveSubObject(playerSelector.portrait);

                playerSelector.portrait = new MenuIllustration((Menu.Menu) menu, playerSelector, "", "MultiplayerPortrait" + index.ToString() + "1",
                    portaitPos, true, true);

                playerSelector.subObjects.Add(playerSelector.portrait);

                playerSelector.portrait.sprite.scale = 0.5f;

                // var portaitSpritPos = new Vector2(18, 12);
                //
                // playerSelector.portrait.pos += portaitSpritPos;
                // playerSelector.portrait.lastPos = playerSelector.portrait.pos;

                playerSelector.classButton.SetSize(playerSelector.classButton.size -= new Vector2(18, 0));

                playerSelector.classButton.menuLabel.label.scale = 0.90f;
                
                //----

                playerSelector.playerLabelSelector.size -= new Vector2(18, 0);
            }
            

            //LETS TRY SOMETHING SIMPLER...
            if (plyCnt > 4) {
                float newX = ((menu.manager.rainWorld.screenSize.x) / plyCnt) * index;
                newX += 5 + Mathf.Lerp(700f, 0f, (Custom.rainWorld.options.ScreenSize.x / 1360)); //AN ATTEMPT TO FIX THE WEIRD SCREEN SIZE SCALING
                playerSelector.pos.x = newX + 0;
                playerSelector.playerLabelSelector._pos.x = newX + 0;
            }
            

            if (plyCnt > 8) {
                playerSelector.pupButton.pos += new Vector2(-85f, 95f); //-45
                playerSelector.pupButton.roundedRect.size *= 0.8f;
                playerSelector.pupButton.selectRect.size *= 0.8f;
            }

            float swapOffset = Custom.LerpMap(plyCnt, 4f, 16f, -80f, -20f);
            jollySwapButtons[index] = new SimpleButton(self.menu, self, "<->", "JOLLYSWAP" + index.ToString(), playerSelector.pos + new Vector2(swapOffset, 120 + (swapOffsetY * 2f)) , new Vector2(40f, 20f));
            menu.elementDescription.Add(jollySwapButtons[index].signalText, menu.Translate("Swap Player <p_n> and Player <p_n2>").Replace("<p_n>", (index + 0).ToString()).Replace("<p_n2>", (index + 1).ToString()));
            self.subObjects.Add(jollySwapButtons[index]);
            
            if (index > 0) {
                // <0 1^ 2> 3v
                jollySwapButtons[index].nextSelectable[3] = playerSelector.pLabelSelectorWrapper; //DOWN
                jollySwapButtons[index].nextSelectable[1] = self.sliderWrapper; //UP
                jollySwapButtons[index].nextSelectable[0] = jollySwapButtons[index - 1]; //LEFT
                jollySwapButtons[index].nextSelectable[2] = jollySwapButtons[index]; //RIGHT - ASSIGN IT TO OURSELF FOR NOW. IF THERES ANOTHER ONE TO OUR RIGHT, IT'LL CHANGE OURS
                jollySwapButtons[index - 1].nextSelectable[2] = jollySwapButtons[index];

                playerSelector.pLabelSelectorWrapper.nextSelectable[1] = jollySwapButtons[index];
                playerSelector.pLabelSelectorWrapper.nextSelectable[3] = playerSelector.pupButton;
                playerSelector.pLabelSelectorWrapper.nextSelectable[0] = self.playerSelector[index-1].pLabelSelectorWrapper;
                self.playerSelector[index].pLabelSelectorWrapper.nextSelectable[2] = self.playerSelector[index].pLabelSelectorWrapper; //SELF, UNLESS THE ONE NEXT TO US UPDATES IT
                self.playerSelector[index - 1].pLabelSelectorWrapper.nextSelectable[2] = self.playerSelector[index].pLabelSelectorWrapper;
                
                if (plyCnt > 8) {
                    self.playerSelector[index].pupButton.nextSelectable[0] = self.playerSelector[index - 1].pupButton;
                    self.playerSelector[index].pupButton.nextSelectable[2] = self.playerSelector[index].pupButton; //SELF, UNLESS THE ONE NEXT TO US UPDATES IT
                    self.playerSelector[index - 1].pupButton.nextSelectable[2] = self.playerSelector[index].pupButton;
                }
                    
            }
            else {
                //JUST PRETEND THIS FIRST ONE DOESN'T EXIST
                jollySwapButtons[0].roundedRect.pos.x -= 1000;
                jollySwapButtons[0].menuLabel.pos.x -= 1000;
                jollySwapButtons[0].selectRect.pos.x -= 1000;

                playerSelector.pLabelSelectorWrapper.nextSelectable[3] = playerSelector.pupButton;
                playerSelector.pLabelSelectorWrapper.nextSelectable[0] = self.playerSelector[index].pLabelSelectorWrapper;
                if (plyCnt > 8)
                    playerSelector.pupButton.nextSelectable[0] = self.playerSelector[index].pupButton;
            }

            playerSelector.pupButton.nextSelectable[1] = playerSelector.pLabelSelectorWrapper;
        }

        jollySwapButtons[1].nextSelectable[0] = jollySwapButtons[1];

        //---
        var slider = self.numberPlayersSlider;
        if (plyCnt > 4) {
            var config = menu.oi.config.Bind("_cosmetic", Custom.rainWorld.options.JollyPlayerCount, new ConfigAcceptableRange<int>(1, plyCnt));

            var uiConfigType = typeof(UIconfig);
            var flags = BindingFlags.Public | BindingFlags.Instance;

            //self.numberPlayersSlider.cfgEntry = config;
            uiConfigType.GetField("cfgEntry", flags)
                .SetValue(self.numberPlayersSlider, config);

            //self.numberPlayersSlider.cosmetic = config.IsCosmetic;
            uiConfigType.GetField("cosmetic", flags)
                .SetValue(self.numberPlayersSlider, config.IsCosmetic);

            slider.defaultValue = config.defaultValue;

            slider.cfgEntry.BoundUIconfig = slider;

            var bl = config.info != null && config.info.acceptable != null;

            slider.min = bl ? (int) config.info.acceptable.Clamp(int.MinValue) : 0;
            slider.max = bl ? (int) config.info.acceptable.Clamp(int.MaxValue) : (slider._IsTick ? 15 : 100);

            slider._size = new Vector2(Math.Max((int) (self.playerSelector[plyCnt - 1].pos - self.playerSelector[0].pos).x, 30), 30f);
            slider.fixedSize = slider._size;

            
        }
        float num1 = 70;
        slider.pos = self.playerSelector[0].pos + new Vector2((num1 / 2f) + 15, 144f + swapOffsetY); //UPPING A FEW PIXELS SO IT OVERLAPS LESS WITH THE PLAYER-SWAP BUTTONS
        slider.Initialize();
        //---

        //Rebind buttons is needed to fix navigating sortof... Needs to be better handled I think
        //self.BindButtons(); //THIS DOESN'T WORK! BREAKS TOO MANY SELECTIONS. FIXING UP TOP
    }


    private void JollySlidingMenu_Singal(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_Singal orig, JollySlidingMenu self, MenuObject sender, string message) {

        var plyCnt = MyriadMod.PlyCnt();

        //VANILLA SLUGPUP TOGGLE DOES NOT ACCOUNT FOR DOUBLE DIGIT PLAYERCOUNT. LETS FIX THAT
        if (message.Contains("toggle_pup") && plyCnt > 8) {
            bool isPup = false;
            if (message.Contains("on")) {
                isPup = true;
                message = message.Replace("_on", "");
            } else {
                message = message.Replace("_off", "");
            }

            string checkMsg = message.Replace("toggle_pup_", ""); //(char.ToString(message[message.Length - 1]) //THIS WAS VANILLA, AND IT ONLY TOOK THE LAST DIGIT
            if (int.TryParse(checkMsg, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) && result < self.playerSelector.Length)
                self.JollyOptions(result).isPup = isPup;
            else
                JollyCustom.Log("Error parsing signal string: " + message, throwException: true);

            message = ""; //TO THROW OFF ORIG FROM REPEATING THE SAME MISTAKES
        }

        orig(self, sender, message);
        
        if (message.Contains("JOLLYSWAP")) {
            for (int i = 0; i < self.playerSelector.Length; ++i){
                if (message == "JOLLYSWAP" + i.ToString()) {
                    JollyPlayerOptions jOptA = Custom.rainWorld.options.jollyPlayerOptionsArray[i];
                    JollyPlayerOptions jOptB = Custom.rainWorld.options.jollyPlayerOptionsArray[i - 1];
                    Custom.rainWorld.options.jollyPlayerOptionsArray[i] = jOptB;
                    Custom.rainWorld.options.jollyPlayerOptionsArray[i - 1] = jOptA;
					bool swapPup = self.JollyOptions(i).isPup != self.JollyOptions(i-1).isPup;
                    //DON'T MIX UP PLAYER NUMBER. ESPECIALLY BECAUSE PLAYER 1 IS ALWAYS FORCE ENABLED
                    Custom.rainWorld.options.jollyPlayerOptionsArray[i].playerNumber = jOptA.playerNumber;
                    Custom.rainWorld.options.jollyPlayerOptionsArray[i - 1].playerNumber = jOptB.playerNumber;

                    self.SetPortraitsDirty(); //REFRESH THE PORTRAITS!
                    self.playerSelector[i].dirty = true;
                    self.playerSelector[i-1].dirty = true;
                    
					//IF ONE OF US WAS A PUP AND THE OTHER WASN'T, TOGGLE BOTH PUP BUTTONS
					if (swapPup) {
						self.playerSelector[i].pupButton.Toggle();
						self.playerSelector[i-1].pupButton.Toggle();
					}
					
					//TRY AND UPDATE THE NAMES TOO
					self.playerSelector[i].playerLabelSelector.value = JollyCustom.GetPlayerName(i);
					self.playerSelector[i-1].playerLabelSelector.value = JollyCustom.GetPlayerName(i-1);
					
					//UPDATE WHO IS ACTUALLY ACTIVE
                    self.NumberPlayersChange(self.numberPlayersSlider.cfgEntry.BoundUIconfig, self.numberPlayersSlider.value, self.numberPlayersSlider.value);
                }
            }
        }
    }

}