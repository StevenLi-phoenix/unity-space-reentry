using System;
using UnityEngine;
public static class FlightTests {
 static int checks;
 static void Check(bool ok,string message){checks++;if(!ok)throw new Exception("FLIGHT TEST: "+message);}
 public static void Run(){
  checks=0;
  var idle=new FlightModel(0);float h=idle.Altitude;idle.Step(false,0);Check(idle.Altitude==h,"zero delta immutable");
  foreach(int mission in new[]{0,1,2}) {
   var good=new FlightModel(mission);for(int i=0;i<20000&&!good.Finished;i++)good.Step(FlightModel.Guidance(good),.02f);
   Check(good.Outcome==FlightOutcome.Landed,"guidance must land mission "+mission+" "+good.Summary());
   Check(good.Elapsed>35&&good.Elapsed<230,"bounded run duration");
   foreach(bool hold in new[]{false,true}){var f=new FlightModel(mission);for(int i=0;i<20000&&!f.Finished;i++){f.Step(hold,.02f);Check(!float.IsNaN(f.Heat)&&f.Speed>=0&&f.Altitude>=0,"finite and nonnegative");}Check(f.Finished,"constant policy terminates");Check(f.Outcome!=FlightOutcome.Landed,"constant input cannot win");}
  }
  var a=new FlightModel(0);var b=new FlightModel(0);for(int i=0;i<1000;i++){bool press=i%90<40;a.Step(press,.02f);b.Step(press,.02f);}Check(a.Summary()==b.Summary(),"deterministic");
  a=new FlightModel(0);a.Heat=1.01f;a.Step(false,.02f);Check(a.Outcome==FlightOutcome.Burned,"thermal failure");
  a=new FlightModel(0);a.Altitude=.001f;a.Speed=2;a.Range=0;a.Step(false,.02f);Check(a.Outcome==FlightOutcome.Impact,"energy failure");
  a=new FlightModel(0);a.Altitude=.001f;a.Speed=.3f;a.Range=60;a.Step(false,.02f);Check(a.Outcome==FlightOutcome.Short,"short landing");
  a=new FlightModel(0);a.Altitude=.001f;a.Speed=.3f;a.Range=-60;a.Step(false,.02f);Check(a.Outcome==FlightOutcome.Overshot,"long landing");
  float t=a.Elapsed;a.Step(true,1);Check(a.Elapsed==t,"finished immutable");
  Debug.Log("FLIGHT_TESTS_PASSED checks="+checks);
 }
}
