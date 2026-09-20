using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System;
public sealed class ReentryGame:MonoBehaviour {
 ReentryWorld world;ReentryHud hud;ReentryAudio sound;FlightModel flight;bool running,paused,held,previousHeld,qa,qaHeld;int mission,qaStage;float accumulator,qaClock,forecastClock,warningClock;double low,high,current;string[] names={"ASTER","COASTAL","NIGHTFALL"};
 void Start(){Application.targetFrameRate=60;var es=new GameObject("Input events");es.AddComponent<EventSystem>();es.AddComponent<InputSystemUIInputModule>();world=new ReentryWorld();hud=new ReentryHud();sound=new ReentryAudio(gameObject);
  hud.Launch.onClick.AddListener(Begin);hud.Retry.onClick.AddListener(Begin);hud.Next.onClick.AddListener(()=>{mission=(mission+1)%3;Begin();});hud.Mission.onClick.AddListener(()=>{mission=(mission+1)%3;ReentryHud.Caption(hud.Mission,$"ROUTE 0{mission+1}");});hud.PauseButton.onClick.AddListener(TogglePause);hud.Resume.onClick.AddListener(TogglePause);hud.Mute.onClick.AddListener(()=>{sound.Muted=!sound.Muted;ReentryHud.Caption(hud.Mute,sound.Muted?"SOUND OFF":"SOUND ON");});hud.Motion.onClick.AddListener(()=>{world.ReduceMotion=!world.ReduceMotion;ReentryHud.Caption(hud.Motion,world.ReduceMotion?"MOTION OFF":"MOTION ON");});hud.Best.text="BEST RECOVERY  /  "+PlayerPrefs.GetInt("EmberBestV2",0).ToString("N0");
#if !UNITY_WEBGL
 qa=Array.IndexOf(Environment.GetCommandLineArgs(),"--qa")>=0;
#endif
 Debug.Log("EMBER_PHYSICAL_READY / model forward -Z, world metres and kilometres calibrated");
 }
 void Begin(){CancelInvoke(nameof(PresentDebrief));flight=new FlightModel(mission);running=true;paused=false;accumulator=0;forecastClock=0;qaHeld=true;hud.Menu.SetActive(false);hud.Debrief.SetActive(false);hud.Flight.SetActive(true);hud.PauseButton.gameObject.SetActive(true);hud.PausePanel.SetActive(false);hud.ResetPlot();UpdateForecast();sound.Tone(true);Debug.Log("MISSION_START "+mission);}
 void TogglePause(){if(!running)return;paused=!paused;hud.PausePanel.SetActive(paused);Debug.Log("FLIGHT_PAUSED "+paused);}
 void OnApplicationFocus(bool focus){if(!focus&&running&&!paused&&!qa)TogglePause();}
 void UpdateForecast(){low=flight.PredictLanding(false);high=flight.PredictLanding(true);current=flight.PredictCurrent();qaHeld=FlightModel.Guidance(flight);}
 void Update(){if(world==null)return;float dt=Mathf.Min(Time.deltaTime,.1f);hud.TickLayout();
  if(!qa&&!running&&(hud.Menu.activeSelf||hud.Debrief.activeSelf)&&Keyboard.current!=null&&Keyboard.current.spaceKey.wasPressedThisFrame)Begin();
  if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame)TogglePause();
  held=(Keyboard.current!=null&&Keyboard.current.spaceKey.isPressed)||(Mouse.current!=null&&Mouse.current.leftButton.isPressed)||(Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.isPressed);
  if(qa)held=qaHeld;
  if(running&&!paused){if(held!=previousHeld){sound.Tone(held);Debug.Log("AOA_COMMAND "+(held?"nose_up":"nose_low"));previousHeld=held;}
   float acceleration=flight.Velocity>1500?8:25;accumulator+=dt*acceleration;
   while(accumulator>=.05f&&!flight.Finished){flight.Step(held,.05);accumulator-=.05f;forecastClock+=.05f;if(forecastClock>=2){forecastClock=0;UpdateForecast();if(qa)held=qaHeld;}}
   hud.UpdateFlight(flight,held,low,high,current);warningClock-=dt;if(flight.DynamicPressure>65000&&warningClock<=0){sound.Tone(false);warningClock=2;}
   if(flight.Finished)Finish();
  }
  world.Tick(flight,running,flight!=null&&flight.Finished,paused?0:dt);if(flight!=null)hud.SetDestination(world.RunwayViewport,flight.Range);sound.Tick(flight==null?0:Mathf.Clamp01((float)flight.HeatFlux/1700000),running&&!paused);
  if(qa){qaClock+=dt;if(qaStage==0&&qaClock>2){Capture("title");qaStage++;}else if(qaStage==1&&qaClock>3){Begin();qaStage++;}else if(qaStage==2&&flight.Height<45000){Capture("entry");qaStage++;}else if(qaStage==3&&flight.Height<2000){Capture("approach");qaStage++;}else if(qaStage==4&&hud.Debrief.activeSelf){Capture("debrief");qaStage++;Invoke(nameof(QuitQA),2);}}
 }
 void Finish(){running=false;bool success=flight.Outcome==FlightOutcome.Landed;hud.PauseButton.gameObject.SetActive(false);hud.Status.text=success?"AUTOLAND CAPTURE\n<size=16>Approach energy within limits</size>":"VEHICLE LOST";hud.Instruction.text=success?"Landing system engaged. Runway acquired.":"Flight recorder saved.";if(!success)world.Burst();sound.Tone(success);Invoke(nameof(PresentDebrief),success?9.5f:2.5f);
  hud.Result.text=success?"CREW RECOVERED.":flight.Outcome==FlightOutcome.Structural?"AIRFRAME BREAKUP.":flight.Outcome==FlightOutcome.Burned?"HEAT SHIELD FAILURE.":flight.Outcome==FlightOutcome.Impact?"UNSAFE APPROACH.":flight.Outcome==FlightOutcome.Short?"LANDED SHORT.":flight.Outcome==FlightOutcome.Skip?"SKIPPED THE ATMOSPHERE.":"RUNWAY OVERSHOT.";
  string reason=success?"The landing system captured a safe approach and completed touchdown.":flight.Outcome==FlightOutcome.Structural?"Dynamic pressure exceeded the airframe limit. Raise the nose earlier.":flight.Outcome==FlightOutcome.Burned?"Shield temperature exceeded 2200 K. Decelerate before descending.":flight.Outcome==FlightOutcome.Impact?"Approach exceeded 180 m/s airspeed or 35 m/s descent. Flare earlier.":flight.Outcome==FlightOutcome.Short?"Your trajectory ended before the approach gate. Use more lift.":"Your trajectory carried beyond the runway. Lower the nose sooner.";
  hud.Details.text=$"{reason}\n\nAPPROACH SPEED  {flight.Velocity:0} m/s       DESCENT  {flight.DescentRate:0.0} m/s\nLANDING ERROR  {flight.LandingError:+0.0;-0.0;0} km       PEAK LOAD  {flight.PeakPressure/1000:0.0} kPa\n\nRECOVERY SCORE  {flight.Score:N0}";
  if(success&&!qa){PlayerPrefs.SetInt("EmberBestV2",Mathf.Max(PlayerPrefs.GetInt("EmberBestV2",0),flight.Score));PlayerPrefs.Save();}Debug.Log("MISSION_END "+flight.Summary());
 }
 void PresentDebrief(){hud.Flight.SetActive(false);hud.Debrief.SetActive(true);}
 void Capture(string label){
#if !UNITY_WEBGL
 ScreenCapture.CaptureScreenshot("/tmp/ember-v2-"+label+".png");Debug.Log("QA_CAPTURE "+label);
#endif
 }
 void QuitQA(){Application.Quit();}
}
