using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        //=================[R e a d m e]=================================================================================//
        //
        //Thank you for using Ivos Ship-Alignment script. There is nothing 
        //to worry about if you won't change any settings. 
        //But just in case, dont blow shit up. GLHF
        //
        //=================[Configuration]===============================================================================//
        //
        string GYROcontrollerName = "gpsCenter";
        string GYROdirection = "forward"; //'forward', 'backward', 'up', 'rocket'
        bool GYROgravityMode = false;
        string PBName = "[Ship-Alignment-Script]";
        //
        //=================[Advanced Settings]===========================================================================//
        //
        int GYROgyroLimit = 3;
        float GYROminAngleRad = 0.0001f;
        double GYROctrl_coeff = 0.5;
        //
        //=================[Help]========================================================================================//
        static Dictionary<string, string> helpDic = new Dictionary<string, string>() {
        //
        //
        //Script specfic
        {"setTarget" , "\"X Y Z\" or 'clipboard'-Switch with clipboard GPS format "},
        //General
        {"showStatus", "Toggles visual status output"},
        {"runStatus" , "Toggles visual status output and disables output routine"},
        {"showHelp"  , "Displays all functions defined in here"},
        {"Thomas", "stinkt :D"},
        //
        //
        };
        //==============================================================================================================//
        //
        //                                       !!!END OF CONFIGURATION!!!
        //                           !!!All Changes From Here On Can Break The Script!!!
        //
        //==============================================================================================================//
        #endregion

        //TODO (other scripts):
        //
        //Higher udatefrequency only when moving
        //Trajectory prediction (weapon speed has to be tested, for projectiles)
        //IGC
        //Gyro update only on Frequencyupdate call
        //Richtung wechseln

        //Init Dependecies 
        MyCommandLine commandLine;
        Output ScriptOutput;
        Dictionary<string, Action> commands;
        //Program Initalization
        public Program()
        {
            //Setup Custom Name
            Me.CustomName = PBName;
            //Setup UpdateFrequency 
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            //Setup Important Dependencies 
            commandLine = new MyCommandLine();
            ScriptOutput = new Output(helpInformation: formatDictKeys<string, string>(helpDic));
            commands = new Dictionary<string, Action>();
            //Associate Commands with according Actions
            commands["setTarget"] = setTarget;
            commands["showStatus"] = showStatus;
            commands["runStatus"] = runStatus;
            commands["help"] = help;

            //Initialize GyroAligment Configuration
            ScriptOutput.assert(GYROInit);
            //Load Storage Routine
            string[] storedData = Storage.Split(';');
            if (storedData.Count() != 0) GYROtarget.setPosition(storedData[0]);
        }

        //Save Routine
        public void Save()
        {
            //Saves Latest target
            if (GYROtarget != null) { Storage = GYROtarget.ToString(); }
        }

        //Main Call
        public void Main(string argument, UpdateType updateSource)
        {
            
            //Parses Arguments to called function
            //Format: command argument1 argument2 ... 
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

            //Main Script Logic
            if (GYROready)
            {
                GYRORun();
                ScriptOutput.sendp("Targeting", GYROtarget.ToString());
                ScriptOutput.sendp("Distance", GYROtargetDistance(), suffix: "m");
                ScriptOutput.sendp("Alignt", GYROaccuracy() * 100, 4, suffix: "%"); ;
                ScriptOutput.sendp("Deviation", GYROdeviation(), suffix: "m");
                ScriptOutput.sendp("Gyro Amount", GYROgyroamount());
            }
            
            //Output Routine
            ScriptOutput.update(Runtime.LastRunTimeMs);
            Echo(ScriptOutput.ToString());
        }
        //Script Specific Commands--------------------------------------------------------------------------------------------------//
        public void setTarget()
        {
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
                ScriptOutput.sendt($"Position changed to [{GYROtarget.ToString()}]");
            } 
            catch { throw new SEE.InvalidPBArguments($"(Error) Invalid coordinate format: '{argument}'"); }
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
