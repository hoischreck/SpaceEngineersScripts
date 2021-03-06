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
    partial class Program {
        //Simple class for toggling between true and false with strings as argument
        public class Toggle
        {
            public bool state;
            public Toggle(bool stateOfProperty = true) { state = stateOfProperty; }
            public void toggle(bool newState) { state = newState; }
            public void toggle(string newState)
            {
                if (newState == "1" || newState == "true" || newState == "True") { state = true; }
                else if (newState == "0" || newState == "false" || newState == "False") { state = false; }
                else { throw new ArgumentException(); }
            }
            public override string ToString()
            {
                return state.ToString();
            }
        }

        //Simple class for toggling between true and false with strings as argument
        //while also having string data, which is returnable if toggle.state == true
        public class ToggleStr
        {
            public bool state;
            public string data;
            public ToggleStr(string toggleData, bool stateOfProperty = true) { state = stateOfProperty; data = toggleData; }
            public void toggle(bool newState) { state = newState; }
            public void toggle(string newState)
            {
                if (newState == "1" || newState == "true" || newState == "True") { state = true; }
                else if (newState == "0" || newState == "false" || newState == "False") { state = false; }
                else { throw new ArgumentException(); }
            }
            public override string ToString()
            {
                if (state) { return data.ToString(); }
                return "";
            }
        }
        
        //Collection of useful functions to parse and reformat data from strings
        //Mainly used for argument analysis
        public class PBArgument
        {
            static public string splitCoordinatesFromClipboard(string coordinates)
            {
                string[] data = coordinates.Split(':');
                return $"{data[2]} {data[3]} {data[4]}";
            }
            static public double[] splitIntoDouble(string s, char delimiter = ' ')
            {
                //only returns if completly successful
                try 
                { 
                string[] sub = s.Split(delimiter);
                int len = sub.Count();
                double[] array = new double[len];
                for (int i = 0; i<len; i++)
                {
                    array[i] = double.Parse(sub[i]);
                }
                return array;
                }
                catch
                {
                    return null;
                }
            }
            static public Vector3D VectorFromString(string s)
            {
                double[] values = splitIntoDouble(s);
                return new Vector3D(values[0], values[1], values[2]);
            }
            static public Vector3D VectorFromCoordinateClipboard(string s)
            {
                return VectorFromString(splitCoordinatesFromClipboard(s));
            }
        }

        public class Utilities
        {
            //Utility-function for formating keys of dict for help information
            static public string formatDictKeys<T1, T2>(Dictionary<T1, T2> dictionary)
            {
                string o = "";
                foreach (T1 key in dictionary.Keys)
                {
                    o += $"-{key}\n";
                }
                return o;
            }
        }

        //simple class that defiens a target represented by a vector which holds its GPS
        public class Target
        {
            Vector3D position;
            public string customTargetName;
            public bool showCustomName = true;
            public Target(double x = 0, double y = 0, double z = 0) { position = new Vector3D(x, y, z); }
            public Target(string coordinateString) { setPosition(coordinateString); }

            public Vector3D getPosition() { return position; }
            public void setPosition(Vector3D v) { position = v; }
            public void setPosition(double x, double y, double z) { position = new Vector3D(x, y, z); }
            public void setPosition(string s, bool standard = false)
            {
                try
                {
                    double[] array = PBArgument.splitIntoDouble(s);
                    position = new Vector3D(array[0], array[1], array[2]);
                }
                catch
                {
                    if (standard) { position = new Vector3D(0, 0, 0); }
                    else { throw new ArgumentException(); }
                }
            }
            public string getXYZAsString()
            {
                return $"{position.X} {position.Y} {position.Z}";
            }
            public override string ToString()
            {
                if (customTargetName != null && showCustomName == true)
                {
                    return customTargetName;
                }
                else { return getXYZAsString(); }
            }
        }        
    }
}
