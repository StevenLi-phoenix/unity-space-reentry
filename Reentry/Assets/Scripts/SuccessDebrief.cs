using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>A single, short victory reveal. The parked ship remains visible beside the result.</summary>
public sealed class SuccessDebrief {
 public GameObject Root {get;private set;}
 readonly TextMeshProUGUI scoreText,record,metrics;readonly VictoryGraphic graphic;readonly CanvasGroup reveal;int score;float clock;
 static readonly Color Gold=new Color(1,.84f,.51f),White=new Color(.95f,.985f,1),Mint=new Color(.53f,.88f,.75f);
 public SuccessDebrief(Transform parent,TMP_FontAsset font){
  Root=new GameObject("Welcome home celebration",typeof(RectTransform),typeof(Canvas));var r=Root.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
  graphic=Root.AddComponent<VictoryGraphic>();graphic.raycastTarget=false;reveal=Root.AddComponent<CanvasGroup>();
  Text(font,"Mission accomplished",70,154,700,36,20,Mint);
  Text(font,"Welcome home.",64,208,1240,110,82,White);
  Text(font,"Crew safe. Ship secured.\nYou brought everyone home.",70,337,720,88,25,White);
  Text(font,"Recovery score",74,466,360,28,18,Mint);scoreText=Text(font,"0",66,493,560,125,92,Gold);
  record=Text(font,"Successful recovery",75,614,650,30,18,Gold);
  metrics=Text(font,"",74,664,1200,52,17,new Color(.79f,.88f,.92f));
  Text(font,"Safely back on Earth",950,350,350,36,20,Mint).alignment=TextAlignmentOptions.Center;
  Root.SetActive(false);
 }
 TextMeshProUGUI Text(TMP_FontAsset font,string value,float x,float y,float w,float h,int size,Color color){var g=new GameObject(value,typeof(RectTransform));var r=g.GetComponent<RectTransform>();r.SetParent(Root.transform,false);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);var t=g.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=color;t.raycastTarget=false;return t;}
 public void Show(FlightModel flight,bool best,bool reduced){score=flight.Score;clock=0;record.text=best?"New personal best!":"A safe return. A mission to be proud of.";metrics.text=$"Approach  {flight.Velocity:0} m/s     |     Landing error  {flight.LandingError:+0.0;-0.0;0} km     |     Peak load  {flight.PeakPressure/1000:0.0} kPa";Root.SetActive(true);Tick(0,reduced);}
 public void Tick(float dt,bool reduced){if(!Root.activeSelf)return;clock+=dt;if(reduced)clock=Mathf.Max(clock,3);scoreText.text=DebriefPresentation.ScoreAt(score,clock,reduced).ToString("N0");reveal.alpha=reduced?1:Mathf.Lerp(.3f,1,Mathf.Clamp01(clock/.35f));float visualTime=reduced?4:Mathf.Min(clock,3);if(graphic.Time!=visualTime){graphic.Time=visualTime;graphic.SetVerticesDirty();}}
}
public sealed class VictoryGraphic:MaskableGraphic {
 public float Time;
 Vector2 P(float x,float y)=>new Vector2(rectTransform.rect.xMin+x,rectTransform.rect.yMax-y);
 void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color,Color? rightColor=null){int n=vh.currentVertCount;vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,rightColor??color,Vector2.zero);vh.AddVert(c,rightColor??color,Vector2.zero);vh.AddVert(d,color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
 void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color color){Vector2 delta=(b-a).normalized;Vector2 side=new Vector2(-delta.y,delta.x)*width*.5f;Quad(vh,a-side,a+side,b+side,b-side,color);}
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();float w=rectTransform.rect.width;
  // Readable left-hand result, transparent over the recovered spacecraft on the right.
  for(int i=0;i<16;i++){float x=w*i/16f,next=w*(i+1)/16f;Color c=new Color(.025f,.105f,.14f,Mathf.Lerp(.91f,.03f,Mathf.SmoothStep(0,1,x/w)));Quad(vh,P(x,132),P(next,132),P(next,833),P(x,833),c,new Color(c.r,c.g,c.b,Mathf.Lerp(.91f,.03f,Mathf.SmoothStep(0,1,next/w))));}
  Color gold=new Color(1,.84f,.51f,.9f),mint=new Color(.53f,.88f,.75f,.95f);Vector2 center=P(1125,267);float r=60;
  for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64,b=(i+1)*Mathf.PI*2/64;Line(vh,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r,2.2f,gold);}
  Line(vh,center+new Vector2(-26,0),center+new Vector2(-5,-21),5,mint);Line(vh,center+new Vector2(-5,-21),center+new Vector2(30,23),5,mint);
  // One brief orbital burst, confined above the ship and away from the buttons.
  float fade=Mathf.Clamp01((2.8f-Time)/1.3f);if(Time<=0||fade<=0)return;
  for(int i=0;i<42;i++){float a=i*2.399963f,seed=(i%7)/7f;float distance=85+(1-Mathf.Exp(-Time*2.8f))*(65+seed*180);Vector2 p=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.62f)*distance+Vector2.down*Time*Time*9;float spin=a+Time*(seed+.3f);Vector2 u=new Vector2(Mathf.Cos(spin),Mathf.Sin(spin))*3,v=new Vector2(-u.y,u.x)*(.45f+seed);Color c=i%3==0?mint:gold;c.a=fade*.9f;Quad(vh,p-u-v,p+u-v,p+u+v,p-u+v,c);}
 }
}
