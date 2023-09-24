using BepInEx.Logging;
using UnityEngine;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using System;

namespace Myriad;
public class MPOptions : OptionInterface {

    private ManualLogSource Logger;
    
    public MPOptions(ManualLogSource logger) {
        downpourCoop = this.config.Bind<bool>("downpourCoop", true);
        longPipeWait = this.config.Bind<bool>("longPipeWait", true);
        grabRelease = this.config.Bind<bool>("grabRelease", true);
        maxPlayers = this.config.Bind<int>("maxPlayers", PreloadMaxPlayerSettings.defMaxCap / 4, new ConfigAcceptableRange<int>(1, 4));
        
        this.Logger = logger;
        
        // var onConfigChanged = maxPlayers.GetType().GetEvent("OnChange");
        // onConfigChanged.AddEventHandler(maxPlayers, Delegate.CreateDelegate(onConfigChanged.EventHandlerType, this, GetType().GetMethod(nameof(maxPlayersChangeEvent))!));
        //
        //this.OnConfigChanged +=     
        // var onConfigChanged = typeof(OptionInterface).GetEvent("OnConfigChanged");
        // onConfigChanged.AddEventHandler(this, Delegate.CreateDelegate(onConfigChanged.EventHandlerType, this, GetType().GetMethod(nameof(SaveSettings))!));
    }
    
    public readonly Configurable<bool> downpourCoop;
    public readonly Configurable<bool> longPipeWait;
    private readonly Configurable<int> maxPlayers; //Used only for player control of the main value from PreloadMaxPlayerSettings
    public readonly Configurable<bool> grabRelease;
    
    public OpSliderTick pCountOp;
    public OpLabel lblOp1;

    // public void maxPlayersChangeEvent() {
    //     Logger.LogError("---");
    //     Logger.LogError(maxPlayers.Value);
    //     Logger.LogError(StackTraceUtility.ExtractStackTrace());
    //     Logger.LogError("---");
    // }

    public void validateAndSetup() {
        var curretPlayerCap = PreloadMaxPlayerSettings.getPlayerCap();
        
        //Make sure desync of config value doesn't occur
        maxPlayers.Value = curretPlayerCap / 4;
        this.config.Save();
        
        On.OptionInterface._SaveConfigFile += (orig, self) => {
            orig(self);
            
            if (self == this) {
                SaveSettings();
            }
        };
    }
    
    public override void Update() {
        base.Update();

        if (pCountOp.GetValueInt() != maxPlayers.Value) {
            this.lblOp1.Show();
        } else {
            this.lblOp1.Hide();
        }
    }
    
    public void SaveSettings() {
        if(pCountOp == null) return;
        
        Logger.LogWarning("CHANGING PLAYER CAP");
        int newPlayerCap = pCountOp.GetValueInt() * 4;
        
        Logger.LogWarning("---");
        Logger.LogWarning(newPlayerCap);
        Logger.LogWarning("---");
        
        PreloadMaxPlayerSettings.SaveSettings(newPlayerCap);
    }

    public static string Translate(string t) {
        return OptionInterface.Translate(t);
    }

    public override void Initialize() {
        // MorePlayers.Logger.LogInfo(" INIT PBOPTIONS");
        
        this.Tabs = new OpTab[]{ new OpTab(this, "Settings") };
        
        try {
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
            Tabs[0].AddItems(new UIelement[] {
                pCountOp = new OpSliderTick(maxPlayers, new Vector2(margin, lineCount-5), barLngt) {
                    description = dsc
                },
                new OpLabel(pCountOp.pos.x + ((barLngt * 1) / 5f), pCountOp.pos.y + 30, Translate("Players:"), bigText: true) {
                    alignment = FLabelAlignment.Center
                },
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
                Tabs[0].AddItems(new UIelement[] {
                    mpBox2 = new OpCheckBox(longPipeWait, new Vector2(margin, lineCount)) {
                        description = dsc
                    },
                    new OpLabel(mpBox2.pos.x + 30, mpBox2.pos.y+3, Translate("Extended Pipe Waiting")) {
                        description = dsc
                    }
                });
            }

            margin += 200;
            if (ModManager.MSC) {
                //ONLY SHOW THIS OPTION IF MSC IS ENABLED
                OpCheckBox mpBox1;
                dsc = Translate("Enable Co-Op for downpour campaigns. Some features may not work as expected");
                Tabs[0].AddItems(new UIelement[] {
                    mpBox1 = new OpCheckBox(downpourCoop, new Vector2(margin, lineCount)) {
                        description = dsc
                    },
                    new OpLabel(mpBox1.pos.x + 30, mpBox1.pos.y+3, Translate("Downpour Co-op")) {
                        description = dsc
                    }
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
            Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Recommended mods for large groups are Stick Together and SBCameraScroll")));
            descLine -= 20;
            Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Some mods may not support more than 4 players")));
            error = false;
        } catch (Exception e) {
            error = true;
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