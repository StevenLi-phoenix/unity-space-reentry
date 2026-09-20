using System;
public static class DebriefTests {
 public static void Run(){
  foreach(int score in new[]{0,8641,10000}){int previous=0;for(int i=0;i<200;i++){int value=DebriefPresentation.ScoreAt(score,i*.01,false);if(value<previous||value>score)throw new Exception("Victory score must increase without exceeding earned score");previous=value;}if(previous!=score)throw new Exception("Victory score must finish exactly");if(DebriefPresentation.ScoreAt(score,0,true)!=score)throw new Exception("Reduced motion must reveal the complete score immediately");}
  if(DebriefPresentation.ScoreAt(8641,-1,false)!=0)throw new Exception("Negative reveal time");
  UnityEngine.Debug.Log("DEBRIEF_TESTS_PASSED / bounded score reveal and reduced motion");
 }
}
