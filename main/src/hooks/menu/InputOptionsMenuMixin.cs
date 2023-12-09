using HUD;
using Menu;
using Myriad.utils;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Myriad.hooks.menu; 

[Mixin(typeof(InputOptionsMenu))]
public class InputOptionsMenuMixin {
    public static InputOptionsMenuMixin INSTANCE = new InputOptionsMenuMixin();

    public void init() {
        On.Menu.InputOptionsMenu.ctor += addMorePlayerOptions;
    }
    
    public static void addMorePlayerOptions(On.Menu.InputOptionsMenu.orig_ctor orig, InputOptionsMenu self, ProcessManager manager) {
        orig(self, manager);
        
        var plyCnt = MyriadMod.PlyCnt();

        if (plyCnt <= 4) { 
            if (ModManager.JollyCoop) {
                //JOLLYCOOP BONUS; SHOW THE CURRENT JOLLY PLAYER NAMEPLATES
                for (int index = self.playerButtons.Length - 1; index >= 0; --index) {
                    string nameTag = JollyCoop.JollyCustom.GetPlayerName(index);
                    if (self.playerButtons[index].menuLabel.text != nameTag)
                        self.playerButtons[index].menuLabel.text += " - " + nameTag;
                }
            }
            return; 
        }

        Vector2 vector2_1 = new Vector2(0.0f, -30f);

        //---

        var newDeviceButton = new InputOptionsMenu.DeviceButton[plyCnt+2];

        self.deviceButtons.CopyTo(newDeviceButton, 0);
        
        self.deviceButtons = newDeviceButton;
        
        //---

        var inputTesterIndex = self.pages[0].subObjects.IndexOf(self.inputTesterHolder);
        
        var buttonOffset = 60.0;
        var deviceBtnHeight = 680.0; //620

        if (plyCnt > 8) {
            deviceBtnHeight = 740;
            buttonOffset /= (plyCnt / 11.0f); //8.5
        }


        for (int index = 0; index < self.deviceButtons.Length; ++index) {
            InputOptionsMenu.DeviceButton deviceButton; 
            
            if(index > 5){
                string str = self.inputDevicedTexts[Math.Min(index, self.inputDevicedTexts.Length - 1)];
                
                if (index > 1) str = Regex.Replace(str, "<X>", (index - 1).ToString());
                
                if (index == 0 && (self.CurrLang != InGameTranslator.LanguageID.English)) str = InGameTranslator.EvenSplit(str, 1);
                
                deviceButton = new InputOptionsMenu.DeviceButton(self, self.pages[0], new Vector2(450f, (float) (deviceBtnHeight - (double) index * buttonOffset)) + vector2_1, str, self.deviceButtons, index);

                self.deviceButtons[index] = deviceButton;
                
                self.pages[0].subObjects.Insert(inputTesterIndex, self.deviceButtons[index]);
            } else {
                deviceButton = self.deviceButtons[index];

                deviceButton.pos = new Vector2(450f, (float)(deviceBtnHeight - (double)index * buttonOffset)) + vector2_1;
                deviceButton.buttonArray = self.deviceButtons;
            }
            
            foreach (var deviceButtonSubObject in deviceButton.subObjects) {
                if (!(deviceButtonSubObject is RectangularMenuObject rectMenuObject)) return;
                
                rectMenuObject.size /= 2;
                rectMenuObject.lastSize /= 2;
            }
            
            deviceButton.size /= 2;
            deviceButton.lastSize /= 2;
            
            deviceButton.menuLabel.pos.y += 30f;

            if(deviceButton.deviceImage != null){
                var padding = 16;
                
                deviceButton.RemoveSubObject(deviceButton.deviceImage);

                deviceButton.deviceImage = new MenuIllustration(self, deviceButton, "", index == 1 ? "KeyboardIcon" : "GamepadIcon", deviceButton.size / 2f, true, true) ;

                deviceButton.subObjects.Add(deviceButton.deviceImage);

                deviceButton.deviceImage.sprite.scale = 0.5f;
            }
            
            if (deviceButton.numberImage != null) {
                deviceButton.Container.RemoveChild(deviceButton.darkFade);
                
                deviceButton.darkFade = new FSprite("Futile_White");
                
                deviceButton.darkFade.shader = self.manager.rainWorld.Shaders["FlatLight"];
                
                deviceButton.darkFade.color = new Color(0.0f, 0.0f, 0.0f);
                
                deviceButton.Container.AddChild(deviceButton.darkFade);
                
                //----
                
                deviceButton.RemoveSubObject(deviceButton.numberImage);

                //var rollOverNum = (index > 5 ? (index % 4 - 1) : index - 1); //This is just to stop errors! Should be replaced with text or something?
                
                string numImg = index == 0 ? "GamepadAny" : "Gamepad10"; //THE GAME DOESN'T SEEM TO KNOW WHAT TO DO WITH THIS AND SO IT SHOWS NO NUMBER. PERFECT
                //index == 0 ? "GamepadAny" : "Gamepad" + (index - 1)
                deviceButton.numberImage = new MenuIllustration(self, deviceButton, "", numImg, deviceButton.size / 2f, true, true);
                
                deviceButton.subObjects.Add(deviceButton.numberImage);
                
                deviceButton.numberImage.sprite.scale = 0.5f;
            }
        }
        
        //---

        var newPlayerButtons = new InputOptionsMenu.PlayerButton[plyCnt];
        
        self.playerButtons.CopyTo(newPlayerButtons, 0);

        self.playerButtons = newPlayerButtons;
        
        //----

        self.rememberPlayersSignedIn = new bool[self.playerButtons.Length];
        
        var perInputOffset = 76f;//143.3333282470703;

        var initalYOffset = 665;

        var xOffset = 32;

        if (plyCnt > 8) {
            initalYOffset = 740;
            perInputOffset /= (plyCnt / 9.8f);
            self.backButton.pos -= new Vector2(85, 0); //COME ON IT LOOKS BETTER THIS WAY...
        }

        for (int index = self.playerButtons.Length - 1; index >= 0; --index) {
            InputOptionsMenu.PlayerButton playerButton;
            
            if (index < 4) {
                playerButton = self.playerButtons[index];
                
                playerButton.pos = new Vector2(200f + xOffset, (float)(initalYOffset - (double)index * perInputOffset)) + vector2_1;
                
                playerButton.originalPos = new Vector2(Mathf.Floor(playerButton.pos.x) + 0.01f, Mathf.Floor(playerButton.pos.y) + 0.01f);
                
                playerButton.buttonArray = self.playerButtons;
            } else {
                playerButton = new InputOptionsMenu.PlayerButton(self, self.pages[0], new Vector2(200f + xOffset, (float) (initalYOffset - (double) index * perInputOffset)) + vector2_1, self.playerButtons, index);

                self.playerButtons[index] = playerButton;
                
                self.pages[0].subObjects.Add(self.playerButtons[index]);
                
                self.rememberPlayersSignedIn[index] = manager.rainWorld.IsPlayerActive(index);
            }
            
            foreach (var playerButtonSubObject in playerButton.subObjects) {
                if (!(playerButtonSubObject is RectangularMenuObject rectMenuObject)) return;
                
                rectMenuObject.size /= 2;
                rectMenuObject.lastSize /= 2;
            }

            playerButton.menuLabel.pos.y += 30f;

            var padding = 16;
            
            playerButton.RemoveSubObject(playerButton.portrait);

            var portraintIndex = index % 4;
            
            playerButton.portrait = new MenuIllustration(self, playerButton, "", $"MultiplayerPortrait{portraintIndex}1", playerButton.size / 4f, true, true);

            playerButton.subObjects.Add(playerButton.portrait);

            playerButton.portrait.sprite.scale = 0.5f;

            playerButton.size /= 2;
            playerButton.lastSize /= 2;

            //INCREASE VISIBILITY
            playerButton.menuLabel.pos.y += 20;
            playerButton.menuLabel.label.MoveToFront();
            
            if (ModManager.JollyCoop) //FINISHING TOUCH TO HELP WITH SETUP... SHOW THE CURRENT JOLLY PLAYER NAMEPLATES (WHICH DEFAULT TO PLAYER NUMBERS ANYWAYS)
                playerButton.menuLabel.text = JollyCoop.JollyCustom.GetPlayerName(index); //(playerButton.index + 1).ToString() + " " + 
        }

        for (int playerNumber = 4; playerNumber < manager.rainWorld.options.controls.Length; ++playerNumber) {
            manager.rainWorld.RequestPlayerSignIn(playerNumber, null);
        }

        for (int index = 4; index < self.playerButtons.Length; ++index) {
            self.playerButtons[index].pointPos = self.playerButtons[index].IdealPointHeight();
            self.playerButtons[index].lastPointPos = self.playerButtons[index].pointPos;
        }
        
        //-----

        foreach (var inputTester in self.inputTesterHolder.testers) self.inputTesterHolder.subObjects.Remove(inputTester);

        self.inputTesterHolder.testers = new InputTesterHolder.InputTester[(self.inputTesterHolder.menu as InputOptionsMenu).playerButtons.Length];
        
        for (int playerIndex = 0; playerIndex < self.inputTesterHolder.testers.Length; ++playerIndex) {
            self.inputTesterHolder.testers[playerIndex] = new InputTesterHolder.InputTester(self.inputTesterHolder.menu, self.inputTesterHolder, playerIndex);
            self.inputTesterHolder.subObjects.Add((MenuObject) self.inputTesterHolder.testers[playerIndex]);
        }
        
        //-----
        
        self.inputTesterHolder.Initiate();

        self.UpdateConnectedControllerLabels();
        
        for (int index = 0; index < self.gamePadButtonButtons.Length; ++index) self.gamePadButtonButtons[index].nextSelectable[2] = self.playerButtons[index < self.gamePadButtonButtons.Length / 2 ? 0 : 1];
    }
}