using System;
using UnityEditor;
using UnityEngine;
public static class AssetChecks {
 public static void Run(){
  var sky=new Material(Shader.Find("Ember/Sky"));if(!sky.HasProperty("_AltitudeKm")||!sky.HasProperty("_Horizon"))throw new Exception("Atmosphere must expose material altitude/horizon parameters");UnityEngine.Object.DestroyImmediate(sky);
  var plasma=new Material(Shader.Find("Ember/Plasma"));if(!plasma.HasProperty("_FlowTime"))throw new Exception("Plasma must expose pauseable animation time");UnityEngine.Object.DestroyImmediate(plasma);
  var volume=ReentryPlasma.VolumeMesh();if(volume.bounds.size.z<30||volume.triangles.Length!=36)throw new Exception("Plasma volume must be closed and include full wake");UnityEngine.Object.DestroyImmediate(volume);
  var terrain=CoastalTerrain.Build();if(terrain.bounds.size.x<400||terrain.vertexCount<30000)throw new Exception("Insufficient permanent ground coverage");foreach(var v in terrain.vertices)if(float.IsNaN(v.y))throw new Exception("Invalid terrain");UnityEngine.Object.DestroyImmediate(terrain);
  var cheer=ReentryAudio.VictorySamples();if(cheer[0]!=0||Math.Abs(cheer[cheer.Length-1])>.00001)throw new Exception("Victory cue must start and end silently");foreach(float sample in cheer)if(float.IsNaN(sample)||Math.Abs(sample)>.3f)throw new Exception("Victory sound headroom");
  foreach(float value in ReentryAudio.TouchdownSamples())if(float.IsNaN(value)||Math.Abs(value)>.2f)throw new Exception("Tire contact sound headroom");
  foreach(bool up in new[]{false,true})foreach(float value in ReentryAudio.CommandSamples(up))if(float.IsNaN(value)||Math.Abs(value)>.1f)throw new Exception("Servo sound headroom");
  foreach(string map in new[]{"HullAlbedo","HullNormal","CarbonAlbedo","CarbonNormal","HullSurface","CarbonSurface"}){var tex=Resources.Load<Texture2D>(map);if(!tex||tex.width<1024)throw new Exception("Missing detailed surface map "+map);}
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Ember.fbx");if(!prefab)throw new Exception("Missing spacecraft");
  var root=UnityEngine.Object.Instantiate(prefab);Transform nose=null,tail=null;foreach(var t in root.GetComponentsInChildren<Transform>()){if(t.name=="NoseAxis")nose=t;if(t.name=="TailAxis")tail=t;}
  if(!nose||!tail||Vector3.Dot((nose.position-tail.position).normalized,Vector3.back)<.99f)throw new Exception("Model nose must point along physical travel -Z");
  foreach(string name in new[]{"ElevonL","ElevonR","BodyFlap","RudderL","RudderR","GearL","GearR","GearN","GearDoorL","GearDoorR","GearDoorN","WheelL0","WheelR0","WheelN0"}){bool found=false;foreach(var t in root.GetComponentsInChildren<Transform>())if(t.name==name){found=true;if(t.childCount==0)throw new Exception("Empty animation hinge "+name);}if(!found)throw new Exception("Missing Blender animation hinge "+name);}
  var animated=new VehicleAnimation(root.transform);animated.Tick(70000,35,35,.1f,null);
  foreach(var t in root.GetComponentsInChildren<Transform>())if(t.name=="GearL"||t.name=="GearR"||t.name=="GearN")foreach(var r in t.GetComponentsInChildren<Renderer>())if(r.enabled)throw new Exception("Stowed wheel visible outside bay");
  animated.Tick(400,8,6,3,null);
  foreach(var t in root.GetComponentsInChildren<Transform>())if(t.name=="GearL"||t.name=="GearR"||t.name=="GearN")foreach(var r in t.GetComponentsInChildren<Renderer>())if(!r.enabled)throw new Exception("Deployed gear hidden");
  if(EnvironmentLighting.Radiance(Vector3.up,false).grayscale>=EnvironmentLighting.Radiance(Vector3.up,true).grayscale)throw new Exception("Orbital reflection must be darker than daylight");
  if(Vector3.Dot(Vector3.forward,(nose.position-tail.position))>=0)throw new Exception("Wake must travel away from nose");
  var site=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Aster.fbx"));bool runway=false;foreach(var r in site.GetComponentsInChildren<Renderer>())if(r.name=="2500m runway"){runway=true;if(Math.Abs(r.bounds.size.z-2500)>1||Math.Abs(r.bounds.size.x-60)>1)throw new Exception("Airport physical dimensions invalid");}
  if(!runway)throw new Exception("Missing runway geometry");UnityEngine.Object.DestroyImmediate(site);UnityEngine.Object.DestroyImmediate(root);Debug.Log("ASSET_AXIS_AND_SCALE_TESTS_PASSED / nose -Z, wake +Z, runway 2500m x 60m");
 }
}
