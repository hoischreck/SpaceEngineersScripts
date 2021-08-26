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
    partial class Program
    {
        //Used as a timer by counting passed time and checking
        //if certain delta was passed
        class Timer
        {
            double start;
            double delta;
            public int seconds;
            float errFactor = 0.03f;

            public Timer(int deltaValue) { start = 0; delta = deltaValue; seconds = deltaValue / 1000; }
            public bool check(double passedTime)
            {
                start += passedTime;
                if (start >= seconds * errFactor) { start = 0; return true; }
                else { return false; }
            }

        }

        //Full Output prompt with exception Handling for Status and Data output.
        //Divides into temporary and permanent data. Therefore, can be be used 
        //to display all information. Does also include exceptions and help.
        class Output
        {
            Status status;
            RunningIcon icon;
            Timer statusTimer;
            public Toggle run;
            public Toggle show;
            public ToggleStr help;
            public Output(string helpInformation = "", int temporaryStatusTime = 10, string[] wrapperFormat = null)
            {
                status = new Status(wrapperFormat);
                icon = new RunningIcon();
                statusTimer = new Timer(temporaryStatusTime * 1000);
                run = new Toggle();
                show = new Toggle();
                help = new ToggleStr(helpInformation, false);
            }

            //Updates internal timer, message status and Icon status
            public void update(double timeSinceLastCall)
            {
                if (run.state)
                {
                    if (statusTimer.check(timeSinceLastCall)) { status.update(); }
                    icon.update();
                }
            }

            //Adds temporary message to data or resets countdown if already existing
            public void sendt(string statusMessage, int timeDelta = -1)
            {
                if (run.state)
                {
                    if (timeDelta == -1)
                    {
                        status.addTemporary(statusMessage, statusTimer.seconds);
                    }
                    else
                    {
                        status.addTemporary(statusMessage, timeDelta);
                    }
                }
            }

            //Adds permanent status message or updates contents of already existing
            public void sendp(string statusName, string statusState = "")
            {
                if (run.state)
                {
                    status.addPermanent(statusName, statusState);
                }
            }

            //Adds permanent status message with number as state that can also be rounded and concatenated 
            //with a suffix or updates state of already  existing status.
            public void sendp(string statusName, double statusState, int roundDecimalPoints = 3, string suffix = "")
            {
                if (run.state)
                {
                    status.addPermanent(statusName, Math.Round(statusState, roundDecimalPoints).ToString() + suffix);
                }
            }

            //Deletes permanent status
            public void deletep(string statusName)
            {
                if (run.state)
                {
                    status.removePermanent(statusName);
                }

            }

            //Returns full formated data. Should be retrieved by Echo()
            public override string ToString()
            {
                if (run.state && show.state) 
                { return $"{status}\n{icon}\n{help}"; }
                else 
                {     
                    //'run' fully disables Status Routine, 'show' only disables output
                    return $"Output disabled\n{help}"; 
                }
            }

            //Calls any function, catches thrown erros and sends them to output
            public void assert(Action func) 
            {
                try
                {
                    func();
                }
                catch (Exception e)
                {
                    if (e is SEE) {
                        if (((SEE)e).Priority) { sendp($"(Error) {e.Message}"); }
                        else { sendt($"(Error) {e.Message}"); }    
                    }
                    else { sendp("(FATAL-ERROR) Unexpected Error Occured", e.Message); }
                }
            }

            //Holds all Data printed in Status (excludes  help)
            class Status
            {
                Dictionary<string, int> temporaryData;
                Dictionary<string, string> permanentData;
                //wrapper must be a string array of 2 elements
                string[] wrapper;
                public Status(string[] status_wrapper)
                {
                    if (status_wrapper != null)
                    { wrapper = status_wrapper; }
                    else
                    {
                        wrapper = new string[2]
                        {
                            "--------------------------------------\n",
                            "--------------------------------------"
                        };
                    }
                    temporaryData = new Dictionary<string, int>();
                    permanentData = new Dictionary<string, string>();
                }

                //Decrements all counters from temporary messages or deletes those messages when timer meets 0
                public void update()
                {
                    List<string> delete = new List<string>();
                    List<string> decrement = new List<string>();
                    foreach (KeyValuePair<string, int> kv in temporaryData)
                    {
                        if (kv.Value < 1) { delete.Add(kv.Key); } else { decrement.Add(kv.Key); }

                    }
                    foreach (string key in delete)
                    {
                        temporaryData.Remove(key);
                    }
                    foreach (string key in decrement)
                    {
                        temporaryData[key]--;
                    }

                }

                public void addTemporary(string statusMessage, int value)
                {
                    if (temporaryData.ContainsKey(statusMessage)) { temporaryData[statusMessage] = value; }
                    else { temporaryData.Add(statusMessage, value); }

                }

                public void addPermanent(string statusMessage, string status)
                {
                    if (permanentData.ContainsKey(statusMessage)) { permanentData[statusMessage] = status; }
                    else { permanentData.Add(statusMessage, status); }
                }

                public void removePermanent(string statusName) { permanentData.Remove(statusName); }

                //Returns the whole internal string data as a formatted string
                public override string ToString()
                {
                    try
                    {
                        string o = "";
                        if (permanentData.Count() > 0)
                        {
                            o += "[Status]:\n";
                            foreach (KeyValuePair<string, string> kv in permanentData)
                            {
                                if (kv.Value != "") { o += $"{kv.Key}: {kv.Value}\n"; }
                                else { o += $"{kv.Key}\n"; }
                            }
                            o += "\n";
                        }
                        o += "[Execution]:\n";
                        if (temporaryData.Count == 0) { o += "Process return 0\n"; }
                        else
                        {
                            foreach (KeyValuePair<string, int> kv in temporaryData)
                            {
                                o += $"{kv.Key} [{kv.Value}]\n";
                            }
                        }
                        return wrapper[0] + o + wrapper[1];
                    }
                    catch
                    {
                        //if null, probably invalid wrapper
                        return "[FATAL-ERROR] Could not return Status";
                    }
                }
            }

            //Simple class to display a simple loading icon to show script activity
            class RunningIcon
            {
                List<string> states;
                int length;
                int i;
                public RunningIcon() { i = 0; states = new List<string> { "|", "/", "--", "\\" }; length = this.states.Count(); }
                public void update()
                {
                    if (i < length - 1) { i++; }
                    else { i = 0; }
                }
                public void setStates(List<string> icon_states)
                {
                    states = icon_states;
                }
                public override string ToString()
                {
                    return states[i];
                }
            }
        }
    }
}
