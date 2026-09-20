using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System;
public sealed class ReentryGame:MonoBehaviour {
 ReentryWorld world;ReentryHud hud;ReentryAudio sound;FlightModel flight;bool pointerBlocked;readonly System.Collections.Generic.List<RaycastResult> uiHits=new System.Collections.Generic.List<RaycastResult>();bool running,paused,held,previousHeld,qa,qaHeld;int mission,qaStage;float accumulator,qaClock,forecastClock,warningClock;double low,high,current;string[] names={"ASTER","COASTAL","NIGHTFALL"};
 void Start(){Application.targetFrameRate=60;var es=new GameObject("Input events");es.AddComponent<EventSystem>();es.AddComponent<InputSystemUIInputModule>();world=new ReentryWorld();hud=new ReentryHud();sound=new ReentryAudio(gameObject);
  hud.Launch.onClick.AddListener(Begin);hud.Retry.onClick.AddListener(Begin);hud.Next.onClick.AddListener(()=>{mission=(mission+1)%3;Begin();});hud.Mission.onClick.AddListener(()=>{mission=(mission+1)%3;ReentryHud.Caption(hud.Mission,$"ROUTE 0{mission+1}");});hud.PauseButton.onClick.AddListener(TogglePause);hud.Resume.onClick.AddListener(TogglePause);hud.Mute.onClick.AddListener(()=>{sound.Muted=!sound.Muted;ReentryHud.Caption(hud.Mute,sound.Muted?"SOUND OFF":"SOUND ON");});hud.Motion.onClick.AddListener(()=>{world.ReduceMotion=!world.ReduceMotion;ReentryHud.Caption(hud.Motion,world.ReduceMotion?"MOTION OFF":"MOTION ON");});hud.Best.text="BEST RECOVERY  /  "+PlayerPrefs.GetInt("EmberBestV2",0).ToString("N0");
#if !UNITY_WEBGL
 qa=Array.IndexOf(Environment.GetCommandLineArgs(),"--qa")>=0;
#endif
#if !UNITY_WEBGL
 if(Array.IndexOf(Environment.GetCommandLineArgs(),"--visual-qa")>=0){qa=true;qaStage=99;Begin();flight.Height=42000;flight.Velocity=4200;flight.Alpha=35*Math.PI/180;flight.Gamma=5*Math.PI/180;flight.Downrange=380000;flight.Temperature=1650;flight.Step(true,.05);running=false;Invoke(nameof(CaptureVisualEntry),2);}
#endif
 Debug.Log("EMBER_PHYSICAL_READY / model forward -Z, world metres and kilometres calibrated");
 }
 void Begin(){CancelInvoke(nameof(PresentDebrief));flight=new FlightModel(mission);running=true;paused=false;accumulator=0;forecastClock=0;previousHeld=false;warningClock=0;qaHeld=true;hud.Menu.SetActive(false);hud.Debrief.SetActive(false);hud.Flight.SetActive(true);hud.PauseButton.gameObject.SetActive(true);hud.PausePanel.SetActive(false);hud.ResetPlot();UpdateForecast();sound.Tone(true);Debug.Log("MISSION_START "+mission);}
 void TogglePause(){if(!running)return;paused=!paused;hud.PausePanel.SetActive(paused);Debug.Log("FLIGHT_PAUSED "+paused);}
 void OnApplicationFocus(bool focus){if(!focus&&running&&!paused&&!qa)TogglePause();}
 void UpdateForecast(){low=flight.PredictLanding(false);high=flight.PredictLanding(true);current=flight.PredictCurrent();qaHeld=FlightModel.Guidance(flight);}
 void Update(){if(world==null)return;float dt=Mathf.Min(Time.deltaTime,.1f);hud.TickLayout();
  if(!qa&&!running&&(hud.Menu.activeSelf||hud.Debrief.activeSelf)&&Keyboard.current!=null&&Keyboard.current.spaceKey.wasPressedThisFrame)Begin();
  if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame)TogglePause();
  bool key=Keyboard.current!=null&&Keyboard.current.spaceKey.isPressed;
  bool mouse=Mouse.current!=null&&Mouse.current.leftButton.isPressed,touch=Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.isPressed;
  if(mouse||touch){var position=touch?Touchscreen.current.primaryTouch.position.ReadValue():Mouse.current.position.ReadValue();var data=new PointerEventData(EventSystem.current){position=position};uiHits.Clear();EventSystem.current.RaycastAll(data,uiHits);if(uiHits.Count>0)pointerBlocked=true;}else pointerBlocked=false;
  held=FlightPresentation.Hold(key,mouse||touch,pointerBlocked);
  if(qa)held=qaHeld;
  if(running&&!paused){if(held!=previousHeld){sound.Command(held);Debug.Log("AOA_COMMAND "+(held?"nose_up":"nose_low"));previousHeld=held;}
   float acceleration=flight.Velocity>1500?8:25;accumulator+=dt*acceleration;
   while(accumulator>=.05f&&!flight.Finished){flight.Step(held,.05);accumulator-=.05f;forecastClock+=.05f;if(forecastClock>=2){forecastClock=0;UpdateForecast();if(qa)held=qaHeld;}}
   hud.UpdateFlight(flight,held,low,high,current);warningClock-=dt;if(flight.DynamicPressure>65000&&warningClock<=0){sound.Tone(false);warningClock=3;}
   if(flight.Finished)Finish();
  }
  world.Tick(flight,running||qaStage==99,flight!=null&&flight.Finished,paused?0:dt);if(flight!=null){hud.SetDestination(world.RunwayViewport,world.DisplayRange,world.DisplayAltitude);if(flight.Outcome==FlightOutcome.Landed)hud.UpdateAutoland(world.DisplayAltitude,world.DisplayRange,world.LandingProgress);}sound.Tick(flight==null?0:Mathf.Clamp01((float)flight.HeatFlux/1700000),running&&!paused);
  if(qa){qaClock+=dt;if(qaStage==0&&qaClock>2){Capture("title");qaStage++;}else if(qaStage==1&&qaClock>3){Begin();qaStage++;}else if(qaStage==2&&flight.Height<45000){Capture("entry");qaStage++;}else if(qaStage==3&&flight.Height<2000){Capture("approach");qaStage++;}else if(qaStage==4&&hud.Debrief.activeSelf){Capture("debrief");qaStage++;Invoke(nameof(QuitQA),2);}}
 }
 void Finish(){running=false;bool success=flight.Outcome==FlightOutcome.Landed;hud.PauseButton.gameObject.SetActive(false);hud.Status.text=success?"AUTOLAND CAPTURE\n<size=16>Approach energy within limits</size>":"VEHICLE LOST";hud.Instruction.text=success?"Landing system engaged. Runway acquired.":"Flight recorder saved.";if(!success)world.Burst();sound.Tone(success);Invoke(nameof(PresentDebrief),success?9.5f:2.5f);
  hud.Result.text=success?"CREW RECOVERED.":flight.Outcome==FlightOutcome.Structural?"AIRFRAME BREAKUP.":flight.Outcome==FlightOutcome.Burned?"HEAT SHIELD FAILURE.":flight.Outcome==FlightOutcome.Impact?"UNSAFE APPROACH.":flight.Outcome==FlightOutcome.Short?"APPROACH MISSED.":flight.Outcome==FlightOutcome.Skip?"SKIPPED THE ATMOSPHERE.":"RUNWAY OVERSHOT.";
  string reason=success?"The landing system captured a safe approach and completed touchdown.":flight.Outcome==FlightOutcome.Structural?"Dynamic pressure exceeded the airframe limit. Raise the nose earlier.":flight.Outcome==FlightOutcome.Burned?"Shield temperature exceeded 2200 K. Decelerate before descending.":flight.Outcome==FlightOutcome.Impact?"Approach exceeded 180 m/s airspeed or 35 m/s descent. Flare earlier.":flight.Outcome==FlightOutcome.Skip?"Lift carried the vehicle out of the atmosphere. Lower the nose before the climb grows.":flight.Outcome==FlightOutcome.Short?"Your trajectory ended before the approach gate. Use more lift.":"Your trajectory carried beyond the runway. Lower the nose sooner.";
  hud.Details.text=$"{reason}\n\n{(success?"CAPTURE SPEED":"LAST AIRSPEED")}  {flight.Velocity:0} m/s       {FlightPresentation.VerticalMotion(flight.DescentRate)}\n{(flight.Height<=500?"LANDING ERROR":"RANGE TO RUNWAY")}  {(flight.Height<=500?flight.LandingError:flight.Range):+0.0;-0.0;0} km       PEAK LOAD  {flight.PeakPressure/1000:0.0} kPa\n\nRECOVERY SCORE  {flight.Score:N0}";
  if(success&&!qa){PlayerPrefs.SetInt("EmberBestV2",Mathf.Max(PlayerPrefs.GetInt("EmberBestV2",0),flight.Score));PlayerPrefs.Save();hud.Best.text="BEST RECOVERY / "+PlayerPrefs.GetInt("EmberBestV2",0).ToString("N0");}Debug.Log("MISSION_END "+flight.Summary());
 }
 void PresentDebrief(){hud.Flight.SetActive(false);hud.Debrief.SetActive(true);}
 void Capture(string label){
#if !UNITY_WEBGL
 ScreenCapture.CaptureScreenshot("/tmp/ember-v2-"+label+".png");Debug.Log("QA_CAPTURE "+label);
#endif
 }
 void CaptureVisualEntry(){UpdateForecast();hud.UpdateFlight(flight,false,low,high,current);Capture("plasma");Invoke(nameof(VisualGround),1);}
 void VisualGround(){flight.Height=1500;flight.Velocity=120;flight.Gamma=-7*Math.PI/180;flight.Alpha=8*Math.PI/180;flight.Downrange=flight.Destination-16000;flight.HeatFlux=0;flight.Temperature=650;flight.G=.5f;flight.DynamicPressure=.5*FlightModel.DensityAt(flight.Height)*flight.Velocity*flight.Velocity;UpdateForecast();hud.UpdateFlight(flight,false,low,high,current);Invoke(nameof(CaptureGround),2);}
 void CaptureGround(){Capture("ground");Invoke(nameof(VisualFinal),1);}
 void VisualFinal(){flight.Height=150;flight.Downrange=flight.Destination-2200;UpdateForecast();hud.UpdateFlight(flight,false,low,high,current);Invoke(nameof(CaptureFinal),2);}
 void CaptureFinal(){Capture("final");Invoke(nameof(QuitQA),2);}
 void QuitQA(){Application.Quit();}
}
