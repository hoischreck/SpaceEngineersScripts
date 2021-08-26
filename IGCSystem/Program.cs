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

        IMyBroadcastListener listener;
        string tag = "test";
        string data = "";
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            listener = IGC.RegisterBroadcastListener(tag);
            listener.SetMessageCallback(tag);
        }



        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Echo($"'{data}'");
            Echo($"'{argument}'");

            if (argument == "send") 
            {
                Echo("Sending");
                IGC.SendBroadcastMessage(tag, "Das ist ein Test");
            }
            if ((updateSource & UpdateType.IGC) > 0)
            {
                data += "Received";
                while (listener.HasPendingMessage)
                {
                    var msg = listener.AcceptMessage();
                    if (msg.Data is string)
                    {
                        data += msg.Data.ToString() + "\n";
                    } 
                }
            }
        }
    }
}
