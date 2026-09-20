using System;
using UnityEditor;
using UnityEngine;
public static class AssetChecks {
 public static void Run(){
  var sky=new Material(Shader.Find("Ember/Sky"));if(!sky.HasProperty("_AltitudeKm")||!sky.HasProperty("_Horizon"))throw new Exception("Atmosphere must expose material altitude/horizon parameters");UnityEngine.Object.DestroyImmediate(sky);
  var plasma=new Material(Shader.Find("Ember/Plasma"));if(!plasma.HasProperty("_FlowTime"))throw new Exception("Plasma must expose pauseable animation time");UnityEngine.Object.DestroyImmediate(plasma);
  var volume=ReentryPlasma.VolumeMesh();if(volume.bounds.size.z<30||volume.triangles.Length!=36)throw new Exception("Plasma volume must be closed and include full wake");UnityEngine.Object.DestroyImmediate(volume);
  var terrain=CoastalTerrain.Build();if(terrain.bounds.size.x<400||terrain.vertexCount<30000)throw new Exception("Insufficient permanent ground coverage");foreach(var v in terrain.vertices)if(float.IsNaN(v.y))throw new Exception("Invalid terrain");UnityEngine.Object.DestroyImmediate(terrain);
  foreach(bool up in new[]{false,true})foreach(float value in ReentryAudio.CommandSamples(up))if(float.IsNaN(value)||Math.Abs(value)>.1f)throw new Exception("Servo sound headroom");
  foreach(string map in new[]{"HullAlbedo","HullNormal","CarbonAlbedo","CarbonNormal"}){var tex=Resources.Load<Texture2D>(map);if(!tex||tex.width<1024)throw new Exception("Missing detailed surface map "+map);}
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Ember.fbx");if(!prefab)throw new Exception("Missing spacecraft");
  var root=UnityEngine.Object.Instantiate(prefab);Transform nose=null,tail=null;foreach(var t in root.GetComponentsInChildren<Transform>()){if(t.name=="NoseAxis")nose=t;if(t.name=="TailAxis")tail=t;}
  if(!nose||!tail||Vector3.Dot((nose.position-tail.position).normalized,Vector3.back)<.99f)throw new Exception("Model nose must point along physical travel -Z");
  if(Vector3.Dot(Vector3.forward,(nose.position-tail.position))>=0)throw new Exception("Wake must travel away from nose");
  var site=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Aster.fbx"));bool runway=false;foreach(var r in site.GetComponentsInChildren<Renderer>())if(r.name=="2500m runway"){runway=true;if(Math.Abs(r.bounds.size.z-2500)>1||Math.Abs(r.bounds.size.x-60)>1)throw new Exception("Airport physical dimensions invalid");}
  if(!runway)throw new Exception("Missing runway geometry");UnityEngine.Object.DestroyImmediate(site);UnityEngine.Object.DestroyImmediate(root);Debug.Log("ASSET_AXIS_AND_SCALE_TESTS_PASSED / nose -Z, wake +Z, runway 2500m x 60m");
 }
}
