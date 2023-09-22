using JollyCoop;
using JollyCoop.JollyMenu;
using Menu;
using Menu.Remix.MixedUI;
using RWCustom;
using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace Myriad.hooks.jollycoop; 

public class JollySlidingMenuMixin {

    public static JollySlidingMenuMixin INSTANCE = new JollySlidingMenuMixin();

    public void init() {
        On.JollyCoop.JollyMenu.JollySlidingMenu.ctor += adjustPlayerSelectGUI;
        On.JollyCoop.JollyMenu.JollySlidingMenu.NumberPlayersChange += accountForMoreThanFour;
        On.JollyCoop.JollyMenu.JollySlidingMenu.Singal += JollySlidingMenu_Singal;
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
        
        if(plyCnt <= 4) return;

        //float num1 = 70;
        //float num2 = (float)((1024.0 - num1 * 8.0) / 5.0);
        //Vector2 pos1 = new Vector2(-25 + num2, 0.0f) + new Vector2(0.0f, menu.manager.rainWorld.screenSize.y * 0.55f);
		
		//TRYING SOMETHING FUNKY
		jollySwapButtons = new SimpleButton[MyriadMod.PlyCnt()];

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
            
            //playerSelector.pos.x -= playerSelector.pos.x - pos1.x;
            //playerSelector.playerLabelSelector._pos.x -= playerSelector.playerLabelSelector._pos.x - pos1.x;
            //pos1 += new Vector2(num2 + num1, 0.0f);

            //LETS TRY SOMETHING SIMPLER...
            float newX = ((menu.manager.rainWorld.screenSize.x) / plyCnt) * index;
            newX += 5 + Mathf.Lerp(700f, 0f, (Custom.rainWorld.options.ScreenSize.x / 1360)); //AN ATTEMPT TO FIX THE WEIRD SCREEN SIZE SCALING
            playerSelector.pos.x = newX + 0;
            playerSelector.playerLabelSelector._pos.x = newX + 0;

            if (plyCnt > 8) {
                playerSelector.pupButton.pos += new Vector2(-85f, 95f); //-45
                playerSelector.pupButton.roundedRect.size *= 0.8f;
                playerSelector.pupButton.selectRect.size *= 0.8f;
            }
			
            jollySwapButtons[index] = new SimpleButton(self.menu, self, "<->", "JOLLYSWAP" + index.ToString(), playerSelector.pos + new Vector2(-20, 130) , new Vector2(40f, 20f));
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

        float num1 = 70;
        slider.pos = self.playerSelector[0].pos + new Vector2((num1 / 2f) + 15, 142f);

        slider._size = new Vector2(Math.Max((int)(self.playerSelector[plyCnt - 1].pos - self.playerSelector[0].pos).x, 30), 30f);
        slider.fixedSize = slider._size;
        
        slider.Initialize();
        
        //---
        
        //Rebind buttons is needed to fix navigating sortof... Needs to be better handled I think
        //self.BindButtons(); //THIS DOESN'T WORK! BREAKS TOO MANY SELECTIONS. FIXING UP TOP
    }


    private void JollySlidingMenu_Singal(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_Singal orig, JollySlidingMenu self, MenuObject sender, string message) {
        orig(self, sender, message);
        
        var plyCnt = MyriadMod.PlyCnt();
        
        if(plyCnt <= 4) return;
        
        if (message.Contains("JOLLYSWAP")) {
            for (int i = 0; i < self.playerSelector.Length; ++i){
                if (message == "JOLLYSWAP" + i.ToString()) {
                    JollyPlayerOptions jOptA = Custom.rainWorld.options.jollyPlayerOptionsArray[i];
                    JollyPlayerOptions jOptB = Custom.rainWorld.options.jollyPlayerOptionsArray[i - 1];
                    Custom.rainWorld.options.jollyPlayerOptionsArray[i] = jOptB;
                    Custom.rainWorld.options.jollyPlayerOptionsArray[i - 1] = jOptA;
					bool swapPup = self.JollyOptions(i).isPup != self.JollyOptions(i-1).isPup;
					
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