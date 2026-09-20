using System;
/// <summary>Presentation decisions use measured motion; input intent is not flight state.</summary>
public static class FlightPresentation {
 public static string VerticalMotion(double descent)=>Math.Abs(descent)<.5?"LEVEL FLIGHT":$"{Math.Abs(descent):0} m/s {(descent<0?"CLIMB":"DESCENT")}";
 public static bool Hold(bool key,bool pointer,bool overUi)=>key||(pointer&&!overUi);
 public static bool VisibleDestination(double altitudeKm,double rangeKm)=>Math.Abs(rangeKm)/6371<Math.Acos(6371/(6371+Math.Max(0,altitudeKm)));
 public static string Guidance(double released,double held,double current,double target){
  if(Math.Abs(current-target)<2)return "Keep the landing forecast centered";
  return (target-current)*(held-released)>0?"HOLD: nose up moves the forecast toward the runway":"RELEASE: nose down moves the forecast toward the runway";
 }
}
/// <summary>Deterministic coastal terrain in km relative to Aster. The runway and flight
/// corridor stay at sea level; mountains sit safely outside the one-button flight plane.</summary>
public static class GroundReference {
 public static double Coast(double x,double z)=>x+12+6*Math.Sin(z*.027)+3*Math.Sin(z*.081);
 public static double Height(double x,double z){
  double land=Math.Max(0,Math.Min(1,Coast(x,z)/3));
  double clear=Math.Max(0,Math.Min(1,(Math.Abs(x)-2)/8));
  double ridge=Math.Pow(Math.Sin(x*.083+Math.Sin(z*.027)),2)*Math.Pow(Math.Cos(z*.045),2);
  return land*clear*(.018+1.1*ridge);
 }
}
