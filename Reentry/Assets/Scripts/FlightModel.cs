using System;
public enum FlightOutcome { Flying, Landed, Burned, Impact, Short, Overshot }
public sealed class FlightModel {
 public float Altitude=80,Speed=7.8f,Heat=.08f,Range,Elapsed,Attack=.2f,PeakHeat,PeakG,G;
 public readonly int Mission;
 public FlightOutcome Outcome {get;private set;}
 public bool Finished=>Outcome!=FlightOutcome.Flying;
 public float Density=>.2f+1.25f*(1-Altitude/80);
 public float TargetSpeed=>.32f+7.48f*(float)Math.Pow(Altitude/80, .78);
 public float PredictedRange=>Range-Speed*Altitude/(.42f+.8f*Attack)*.72f;
 public FlightModel(int mission){Mission=Math.Max(0,Math.Min(2,mission));Range=Calibration(Mission);}
 private FlightModel(int mission,bool calibration){Mission=mission;Range=10000;}
 static readonly float[] ranges=new float[3];
 static float Calibration(int m){if(ranges[m]>0)return ranges[m];var f=new FlightModel(m,true);for(int i=0;i<20000&&!f.Finished;i++)f.Step(Guidance(f),.02f);return ranges[m]=10000-f.Range;}
 public static bool Guidance(FlightModel f)=>f.Speed>f.TargetSpeed && f.Heat<.76f;
 public void Step(bool hold,float dt){
  if(Finished||dt<=0||float.IsNaN(dt)||float.IsInfinity(dt))return;
  dt=Math.Min(dt,.05f);Elapsed+=dt;
  Attack=Move(Attack,hold?1f:0f,dt*1.3f);
  float density=Density;
  float drag=(.024f+.145f*Attack)*density*(.65f+Speed/12)*(1+Mission*.04f);
  G=drag*7.2f;PeakG=Math.Max(PeakG,G);
  Speed=Math.Max(.16f,Speed-drag*dt);
  Altitude=Math.Max(0,Altitude-(.42f+.8f*Attack)*(.35f+Speed/8)*dt);
  Range-=Speed*.9f*dt;
  Heat=Math.Max(0,Heat+((float)Math.Sqrt(density)*Speed*Speed*Speed*.00065f*(.3f+1.5f*Attack)-.08f-.105f*Heat)*dt);
  PeakHeat=Math.Max(PeakHeat,Heat);
  if(Heat>=1)Outcome=FlightOutcome.Burned;
  else if(Altitude<=0){if(Speed>.85f)Outcome=FlightOutcome.Impact;else if(Range>18)Outcome=FlightOutcome.Short;else if(Range< -18)Outcome=FlightOutcome.Overshot;else Outcome=FlightOutcome.Landed;}
  else if(Elapsed>=230)Outcome=FlightOutcome.Short;
 }
 static float Move(float x,float y,float d)=>x<y?Math.Min(x+d,y):Math.Max(x-d,y);
 public int Score=>Finished&&Outcome==FlightOutcome.Landed?Math.Max(0,(int)(10000-Math.Abs(Range)*150-PeakHeat*1800-Speed*600)):0;
 public string Summary()=>$"{Outcome} time={Elapsed:F2} alt={Altitude:F2} speed={Speed:F2} range={Range:F2} heat={Heat:F2}";
}
