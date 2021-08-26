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

        IMyBroadcastListener IGClistener;
        public void IGCInitListener()
        {
            
        }

        

        public void IGCsendToTag<T>(string tag, T data)
        {
            IGC.SendBroadcastMessage<T>(tag, data);
        }
        public void IGCsendToTags<T>(string[] tags, T data)
        {
            foreach (string t in tags)
            {
                IGC.SendBroadcastMessage<T>(tag, data);
            }
        }


        
    }
}
