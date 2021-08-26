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
        //=============================================[Configuration]===================================================
        //
        string GYROcontrollerName = "gpsCenter";
        string GYROdirection = "forward"; //'forward', 'backward', 'up', 'rocket'
        bool GYROgravityMode = false;
        string PBName = "[Ship-Alignment-Script]";
        //
        //===========================================[Advanced Settings]=================================================
        //
        int GYROgyroLimit = 3;
        float GYROminAngleRad = 0.0001f;
        double GYROCTRL_COEFF = 0.5;
        //
        //================================================[Help]=========================================================
        static Dictionary<string, string> helpDic = new Dictionary<string, string>() {
        //
        //
        {"setTarget" , "\"X Y Z\" or 'clipboard'-Switch with clipboard GPS format "},
        {"showStatus", "Toggles visual status output"},
        {"runStatus" , "Toggles visual status output and disables output routine"},
        {"showHelp"  , "Displays all functions defined in here"},
        {"Thomas", "stinkt :D"},
        //
        //
        };
        //==============================================================================================================
        //
        //                                      !!!END OF CONFIGURATION!!!
        //                           !!!All Changes From Here On Can Break The Script!!!
        //
        //==============================================================================================================
        #endregion
        
        //TODO (other scripts):
        //
        //Higher udatefrequency only when moving
        //Trajectory prediction (weapon speed has to be tested, for projectiles)
        //IGC
        //Gyro update only on Frequencyupdate call
        //
        //

        MyCommandLine commandLine;
        Output status_out;
        Dictionary<string, Action> commands;
        public Program()
        {
            //Setup Custom Name
            Me.CustomName = PBName;
            //Setup UpdateFrequency 
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            //Setup Important Dependencies 
            commandLine = new MyCommandLine();
            status_out = new Output(helpInformation: formatDictKeys<string, string>(helpDic));
            commands = new Dictionary<string, Action>();
            //Associate Commands with according Actions
            commands["setTarget"] = setTarget;
            commands["showStatus"] = showStatus;
            commands["runStatus"] = runStatus;
            commands["help"] = help;

            //Initialize GyroAligment Configuration
            status_out.assert(GYROInit);
            //Storage Routine
            //string[] storedData = Storage.Split(';');
            //if (storedData.Count() != 0) GYROtarget.setPosition(storedData[0]);
        }

        public void Save()
        {
            //Saves Latest target
            Storage = GYROtarget.ToString();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            
            if (commandLine.TryParse(argument))
            {
                Action commandAction;
                string command = commandLine.Argument(0);
                string output;
                if (commandLine.Switch("help") && helpDic.TryGetValue(command, out output))
                {
                    status_out.sendt(output);
                }
                else if (commands.TryGetValue(command, out commandAction)) 
                {
                    status_out.assert(commandAction);
                } 
                else { status_out.sendt($"(Error) Invalid command: '{command}'"); }
            }

            if (GYROready) 
            {
                GYRORun();
                status_out.sendp("Targeting", GYROtarget.ToString());
                status_out.sendp("Distance", GYROtargetDistance(), suffix: "m");
                status_out.sendp("Alignt", GYROaccuracy()*100, 4, suffix: "%");;
                status_out.sendp("Deviation", GYROdeviation(), suffix: "m");
                status_out.sendp("Gyro Amount", GYROgyroamount());
            }
            status_out.update(Runtime.LastRunTimeMs);
            Echo(status_out.ToString());
        }
        //Script Commands--------------------------------------------------------------------------------------------------//
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
                status_out.sendt($"Position changed to [{GYROtarget.ToString()}]");
            } 
            catch { throw new SEE.InvalidPBArguments($"(Error) Invalid coordinate format: '{argument}'"); }
        }
        //Status Commands--------------------------------------------------------------------------------------------------//
        public void showStatus()
        {
            string argument = commandLine.Argument(1);
            try
            {
                status_out.show.toggle(argument);
            }
            catch { throw new SEE.InvalidPBArguments($"(Error) Invalid bool: '{argument}'"); }
        }
        public void runStatus()
        {
            string argument = commandLine.Argument(1);
            try
            {
                status_out.run.toggle(argument);
            }
            catch { throw new SEE.InvalidPBArguments($"(Error) Invalid bool: '{argument}'"); }
        }
        //Help------------------------------------------------------------------------------------------------------------//
        public void help()
        {
            string argument = commandLine.Argument(1);
            try
            {
                if (argument == null) { status_out.help.toggle(true); }
                else { status_out.help.toggle(argument); }
            }
            catch { throw new SEE.InvalidPBArguments($"(Error) Invalid bool: '{argument}'"); }
        }

    }
}
