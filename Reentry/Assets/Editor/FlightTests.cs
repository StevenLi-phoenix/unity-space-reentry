using System;
using UnityEngine;
public static class FlightTests {
 static int count;static void Check(bool b,string message){count++;if(!b)throw new Exception("FLIGHT TEST: "+message);}
 public static void Run(){count=0;
  Check(FlightModel.DensityAt(3000)>FlightModel.DensityAt(70000)*1000,"dense low atmosphere");
  var f=new FlightModel(0);f.Height=3000;f.Velocity=8000;f.Step(false,.05);Check(f.Outcome==FlightOutcome.Structural,"8 km/s at 3 km must destroy vehicle immediately");
  f=new FlightModel(0);f.Temperature=2400;f.Step(true,.05);Check(f.Outcome==FlightOutcome.Burned,"thermal failure");
  var a=new FlightModel(0);var b=new FlightModel(0);a.Height=b.Height=40000;a.Velocity=b.Velocity=4000;for(int i=0;i<100;i++){a.Step(true,.05);b.Step(false,.05);}Check(a.Velocity<b.Velocity,"nose up increases braking");Check(a.Gamma>b.Gamma,"nose up generates lift instead of scripted descent");
  a=new FlightModel(0);b=new FlightModel(0);a.Step(true,1);for(int i=0;i<20;i++)b.Step(true,.05);Check(Math.Abs(a.Height-b.Height)<.0001,"step partition invariance");
  foreach(int mission in new[]{0,1,2}){f=new FlightModel(mission);bool held=true;for(int i=0;i<44000&&!f.Finished;i++){if(i%40==0)held=FlightModel.Guidance(f);f.Step(held,.05);Check(!double.IsNaN(f.Height)&&!double.IsNaN(f.Velocity)&&f.Velocity>0,"finite physical state");if(f.Height<3000)Check(f.Velocity<300,"low altitude physically decelerated");}Check(f.Outcome==FlightOutcome.Landed,"feedback-driven policy reaches actual gate "+mission+" "+f.Summary());Debug.Log("PHYSICAL_ROUTE "+mission+" "+f.Summary());}
  foreach(bool hold in new[]{false,true}){f=new FlightModel(0);for(int i=0;i<44000&&!f.Finished;i++)f.Step(hold,.05);Check(f.Finished&&f.Outcome!=FlightOutcome.Landed,"constant button cannot win");}
  f=new FlightModel(0);double h=f.Height;f.Step(false,double.NaN);f.Step(false,0);Check(h==f.Height,"invalid dt ignored");
  Debug.Log("PHYSICAL_FLIGHT_TESTS_PASSED checks="+count);
 }
}
