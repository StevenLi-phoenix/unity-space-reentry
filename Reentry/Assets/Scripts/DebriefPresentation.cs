using System;
public static class DebriefPresentation {
 public static int ScoreAt(int score,double seconds,bool reducedMotion){if(reducedMotion)return score;double u=Math.Max(0,Math.Min(1,seconds/1.15));return (int)Math.Round(score*(1-Math.Pow(1-u,3)));}
}
