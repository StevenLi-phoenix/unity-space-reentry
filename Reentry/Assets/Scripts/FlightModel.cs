using System;
public enum FlightOutcome {Flying,Landed,Burned,Impact,Short,Overshot,Structural,Skip}
/// <summary>Planar spherical-Earth entry. SI internals; exponential atmosphere, aerodynamic
/// forces, gravity/curvature and Sutton-Graves-style stagnation heating with radiative cooling.
/// Coefficients describe a fictional lifting body, not a certified flight model.</summary>
public sealed class FlightModel {
 public const double EarthRadius=6371000,Mass=9000,Area=26,GateHeight=500;
 public double Height=70000,Velocity=6500,Gamma=-7*Math.PI/180,Downrange,Alpha=10*Math.PI/180,Temperature=300,Time,Damage;
 public readonly int Mission;public readonly double Destination;
 public float Altitude=>(float)(Height/1000);public float Speed=>(float)(Velocity/1000);
 public float Range=>(float)((Destination-Downrange)/1000);public float Heat=>(float)Math.Max(0,(Temperature-300)/1900);
 public float Attack=>(float)(Alpha*180/Math.PI);public float Elapsed=>(float)Time;
 public float PeakHeat,PeakG,G;public double DynamicPressure,HeatFlux,DragAcceleration,LiftAcceleration,PeakPressure;
 public FlightOutcome Outcome {get;private set;}public bool Finished=>Outcome!=FlightOutcome.Flying;
 public double DescentRate=>-Velocity*Math.Sin(Gamma);public double LandingError=>(Downrange+4000-Destination)/1000;
 public FlightModel(int mission){Mission=Math.Max(0,Math.Min(2,mission));Destination=(1354+25*Mission)*1000;}
 FlightModel(FlightModel f){Mission=f.Mission;Destination=f.Destination;Height=f.Height;Velocity=f.Velocity;Gamma=f.Gamma;Downrange=f.Downrange;Alpha=f.Alpha;Temperature=f.Temperature;Time=f.Time;}
 public static double DensityAt(double h)=>1.225*Math.Exp(-Math.Max(-500,h)/7200);
 public static void Coefficients(double alpha,double velocity,out double cd,out double cl){double sin=Math.Sin(alpha);cd=.18+1.5*sin*sin;cl=.48*Math.Sin(2*alpha);double sub=Math.Max(0,Math.Min(1,(700-velocity)/400));double cls=2*Math.PI*alpha*(alpha<.3?1:Math.Max(.15,1-(alpha-.3)*2));cd=cd*(1-sub)+(.035+.09*cls*cls+.65*sin*sin)*sub;cl=cl*(1-sub)+cls*sub;}
 public void Step(bool held,double dt){if(Finished||dt<=0||double.IsNaN(dt)||double.IsInfinity(dt))return;while(dt>0&&!Finished){double h=Math.Min(dt,.05);Integrate(held,h,true);dt-=h;}}
 void Integrate(bool held,double dt,bool failures,double attitude=-1){
  double target=(Velocity<700?(held?12:4):(held?40:5))*Math.PI/180;
  if(attitude>=0)target=(Velocity<700?4+8*attitude:5+35*attitude)*Math.PI/180;
  Alpha+=Math.Max(-dt*.08,Math.Min(dt*.08,target-Alpha));
  double rho=DensityAt(Height),q=.5*rho*Velocity*Velocity;Coefficients(Alpha,Velocity,out double cd,out double cl);
  double drag=q*Area*cd/Mass,lift=q*Area*cl/Mass,g=9.80665*Math.Pow(EarthRadius/(EarthRadius+Height),2);
  double dh=Velocity*Math.Sin(Gamma),dx=Velocity*Math.Cos(Gamma)*EarthRadius/(EarthRadius+Height),dv=-drag-g*Math.Sin(Gamma),dg=lift/Math.Max(10,Velocity)+(Velocity/(EarthRadius+Height)-g/Math.Max(10,Velocity))*Math.Cos(Gamma);
  Height+=dh*dt;Downrange+=dx*dt;Velocity=Math.Max(10,Velocity+dv*dt);Gamma+=dg*dt;
  double heating=1.83e-4*Math.Sqrt(rho/1.2)*Velocity*Velocity*Velocity,radiation=.85*5.67e-8*(Math.Pow(Temperature,4)-Math.Pow(300,4));
  Temperature=Math.Max(250,Temperature+dt*(heating-radiation)/55000);Time+=dt;DynamicPressure=q;HeatFlux=heating;DragAcceleration=drag;LiftAcceleration=lift;G=(float)(drag/9.80665);PeakG=Math.Max(PeakG,G);PeakHeat=Math.Max(PeakHeat,Heat);PeakPressure=Math.Max(PeakPressure,q);Damage+=dt*Math.Max(0,(q-100000)/100000);
  if(!failures)return;
  if(q>250000||Damage>2)Outcome=FlightOutcome.Structural;
  else if(Temperature>2200)Outcome=FlightOutcome.Burned;
  else if(Height>120000)Outcome=FlightOutcome.Skip;
  else if(Height<=GateHeight){if(Velocity>180||DescentRate>35)Outcome=FlightOutcome.Impact;else if(LandingError< -2)Outcome=FlightOutcome.Short;else if(LandingError>2)Outcome=FlightOutcome.Overshot;else Outcome=FlightOutcome.Landed;}
  else if(Time>=2200)Outcome=FlightOutcome.Short;
 }
 /// <summary>Projected runway location for a sustained command. Loads are not ignored in the
 /// real simulation; this geometric forecast is shown alongside separate thermal/load warnings.</summary>
 public double PredictLanding(bool held){var f=new FlightModel(this);for(int i=0;i<2600&&f.Height>GateHeight&&f.Height<120000;i++)f.Integrate(held,1,false);return (f.Downrange+4000)/1000;}
 public double PredictCurrent(){var f=new FlightModel(this);double low=Velocity<700?4:5,span=Velocity<700?8:35;double attitude=Math.Max(0,Math.Min(1,(Alpha*180/Math.PI-low)/span));for(int i=0;i<2600&&f.Height>GateHeight&&f.Height<120000;i++)f.Integrate(false,1,false,attitude);return (f.Downrange+4000)/1000;}
 public static bool Guidance(FlightModel f){if(f.Height<1500)return f.Gamma< -6*Math.PI/180;double low=f.PredictLanding(false),high=f.PredictLanding(true);double ratio=(f.Destination/1000-low)/(high-low);return (Math.Abs(high-low)>.001&&ratio>.5)||(f.DynamicPressure>60000&&f.Velocity>2000);}
 public int Score=>Outcome==FlightOutcome.Landed?Math.Max(0,(int)(10000-Math.Abs(LandingError)*1000-PeakHeat*1000-Math.Max(0,PeakG-5)*100)):0;
 public string Summary()=>$"{Outcome} time={Time:F2} alt={Height:F2}m velocity={Velocity:F2}m/s range={Range:F2}km gamma={Gamma*180/Math.PI:F2}deg temp={Temperature:F0}K q={DynamicPressure:F0}Pa";
}
