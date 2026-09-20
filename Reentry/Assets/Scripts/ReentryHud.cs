using UnityEngine;
using UnityEngine.UI;
using TMPro;
public sealed class ReentryHud {
 public GameObject Menu,Flight,Debrief,PausePanel;public TextMeshProUGUI Status,Instruction,Result,Details,Best;
 public Button Launch,Retry,Next,PauseButton,Resume,Mute,Motion,Mission;
 TextMeshProUGUI telemetry,thermal,forecast,command,phase;Image heatBar,loadBar,predictionMarker,holdMarker,releaseMarker;Image destination;TextMeshProUGUI destinationText;TMP_FontAsset font;Transform root;CanvasScaler scaler;FlightPlot plot;
 readonly Color ink=new Color(.82f,.89f,.91f),cyan=new Color(.3f,.85f,.95f),orange=new Color(1,.42f,.16f);
 public ReentryHud(){font=Resources.Load<TMP_FontAsset>("Fonts/BodySDF");var canvas=new GameObject("Flight deck").AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.gameObject.AddComponent<GraphicRaycaster>();scaler=canvas.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1440,900);root=canvas.transform;
  Label(root,"E M B E R",40,25,350,45,27,Color.white);Label(root,"O R B I T A L   R E C O V E R Y",40,73,450,22,12,cyan);
  Menu=Group("Briefing");Panel(Menu.transform,20,130,610,670,new Color(.009f,.017f,.029f,.9f));Label(Menu.transform,"BRING IT\nHOME.",48,152,700,190,80,Color.white);Label(Menu.transform,"Your heat shield is your brake.\nYour lift decides where you land.",52,375,650,90,27,ink);
  Label(Menu.transform,"HOLD SPACE / MOUSE / TOUCH",52,500,630,33,19,orange);Label(Menu.transform,"Nose up: more lift, more drag.\nRELEASE: lower the nose and descend.\n\nKeep the landing forecast on the runway.\nWatch heat and aerodynamic load.",52,550,620,145,22,ink);
  Launch=Button(Menu.transform,"BEGIN ENTRY",52,735,230,60);Mission=Button(Menu.transform,"ROUTE 01",300,735,230,60);Best=Label(Menu.transform,"",52,814,700,30,15,cyan);
  Flight=Group("Flight instruments");telemetry=Label(Flight.transform,"",40,125,480,130,32,Color.white);thermal=Label(Flight.transform,"",40,285,440,80,18,ink);Panel(Flight.transform,40,375,270,4,new Color(.1f,.15f,.19f));heatBar=Panel(Flight.transform,40,375,270,4,orange);Panel(Flight.transform,40,395,270,4,new Color(.1f,.15f,.19f));loadBar=Panel(Flight.transform,40,395,270,4,cyan);
  phase=Label(Flight.transform,"",900,32,500,30,17,ink);phase.alignment=TextAlignmentOptions.Right;
  Status=Label(Flight.transform,"",40,430,460,100,22,orange);
  Panel(Flight.transform,920,110,470,180,new Color(.006f,.018f,.03f,.85f));var graph=new GameObject("Trajectory",typeof(RectTransform));Place(graph,Flight.transform,920,110,470,180);plot=graph.AddComponent<FlightPlot>();plot.raycastTarget=false;Label(Flight.transform,"TRAJECTORY  /  ALTITUDE EXAGGERATED",932,120,445,20,12,cyan);
  Panel(Flight.transform,365,702,710,140,new Color(.008f,.019f,.03f,.92f));Label(Flight.transform,"PROJECTED LANDING",390,718,400,25,14,ink);forecast=Label(Flight.transform,"",670,714,370,30,22,Color.white);forecast.alignment=TextAlignmentOptions.Right;
  Panel(Flight.transform,400,787,640,2,new Color(.3f,.42f,.5f));Panel(Flight.transform,710,772,20,31,new Color(.2f,.8f,.75f,.55f));predictionMarker=Panel(Flight.transform,716,774,8,26,Color.white);holdMarker=Panel(Flight.transform,400,781,5,14,orange);releaseMarker=Panel(Flight.transform,400,781,5,14,cyan);
  Label(Flight.transform,"SHORT",400,809,120,20,12,ink);Label(Flight.transform,"RUNWAY",679,809,100,20,12,cyan);Label(Flight.transform,"LONG",990,809,90,20,12,ink);
  command=Label(Flight.transform,"",400,602,640,45,26,Color.white);command.alignment=TextAlignmentOptions.Center;Instruction=Label(Flight.transform,"",390,652,660,38,20,ink);Instruction.alignment=TextAlignmentOptions.Center;
  destination=Panel(Flight.transform,0,0,9,9,cyan);destinationText=Label(destination.transform,"ASTER",15,-8,230,35,15,cyan);
  PauseButton=Button(Flight.transform,"PAUSE",1230,805,160,40);
  Debrief=Group("Debrief");Panel(Debrief.transform,0,110,1440,730,new Color(.008f,.015f,.025f,.94f));Label(Debrief.transform,"FLIGHT RECORDER",70,158,800,40,17,cyan);Result=Label(Debrief.transform,"",70,235,1290,100,65,Color.white);Details=Label(Debrief.transform,"",70,385,1270,270,27,ink);Retry=Button(Debrief.transform,"TRY AGAIN",70,740,260,60);Next=Button(Debrief.transform,"NEXT ROUTE",355,740,270,60);
  PausePanel=Group("Paused");Panel(PausePanel.transform,0,100,1440,740,new Color(.008f,.015f,.025f,.97f));Label(PausePanel.transform,"FLIGHT PAUSED",430,310,730,90,53,Color.white);Resume=Button(PausePanel.transform,"RESUME",500,475,440,65);
  Mute=Button(root,"SOUND ON",1060,855,160,30);Motion=Button(root,"MOTION ON",1230,855,170,30);Flight.SetActive(false);Debrief.SetActive(false);PausePanel.SetActive(false);
 }
 public void SetDestination(Vector3 viewport,float range){destination.gameObject.SetActive(Flight.activeSelf&&viewport.z>0&&viewport.x>.03f&&viewport.x<.97f&&viewport.y>.1f&&viewport.y<.9f);destination.rectTransform.anchorMin=destination.rectTransform.anchorMax=new Vector2(viewport.x,viewport.y);destination.rectTransform.anchoredPosition=Vector2.zero;destinationText.text=$"ASTER / {range:0.0} km";}
 public void TickLayout(){scaler.matchWidthOrHeight=(float)Screen.width/Screen.height<1.6f?0:1;}
 public void ResetPlot(){plot.ResetTrack();}
 public void UpdateFlight(FlightModel f,bool held,double low,double high,double current){
  telemetry.text=$"{f.Altitude:0.0} <size=18>km ALTITUDE</size>\n{f.Velocity:0} <size=18>m/s AIRSPEED</size>";
  thermal.text=$"SHIELD  {f.Temperature:0} K / 2200 K\nLOAD     {f.DynamicPressure/1000:0.0} kPa / 100 kPa";heatBar.rectTransform.sizeDelta=new Vector2(270*Mathf.Clamp01(f.Heat),4);loadBar.rectTransform.sizeDelta=new Vector2(270*Mathf.Clamp01((float)f.DynamicPressure/100000),4);
  phase.text=$"{f.Range:0.0} KM TO ASTER  /  {(f.Velocity>1500?"HYPERSONIC":f.Height>1500?"GLIDE":"FINAL APPROACH")}";
  Status.text=f.DynamicPressure>65000?"AIRFRAME LOAD RISING\n<size=16>Raise the nose to decelerate</size>":f.Temperature>1900?"HEAT SHIELD NEAR LIMIT\n<size=16>Shed speed before descending</size>":$"AOA  {f.Attack:0} deg\n<size=17>{f.G:0.0} g BRAKING  /  {f.DescentRate:0} m/s DESCENT</size>";
  command.text=held?"<color=#FF914E>NOSE UP / LIFT + BRAKING</color>":"<color=#65DDEE>NOSE LOW / DESCENDING</color>";
  double center=current,target=f.Destination/1000;double error=center-target;
  forecast.text=Mathf.Abs((float)error)<2?"ON RUNWAY":$"{System.Math.Abs(error):0.0} KM {(error<0?"SHORT":"LONG")}";
  float scale=Mathf.Max(8,Mathf.Min(160,(float)System.Math.Abs(high-low)*.7f));SetMarker(predictionMarker,(float)error,scale);SetMarker(holdMarker,(float)(high-target),scale);SetMarker(releaseMarker,(float)(low-target),scale);
  Instruction.text=f.Height<1500?"Flare: keep descent below 35 m/s":f.DynamicPressure>65000?"HOLD: reduce speed before denser air":error< -2?"HOLD to carry the trajectory farther":error>2?"RELEASE to bring the trajectory closer":"Alternate gently to keep the runway centered";
  plot.Flight=f;plot.SetVerticesDirty();
 }
 static void SetMarker(Image image,float error,float scale){var p=image.rectTransform.anchoredPosition;p.x=716+Mathf.Clamp(error/scale,-1,1)*300;image.rectTransform.anchoredPosition=p;}
 GameObject Group(string name){var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(root,false);var r=(RectTransform)g.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return g;}
 void Place(GameObject g,Transform p,float x,float y,float w,float h){g.transform.SetParent(p,false);var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
 TextMeshProUGUI Label(Transform p,string value,float x,float y,float w,float h,int size,Color color){var g=new GameObject("Label",typeof(RectTransform));Place(g,p,x,y,w,h);var text=g.AddComponent<TextMeshProUGUI>();text.font=font;text.text=value;text.fontSize=size;text.color=color;text.raycastTarget=false;return text;}
 Image Panel(Transform p,float x,float y,float w,float h,Color color){var g=new GameObject("Panel",typeof(RectTransform));Place(g,p,x,y,w,h);var image=g.AddComponent<Image>();image.color=color;return image;}
 Button Button(Transform p,string value,float x,float y,float w,float h){var panel=Panel(p,x,y,w,h,new Color(.05f,.12f,.16f,.97f));var b=panel.gameObject.AddComponent<Button>();var colors=b.colors;colors.highlightedColor=new Color(.3f,.75f,.85f);colors.pressedColor=new Color(1,.6f,.2f);b.colors=colors;var text=Label(panel.transform,value,0,0,w,h,16,ink);text.alignment=TextAlignmentOptions.Center;return b;}
 public static void Caption(Button button,string value)=>button.GetComponentInChildren<TextMeshProUGUI>().text=value;
}
public sealed class FlightPlot:MaskableGraphic {
 public FlightModel Flight;readonly System.Collections.Generic.List<Vector2> history=new System.Collections.Generic.List<Vector2>();double lastTime;
 public void ResetTrack(){history.Clear();lastTime=-10;}
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(Flight==null)return;float w=rectTransform.rect.width,h=rectTransform.rect.height;Vector2 P(double x,double y)=>new Vector2(12+(float)(x/Flight.Destination)*(w-24),-h+15+(float)(y/80000)*(h-45));
  void Line(Vector2 a,Vector2 b,float thick,Color c){var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*thick;int i=vh.currentVertCount;vh.AddVert(a-n,c,Vector2.zero);vh.AddVert(a+n,c,Vector2.zero);vh.AddVert(b+n,c,Vector2.zero);vh.AddVert(b-n,c,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
  Line(P(0,0),P(Flight.Destination,0),1,new Color(.2f,.4f,.5f));
  if(Flight.Time-lastTime>4){history.Add(P(Flight.Downrange,Flight.Height));lastTime=Flight.Time;}
  for(int i=1;i<history.Count;i++)Line(history[i-1],history[i],1,new Color(.3f,.8f,.9f));
  Vector2 now=P(Flight.Downrange,Flight.Height);Line(now-Vector2.one*3,now+Vector2.one*3,2,Color.white);Vector2 target=P(Flight.Destination,0);Line(target-Vector2.up*6,target+Vector2.up*12,2,new Color(.2f,1,.6f));
 }
}
