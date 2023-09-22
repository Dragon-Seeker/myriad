using Menu;
using UnityEngine;

namespace Myriad.hooks.menu; 

public class InputTesterHolderMixin {
    public static InputTesterHolderMixin INTANCE = new InputTesterHolderMixin();

    public void init() {
        On.Menu.InputTesterHolder.InputTester.ctor += InputTester_ctor;
        On.Menu.InputTesterHolder.InputTester.GetToPos += InputTester_GetToPos;
        On.Menu.InputTesterHolder.InputTester.TestButton.ctor += TestButton_ctor;
        On.Menu.InputTesterHolder.Back.Update += Back_Update;
    }
    
    private Vector2 InputTester_GetToPos(On.Menu.InputTesterHolder.InputTester.orig_GetToPos orig, InputTesterHolder.InputTester self) {
        Vector2 result = orig(self);
        
        if (MyriadMod.PlyCnt() > 8 && self.playerIndex % 2 != 0) result -= new Vector2(60, 0);
        
        return result;
    }

    public void Back_Update(On.Menu.InputTesterHolder.Back.orig_Update orig, InputTesterHolder.Back self) {
        orig(self);
        
        if (MyriadMod.PlyCnt() > 8) {
            //self.textLabel.pos.y -= 60f;
            //self.textLabel.pos.x += 55f;
            self.textLabel.pos += new Vector2(-280, 90);
        }
    }

    public void InputTester_ctor(On.Menu.InputTesterHolder.InputTester.orig_ctor orig, InputTesterHolder.InputTester self, Menu.Menu menu, MenuObject owner, int playerIndex) {
        // self.rad = 15; //TOO EARLY! IT DIDN'T WORK
        orig(self, menu, owner, playerIndex);
        
		self.rad = 22; //THIS SHOULD WORK NOW. AND I BELEIVE GRAFUPDATE() SHOULD HANDLE THE REST
		self.crossSpriteH.scaleX = self.rad * 2f;
		self.crossSpriteV.scaleY = self.rad * 2f;
        self.centerKnobSprite.scale = 0.7f;
        //BASELINE y = -15

        AdjustTestButton(self, 4, 210, -20); //Pickup/Eat
        AdjustTestButton(self, 5, 0, 10); //Jump
        AdjustTestButton(self, 6, 0, 15); //Throw
        AdjustTestButton(self, 7, 210, -5); //Pause

        //THE SYMBOLS DONT MOVE! >:(
        //AdjustTestButton(self, 0, 112 - 15, -0); //left
        //AdjustTestButton(self, 1, 56, -56 + 15); //up
        //AdjustTestButton(self, 2, 15, 0); //right
        //AdjustTestButton(self, 3, 56, 56 + 0); //down
    }

    private void TestButton_ctor(On.Menu.InputTesterHolder.InputTester.TestButton.orig_ctor orig, InputTesterHolder.InputTester.TestButton self, Menu.Menu menu, MenuObject owner, Vector2 pos, string symbolName, int symbolRotat, string labelText, int buttonIndex, int playerIndex) {
        if (symbolName != null && symbolName == "Menu_Symbol_Arrow") {
            pos *= 0.4f; //ARROWS ARE SLIPPERY . SHRINK THEM BEFOREHAND
            
            if (symbolRotat == 2) pos.y = 0; //DOWN ARROW
            
            //THEN SHIFT THEM ALL OVER
            pos += new Vector2(60, -15);
        }
        
        //YOU KNOW WHAT, THEY'RE ALL LOW! MOVE THEM ALL UP
        pos += new Vector2(0, 5);

        orig(self, menu, owner, pos, symbolName, symbolRotat, labelText, buttonIndex, playerIndex);
    }
    
    public static void AdjustTestButton(InputTesterHolder.InputTester self, int index, float xShift, float yShift) {
        self.testButtons[index].roundedRect.pos += new Vector2(xShift, yShift);
        
        if (self.testButtons[index].symbolSprite != null) {
            self.testButtons[index].symbolSprite.x += xShift;
            self.testButtons[index].symbolSprite.y += yShift;
            self.testButtons[index].symbolSprite.SetPosition(self.testButtons[index].symbolSprite.x, self.testButtons[index].symbolSprite.y);
            Debug.Log("SYMBOL SPRITE!");
        } else if (self.testButtons[index].extraRect != null) {
            self.testButtons[index].extraRect.pos += new Vector2(xShift, yShift);
        }

        if (self.testButtons[index].labelText != null) {
            self.testButtons[index].menuLabel.pos += new Vector2(xShift, yShift);
        }
        
    }
}