using System;
public enum LandingStage { Approach, Flare, Touchdown, Rollout, Stopped }
public struct LandingFrame { public double Height,Range,Speed,Pitch,WheelDistance,Compression;public LandingStage Stage; }
/// <summary>Presentation only, after FlightModel accepts the 500m energy gate.
/// Position integrates the displayed physical speed. Approach is 6x time; rollout 3x.
/// 70m/s touchdown and 1.9m/s² wheel braking stop within the 2500m runway.</summary>
public sealed class LandingSequence {
 public const double ApproachTimeScale=6,RollTimeScale=3,TouchdownRange=800,TouchdownSpeed=70,Braking=1.9;
 readonly double range,height,speed,pitch,approachPhysical;
 public double ApproachSeconds=>approachPhysical/ApproachTimeScale;
 public double Duration=>ApproachSeconds+TouchdownSpeed/Braking/RollTimeScale;
 public LandingSequence(double range,double height,double speed,double pitch){this.range=range;this.height=height;this.speed=Math.Max(1,speed);this.pitch=pitch;approachPhysical=2*(range-TouchdownRange)/(this.speed+TouchdownSpeed);}
 static double Clamp(double x)=>Math.Max(0,Math.Min(1,x));
 static double Smooth(double x){x=Clamp(x);return x*x*(3-2*x);}
 public LandingFrame Evaluate(double seconds){
  double t=Math.Max(0,seconds);var s=new LandingFrame();
  if(t<ApproachSeconds){double u=Clamp(t/ApproachSeconds),physical=t*ApproachTimeScale;s.Speed=speed+(TouchdownSpeed-speed)*u;s.Range=range-speed*physical-(TouchdownSpeed-speed)*physical*physical/(2*approachPhysical);
   // Early descent, then a tangent-continuous flare to the main-wheel contact plane.
   double ground=GroundHeight(6),v=1-u;s.Height=ground+(height-ground)*v*v*(1+.1*u);s.Pitch=pitch+(6-pitch)*Smooth(u/.65);s.Stage=u>.76?LandingStage.Flare:LandingStage.Approach;
  }else{double r=Math.Min(TouchdownSpeed/Braking,(t-ApproachSeconds)*RollTimeScale);s.Speed=seconds>=Duration?0:Math.Max(0,TouchdownSpeed-Braking*r);s.WheelDistance=TouchdownSpeed*r-.5*Braking*r*r;s.Range=TouchdownRange-s.WheelDistance;s.Pitch=6*(1-Smooth(r/4.5));s.Compression=.07*Math.Sin(Math.Min(1,r/1.5)*Math.PI)*Math.Exp(-r*.5);s.Height=GroundHeight(s.Pitch)-s.Compression;s.Stage=seconds>=Duration?LandingStage.Stopped:r<4.5?LandingStage.Touchdown:LandingStage.Rollout;}
  return s;
 }
 public static double GroundHeight(double pitch)=>2+1.24*Math.Cos(pitch*Math.PI/180)+1.8*Math.Sin(pitch*Math.PI/180);
}
public static class VehiclePose {
 public static double NoseWheelDistance(double distance)=>Math.Max(0,distance-(LandingSequence.TouchdownSpeed*4.5-.5*LandingSequence.Braking*4.5*4.5));
 public static float Elevon(float attack,float pitch,bool flare)=>flare?24:Math.Max(-8,Math.Min(24,(attack-4)*.65f));
}
