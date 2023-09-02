using BepInEx;
using UnityEngine;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using static System.Runtime.CompilerServices.RuntimeHelpers;

using Menu;
using RWCustom;
using Rewired;
using System.Reflection;
using ManySlugCats;
using Steamworks;
using System.IO;
using Newtonsoft.Json;

public class MPOptions : OptionInterface
{
    

    public MPOptions()
    {
        MPOptions.downpourCoop = this.config.Bind<bool>("downpourCoop", true);
		MPOptions.longPipeWait = this.config.Bind<bool>("longPipeWait", true);
		MPOptions.grabRelease = this.config.Bind<bool>("grabRelease", true); 
		MPOptions.maxPlayers = this.config.Bind<int>("maxPlayers", SaveStuff.defMaxCap / 4, new ConfigAcceptableRange<int>(1, 4));

        //this.OnConfigChanged +=     
        var onConfigChanged = typeof(OptionInterface).GetEvent("OnConfigChanged");
        onConfigChanged.AddEventHandler(this, Delegate.CreateDelegate(onConfigChanged.EventHandlerType, this, GetType().GetMethod(nameof(SaveSettings))!));
    }
	
	public static Configurable<bool> downpourCoop;
	public static Configurable<bool> longPipeWait;
    public static Configurable<int> maxPlayers;
	public static Configurable<bool> grabRelease;
	public OpSliderTick pCountOp;
    public OpLabel lblOp1;

    public override void Update() {
        base.Update();

        if (pCountOp.GetValueInt() != maxPlayers.Value) {
            this.lblOp1.Show();
        }
        else {
            this.lblOp1.Hide();
        }
    }

    public void SaveSettings() {
        base.Update();
        Debug.Log("CHANGING PLAYER CAP");

        var data = new SaveStuff.MyData() {
            pCap = (pCountOp.GetValueInt() * 4)
            //MyString = "asdasd"
        };

        Directory.CreateDirectory(Path.GetDirectoryName(SaveStuff.SavePath)!); //CREATE DIRECTORY IF IT DOESNT EXIST
        File.WriteAllText(SaveStuff.SavePath, JsonConvert.SerializeObject(data));

    }

    public static string Translate(string t)
    {
        return OptionInterface.Translate(t);
    }


    public override void Initialize()
    {
        // MorePlayers.Logger.LogInfo(" INIT PBOPTIONS");
        base.Initialize();

		this.Tabs = new OpTab[]
		{
			new OpTab(this, "Settings")
		};

        try
        {
			float lineCount = 555;
			int margin = 20;
            string dsc = "";

            Tabs[0].AddItems(new OpLabel(265, 575, Translate("Myriad Options"), bigText: true) { alignment = FLabelAlignment.Center });
            lineCount -= 50;

			//IF I EVER WANT TO CHANGE THIS... OpUpdown should be the way to go
			dsc = Translate("Number of controls");
			int barLngt = 55 * 3;
            float sldPad = 0;
            float pcLabelY = 12f;
			Tabs[0].AddItems(new UIelement[]
			{
				pCountOp = new OpSliderTick(MPOptions.maxPlayers, new Vector2(margin, lineCount-5), barLngt)
				{description = dsc},
				new OpLabel(pCountOp.pos.x + ((barLngt * 1) / 5f), pCountOp.pos.y + 30, Translate("Players:"), bigText: true)
				{alignment = FLabelAlignment.Center},
                new OpLabel(pCountOp.pos.x - sldPad, pCountOp.pos.y -pcLabelY, "4"),
                new OpLabel(pCountOp.pos.x + (barLngt * 0.33f) + sldPad -5, pCountOp.pos.y -pcLabelY, "8"),
                new OpLabel(pCountOp.pos.x + (barLngt * 0.67f) + sldPad -5, pCountOp.pos.y -pcLabelY, "12"),
                new OpLabel(pCountOp.pos.x + (barLngt * 1) + sldPad -5, pCountOp.pos.y -pcLabelY, "16"),
                lblOp1 = new OpLabel(pCountOp.pos.x, pCountOp.pos.y -(pcLabelY * 3f), Translate("A restart is required to apply changes")),
            });
            //mod_menu_restart|The applied mods require the game to be re-launched. Press continue and then launch the game again.

            margin += 225;
            if (!MyriadMod.coopLeashEnabled) {
                //ONLY IF CO-OP LEASH IS NOT ENABLED
                OpCheckBox mpBox2;
                dsc = Translate("Greatly increases the maximum waiting period for other players in Smart Shortcuts");
                Tabs[0].AddItems(new UIelement[]
                {
                    mpBox2 = new OpCheckBox(MPOptions.longPipeWait, new Vector2(margin, lineCount))
                    {description = dsc},
                    new OpLabel(mpBox2.pos.x + 30, mpBox2.pos.y+3, Translate("Extended Pipe Waiting"))
                    {description = dsc}
                });
            }
            
            


            margin += 200;
            if (ModManager.MSC) {
                //ONLY SHOW THIS OPTION IF MSC IS ENABLED
                OpCheckBox mpBox1;
                dsc = Translate("Enable Co-Op for downpour campaigns. Some features may not work as expected");
                Tabs[0].AddItems(new UIelement[]
                {
                    mpBox1 = new OpCheckBox(MPOptions.downpourCoop, new Vector2(margin, lineCount))
                    {description = dsc},
                    new OpLabel(mpBox1.pos.x + 30, mpBox1.pos.y+3, Translate("Downpour Co-op"))
                    {description = dsc}
                });
            }


            int descLine = 200;
            Tabs[0].AddItems(new OpLabel(25f, descLine + 25f, "--- Additional Info ---"));

            Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Only 4 players need to stand still in a shelter to trigger the door close")));
            descLine -= 20;
            Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Arena shelters can hold additional slugcats")));
            descLine -= 20;

            descLine -= 35;
            Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Please report bugs & crashes on the steam workshop or Discord")));
            descLine -= 20;
            Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("located in: / Program Files(86) / Steam / steamapps / common / Rain World / exceptionLog.txt")));


            descLine -= 35;
            Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Recommended mods for large groups are Stick-Together and SBCameraScroll")));
            descLine -= 20;
            Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Some mods may not support more than 4 players")));


        } catch (Exception e)
        {
            //MorePlayers.Logger.LogInfo("CATCH! ERROR 2 " + e);
        }
    }
}

public static class SaveStuff {

    public const int defMaxCap = 8;

    //public static readonly string SavePath = Path.Combine(Application.dataPath, "Myriad", "Myriad.json");
    
    public static readonly string SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Videocult", "Rain World", "Myriad", "Myriad.json");
    

    public class MyData {
        public int pCap;
    }

    public static MyData LoadSettings() {
        
        if (File.Exists(SavePath)) {
            return JsonConvert.DeserializeObject<MyData>(File.ReadAllText(SavePath));
        } else {
            return new MyData() {
                pCap = defMaxCap
            };
        }
    }
}