using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System;
using System.Collections;
public sealed class ReentryGame:MonoBehaviour {
 ReentryWorld world;ReentryHud hud;ReentryAudio sound;FlightModel flight;bool running,paused,held;int mission;float accumulator;bool qa;int qaStage;float qaClock;string[] names={"DAWN","PERIAPSIS","NIGHTFALL"};
 void Start(){Application.targetFrameRate=60;var es=new GameObject("Input events");es.AddComponent<EventSystem>();es.AddComponent<InputSystemUIInputModule>();world=new ReentryWorld();hud=new ReentryHud();sound=new ReentryAudio(gameObject);hud.Launch.onClick.AddListener(Begin);hud.Retry.onClick.AddListener(Begin);hud.Next.onClick.AddListener(()=>{mission=(mission+1)%3;Begin();});hud.Mission.onClick.AddListener(()=>{mission=(mission+1)%3;ReentryHud.Caption(hud.Mission,$"MISSION 0{mission+1}  /  {names[mission]}");});hud.PauseButton.onClick.AddListener(TogglePause);hud.Resume.onClick.AddListener(TogglePause);hud.Mute.onClick.AddListener(()=>{sound.Muted=!sound.Muted;ReentryHud.Caption(hud.Mute,sound.Muted?"SOUND OFF":"SOUND ON");});hud.Motion.onClick.AddListener(()=>{world.ReduceMotion=!world.ReduceMotion;ReentryHud.Caption(hud.Motion,world.ReduceMotion?"MOTION OFF":"MOTION ON");});hud.Best.text="PERSONAL BEST  /  "+PlayerPrefs.GetInt("EmberBest",0).ToString("N0");
 #if !UNITY_WEBGL
 qa=Array.IndexOf(Environment.GetCommandLineArgs(),"--qa")>=0;
 #endif
 Debug.Log("EMBER_READY 1.0.0");}
 void Begin(){CancelInvoke(nameof(PresentDebrief));hud.PauseButton.gameObject.SetActive(true);flight=new FlightModel(mission);running=true;paused=false;accumulator=0;hud.Menu.SetActive(false);hud.Debrief.SetActive(false);hud.Flight.SetActive(true);hud.PausePanel.SetActive(false);sound.Tone(true);Debug.Log("MISSION_START "+mission);}
 void TogglePause(){if(!running)return;paused=!paused;hud.PausePanel.SetActive(paused);Debug.Log("FLIGHT_PAUSED "+paused);}
 void OnApplicationFocus(bool focus){if(!focus&&running&&!paused&&!qa)TogglePause();}
 void Update(){if(world==null)return;float dt=Mathf.Min(Time.deltaTime,.1f);if(!qa&&!running&&(hud.Menu.activeSelf||hud.Debrief.activeSelf)&&Keyboard.current!=null&&Keyboard.current.spaceKey.wasPressedThisFrame)Begin();if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame)TogglePause();
  held=(Keyboard.current!=null&&Keyboard.current.spaceKey.isPressed)||(Mouse.current!=null&&Mouse.current.leftButton.isPressed)||(Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.isPressed);
  if(running&&!paused){accumulator+=dt;while(accumulator>=.02f&&!flight.Finished){flight.Step(qa?FlightModel.Guidance(flight):held,.02f);accumulator-=.02f;}hud.UpdateFlight(flight,held);if(flight.Finished)Finish();}
  world.Tick(flight,running,flight!=null&&flight.Finished,paused?0:dt);sound.Tick(flight?.Heat??0,running&&!paused);
  if(qa){qaClock+=dt;if(qaStage==0&&qaClock>2){Capture("title");qaStage++;}else if(qaStage==1&&qaClock>3){Begin();qaStage++;}else if(qaStage==2&&flight.Elapsed>22){Capture("entry");qaStage++;}else if(qaStage==3&&flight.Altitude<15){Capture("approach");qaStage++;}else if(qaStage==4&&flight.Finished&&hud.Debrief.activeSelf){Capture("debrief");qaStage++;Invoke(nameof(QuitQA),2);}}
 }
 void Capture(string label){
#if !UNITY_WEBGL
 ScreenCapture.CaptureScreenshot("/tmp/ember-"+label+".png");Debug.Log("QA_CAPTURE "+label);
#endif
 }
 void Finish(){running=false;hud.Status.text=flight.Outcome==FlightOutcome.Landed?"TOUCHDOWN CONFIRMED":"RECOVERY SIGNAL LOST";hud.Instruction.text="";hud.PauseButton.gameObject.SetActive(false);Invoke(nameof(PresentDebrief),3.5f);bool success=flight.Outcome==FlightOutcome.Landed;world.Burst();sound.Tone(success);hud.Result.text=success?"WELCOME HOME.":flight.Outcome==FlightOutcome.Burned?"SIGNAL LOST.":flight.Outcome==FlightOutcome.Impact?"TOO MUCH ENERGY.":flight.Outcome==FlightOutcome.Short?"SHORT OF HOME.":"TARGET OVERSHOT.";
  string advice=success?"Aster confirms touchdown. Crew recovered.":flight.Outcome==FlightOutcome.Burned?"The heat shield failed. Release earlier when thermal load rises.":flight.Outcome==FlightOutcome.Impact?"Touchdown speed exceeded 0.85 km/s. Brake earlier during descent.":flight.Outcome==FlightOutcome.Short?"You spent your energy too early. Glide longer to reach the runway.":"You carried too much range. Brake earlier to shorten the trajectory.";
  hud.Details.text=$"{advice}\n\nFLIGHT TIME    {flight.Elapsed:0.0}s          PEAK HEAT    {flight.PeakHeat*100:0}%\nLANDING SPEED    {flight.Speed:0.00} km/s          RANGE ERROR    {flight.Range:+0.0;-0.0;0} km\n\nRECOVERY SCORE    {flight.Score:N0}";
  if(success&&!qa){PlayerPrefs.SetInt("EmberBest",Mathf.Max(PlayerPrefs.GetInt("EmberBest",0),flight.Score));PlayerPrefs.Save();}Debug.Log("MISSION_END "+flight.Summary());
 }
 void PresentDebrief(){hud.Flight.SetActive(false);hud.Debrief.SetActive(true);}
 void QuitQA(){Application.Quit();}
}
