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
        //Space Engineers Exception class for Error Handling
        public class SEE : Exception
        {
            //Priority: false = Temporary, true = Permanent (output-status-type)
            public bool Priority;
            public SEE(string message, bool priority = false) : base(message) { Priority = priority; }
            
            //Scripting Errors for debugging---------------------------------------------------------------------------------------------------------------//

            //A General Error Occured due to internal script errors
            public class ScriptException : SEE { public ScriptException(string errMsg = "(Fatal-Error) Runtime Script-Exception", bool priority = true) : base(errMsg, priority) { } }
            //Call of not initialzed object
            public class InitNotCompleted : SEE { public InitNotCompleted(string errMsg = "(Fatal-Error) Waiting for Initialization", bool priority = true) : base(errMsg, priority) { } }

            //Ingame Errors for user feedback--------------------------------------------------------------------------------------------------------------//

            //A desired block was not found
            public class MissingBlockException : SEE { public MissingBlockException(string errMsg = "(Error) MissingBlock", bool priority = false) : base(errMsg, priority) { } }
            //Givin arguments we invalid
            public class InvalidPBArguments : SEE { public InvalidPBArguments(string errMsg = "(Error) InvalidPBArguments", bool priority = false) : base(errMsg, priority) { } }
            
        }
    }
}