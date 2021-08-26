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
        public class SEE : Exception
        {
            //false = Temporary, true = Permanent
            public bool Priority;
            public SEE(string message, bool priority = false) : base(message) { Priority = priority; }
            public class ScriptException : SEE { public ScriptException(string errMsg = "(Error) Runtime Script-Exception", bool priority = false) : base(errMsg, priority) { }}
            public class MissingBlockException : SEE {public MissingBlockException(string errMsg = "(Error) MissingBlock", bool priority = false) : base(errMsg, priority) { }}
            public class InvalidPBArguments : SEE { public InvalidPBArguments(string errMsg = "(Error) InvalidPBArguments", bool priority = false) : base(errMsg, priority) { } }
            public class InitNotCompleted : SEE { public InitNotCompleted(string errMsg = "(Error) Waiting for Initialization", bool priority = false) : base(errMsg, priority) { } }
        }
    }
}
