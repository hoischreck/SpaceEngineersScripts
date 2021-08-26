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
		bool GYROready = false;
		List<string> GYROdirections = new List<string> { "forward", "backward", "up", "rocket" };

		//string GYROcontrollerName = "gpsCenter";
		//string GYROdirection = "forward";
		//int GYROgyroLimit = 3;
		//bool GYROgravityMode = false;
		//float GYROminAngleRad = 0.0001f;
		//double GYROCTRL_COEFF = 0.5;
		Target GYROtarget = new Target();
		double? GYROdAimed = null;
		IMyRemoteControl GYROgpsCenterController;
		List<IMyGyro> GYROgyros;
		Vector3D GYROvectorToTarget;

		public void GYROInit()
		{
			List<IMyTerminalBlock> controllersList = new List<IMyTerminalBlock>();
			GridTerminalSystem.SearchBlocksOfName(GYROcontrollerName, controllersList, controller => controller is IMyRemoteControl);
			if (controllersList.Count() < 1) throw new SEE.MissingBlockException($"(Error) No 'Remote Controll' with name '{GYROcontrollerName}' found", true);
			GYROgpsCenterController = (IMyRemoteControl)controllersList[0];
			GYROvectorToTarget = GYROtarget.getPosition() - GYROgpsCenterController.GetPosition();
			_GYROsetup();
			GYROready = true;
		}

		public double GYROangle() { if (GYROready && GYROdAimed != null) { return GYROdAimed.Value; } else { throw new SEE.InitNotCompleted(); } }
		public double GYROtargetDistance() { if (GYROready) { return GYROvectorToTarget.Length(); } else { throw new SEE.InitNotCompleted(); } }
		public double GYROaccuracy() { if (GYROready && GYROdAimed != null) { return 1 - (GYROdAimed.Value / (double)180); } else { throw new SEE.InitNotCompleted(); } }
		public double GYROdeviation() { if (GYROready && GYROdAimed != null) { return Math.Tan(GYROdAimed.Value) * GYROvectorToTarget.Length(); } else { throw new SEE.InitNotCompleted(); } }
		public double GYROgyroamount() { if (GYROready && GYROgyros != null) { return GYROgyros.Count(); } else { throw new SEE.InitNotCompleted(); } }

		public void GYRORun()
		{
			if (GYROready)
			{
				if (GYROgravityMode)
				{
					//Gravity Mode
					GYROdAimed = _GYROMain(GYROdirection);
				}
				else
				{
					//Targeting mode
					GYROvectorToTarget = GYROtarget.getPosition() - GYROgpsCenterController.GetPosition();
					GYROdAimed = _GYROMain(GYROdirection, GYROvectorToTarget, GYROgpsCenterController);
				}
			}
			else { throw new SEE.InitNotCompleted(); }
		}
		double _GYROMain(string argument)
		{
			Vector3D grav = (GYROgpsCenterController as IMyRemoteControl).GetNaturalGravity();
			return _GYROMain(argument, grav, GYROgpsCenterController);
		}
		double _GYROMain(string argument, Vector3D vDirection, IMyTerminalBlock gyroControlPoint)
		{
			double bAligned = 0;
			//	Echo("GyroMain(" + argument + ",VECTOR3D)");	 
			Matrix or;
			gyroControlPoint.Orientation.GetMatrix(out or);

			Vector3D down;
			argument = argument.ToLower();
			if (argument.Contains("rocket"))
				down = or.Backward;
			else if (argument.Contains("up"))
				down = or.Up;
			else if (argument.Contains("backward"))
				down = or.Backward;
			else if (argument.Contains("forward"))
				down = or.Forward;
			else
				down = or.Down;

			vDirection.Normalize();

			for (int i = 0; i < GYROgyros.Count; ++i)
			{
				var g = GYROgyros[i];
				g.Orientation.GetMatrix(out or);
				var localDown = Vector3D.Transform(down, MatrixD.Transpose(or));
				var localGrav = Vector3D.Transform(vDirection, MatrixD.Transpose(g.WorldMatrix.GetOrientation()));

				//Since the gyro ui lies, we are not trying to control yaw,pitch,roll but rather we 
				//need a rotation vector (axis around which to rotate) 
				var rot = Vector3D.Cross(localDown, localGrav);
				double dot2 = Vector3D.Dot(localDown, localGrav);
				double ang = rot.Length();
				ang = Math.Atan2(ang, Math.Sqrt(Math.Max(0.0, 1.0 - ang * ang)));
				if (dot2 < 0) ang = Math.PI - ang; // compensate for >+/-90
				if (ang < GYROminAngleRad)
				{ // close enough 
					g.SetValueBool("Override", false);
					continue;
				}
				//		Echo("Auto-Level:Off level: "+(ang*180.0/3.14).ToString()+"deg"); 

				double ctrl_vel = g.GetMaximum<float>("Yaw") * (ang / Math.PI) * GYROCTRL_COEFF;
				ctrl_vel = Math.Min(g.GetMaximum<float>("Yaw"), ctrl_vel);
				ctrl_vel = Math.Max(0.01, ctrl_vel);
				rot.Normalize();
				rot *= ctrl_vel;
				float pitch = (float)rot.GetDim(0);
				g.SetValueFloat("Pitch", pitch);

				float yaw = -(float)rot.GetDim(1);
				g.SetValueFloat("Yaw", yaw);

				float roll = -(float)rot.GetDim(2);
				g.SetValueFloat("Roll", roll);
				//		g.SetValueFloat("Power", 1.0f); 
				g.SetValueBool("Override", true);
				bAligned = ang;
			}
			return bAligned;
		}


		string _GYROsetup()
		{
			var l = new List<IMyTerminalBlock>();
			_GYROsgyrosOff(); // turn off any working gyros from previous runs
							  // NOTE: Uses grid of controller, not ME, nor localgridfilter
			GridTerminalSystem.GetBlocksOfType<IMyGyro>(l, x => x.CubeGrid == GYROgpsCenterController.CubeGrid);
			var l2 = new List<IMyTerminalBlock>();
			for (int i = 0; i < l.Count; i++)
			{
				if (l[i].CustomName.Contains("!NAV") || l[i].CustomData.Contains("!NAV"))
				{
					continue;
				}
				l2.Add(l[i]);
			}
			GYROgyros = l2.ConvertAll(x => (IMyGyro)x);
			if (GYROgyros.Count > GYROgyroLimit)
				GYROgyros.RemoveRange(GYROgyroLimit, GYROgyros.Count - GYROgyroLimit);
			return "G" + GYROgyros.Count.ToString("00");
		}
		void _GYROsgyrosOff()
		{
			if (GYROgyros != null)
			{
				for (int i = 0; i < GYROgyros.Count; ++i)
				{
					GYROgyros[i].SetValueBool("Override", false);
				}
			}
		}
	}
}
