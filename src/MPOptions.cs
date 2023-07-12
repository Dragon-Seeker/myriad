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



public class MPOptions : OptionInterface
{
    public MPOptions()
    {
        MPOptions.downpourCoop = this.config.Bind<bool>("downpourCoop", true);
		MPOptions.longPipeWait = this.config.Bind<bool>("longPipeWait", true);
		MPOptions.grabRelease = this.config.Bind<bool>("grabRelease", true); 
		MPOptions.maxPlayers = this.config.Bind<int>("maxPlayers", 5, new ConfigAcceptableRange<int>(4, 16));
    }
	
	public static Configurable<bool> downpourCoop;
	public static Configurable<bool> longPipeWait;
    public static Configurable<int> maxPlayers;
	public static Configurable<bool> grabRelease;
	public OpSliderTick pCountOp;
	
	
	// public override void Update()
    // {
        // base.Update();
    // }
    
    public static string BPTranslate(string t)
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
			
			//IF I EVER WANT TO CHANGE THIS... OpUpdown should be the way to go
			dsc = BPTranslate("Number of controls");
			int barLngt = 35 * 3;
            float sldPad = 15;
			Tabs[0].AddItems(new UIelement[]
			{
				pCountOp = new OpSliderTick(MPOptions.maxPlayers, new Vector2(margin, lineCount-5), barLngt)
				{description = dsc},
				new OpLabel(pCountOp.pos.x + ((barLngt * 1) / 5f), pCountOp.pos.y + 30, BPTranslate("Players:"), bigText: true)
				{alignment = FLabelAlignment.Center},
                new OpLabel(pCountOp.pos.x - sldPad, pCountOp.pos.y +5, "4"),
                new OpLabel(pCountOp.pos.x + (barLngt * 1) + sldPad -5, pCountOp.pos.y +5, "8")
            });

            OpCheckBox mpBox1;
            dsc = BPTranslate("Enable Co-Op for downpour campaigns. Some features may not work as expected");
			Tabs[0].AddItems(new UIelement[]
			{
                mpBox1 = new OpCheckBox(MPOptions.downpourCoop, new Vector2(margin + 175, lineCount))
				{description = dsc},
				new OpLabel(mpBox1.pos.x + 30, mpBox1.pos.y+3, BPTranslate("Downpour Co-op"))
                {description = dsc}  //bumpBehav = chkBox5.bumpBehav, 
			});

            OpCheckBox mpBox2;
            //lineCount -= 60;
            dsc = BPTranslate("Greatly increases how long you can wait in a pipe for other players to catch up");
			Tabs[0].AddItems(new UIelement[]
			{
                mpBox2 = new OpCheckBox(MPOptions.longPipeWait, new Vector2(margin + 325, lineCount))
				{description = dsc},
				new OpLabel(mpBox2.pos.x + 30, mpBox2.pos.y+3, BPTranslate("Extended Pipe Waiting"))
                {description = dsc}  //bumpBehav = chkBox5.bumpBehav, 
				
			});
			
			// MorePlayers.Logger.LogInfo(" INIT PBOPTIONS FORM COMPLETE");
        }
        catch (Exception e)
        {
            //MorePlayers.Logger.LogInfo("CATCH! ERROR 2 " + e);
        }
    }
}