using System;
using BepInEx;
using UnityEngine;
using Menu.Remix.MixedUI;
using RWCustom;
using JollyCoop;
using Rewired;
using Kittehface.Framework20;
using Steamworks;
using RewiredConsts;
using Rewired.Dev;
using JollyCoop.JollyMenu;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using MoreSlugcats;

using System.Security;
using System.Security.Permissions;
using System.Reflection;


using Rewired.Data.Mapping;
using Rewired.Utils;
using Rewired.Utils.Classes;
using Rewired.Utils.Classes.Data;
using static System.Runtime.CompilerServices.RuntimeHelpers;

/*
NOTE! SOME MODS MAY NOT BE BUILT TO HANDLE MORE THAN 4 PLAYERS! (Especially custom slugcats or game mechanics)
If you experience an issue, please include a list of mods 

this.enteringShortCut = new IntVector2?(tilePosition);
*/

// BPOptions._SaveConfigFile(); I GUESS?

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

// [BepInPlugin("me.moreplayers", "More Players", "0.1")]

public class MorePlayers
{
    // public static MorePlayers instance;
    public static new ManualLogSource Logger;
	// public static MPOptions myOptions;

    public static int plyCnt = 8;
	
	public static int PlyCnt()
	{
		return plyCnt;
	}

	
	
    public void OnEnable()
    {
	    Logger = BepInEx.Logging.Logger.CreateLogSource("MorePlayers");
	    
        try {
	        // On.RainWorld.OnModsInit += RainWorld_OnModsInit;

            // On.Options.ToString += Options_ToString;
            
            // On.JollyCoop.JollyMenu.JollySlidingMenu.NumberPlayersChange += MPJollySlidingMenu_NumberPlayersChange1;
            On.Menu.MenuIllustration.ctor += MenuIllustration_ctor;
            //On.Menu.InputOptionsMenu.ctor += MPInputOptionsMenu_ctor; //INPUT MENU
            //On.Menu.InputOptionsMenu.PlayerButton.ctor += PlayerButton_ctor; //INPUT MENU


            IL.Options.ctor += Options_ctor; 									//3
            IL.JollyCoop.JollyMenu.JollySlidingMenu.ctor += Replace4WithMore;   //3
            IL.StoryGameSession.CreateJollySlugStats += Replace4WithMore;       //1
            IL.PlayerGraphics.PopulateJollyColorArray += Replace4WithMore;      //1
            IL.RoomSpecificScript.SU_C04StartUp.ctor += Replace4WithMore;       //2
            IL.ArenaSetup.ctor += Replace4WithMore;                             //2
            //IL.Menu.InputOptionsMenu.ctor += Options_ctor;                      //1  //INPUT MENU
            IL.Menu.MultiplayerMenu.InitiateGameTypeSpecificButtons += Options_ctor; //SHOULD GRAB 2
			IL.Options.ControlSetup.SaveAllControllerUserdata += Replace4WithMore;  //1
			IL.RainWorld.JoystickConnected += Replace4WithMore; //5-19
			IL.RainWorld.JoystickPreDisconnect += Replace4WithMore;
			//THIS DIDN'T HAVE AN IL AND IDK EXACTLY WHAT IT DOES BUT IT LOOKS LIKE IT SHOULD REPLACE 4 WITH MORE
			IL.RWInput.PlayerUIInput += Replace4WithMore;

            RainWorld.PlayerObjectBodyColors = new Color[plyCnt];

            //On.Options.FromString += Options_FromString;
            // On.Options.ApplyOption += Options_ApplyOption;
            On.Options.ControlSetup.UpdateControlPreference += ControlSetup_UpdateControlPreference;
            // On.Menu.IntroRoll.Update += IntroRoll_Update; //THIS IS WHERE THINGS FIRST BREAK. -WE TRIED TO ADD REWIRED.PLAYER HERE, BUT ALAS...

            On.RainWorldGame.JollySpawnPlayers += RainWorldGame_JollySpawnPlayers;
            //On.JollyCoop.JollyMenu.JollyPlayerSelector.Update += JollyPlayerSelector_Update;
            //LOCK DOWNPOUR STORY CHARACTER SELECTION FOR PLAYER 1
			
            // On.JollyCoop.JollyCustom.ForceActivateWithMSC += JollyCustom_ForceActivateWithMSC; //FOR JOLLYCAMPAINGN. JUST RETURN TRUE INSTEAD OF ORIG.
            // On.Options.ToStringNonSynced += Options_ToStringNonSynced;
            // On.Options.FromUnsyncedString += Options_FromUnsyncedString; //DISABLING CUZ MPOPTIONS ISN'T LOADED YET


            //ADJUST MENU LAYOUT
            // On.JollyCoop.JollyMenu.JollySlidingMenu.ctor += JollySlidingMenu_ctor;
            // On.Menu.MultiplayerMenu.InitiateGameTypeSpecificButtons += MultiplayerMenu_InitiateGameTypeSpecificButtons;

			//THINGS THAT WOULD OTHERWISE RANDOMLY CRASH
            // On.RWInput.PlayerInputLogic += RWInput_PlayerInputLogic;
            // On.RWInput.CheckPauseButton += RWInput_CheckPauseButton;
            // On.RWInput.CheckPauseButton += RWInput_CheckPauseButton1;

            // On.Menu.InputOptionsMenu.Update += InputOptionsMenu_Update;
            // On.Menu.InputOptionsMenu.InputSelectButton.ButtonText += InputSelectButton_ButtonText; //INPUT MENU
            // On.Menu.InputOptionsMenu.SetCurrentlySelectedOfSeries += InputOptionsMenu_SetCurrentlySelectedOfSeries;

            //AFTER SWITCHING BACK TO MSOPTIONS WAY
            // On.Options.ControlSetup.GetButton += ControlSetup_GetButton;
            On.PlayerGraphics.SlugcatColor += PlayerGraphics_SlugcatColor;
            On.Menu.MultiplayerMenu.ArenaImage += MultiplayerMenu_ArenaImage;
        }
        catch (Exception arg)
        {
            Logger.LogError(string.Format("Failed to initialize", arg));
            throw;
        }
    }


    //FOR ARENA IMAGES WHILE MSC IS ENABLED
    private string MultiplayerMenu_ArenaImage(On.Menu.MultiplayerMenu.orig_ArenaImage orig, MultiplayerMenu self, SlugcatStats.Name classID, int color)
    {
        //DON'T LET OUR IMAGE NAME GO OUT OF BOUNDS!
        while (color > 3)
        {
            color = color - 4;
        }
        return orig(self, classID, color);
    }

	

    private Color PlayerGraphics_SlugcatColor(On.PlayerGraphics.orig_SlugcatColor orig, SlugcatStats.Name i)
    {
        //Logger.LogInfo("SLUGCAT COLOR pt1 " + " - " + i);
        Color result = orig(i);
        //Logger.LogInfo("SLUGCAT COLOR pt2 " + result + " - " + i);

        if (i == null)
            return result;

        string source = i.ToString();
        int pNum = 0;
        if (source.Contains("JollyPlayer"))
        {
            string split = "JollyPlayer";
            pNum = int.Parse(source.Substring(source.IndexOf(split) + split.Length));
        }

        


        if (pNum > 4)
        {
            //IT'S A BONUS CAT! AND THE GAME IS BAD AT HANDLING US. SO WE GOTTA FIX IT OURSELVES...
            if (ModManager.CoopAvailable)
            {
                if ((Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO && (pNum - 1) > 0) || Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM)
                {
                    return PlayerGraphics.JollyColor(pNum - 1, 0);
                }
                //WHA?? WHAT IS THIS DOING HERE? WHAT DOES IT DO?.. WHATEVER I GUESS I'M KEEPING IT
                i = (Custom.rainWorld.options.jollyPlayerOptionsArray[pNum - 1].playerClass ?? i);
            }
            if (PlayerGraphics.CustomColorsEnabled())
            {
                return PlayerGraphics.CustomColorSafety(0);
            }
            //Debug.Log("DETERMINING CUSTOM COLOR FOR: " + i + " - P" + pNum);
            return PlayerGraphics.DefaultSlugcatColor(i);
        }
        //i
        return result;
    }



    
    // private bool ControlSetup_GetButton(On.Options.ControlSetup.orig_GetButton orig, Options.ControlSetup self, int actionID)
    // {
    //     
    //     //THIS LIKE CAUSED THE GAME TO FREEZE FOR SOME REASON
    //     if (self.active && self.index > 3)
    //     {
    //         //RETURN TRUE IF HELD ACTION KEY
    //         int plr = self.index;
    //         switch (actionID)
    //         {
    //             case 0:
    //                 return false;
    //             case 1:
    //                 return Input.GetKey(MPOptions.mapKey[plr - 4].Value);
    //             case 2:
    //                 return Input.GetKey(MPOptions.grabKey[plr - 4].Value);
    //             case 3:
    //                 return Input.GetKey(MPOptions.jumpKey[plr - 4].Value);
    //             case 4:
    //                 return Input.GetKey(MPOptions.throwKey[plr - 4].Value);
    //             case 5:
    //                 return Input.GetKey(MPOptions.leftKey[plr - 4].Value);
    //             case 6:
    //                 return Input.GetKey(MPOptions.upKey[plr - 4].Value);
    //             case 7:
    //                 return Input.GetKey(MPOptions.rightKey[plr - 4].Value);
    //             case 8:
    //                 return Input.GetKey(MPOptions.downKey[plr - 4].Value);
    //             case 11:
    //                 return Input.GetKey(MPOptions.mapKey[plr - 4].Value); //WAIT, WHY IS THIS ONE ALSO MAP?? THIS MAKES NO SENSE. MAYBE THE REST ARE WRONG...
    //         }
    //         Debug.Log("INVALID BUTTONDOWN DETECTED " + actionID + " - P" + plr);
    //         return false;
    //     }
    //     return orig(self, actionID);
    // }



    //TO ACTUALLY SET OUR "ISUSINGKEYBOARD" MPOPTIONS VALUES, SINCE WE HAVENT DONE THAT FOR A WHILE
    // private void InputOptionsMenu_SetCurrentlySelectedOfSeries(On.Menu.InputOptionsMenu.orig_SetCurrentlySelectedOfSeries orig, InputOptionsMenu self, string series, int to)
    // {
    //     Debug.Log("SETTING INPUT OPTIONS FOR PLAYER: " + self.manager.rainWorld.options.playerToSetInputFor + " - " + to);
    //
    //     int player = self.manager.rainWorld.options.playerToSetInputFor;
    //     if (series == "DeviceButtons" && player > 3)
    //     {
    //         if (to == 1)
    //         {
    //             //self.CurrentControlSetup.UpdateControlPreference(Options.ControlSetup.ControlToUse.KEYBOARD, false); //FREE
    //             MPOptions.usingKeyboard[player - 4].Value = true;
    //         }
    //         else if (to == 0)
    //         {
    //             //self.CurrentControlSetup.gamePadNumber = 0;
    //             //self.CurrentControlSetup.UpdateControlPreference(Options.ControlSetup.ControlToUse.ANY, false); //FREE
    //             //THEY BETTER NOT!!
    //         }
    //         else
    //         {
    //             //self.CurrentControlSetup.gamePadGuid = null;
    //             //self.CurrentControlSetup.gamePadNumber = to - 2; //I THINK THEY CAN ACTUALLY STILL DO THIS AND IT STILL GOES TO THE CORRECT USER
    //             //self.CurrentControlSetup.UpdateControlPreference(Options.ControlSetup.ControlToUse.SPECIFIC_GAMEPAD, false); //FREE
    //             MPOptions.usingKeyboard[player - 4].Value = true;
    //             MPOptions.gamepadNum[player - 4].Value = (to - 2);
    //         }
    //         Debug.Log("SETTING INPUT OPTIONS FOR PLAYER 2: " + self.manager.rainWorld.options.playerToSetInputFor + " - " + MPOptions.usingKeyboard[player - 4].Value + " - " + MPOptions.gamepadNum[player - 4].Value);
    //     }
    //
    //     orig(self, series, to);
    // }



    //WHY DO P5 AND 6 STILL LOAD POINTING AT CONTROLLER SLOT 3&4? COULD IT BE THAT THE DATA READ FROM Options_ToString  IS SETTING THEM THIS WAY?
    //TAKE A LOOK AT THE OPTIONS TEXT FILES AND SEE WHAT IS BEING SAVED FOR THOSE VALUES

    //DO CONTROLLERS EVEN WORK?

    //IT STAYS CONNECTED TO THE CORRECT INPUT TYPE UNTIL YOU SELECT A NEW KEY WITH A CONTROLLER
    // private string InputSelectButton_ButtonText(On.Menu.InputOptionsMenu.InputSelectButton.orig_ButtonText orig, Menu.Menu menu, bool gamePadBool, int player, int button, bool inputTesterDisplay)
    // {
    //     string result = orig(menu, gamePadBool, player, button, inputTesterDisplay);
    //     if (player > 3)
    //     {
    //         
    //         ActionElementMap actionElementMap = null;
    //         for (int i = 0; i < (menu as InputOptionsMenu).inputActions[button].Length; i++)
    //         {
    //             //Debug.Log("INPUT BUTTON TEXT: " + (menu as InputOptionsMenu).inputActions[button][i] + " - " + button + " - " + (menu as InputOptionsMenu).inputActions[i][0]);
    //             int inputType = 0; //OKAY THIS DOESN'T WORK
    //             if (MPOptions.usingKeyboard[player - 4].Value == false)
    //                 inputType = 1;
    //
    //             inputType = i;
    //
    //             Debug.Log("INPUT TYPE: " + MPOptions.usingKeyboard[player - 4].Value);
    //             Debug.Log("INPUT BUTTON TEXT: " + (menu as InputOptionsMenu).inputActions[button][inputType] + " - " + (menu as InputOptionsMenu).inputActionCategories[button][inputType] + " - " + (menu as InputOptionsMenu).inputAxesPositive[button]);
    //
    //
    //             result = "????";
    //             switch ((menu as InputOptionsMenu).inputActions[button][inputType])
    //             {
    //                 case 0:
    //                     result = MPOptions.jumpKey[player - 4].Value.ToString();
    //                     break;
    //                 case 1:
    //                     result = ((menu as InputOptionsMenu).inputAxesPositive[button]) ? MPOptions.rightKey[player - 4].Value.ToString() : MPOptions.leftKey[player - 4].Value.ToString();
    //                     break;
    //                 case 2:
    //                     result = ((menu as InputOptionsMenu).inputAxesPositive[button]) ? MPOptions.upKey[player - 4].Value.ToString() : MPOptions.downKey[player - 4].Value.ToString();
    //                     break;
    //                 case 3:
    //                     result = MPOptions.grabKey[player - 4].Value.ToString();
    //                     break;
    //                 case 4:
    //                     result = MPOptions.throwKey[player - 4].Value.ToString();
    //                     break;
    //                 case 5:
    //                     result = "_"; //FORGET IT LOL
    //                     break;
    //                 case 6:
    //                     result = "_A"; 
    //                     break;
    //                 case 7:
    //                     result = "_B";
    //                     break;
    //                 case 8:
    //                     result = "_C";
    //                     break;
    //                 case 9:
    //                     result = "_D";
    //                     break;
    //                 case 10:
    //                     result = "_E";
    //                     break;
    //                 case 11:
    //                     result = MPOptions.mapKey[player - 4].Value.ToString();
    //                     break;
    //             }
    //             //MPOptions.usingKeyboard[k - 4].Value
    //             //return (menu as InputOptionsMenu).inputActions[button][0].ToString() + " - " + (menu as InputOptionsMenu).inputAxesPositive[button].ToString();
    //             //Debug.Log("INPUT BUTTON TEXT: " + (menu as InputOptionsMenu).inputActions[button][0] + " - " + (menu as InputOptionsMenu).inputActionCategories[button][0] + " - " + (menu as InputOptionsMenu).inputAxesPositive[button]);
    //             return result;
    //             // return (menu as InputOptionsMenu).inputActions[button][inputType] + " - " + (menu as InputOptionsMenu).inputActionCategories[button][inputType] + " - " + (menu as InputOptionsMenu).inputAxesPositive[button];
    //         }
    //
    //         if (actionElementMap == null)
    //         {
    //             return "BORKED";
    //         }
    //         return actionElementMap.elementIdentifierName;
    //     }
    //     return result;
    // }

    // private void InputOptionsMenu_Update(On.Menu.InputOptionsMenu.orig_Update orig, InputOptionsMenu self)
    // {
    //     bool flag = true;
    //     for (int i = 0; i < self.inputMappers.Length; i++)
    //     {
    //         if (self.inputMappers[i].status != InputMapper.Status.Idle)
    //         {
    //             flag = false;
    //         }
    //     }
    //
    //     if (self.mappersStarted && flag)
    //     {
    //         Debug.Log("BLEH: ");
    //         if (self.selectedObject is InputOptionsMenu.InputSelectButton) //IF WE'VE SELECTED AN INPUT BUTTON
    //         {
    //             
    //             int plr = self.manager.rainWorld.options.playerToSetInputFor;
    //             int action = (self.selectedObject as InputOptionsMenu.InputSelectButton).index; //THIS DETERMINES WHAT ACTION IT WAS FOR
    //
    //             //CHECK EVERY SINGLE INPUT BTN AND SEE IF ANY OF THEM ARE HELD DOWN.
    //             foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)).OfType<KeyCode>())
    //             {
    //                 if (Input.GetKey(keyCode)) // || CustomInputExt.ResolveButtonDown(keyCode, ctrl, self.CurrentControlSetup.GetActivePreset())) {
    //                 {
    //                     Debug.Log("CUSTOM KEYBIND DETECTED: " + action + " PLR: " + plr);
    //
    //                     switch (action)
    //                     {
    //                         case 0:
    //                             //MPOptions.mapKey[plr - 4].Value = keyCode;
    //                             break;
    //                         case 1:
    //                             MPOptions.mapKey[plr - 4].Value = keyCode;
    //                             break;
    //                         case 2:
    //                             MPOptions.grabKey[plr - 4].Value = keyCode;
    //                             break;
    //                         case 3:
    //                             MPOptions.jumpKey[plr - 4].Value = keyCode;
    //                             break;
    //                         case 4:
    //                             MPOptions.throwKey[plr - 4].Value = keyCode;
    //                             break;
    //                         case 5:
    //                             MPOptions.leftKey[plr - 4].Value = keyCode;
    //                             break;
    //                         case 6:
    //                             MPOptions.upKey[plr - 4].Value = keyCode;
    //                             break;
    //                         case 7:
    //                             MPOptions.rightKey[plr - 4].Value = keyCode;
    //                             break;
    //                         case 8:
    //                             MPOptions.downKey[plr - 4].Value = keyCode;
    //                             break;
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //
    //     orig(self);
    // }

    // private bool RWInput_CheckPauseButton1(On.RWInput.orig_CheckPauseButton orig, int playerNumber, RainWorld rainWorld)
    // {
    //     if (playerNumber > 4)
    //         return false;
    //     return orig(playerNumber, rainWorld);
    // }
    //
    // private bool RWInput_CheckPauseButton(On.RWInput.orig_CheckPauseButton orig, int playerNumber, RainWorld rainWorld)
    // {
    //     if (playerNumber > 4)
    //         return false;
    //     return orig(playerNumber, rainWorld);
    // }

   //  private Player.InputPackage RWInput_PlayerInputLogic(On.RWInput.orig_PlayerInputLogic orig, int categoryID, int playerNumber, RainWorld rainWorld)
   //  {
   //      if (rainWorld.options.controls[playerNumber].player == null)
   //      {
   //          Debug.Log("NULLED PLAYER INFO! ");
   //          //rainWorld.options.controls[playerNumber] = new Options.ControlSetup(playerNumber, false);
			// //FILL THIS IN WITH DUMMY INFO COPIED FROM THE PREVIOUS PLAYER, JUST SO THAT THE GAME DOESN'T CRASH. WE'LL BE IGNORING MOST OF THAT INFO PAST PLAYER 4 ANYWAYS
   //          rainWorld.options.controls[playerNumber].player = rainWorld.options.controls[playerNumber - 1].player;
   //          Debug.Log("NULLED PLAYER INFO COMPLETE! " + rainWorld.options.controls[playerNumber]);
   //
   //          //HEY DON'T FORGET THE OTHER PARTS OF THEIR CONTROL PACKAGE!
			// //intG
   //          //string myGamePad = Math.Max(int.Parse(MPOptions.gamePadType[playerNumber - 4].Value) - 1, 0).ToString();
   //          string myGamePad = MPOptions.gamePadType[playerNumber - 4].Value;
   //          rainWorld.options.controls[playerNumber].gamePadNumber = MPOptions.GetGamepadInt2(myGamePad);
   //          rainWorld.options.controls[playerNumber].usingGamePadNumber = rainWorld.options.controls[playerNumber].gamePadNumber;
   //          rainWorld.options.controls[playerNumber].controlPreference = (myGamePad == "Keyboard") ? Options.ControlSetup.ControlToUse.KEYBOARD : Options.ControlSetup.ControlToUse.SPECIFIC_GAMEPAD;
   //          //rainWorld.options.controls[playerNumber].UpdateControlPreference((myGamePad == "Keyboard") ? Options.ControlSetup.ControlToUse.KEYBOARD : Options.ControlSetup.ControlToUse.SPECIFIC_GAMEPAD, false);
   //
   //
   //          //WE CAN'T RELY ON THIS TO CATCH PLAYER 5 BECAUSE THEY HAVE A SLOT ALREADY!
   //          //OKAY THIS IS KIND OF SILLY BUT SINCE WE DON'T WANT THIS TO RUN EVERY TICK, LET'S JUST RUN IT 3 TIMES. 
   //          //string p5GamePad = Math.Max(int.Parse(MPOptions.gamePadType[0].Value) - 1, 0).ToString();
   //          string p5GamePad = MPOptions.gamePadType[0].Value;
   //          rainWorld.options.controls[4].gamePadNumber = MPOptions.GetGamepadInt2(p5GamePad);
   //          rainWorld.options.controls[4].usingGamePadNumber = rainWorld.options.controls[4].gamePadNumber;
   //          rainWorld.options.controls[4].controlPreference = (p5GamePad == "Keyboard") ? Options.ControlSetup.ControlToUse.KEYBOARD : Options.ControlSetup.ControlToUse.SPECIFIC_GAMEPAD;
   //      }
   //
   //
   //      try
   //      {
   //          //Debug.Log("NULLED PLAYER INFO! " + playerNumber + " - " + rainWorld.options.controls[playerNumber].gamePadNumber);
   //          //bool thing = Input.legacy.Input.GetKey(KeyCode.UpArrow); //options.controls[playerNumber].KeyboardLeft
   //          //OKAY WE GOTTA CATCH IT AND REPLACE IT
   //          if (playerNumber > 3)
   //          {
   //              Player.InputPackage inputPackage = new Player.InputPackage();
   //              int plr = playerNumber;
			// 	
   //              if (categoryID == 0)
   //              {
   //                  if (Input.GetKey(MPOptions.leftKey[plr - 4].Value))
   //                  {
   //                      inputPackage.x--;
   //                  }
   //                  if (Input.GetKey(MPOptions.rightKey[plr - 4].Value))
   //                  {
   //                      inputPackage.x++;
   //                  }
   //                  if (Input.GetKey(MPOptions.downKey[plr - 4].Value))
   //                  {
   //                      inputPackage.y--;
   //                  }
   //                  if (Input.GetKey(MPOptions.upKey[plr - 4].Value))
   //                  {
   //                      inputPackage.y++;
   //                  }
   //                  if (inputPackage.y < 0)
   //                  {
   //                      inputPackage.downDiagonal = inputPackage.x;
   //                  }
   //                  if (Input.GetKey(MPOptions.jumpKey[plr - 4].Value))
   //                  {
   //                      inputPackage.jmp = true;
   //                  }
   //                  if (Input.GetKey(MPOptions.throwKey[plr - 4].Value))
   //                  {
   //                      inputPackage.thrw = true;
   //                  }
   //                  if (Input.GetKey(MPOptions.mapKey[plr - 4].Value))
   //                  {
   //                      inputPackage.mp = true;
   //                  }
   //                  if (Input.GetKey(MPOptions.grabKey[plr - 4].Value))
   //                  {
   //                      inputPackage.pckp = true;
   //                  }
			// 		//WAIT! I THINK I FINALLY GOT IT...
			// 		if (MPOptions.gamePadType[playerNumber - 4].Value == "Keyboard")
			// 		{
			// 			inputPackage.analogueDir = new Vector2(inputPackage.x, inputPackage.y); //UHH... IS THIS GOOD ENOUGH?
			// 		}
			// 		else
			// 		{   //GETTING THE ANOLOG INPUT WITHOUT USING REWIRED.PLAYER!!!!!
   //                      string port = (rainWorld.options.controls[plr].gamePadNumber + 1).ToString();
   //                      inputPackage.analogueDir = new Vector2(Input.GetAxisRaw("Horizontal" + port), Input.GetAxisRaw("Vertical" + port));
			// 		}
			// 		
			// 		
			// 		
			// 		inputPackage.analogueDir = Vector2.ClampMagnitude(inputPackage.analogueDir * (ModManager.MMF ? rainWorld.options.analogSensitivity : 1f), 1f);
			// 		if (inputPackage.analogueDir.x < -0.5f)
			// 			inputPackage.x = -1;
			// 		if (inputPackage.analogueDir.x > 0.5f)
			// 			inputPackage.x = 1;
			// 		if (inputPackage.analogueDir.y < -0.5f)
			// 			inputPackage.y = -1;
			// 		if (inputPackage.analogueDir.y > 0.5f)
			// 			inputPackage.y = 1;
			// 		
			// 		if (ModManager.MMF)
			// 		{
			// 			if (inputPackage.analogueDir.y < -0.05f || inputPackage.y < 0)
			// 			{
			// 				if (inputPackage.analogueDir.x < -0.05f || inputPackage.x < 0)
			// 					inputPackage.downDiagonal = -1;
			// 				else if (inputPackage.analogueDir.x > 0.05f || inputPackage.x > 0)
			// 					inputPackage.downDiagonal = 1;
			// 			}
			// 		}
			// 		else if (inputPackage.analogueDir.y < -0.05f)
			// 		{
			// 			if (inputPackage.analogueDir.x < -0.05f)
			// 				inputPackage.downDiagonal = -1;
			// 			else if (inputPackage.analogueDir.x > 0.05f)
			// 				inputPackage.downDiagonal = 1;
			// 		}
   //              }
			// 	else
			// 	{
   //                  //AND NOW THE CONTROLLER VERSION... OH BOY...
   //                  //WAIT NO I DON'T THINK THIS IS CONTROLLER VERSION. I THINK THIS IS JUST... ""OTHER""...
   //                  //Debug.Log("SOME WEIRD OTHER CONTROL! CATEGORY 1 (NOT 0)");
			// 		
			// 		//ACTUALLY, I THINK IT BECOMES CATEGORY 1 IF IT'S GETTING INPUT FOR UI PURPOSES. BUT IN-GAME JUST USES CATEGORY 0? I THINK
   //              }
   //
   //              return inputPackage;
   //          }
   //      }
   //      catch (Exception arg)
   //      {
   //          Logger.LogError(string.Format("REDUCE! - ", arg));
   //      }
   //      
   //
   //
   //      
   //
   //      Player.InputPackage result = orig(categoryID, playerNumber, rainWorld);
   //      return result;
   //  }
    

    // private void JollyPlayerSelector_Update(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_Update orig, JollyPlayerSelector self)
    // {
    //     orig(self);
    //
    //     if (self.index == 0 && SlugcatStats.IsSlugcatFromMSC(self.JollyOptions(self.index).playerClass)
    //         && !(ModManager.Expedition && self.menu.manager.rainWorld.ExpeditionMode))
    //     {
    //         self.classButton.GetButtonBehavior.greyedOut = true;
    //     }
    //
    //     //AAAND UNLOCK P1 EXPEDITION MODE BECAUSE WHY NOT
    //     if (self.index == 0 && ModManager.Expedition && self.menu.manager.rainWorld.ExpeditionMode)
    //         self.classButton.GetButtonBehavior.greyedOut = false;
    // }
    
	//CRASH FOR World/RainWorld_Data/StreamingAssets\illustrations\multiplayerportrait41-white.png
    private void MultiplayerMenu_InitiateGameTypeSpecificButtons(On.Menu.MultiplayerMenu.orig_InitiateGameTypeSpecificButtons orig, MultiplayerMenu self)
    {
        orig(self);

        if (PlyCnt() > 5)
        {
            float shift = 100; //NORMALLY 120
            if (ModManager.MSC)
            {
                for (int i = 0; i < self.playerClassButtons.Length; i++)
                {
                    self.playerClassButtons[i].pos.x -= shift;
                }
            }

            for (int i = 0; i < self.playerJoinButtons.Length; i++)
            {
                self.playerJoinButtons[i].pos.x -= shift;
            }
        }
    }
	
	
	
	// public JollySlidingMenu(JollySetupDialog menu, MenuObject owner, Vector2 pos)
	public static void JollySlidingMenu_ctor(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_ctor orig, JollyCoop.JollyMenu.JollySlidingMenu self, JollySetupDialog menu, MenuObject owner, Vector2 pos)
	{
		orig(self, menu, owner, pos);
		
		if (PlyCnt() > 5)
		{
            //for (int i = 0; i < self.playerSelector.Length; i++)
            //{
            //	self.playerSelector[i].pos.x -= 200 + (8.5f * (i * PlyCnt() - 4));
            //    self.playerSelector[i].playerLabelSelector.SetPos(new Vector2(self.playerSelector[i].pos.x, self.playerSelector[i].playerLabelSelector.pos.y));
            //}

            //WHAT IF WE DID A SECOND ROW INSTEAD... THEY LEFT SO MUCH ROOM
            
            for (int i = 0; i < self.playerSelector.Length; i++)
            {
                float downShift = -25;
                if (i > 3)
                {
                    downShift = 150f;
                    self.playerSelector[i].pos.x = self.playerSelector[i-4].pos.x;
                }
                self.playerSelector[i].pos.y -= downShift;
                self.playerSelector[i].playerLabelSelector.SetPos(new Vector2(self.playerSelector[i].pos.x, self.playerSelector[i].playerLabelSelector.pos.y - downShift));
            }

            //WE HAVE TO SHIFT SOME OF THE OTHER MENU UP TOO
            self.numberPlayersSlider.SetPos(new Vector2(self.numberPlayersSlider.pos.x, self.numberPlayersSlider.pos.y + 40));
            //self.numberPlayersSlider.la = menu.Translate("Adjust the number of players"); //CAN'T MOVE THIS... WE'LL JUST HAVE TO PRETEND IT'S SUPPOSED TO GO UNDERNEATH
        }
	}



    /*
	
	
	*/


 //    public static void BPPlayer_Update(On.Player.orig_Update orig, Player self, bool eu)
	// {
	// 	orig(self, eu);
	// 	
	// 	//BRING BACK THIS CLASSIC JOLLYCOOP FIXES FEATURE THAT IS VERY MUCH NEEDED IN CO-OP
	// 	if (!rotundWorldEnabled && MPOptions.grabRelease.Value && self.input[0].jmp && !self.input[1].jmp && self.grabbedBy?.Count > 0)
	// 	{
	// 		for (int graspIndex = self.grabbedBy.Count - 1; graspIndex >= 0; graspIndex--)
	// 		{
	// 			if (self.grabbedBy[graspIndex] is Creature.Grasp grasp && grasp.grabber is Player player_)
	// 			{
	// 				if (!self.isNPC || (player_.isNPC)) //PUPS SHOULD LET GO OF OTHER PUPS
	// 				player_.ReleaseGrasp(grasp.graspUsed); // list is modified
	// 			}
	// 		}
	// 	}
	// }

    

    // private bool JollyCustom_ForceActivateWithMSC(On.JollyCoop.JollyCustom.orig_ForceActivateWithMSC orig)
    // {
    //     if (MPOptions.downpourCoop.Value)
    //         return true;
    //     else
    //         return orig();
    // }


 //    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
	// {
	// 	orig(self);
 //        MachineConnector.SetRegisteredOI("me.moreplayers", new MPOptions());
 //
 //        for (int i = 0; i < ModManager.ActiveMods.Count; i++)
	// 	{
	// 		if (ModManager.ActiveMods[i].id == "willowwisp.bellyplus")
 //            {
	// 			rotundWorldEnabled = true;
	// 			Debug.Log("ROTUND WORLD DETECTED");
	// 		}
 //        }
	// 	
 //    }
	//
	// public static bool rotundWorldEnabled = false;
	
	
	// public static void UpdateMPConfigs()
	// {
		// Debug.Log("SAVING CONFIGS");
		// MachineConnector.SaveConfig(MPOptions as OptionInterface);
	// }
	
	
	
	
	// private bool Options_ApplyOption(On.Options.orig_ApplyOption orig, Options self, string[] splt2)
 //    {
 //        
	// 	bool result = false;
 //        try
 //        {
 //            Logger.LogInfo("APPLY OPTION: " + splt2[0] + " - " +self.controls.Length);
 //            result = orig.Invoke(self, splt2);
 //        }
 //        catch
 //        {
 //            Logger.LogInfo("FAILED TO APPLY OPTION!!");
 //            result = false;
 //        }
	// 	
	// 	//6-8-23 WE'RE GONNA TRY DISABLING THIS BECAUSE WE PULL DIRECTLY FROM THE MPOPTIONS NOW
	// 	// 6-9-23 WE CAN'T DO THAT DUMMY, I DON'T THINK THEY'VE INITIALIZED YET!
	// 	//5-18-23 AFTER RUNNING THE ORIGINAL, IF WE'RE LOOKING FOR INPUT, SETUP THE REST OF OUR PLAYERS PAST 4
	// 	if (splt2[0] == "InputSetup2")
	// 	{
	// 		result = false; //BECAUSE WE HIT A MATCH
	// 		Logger.LogInfo("---APPLYING EXTRA INPUT OPTION! " + self.controls.Length + " - " + splt2[1] + " - " + splt2[2]);
 //            // Options.ControlSetup myControlArry = self.controls[k];
	// 		//self.controls[int.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture)].FromString(splt2[2]);
 //            //THAT JUST CRASHES, SO LETS LOAD THE DATA INTO A HOLDING CELL AND THEN ONLY TRANSFER INTO ENTRIES THAT EXIST.
 //            Options.ControlSetup[] myControlArry = self.controls; //WE SET IT TO CONTROL SO THAT THE NUMBER OF ENTRIES IN THE TABLE MATCHES CONTROL
 //            int entryNum = int.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
 //            //myControlArry[entryNum].FromString(splt2[2]);
 //            //for (int k = 4; k < self.controls.Length; k++) //CAN'T USE THAT! I DON'T THINK MPOPTIONS HAS LOADED YET
 //            //{
 //            //    Logger.LogInfo("-APPLYING TO ARRAY " + k + " out of: " + myControlArry.Length);
 //            //    self.controls[k] = myControlArry[k];
 //            //}
 //            //APPARENTLY EACH LINE GETS SENT IN INDIVIDUALLY SO WE CAN JUST DO THIS..
 //            try
 //            {
 //                Logger.LogInfo("-APPLYING TO ARRAY " + entryNum);
 //                self.controls[entryNum] = myControlArry[entryNum];
 //            }
 //            catch (Exception arg)
 //            {
 //                Logger.LogError(string.Format("FAILED READ/APPLY EXTRA INPUT SETTINGS! - ", arg));
 //            }
 //        }
	// 	
	// 	
	// 	
	// 	
	// 	
	// 	//OKAY I'M PUTTING THIS HERE INSTEAD OF APPLYOPTION()
	// 	//OKAY BUT WHAT IF WE COULD STILL SAVE JOLLYOPTIONS?...
 //        if (false)
 //        {
 //            if (splt2[0] == "JollySetupPlayers" && ModManager.JollyCoop)
 //            {
 //                Logger.LogInfo("---APPLYING EXTRA JOLLYSETUP ");
 //
 //                //5-18-23 AND THEN ADD THE EXTRA ONES
 //                for (int k = 4; k < PlyCnt(); k++)
 //                {
 //                    /*string[] array3 = Regex.Split(splt2[1], "<optC>");
 //                    // int num = 0;
 //                    string[] array4 = array3;
 //                    foreach (string text in array4)
 //                    {
 //                        if (!(text == string.Empty))
 //                        {
 //                            JollyPlayerOptions jollyPlayerOptions = new JollyPlayerOptions(k);
 //                            // jollyPlayerOptions.FromString(text);
 //                            jollyPlayerOptions.FromString(MPOptions.jollySetup[k - 4].Value);
 //                            self.jollyPlayerOptionsArray[jollyPlayerOptions.playerNumber] = jollyPlayerOptions;
 //                            // num++;
 //                        }
 //                    }
 //                    */
 //
 //                    try
 //                    {
 //                        Logger.LogInfo("--- JOLLY PLAYER OPTIONS! " + MPOptions.jollySetup[k - 4].Value);
 //                        JollyPlayerOptions jollyPlayerOptions = new JollyPlayerOptions(k);
 //                        jollyPlayerOptions.FromString(MPOptions.jollySetup[k - 4].Value); //PULL FROM OUR SAVED VAL
 //                        self.jollyPlayerOptionsArray[jollyPlayerOptions.playerNumber] = jollyPlayerOptions;
 //                    }
 //                    catch (Exception arg)
 //                    {
 //                        Logger.LogError(string.Format("FAILED READ INITIALIZE JOLLY PLAYER OPTIONS! - ", arg));
 //                    }
 //                }
 //            }
 //        }
	// 	
	// 	
	// 	
	// 	//6-8-23 TRYING AGAIN BUT FOR JOLLY EXTRAS
 //        
	// 	if (splt2[0] == "JollySetupPlayersExtra")
	// 	{
	// 		result = false; //BECAUSE WE HIT A MATCH
	// 		if (ModManager.JollyCoop)
	// 		{
	// 			string[] array3 = Regex.Split(splt2[1], "<optC>");
	// 			int num = 4; //START AT 4
	// 			foreach (string text in array3)
	// 			{
	// 				if (!(text == string.Empty))
	// 				{
 //                        Logger.LogInfo("--- JOLLY PLAYER OPTIONS!(V2) " + text);
 //                        JollyPlayerOptions jollyPlayerOptions = new JollyPlayerOptions(num);
	// 					jollyPlayerOptions.FromString(text); 
	// 					self.jollyPlayerOptionsArray[jollyPlayerOptions.playerNumber] = jollyPlayerOptions;
	// 					num++;
	// 				}
	// 			}
	// 		}
	// 		else
	// 		{
	// 			result = true;
	// 		}
	// 	}
	// 	
 //        return result;
 //    }
	
	
	
	

   //6-8 WAIT NO WE NEED TO USE THIS THIS BREAKS THE SAVES OTHERWISE
 //    private string Options_ToStringNonSynced(On.Options.orig_ToStringNonSynced orig, Options self)
 //    {
	// 	int servMemory = self.playerToSetInputFor;
	// 	self.playerToSetInputFor = Math.Min(self.playerToSetInputFor, 3); //DON'T LET THIS GET SAVED OUTSIDE OF THE NORMAL RANGE
	// 	//AND DON'T LET CONTROLS SAVE FOR MORE THAN 4 PLAYERS
	// 	Options.ControlSetup[] controlsMemory = self.controls;
	// 	self.controls = new Options.ControlSetup[4];
	// 	for (int i = 0; i < self.controls.Length; i++)
	// 	{
	// 		self.controls[i] = controlsMemory[i];
	// 	}
 //
 //        //OKAY, AND WE CAN ALSO REDIRECT THE INPUT SETTINGS FOR OUR EXTRA PLAYERS TO SOMEWHERE ELSE
 //        //for (int i = 0; i < controlsMemory.Length - 4; i++)
 //        //{
 //        //    //BPOptions.gamepadNum[k - 4].Value
 //        //}
 //        //NAH NVM WE DONT HAVE THAT STUFF LOADED
	// 	
	// 	
	// 	string result = orig(self);
 //
 //        //5-18-23 INSTEAD, MAYBE WE OUTPUT THAT STUFF TO A FILE AS A DIFFERENT NAME?
 //        //for (int k = 4; k < PlyCnt(); k++)
 //        //      {
 //        //          Logger.LogInfo("---SAVING CUTSOM OPTIONS STRING FOR PLAYER: " + k);
 //        //          //controlsMemory[k].gamePad = 0;
 //        //          controlsMemory[k].str
 //        //          result += string.Format(CultureInfo.InvariantCulture, "InputSetup2<optB>{0}<optB>{1}<optA>", k, controlsMemory[k]);
 //        //          //!!! WARNING!!!! THE GAME SEEMS TO WANT TO SET P5'S SPECIFIC CONTROLLER TO #3, WHICH CRASHES THE GAME IF NOT PLUGGED IN. UNDO THAT!!!
 //        //      }
 //
 //
 //        //DANG, STEAM ALWAYS SETS P5'S GAMEPAD TO 4 BY DEFAULT. WE NEED TO SAVE THE CONTROLS IN A WAY THAT IGNORES THAT
 //        for (int k = 4; k < PlyCnt(); k++)
 //        {
 //            string plrSetting = "InputSetup2<optB>" + k + "<optB>SPECIFIC_GAMEPAD<ctrlA>0<ctrlA><ctrlA>0<ctrlA>0<optA>";
 //            result += plrSetting;
 //            Logger.LogInfo("---SAVING CUTSOM OPTIONS STRING FOR PLAYER: " + plrSetting);
 //        }
 //        //
 //
 //        self.controls = controlsMemory;
	// 	self.playerToSetInputFor = servMemory;
	// 	//WE CAN'T SAVE THIS YET, MPOTIONS HASNT LOADED YET
	// 	// MPOptions.playerToSetInput.Value = servMemory; //ALSO SAVE IT AS MOD DATA TOO
 //        return result;
	// }
	
	
	
	//THIS IS FOR SAVING OUR DATA (TO THE TEXT FILES AND STUFF)
	// private string Options_ToString(On.Options.orig_ToString orig, Options self)
 //    {
 //        //I'M SORRY, IL'S ARE JUST TOO HARD ; ;
 //        string text = "";
 //        JollyPlayerOptions[] optionsMemory = self.jollyPlayerOptionsArray;
 //        Options.ControlSetup[] controlsMemory = self.controls;
 //
 //        //DON'T JUST USE 4. CAP AT 4. OTHERWISE GAME WILL FREAK IF... WAIT WHAT? I DON'T UNERSTAND...
 //        //int plyCount = Math.Min(self.jollyPlayerOptionsArray.Length, 4);
 //        Debug.Log("JOLLYPLAYEROOPTIONSARRAY LENGTH: " + self.jollyPlayerOptionsArray.Length);
 //
 //        //REBUILD THE ARRAY WITH A MAX OF 4 PLAYERS
 //        self.jollyPlayerOptionsArray = new JollyPlayerOptions[4];
 //        self.controls = new Options.ControlSetup[4]; //WE MAY NEED THE SAME FOR INPUT SETUP TOO...
 //        for (int j = 0; j < self.jollyPlayerOptionsArray.Length; j++)
 //        {
 //            self.jollyPlayerOptionsArray[j] = optionsMemory[j];
 //        }
 //        for (int i = 0; i < self.controls.Length; i++)
 //        {
 //            self.controls[i] = controlsMemory[i];
 //        }
 //
 //        //RUN THE ORIGINAL WITH THE SHORTENED TABLE
 //        text = orig(self);
	// 	
 //        /* 6-8-DISABLING
	// 	//5-18-23 AND THEN ADD THE EXTRA ONES
	// 	for (int k = 4; k < PlyCnt(); k++)
 //        {
 //            Logger.LogInfo("---SAVING CUTSOM OPTIONS STRING FOR PLAYER: " + k);
 //            text += string.Format(CultureInfo.InvariantCulture, "InputSetup2<optB>{0}<optB>{1}<optA>", k, controlsMemory[k]);
	// 		Logger.LogInfo("---SAVING CUTSOM COMPLETE! ");
	// 		
	// 		
	// 		//OKAY BUT WHAT IF WE COULD STILL SAVE JOLLYOPTIONS?... BUT IN MPOPTIONS, NOT IN THE TEXT FILE
	// 		if (ModManager.JollyCoop) //splt2[0] == "JollySetupPlayers" && 
	// 		{
	// 			//OKAY WAIT, THIS ONE IS FOR STORING INTO THE TEXT FILE (OR MPOPTIONS, IN OUR CASE)
	// 			Logger.LogInfo("--- SAVING EXTRA JOLLY PLAYER OPTIONS! ");
	// 			try
	// 			{
	// 				MPOptions.jollySetup[k - 4].Value = optionsMemory[k].ToString();
	// 				Logger.LogInfo("--- SUCCESS! " + MPOptions.jollySetup[k - 4].Value);
	// 			}
	// 			catch (Exception arg)
	// 			{
	// 				Logger.LogError(string.Format("FAILED TO SAVE JOLLY PLAYER OPTIONS! - ", arg));
	// 			}
	// 		}
 //        }
	// 	*/
	// 	
	// 	//6-8-23 -OKAY THE MODDED VERSION KINDA STINKS BECAUSE IT DOESN'T ACTUALLY SAVE MOST OF THE TIME
	// 	if (ModManager.JollyCoop)
	// 	{
	// 		text += "JollySetupPlayersExtra<optB>";
	// 		for (int j = 4; j < optionsMemory.Length; j++)
	// 		{
	// 			text = text + optionsMemory[j].ToString() + "<optC>";
	// 		}
	// 		text += "<optA>";
	// 	}
	// 	
	// 	//MAYBE WE COULD GIVE THIS ONE MORE TRY TOO
	// 	// UpdateMPConfigs();
	// 	
 //
 //        //THEN RESTORE THE ARRAY TO IT'S FULL PLAYER COUNT
 //        self.jollyPlayerOptionsArray = optionsMemory;
 //        self.controls = controlsMemory;
 //        return text;
 //    }



    //OKAY playerToSetInputFor IS GETTING SET BACK TO 4 BETWEEN GAMES, VIA APPLYOPTION. BUT WHICH APPLYOPTION IS DOING IT? THERE'S LIKE 4
	//THIS ONES DISABLED NOW
	
    // private void Options_FromUnsyncedString(On.Options.orig_FromUnsyncedString orig, Options self, string s)
    // {
    //     orig(self, s);
    //
    //     Logger.LogInfo("CHECKING MPOPTIONS...");
    //     if (MPOptions.playerToSetInput == null)
    //     {
    //         Logger.LogInfo("---MPOPTIONS NOT LOADED YET!! " );
    //         return;
    //     }
    //
    //     //OKAY NOW WE JUST RUN APPLYOPTIONS STUFF FOR EACH RELEVAN VARIABLES
    //     for (int k = 4; k < PlyCnt(); k++)
    //     {
    //         Logger.LogInfo("---RE-APPLYING EXTRA OPTION! " + k);
    //
    //         if (MPOptions.playerToSetInput.Value > 3)
    //             self.playerToSetInputFor = MPOptions.playerToSetInput.Value;
    //
    //         Options.ControlSetup myControlArry = self.controls[k];
    //         try
    //         {
    //             myControlArry.gamePadNumber = MPOptions.gamepadNum[k - 4].Value;
    //             Logger.LogInfo("---GAMEPAD NUM " + myControlArry.gamePadNumber);
    //             myControlArry.xInvert = MPOptions.invertX[k - 4].Value; //(array2[6] == "1");
    //             myControlArry.yInvert = MPOptions.invertY[k - 4].Value; //(array2[7] == "1");
    //             myControlArry.UpdateControlPreference((MPOptions.usingKeyboard[k - 4].Value) ? Options.ControlSetup.ControlToUse.KEYBOARD : Options.ControlSetup.ControlToUse.SPECIFIC_GAMEPAD, true);
    //         }
    //         catch
    //         {
    //             Logger.LogInfo("RE-FAILED TO APPLY OPTION CHANGES FROM BPOPTIONS. DEFAULTING TO BACKUP");
    //             myControlArry.gamePadNumber = 0;
    //             myControlArry.xInvert = false;
    //             myControlArry.yInvert = false;
    //             myControlArry.UpdateControlPreference(Options.ControlSetup.ControlToUse.KEYBOARD, true);
    //         }
    //     }
    // }

    
	
	



    private void RainWorldGame_JollySpawnPlayers(On.RainWorldGame.orig_JollySpawnPlayers orig, RainWorldGame self, WorldCoordinate location)
    {

        Debug.Log("---Number of jolly players: " + self.rainWorld.options.JollyPlayerCount.ToString()); // 
        Debug.Log("---accesing directly: " + Enumerable.Count<JollyPlayerOptions>(self.rainWorld.options.jollyPlayerOptionsArray, (JollyPlayerOptions x) => x.joined).ToString());
        Debug.Log("---BONUS: ARRAY LENGTH" + self.rainWorld.options.jollyPlayerOptionsArray.Length);
        int PlayerArrSize = self.rainWorld.options.jollyPlayerOptionsArray.Length;

        JollyPlayerOptions[] jollyPlayerOptionsArray = self.rainWorld.options.jollyPlayerOptionsArray;
        for (int i = 0; i < jollyPlayerOptionsArray.Length; i++)
        {
            Debug.Log(jollyPlayerOptionsArray[i].ToString());
            //rainWorld.options.jollyPlayerOptionsArray[j].joined
            Debug.Log("JOLLYGOROUND " + self.rainWorld.options.jollyPlayerOptionsArray[i].joined);
        }

        Debug.Log("----JOLLY SPAWN BEGIN");
        //THIS PART RIGHT HERE. FOR STORY MODE AT LEAST THIS IS IRRELEVANT, BUT IF WE AREN'T SET TO JOIN, IT WILL FALL BACK ON THIS CHECK AS A BACKUP 
        //AND SINCE THE SETUP VALUES FOR PLAYER 5+ DOESN'T EXIST, IT WILL CRASH
        //CAN WE JUST... CUT THAT PART OUT? DO WE EVEN NEED IT?
        //I GUESS IF WE HAVEN'T SELECTED A 5 PLAYER GAME, WE COULD BRIEFLY PRETEND OUR ARRAY LENGTH IS 4?...
		
		
        //IF WE'RE PLAYING WITH A NORMAL AMOUNT (LESS THAN 5) RUN THIS
        //if (self.rainWorld.options.JollyPlayerCount <= 4) //|| plyCnt
        if (self.rainWorld.options.JollyPlayerCount < PlayerArrSize)
        {
            //WE HAVE TO BRIEFLY PRETEND OUR PLAYEROPTIONSARRAY IS ONLY AS MANY ENTRIES AS WE'VE SELECTED
            JollyPlayerOptions[] optionsMemory = self.rainWorld.options.jollyPlayerOptionsArray;
            self.rainWorld.options.jollyPlayerOptionsArray = new JollyPlayerOptions[self.rainWorld.options.JollyPlayerCount];
            for (int j = 0; j < self.rainWorld.options.jollyPlayerOptionsArray.Length; j++)
            {
                self.rainWorld.options.jollyPlayerOptionsArray[j] = optionsMemory[j];
            }
            Debug.Log("---CUSTOM JOLLY SPAWN" + self.rainWorld.options.jollyPlayerOptionsArray.Length);
            orig(self, location);

            //THEN RESTORE THE ARRAY TO IT'S FULL PLAYER COUNT
            self.rainWorld.options.jollyPlayerOptionsArray = optionsMemory;
        }
        else
            orig(self, location); //IF WE'RE PLAYING WITH EVERYONE ENABLED, WE'RE GOOD TO RUN THE ORIGINAL FOR SOME REASON

        Debug.Log("----JOLLY SPAWN SUCCESS");
    }

    


    // private void IntroRoll_Update(On.Menu.IntroRoll.orig_Update orig, IntroRoll self)
    // {
    //     //Logger.LogInfo("INTRO ROLL " + self.manager.rainWorld.options.controls.Length);
    //     //HOLD IT! SOME OF OUR PLAYERS MAY BE NULL UNTIL WE ADD THEM
    //     try
    //     {
    //         for (int i = 0; i < self.manager.rainWorld.options.controls.Length; i++)
    //         {
    //             //Logger.LogInfo("UPDATE INTRO ROLL" + self.manager.rainWorld.options.controls[i].player);
    //             if (self.manager.rainWorld.options.controls[i].player == null)
    //             {
    //                 //Rewired.Data.UserData.
    //                 //BUT YEA ITS DEF A REWIRED THING...
    //
    //                 //Custom.rainWorld.rewiredInputManager.userData.AddPlayer();
    //                 //self.manager.rainWorld.rewiredInputManager.userData.AddPlayer();
    //                 //self.manager.rainWorld.rewiredInputManager.userData.DuplicatePlayer(1); //THIS ISN'T IT EITHER
    //                 //Logger.LogInfo("UPDATE INTRO ROLL COMPLETE" + i);
    //                 //NO MATTER WHAT I DO, IT SEEMS THAT THE NUMBER OF ENTRIES IN ReInput.players.Players[] DOES NOT INCREASE. SO MAYBE ADDPLAYER() ETC IS NOT WHAT WE NEED HERE...
    //
    //                 //self.manager.rainWorld.options.controls[i].player = new Rewired.Player;
    //
    //                 //SET THE NEW PLAYERS SETTINGS EQUAL TO THE PREV ONE. BECAUSE IT DEFAULTS TO NULL I GUESS
    //                 //self.manager.rainWorld.options.controls[i].player = self.manager.rainWorld.options.controls[i-1].player;
    //                 //ReInput.players.
    //                 //self.manager.rainWorld.options.controls[i].player.id = 4;
    //
    //                 //ReInput.players.Players[i] = new Rewired.Player(false, i, "Player" + i, "Player" + i, i, i, i);
    //                 //THIS IS STRAIGJT FROM ONE OF THOSE REWIRED GIBBERISH CLASSES 
    //                 //player = new Player(false, i - 1, player_Editor.name, player_Editor.descriptiveName, ypwUncqYrrgWARbofZIocbdoyJzm, iZBHTLMJEscsCxujGlXwPdwYSOZv, oejthPnlmICeXCzkndFXvfozHuLjA);
    //                 //ReInput.players.Players[i] = new Rewired.Player(false, i - 1, player_Editor.name, player_Editor.descriptiveName, ypwUncqYrrgWARbofZIocbdoyJzm, iZBHTLMJEscsCxujGlXwPdwYSOZv, oejthPnlmICeXCzkndFXvfozHuLjA);
    //
    //                 //foreach (Rewired.Player player in ReInput.players.GetPlayers(false))
    //                 //UNCOMMENT THIS BLOCK TO SEE WHERE THE BIG ISSUE STANDS (PLAYER EXISTS, BUT IS NULL, AND THERE IS NO WAY TO POPULATE IT...)
    //                 /*
    //                 foreach (Rewired.Player player in ReInput.players.Players)
    //                 {
    //                     Logger.LogInfo("--------CHECKING REWIRED PLAYER!!!" + i + " - " + player + " - " + player.name + " - " + ReInput.players.Players.Count);
    //                     if (player.id == i) //self.index)
    //                     {
    //                         //this.player = player;
    //                         Logger.LogInfo("--------ADDING REWIRED PLAYER!!!" + i);
    //                         self.manager.rainWorld.options.controls[i].player = player;
    //                         break;
    //                     }
    //                 }
    //                 */
    //
    //
    //
    //                 //self.manager.rainWorld.options.controls[i].player.id = self.manager.rainWorld.options.controls[i].index;
    //                 //Rewired.Player player = new Rewired.Player(false, i, "Player"+i, "Player" + i, );
    //                 //Rewired.Player.ControllerHelper
    //                 //self.manager.rainWorld.options.controls[i].player = player;
    //                 //self.manager.rainWorld.options.controls[i].player = new Rewired.Player Options.ControlSetup
    //                 //self.manager.rainWorld.rewiredInputManager.userData.pla
    //                 //self.manager.rainWorld.options.controls[i].player = self.manager.rainWorld.rewiredInputManager.userData.GetPlayer(i);
    //             }
    //         }
    //         //self.manager.rainWorld.options.InitRewiredObjects();
    //         //self.manager.rainWorld.options.ControlSetup.InitRewiredObjects();
    //         //WE WANT TO RUN THIS I THINK
    //     }
    //     catch (Exception arg)
    //     {
    //         Logger.LogInfo("FAILED TO ADD SPECIAL NEW PLAYER : " + arg);
    //     }
    //     
    //
    //
    //     //OKAY, ATTEMPT 3. WE JUST HAVE TO PRETEND THE EXTRA PLAYERS DON'T EXIST FOR A MOMENT... AGAIN
    //     Options.ControlSetup[] controlsMemory = self.manager.rainWorld.options.controls;
    //     self.manager.rainWorld.options.controls = new Options.ControlSetup[4];
    //     for (int i = 0; i < self.manager.rainWorld.options.controls.Length; i++) //DON'T LET CONTROLS SAVE FOR MORE THAN 4 PLAYERS
    //     {
    //         self.manager.rainWorld.options.controls[i] = controlsMemory[i];
    //     }
    //
    //     orig(self); //THE ORIGINAL
    //     
    //     self.manager.rainWorld.options.controls = controlsMemory; //AND THEN PUT IT BACK
    // }





    private void ControlSetup_UpdateControlPreference(On.Options.ControlSetup.orig_UpdateControlPreference orig, Options.ControlSetup self, Options.ControlSetup.ControlToUse preference, bool forceUpdate)
    {
        try
        {
            Logger.LogInfo("APPLY CONTROL PREFERENCE: " + preference + " - " + forceUpdate);
            //self.InitRewiredObjects();
            orig.Invoke(self, preference, forceUpdate);
        }
        catch
        {
            Logger.LogInfo("FAILED TO UPDATE CONTROL PREFERENCE!!");
            
        }
    }

    
    //THESE WERE FOR THE INPUT MENU BUT WE DIN'T USE IT ANYMORE
    // private void Options_FromString(On.Options.orig_FromString orig, Options self, string s)
    // {
    //     self.validation = false;
    //     self.dlcTutorialShown = false;
    //     self.remixTutorialShown = false;
    //     self.lastGameVersion = null;
    //     self.commentary = false;
    //     self.vsync = false;
    //     self.enabledMods = new List<string>();
    //     self.modLoadOrder = new Dictionary<string, int>();
    //     self.friendlyLizards = true;
    //     self.jollyHud = true;
    //     self.friendlyFire = false;
    //     self.friendlySteal = true;
    //     self.smartShortcuts = true;
    //     self.cameraCycling = true;
    //     self.modChecksums = new Dictionary<string, string>();
    //     self.unrecognizedSaveStrings.Clear();
    //     string[] array = Regex.Split(s, "<optA>");
    //     for (int i = 0; i < array.Length; i++)
    //     {
    //         string[] array2 = Regex.Split(array[i], "<optB>");
    //         //for (int i = 0; i < array.Length; i++)
    //         //{
    //         //    Debug.Log("LOAD OPTION: " + array2[i]);
    //         //}
    //             
    //         if (self.ApplyOption(array2) && array[i].Trim().Length > 0 && array2.Length >= 1)
    //         {
    //             self.unrecognizedSaveStrings.Add(array[i]);
    //         }
    //     }
    //     ModManager.RefreshModsLists(self.rainWorld);
    // }
    //
    // private void MPInputOptionsMenu_ctor(On.Menu.InputOptionsMenu.orig_ctor orig, InputOptionsMenu self, ProcessManager manager)
    // {
    //     orig.Invoke(self, manager);
    //     self.backButton.pos -= new Vector2(120f, 0);
    // }
    //
    //
    // private void PlayerButton_ctor(On.Menu.InputOptionsMenu.PlayerButton.orig_ctor orig, InputOptionsMenu.PlayerButton self, Menu.Menu menu, MenuObject owner, Vector2 pos, InputOptionsMenu.PlayerButton[] array, int index)
    // {
    //     orig(self, menu, owner, pos, array, index);
    //
    //     //SHIFT BUTTONS UP IF WE NEED ROOM
    //     if (PlyCnt() == 5)
    //     {
    //         self.originalPos.y += 10 + (3 * (index * PlyCnt() - 4));
    //     }
    //     else if (PlyCnt() > 5)
    //     {
    //         //INCREASE THE HIGHT, AND POSSIBLY BRING THEM CLOSER TOGETHER
    //         self.originalPos.y += 25 + (8 * (index * PlyCnt() - 4));
    //         self.menuLabel.pos.y += 25;
    //         //self.menuLabel.Container.MoveToFront();
    //         self.menuLabel.label.MoveToFront();
    //     }
    // }

    private void MenuIllustration_ctor(On.Menu.MenuIllustration.orig_ctor orig, MenuIllustration self, Menu.Menu menu, MenuObject owner, string folderName, string fileName, Vector2 pos, bool crispPixels, bool anchorCenter)
    {
        string newFileName = fileName;
        if (fileName.StartsWith("MultiplayerPortrait"))
        {
            string substr1 = fileName.Replace("MultiplayerPortrait", "").Substring(0, 1); //GETS THE PLAYER NUMBER
            string substr2 = fileName.Replace("MultiplayerPortrait", "").Substring(1); //THE REST OF THE NUMBERS
            //IF OUR PLAYER NUM IS HIGHER THAN EXPECTED, RETURN THE 4TH PLAYER IMAGE VERSION
            if (Convert.ToInt32(substr1) > 3)
                substr1 = "3";
            
            //REBUILD IT
            newFileName = "MultiplayerPortrait" + substr1 + substr2;
            Debug.Log("FINAL FILE: " + substr1 + substr2 + "  -  " + newFileName);
        }

        orig.Invoke(self, menu, owner, folderName, newFileName, pos, crispPixels, anchorCenter);
    }

    

    private void Replace4WithMore(ILContext il)
    {
        var cursor = new ILCursor(il);
        var x = 0;
        while (cursor.TryGotoNext(MoveType.After,
            //i => i.MatchLdarg(0),
            i => i.MatchLdcI4(4)
        ))
        {
            x++;
            //cursor.Emit(OpCodes.Ldloc, player); //THESE LIKE, BECOME ARGUMENTS WITHIN EMITDELEGATE  I THINK?
            //cursor.Emit(OpCodes.Ldloc, k);

            //cursor.EmitDelegate((float rad, Player player, int k) =>
            cursor.EmitDelegate((int oldNum) =>
            {
                return plyCnt;
            });
        }

        Logger.LogInfo("SLIDING MENU IL LINES ADDED! " + x);
    }

    private void Options_ctor(MonoMod.Cil.ILContext il)
    {
        var cursor = new ILCursor(il);
        var x = 0;
        while (cursor.TryGotoNext(MoveType.After, 
            i => i.MatchLdarg(0),
            i => i.MatchLdcI4(4)
        ))
        {
            x++;
            //cursor.Emit(OpCodes.Ldloc, player); //THESE LIKE, BECOME ARGUMENTS WITHIN EMITDELEGATE  I THINK?
            //cursor.Emit(OpCodes.Ldloc, k);

            //cursor.EmitDelegate((float rad, Player player, int k) =>
            cursor.EmitDelegate((int oldNum) =>
            {
                return plyCnt;
            });
        }

        Logger.LogInfo("TESTMYSLUGCAT IL LINES ADDED! " + x);
    }

    private void MPJollySlidingMenu_NumberPlayersChange1(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_NumberPlayersChange orig, JollySlidingMenu self, UIconfig config, string value, string oldvalue)
    {
        int num;
        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out num))
        {
            if (num > plyCnt || num < 1)
            {
                return;
            }
            for (int i = 0; i < self.Options.jollyPlayerOptionsArray.Length; i++)
            {
                self.Options.jollyPlayerOptionsArray[i].joined = (i <= num - 1);
            }
        }
        self.UpdatePlayerSlideSelectable(num - 1);
    }

    
}