using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        #region mdk preserve
        //===[R E A D M E]=============================================================================================
        //
        //  Thank you for using Ivos Ship-Alignment-Script. There is nothing to worry about
        //  unless you change the configuration. If you decided to, I hope god will be with you.
        //                                                      GLHF
        //
        //===[Configuration]=============================================================================================
        //
        string RemoteControllName = "gpsCenter";
        string Direction = "forward"; //'forward', 'backward', 'up', 'rocket'
        bool GravityMode = false;
        
        string ProgrammableBlockName = "[Ship-Alignment-Script]";
        //
        //===[Advanced Settings]=========================================================================================
        //
        int GyroscopeLimit = 3;
        float MinimumAngle = 0.0001f;
        double ControlCoefficient = 0.5;
        //
        //===[Help]======================================================================================================
        static Dictionary<string, string> helpDic = new Dictionary<string, string>() {
        //
        //
        {"setTarget (-clipboard)", "\"X Y Z\" or 'clipboard'-Switch with \"clipboard GPS\" format "},
        {"setDirection",           "Changes direction ('forward', 'backward', 'up', 'rocket')"},
        {"gravityMode",            "Toggles gravityMode or changes to desired bool"},
        {"kill",            "Disables PB and resets all Gyroscopes"},

        {"showStatus",             "Toggles visual status output"},
        {"runStatus" ,             "Toggles visual status output and disables output routine"},
        {"showHelp"  ,             "Displays all functions defined in here"},
        {"Thomas",                 "stinkt :D"},
        //
        //
        };
        //==============================================================================================================
        //
        //                                                    !!!END OF CONFIGURATION!!!
        //                                       !!!All Changes From Here On Can Break The Script!!!
        //
        //==============================================================================================================
        #endregion
        //TODO (other scripts):
        //
        //Higher udatefrequency only when moving
        //Trajectory prediction (weapon speed has to be tested, for projectiles)
        //IGC
        //Gyro update only on Frequencyupdate call
        //Richtung wechseln
        //outsource strings from helpDic and commands-Dic

        //Init Dependecies 
        MyCommandLine commandLine;
        OutputHandler ScriptOutput;
        Dictionary<string, Action> commands;

        //Program Initalization
        public Program()
        {
            //Setup Custom Name
            Me.CustomName = ProgrammableBlockName;
            //Setup UpdateFrequency 
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            //Setup Important Dependencies 
            commandLine = new MyCommandLine();
            
            ScriptOutput = new OutputHandler(helpInformation: Utilities.formatDictKeys<string, string>(helpDic));
            commands = new Dictionary<string, Action>();
            //Associate Commands with according Actions
            commands["setTarget"] = setTarget;
            commands["setDirection"] = setDirection;
            commands["gravityMode"] = gravityMode;
            commands["kill"] = reset;

            commands["showStatus"] = showStatus;
            commands["runStatus"] = runStatus;
            commands["help"] = help;

            //Initialize GyroAligment Configuration
            ScriptOutput.assert(GYROInit);
            //Load Storage Routine
            Load();
            
        }

        //Loading Routine
        public void Load()
        {
            string[] storedData = Storage.Split(';');
            if (GYROready && storedData.Count() == 3)
            {
                GYROtarget.setPosition(storedData[0]);
                GYROdirection = storedData[1];
                GYROgravityMode.toggle(storedData[2]);
                if (GYROgravityMode.state) { GYROtarget.customTargetName = "Natural Gravity"; }
                else { GYROtarget.customTargetName = null; }
            }
        }
        //Save Routine
        public void Save()
        {
            //Saves Target;Direction;GravityMode
            if (GYROready) {
                Storage =
                    $"{GYROtarget.getXYZAsString()};" +
                    $"{GYROdirection};" +
                    $"{GYROgravityMode}";
            }
        }

        //Main Call
        public void Main(string argument, UpdateType updateSource)
        {

            //Parses Arguments to called function
            //Format: command argument1 argument2 ... 
            //Mulitple Calls with ";" as delimiter possible
            string[] calls = argument.Split(';');
            foreach (string arg in calls) 
            {
                argument = arg;
                if (commandLine.TryParse(argument))
                {
                    Action commandAction;
                    string command = commandLine.Argument(0);
                    string output;
                    //Outputs helpinformation for called command if -help flag set
                    if (commandLine.Switch("help") && helpDic.TryGetValue(command, out output))
                    {
                        ScriptOutput.sendt(output);
                    }
                    else if (commands.TryGetValue(command, out commandAction))
                    {
                        ScriptOutput.assert(commandAction);
                    }
                    else { ScriptOutput.sendt($"(Error) Invalid command: '{command}'"); }
                }
            }
            

            //Main Script Logic
            if (GYROready)
            {
                GYRORun();
                ScriptOutput.sendp("Targeting", GYROtarget.ToString());
                ScriptOutput.sendp("Distance", GYROtargetDistance(), suffix: "m");
                ScriptOutput.sendp("Alignt", GYROaccuracy() * 100, 4, suffix: "%"); ;
                ScriptOutput.sendp("Deviation", GYROdeviation(), suffix: "m");
                ScriptOutput.sendp("Direction", GYROdirection);
                ScriptOutput.sendp("Gyro Amount", GYROgyroamount());
            }
            
            //Output Routine
            ScriptOutput.update(Runtime.LastRunTimeMs);
            Echo(ScriptOutput.ToString());
        }
        //Script Specific Commands--------------------------------------------------------------------------------------------------//
        public void setTarget()
        {
            //format: "x y z"
            string argument = commandLine.Argument(1);
            try
            {     
                if (commandLine.Switch("clipboard"))
                {
                    GYROtarget.setPosition(PBArgument.VectorFromCoordinateClipboard(argument));
                }
                else
                {
                    GYROtarget.setPosition(argument);
                }
                ScriptOutput.sendt($"Target changed to [{GYROtarget}]");
            } 
            catch { throw new SEE.InvalidPBArguments($"(Error) Invalid coordinate format: '{argument}'"); }
        }

        public void setDirection()
        {
            string argument = commandLine.Argument(1);
            try
            {
                if (GYROdirections.Contains(argument))
                {
                    GYROdirection = argument;
                    ScriptOutput.sendt($"Direction changed to '{GYROdirection}'");

                }
                else { throw new SEE.ValueDoesNotExist(); }
            }
            catch { throw new SEE.InvalidPBArguments($"(Error) Invalid direction: '{argument}'"); }
        }

        public void gravityMode()
        {
            string argument = commandLine.Argument(1);
            try
            {
                if (argument == null) { GYROgravityMode.toggle(true); }
                else { GYROgravityMode.toggle(argument); }
                ScriptOutput.sendt($"Gravity mode was set to '{GYROgravityMode.state}'");

                if (GYROgravityMode.state) { GYROtarget.customTargetName = "Natural Gravity"; }
                else { GYROtarget.customTargetName = null; }
            }
            catch { throw new SEE.InvalidPBArguments($"(Error) Invalid bool: '{argument}'"); }
        }

        public void reset()
        {
            _GYROsgyrosOff();
            Me.Enabled = false;
            //Me.setValueBool("onOff", false)
            
        }

        //General(Output, help)--------------------------------------------------------------------------------------------------//
        public void showStatus()
        {
            string argument = commandLine.Argument(1);
            try
            {
                ScriptOutput.show.toggle(argument);
            }
            catch { throw new SEE.InvalidPBArguments($"(Error) Invalid bool: '{argument}'"); }
        }
        public void runStatus()
        {
            string argument = commandLine.Argument(1);
            try
            {
                ScriptOutput.run.toggle(argument);
            }
            catch { throw new SEE.InvalidPBArguments($"(Error) Invalid bool: '{argument}'"); }
        }
        public void help()
        {
            string argument = commandLine.Argument(1);
            try
            {
                if (argument == null) { ScriptOutput.help.toggle(true); }
                else { ScriptOutput.help.toggle(argument); }
            }
            catch { throw new SEE.InvalidPBArguments($"(Error) Invalid bool: '{argument}'"); }
        }

    }
}
