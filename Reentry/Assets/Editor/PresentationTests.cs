using System;
using UnityEngine;
public static class PresentationTests {
 static void Check(bool value,string message){if(!value)throw new Exception("PRESENTATION TEST: "+message);}
 public static void Run(){
  Check(FlightPresentation.VerticalMotion(-387)=="387 m/s CLIMB","climb must never say descent");
  Check(FlightPresentation.VerticalMotion(28)=="28 m/s DESCENT","positive descent");
  Check(FlightPresentation.VerticalMotion(.1)=="LEVEL FLIGHT","level deadband");
  Check(!FlightPresentation.Hold(false,true,true),"HUD pointer must not command flight");
  Check(FlightPresentation.Hold(true,true,true),"keyboard still controls flight over HUD");
  Check(FlightPresentation.Hold(false,true,false),"world pointer controls flight");
  Check(!FlightPresentation.VisibleDestination(45,973),"Earth occludes distant destination");
  Check(FlightPresentation.VisibleDestination(2,20),"nearby ground marker visible");
  Check(FlightPresentation.Guidance(1300,1600,1310,1354).StartsWith("HOLD"),"short forecast must move toward target even if held endpoint overshoots");
  Check(FlightPresentation.Guidance(1600,1300,1310,1354).StartsWith("RELEASE"),"handle reversed lift/drag range response");
  Check(Math.Abs(GroundReference.Height(0,0))<.00001,"runway stays sea level");
  Check(GroundReference.Height(0,5)<.005,"approach corridor clear");
  double peak=0;for(int x=10;x<=50;x+=5)for(int z=-40;z<=40;z+=5)peak=Math.Max(peak,GroundReference.Height(x,z));Check(peak>.5,"terrain region contains elevation references");
  Check(GroundReference.Coast(-35,0)<0&&GroundReference.Coast(0,0)>0,"coast has both sea and land");
  Debug.Log("PRESENTATION_TESTS_PASSED 14 motion/input/horizon/guidance/terrain checks");
 }
}
