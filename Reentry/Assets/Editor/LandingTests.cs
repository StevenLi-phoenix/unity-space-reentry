using System;
public static class LandingTests {
 static void Check(bool ok,string reason){if(!ok)throw new Exception("Landing: "+reason);}
 public static void Run(){int checks=0;
  foreach(double range in new[]{2000d,2900d,6000d})foreach(double speed in new[]{66.75d,120d,180d}){
   var sequence=new LandingSequence(range,500,speed,-2);
   var start=sequence.Evaluate(0);Check(Math.Abs(start.Range-range)<.001&&Math.Abs(start.Height-500)<.001,"capture continuity");
   var previous=start;
   for(double t=.01;t<sequence.Duration+2;t+=.01){var s=sequence.Evaluate(t);Check(s.Range<=previous.Range+.00001,"forward motion");Check(s.Speed>=0&&s.Height>=3.1,"finite ground clearance");Check(s.Stage>=previous.Stage,"stage order");if(s.Stage>=LandingStage.Touchdown)Check(s.Range<1250&&s.Range>-1250,"wheels remain on runway");previous=s;checks+=4;}
   double touchdown=sequence.ApproachSeconds;var a=sequence.Evaluate(touchdown-1e-6);var b=sequence.Evaluate(touchdown+1e-6);Check(Math.Abs(a.Height-b.Height)<.001&&Math.Abs(a.Speed-b.Speed)<.001&&Math.Abs(a.Range-b.Range)<.001,"touchdown continuity");
   var stop=sequence.Evaluate(sequence.Duration);Check(stop.Stage==LandingStage.Stopped&&stop.Speed==0,"complete only at stop");Check(Math.Abs(sequence.Evaluate(sequence.Duration+20).Range-stop.Range)<.0001,"no post-stop drift");
   var rolled=sequence.Evaluate(touchdown+4);double eps=.0001;double derivative=(sequence.Evaluate(touchdown+4-eps).Range-sequence.Evaluate(touchdown+4+eps).Range)/(2*eps*LandingSequence.RollTimeScale);Check(Math.Abs(derivative-rolled.Speed)<.01,"rollout speed matches displacement");
  }
  Check(VehiclePose.NoseWheelDistance(100)==0&&VehiclePose.NoseWheelDistance(500)>0,"nose wheel waits for contact");
  Check(VehiclePose.Elevon(35,8,false)>VehiclePose.Elevon(5,8,false),"actual AOA drives elevons");Check(VehiclePose.Elevon(10,8,true)>20,"flare deflects elevons");
  UnityEngine.Debug.Log("LANDING_TESTS_PASSED "+checks+" sampled checks / capture, flare, wheels, braking and stop");
 }
}
