using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public sealed class ReentryHud {
 public GameObject Menu,Flight,Debrief,PausePanel;public TextMeshProUGUI Metrics,Status,Instruction,Result,Details,Best,MissionLabel;public Image Heat,Energy;public Button Launch,Retry,Next,PauseButton,Resume,Mute,Motion,Mission;TMP_FontAsset font;Transform root;
 Color ink=new Color(.78f,.88f,.92f),cyan=new Color(.3f,.85f,.96f),orange=new Color(1,.4f,.19f);
 public ReentryHud(){
  font=Resources.Load<TMP_FontAsset>("Fonts/BodySDF");var canvas=new GameObject("Flight instrumentation").AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.gameObject.AddComponent<GraphicRaycaster>();var scaler=canvas.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1440,900);scaler.matchWidthOrHeight=(float)Screen.width/Screen.height<1.6f?0:1;root=canvas.transform;
  Label(root,"E M B E R   /   O R B I T A L   R E C O V E R Y",40,27,900,30,16,cyan);
  Label(root,"FLIGHT SYSTEM  /  01",1140,27,260,30,14,ink);
  Menu=Group("Mission briefing");
  Label(Menu.transform,"THE LONG WAY\nHOME.",65,150,820,240,94,Color.white);
  Label(Menu.transform,"80 KILOMETERS ABOVE EARTH.\nONE SHIP. ONE BUTTON. MAKE IT COUNT.",70,425,730,75,22,ink);
  Label(Menu.transform,"HOLD  /  SPACE, MOUSE OR TOUCH",70,545,700,35,20,orange);
  Label(Menu.transform,"Raise the nose. Shed speed. Build heat.\nRelease to cool and carry your momentum.\nReach Aster with low speed and range within 18 km.",70,590,650,110,22,ink);
  Launch=Button(Menu.transform,"BEGIN RE-ENTRY",70,745,300,64);Mission=Button(Menu.transform,"MISSION 01  /  DAWN",395,745,355,64);
  Best=Label(Menu.transform,"",70,825,800,28,16,cyan);
  Flight=Group("Flight HUD");
  Metrics=Label(Flight.transform,"",45,94,430,230,27,ink);
  Label(Flight.transform,"THERMAL LOAD",45,350,300,25,14,ink);Panel(Flight.transform,45,391,310,5,new Color(.1f,.17f,.21f));Heat=Panel(Flight.transform,45,391,310,5,orange);Heat.type=Image.Type.Filled;Heat.fillMethod=Image.FillMethod.Horizontal;
  Label(Flight.transform,"ENERGY / ALTITUDE CORRIDOR",45,425,350,25,14,ink);Panel(Flight.transform,45,466,310,5,new Color(.1f,.17f,.21f));Energy=Panel(Flight.transform,45,466,310,5,cyan);Energy.type=Image.Type.Filled;Energy.fillMethod=Image.FillMethod.Horizontal;
  Status=Label(Flight.transform,"",850,105,535,110,31,Color.white);Status.alignment=TextAlignmentOptions.TopRight;
  Instruction=Label(Flight.transform,"",365,758,800,90,24,Color.white);Instruction.alignment=TextAlignmentOptions.Center;
  Label(Flight.transform,"HOLD TO BRAKE   /   RELEASE TO GLIDE",455,855,510,24,15,ink);
  PauseButton=Button(Flight.transform,"II  PAUSE",1200,800,185,45);
  Debrief=Group("Debrief");Panel(Debrief.transform,0,65,1440,835,new Color(.015f,.03f,.055f,.92f));
  Label(Debrief.transform,"M I S S I O N   D E B R I E F",80,145,950,40,19,cyan);Result=Label(Debrief.transform,"",75,225,1270,120,70,Color.white);Details=Label(Debrief.transform,"",80,385,1180,240,28,ink);Retry=Button(Debrief.transform,"FLY AGAIN",80,745,290,65);Next=Button(Debrief.transform,"NEXT MISSION",400,745,320,65);
  PausePanel=Group("Pause menu");Panel(PausePanel.transform,0,60,1440,840,new Color(.015f,.03f,.055f,.96f));Label(PausePanel.transform,"FLIGHT PAUSED",380,250,800,90,62,Color.white);Resume=Button(PausePanel.transform,"RESUME FLIGHT",440,450,560,70);
  Mute=Button(root,"SOUND ON",1010,855,175,30);Motion=Button(root,"MOTION ON",1200,855,195,30);
  Flight.SetActive(false);Debrief.SetActive(false);PausePanel.SetActive(false);
 }
 GameObject Group(string name){var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(root,false);var r=(RectTransform)g.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return g;}
 RectTransform Place(GameObject g,Transform parent,float x,float y,float w,float h){g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
 TextMeshProUGUI Label(Transform parent,string text,float x,float y,float w,float h,int size,Color color){var g=new GameObject(text,typeof(RectTransform));Place(g,parent,x,y,w,h);var t=g.AddComponent<TextMeshProUGUI>();t.font=font;t.text=text;t.fontSize=size;t.color=color;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;return t;}
 Image Panel(Transform p,float x,float y,float w,float h,Color color){var g=new GameObject("Panel",typeof(RectTransform));Place(g,p,x,y,w,h);var image=g.AddComponent<Image>();image.color=color;return image;}
 Button Button(Transform p,string text,float x,float y,float w,float h){var im=Panel(p,x,y,w,h,new Color(.06f,.14f,.19f,.95f));var b=im.gameObject.AddComponent<Button>();var colors=b.colors;colors.highlightedColor=new Color(.3f,.7f,.85f);colors.pressedColor=new Color(1,.4f,.1f);b.colors=colors;var l=Label(im.transform,text,10,0,w-20,h,18,ink);l.alignment=TextAlignmentOptions.Center;return b;}
 public static void Caption(Button b,string text)=>b.GetComponentInChildren<TextMeshProUGUI>().text=text;
 public void UpdateFlight(FlightModel f,bool held){Metrics.text=$"ALTITUDE     {f.Altitude:00.0} <size=17>KM</size>\n\nVELOCITY     {f.Speed:0.00} <size=17>KM/S</size>\n\nRANGE          {f.Range:000.0} <size=17>KM</size>";Heat.rectTransform.sizeDelta=new Vector2(310*Mathf.Clamp01(f.Heat),5);Energy.rectTransform.sizeDelta=new Vector2(310*Mathf.Clamp01(f.Speed/7.8f),5);Heat.color=f.Heat>.78f?Color.red:orange;
  Status.text=f.Heat>.78f?"THERMAL WARNING\n<size=18>RELEASE TO COOL THE SHIELD</size>":f.Altitude<12?"FINAL APPROACH\n<size=18>LOW ENERGY. PRECISION LANDING.</size>":f.Altitude<45?"ATMOSPHERIC FLIGHT\n<size=18>MANAGE YOUR ENERGY</size>":"ENTRY INTERFACE\n<size=18>THE ATMOSPHERE IS YOUR BRAKE</size>";
  string advice=f.Heat>.7f?"SHIELD HOT  /  RELEASE TO COOL":f.Speed>f.TargetSpeed+.28f?"EXCESS ENERGY  /  HOLD TO BRAKE":f.Speed<f.TargetSpeed-.28f?"PRESERVE ENERGY  /  RELEASE TO GLIDE":"ON CORRIDOR  /  KEEP BALANCING";Instruction.text=(held?"<color=#FF8A4F>HIGH DRAG</color>":"<color=#6DDCF2>GLIDING</color>")+"\n<size=18>"+advice+"</size>";
 }
}
