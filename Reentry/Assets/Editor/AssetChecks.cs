using System;
using UnityEditor;
using UnityEngine;
public static class AssetChecks {
 public static void Run(){
  var sheath=ReentryWorld.PlasmaMesh();if(sheath.vertexCount!=3185||sheath.triangles.Length!=18432)throw new Exception("Plasma topology changed unexpectedly");foreach(var v in sheath.vertices)if(float.IsNaN(v.x)||float.IsInfinity(v.z))throw new Exception("Nonfinite plasma mesh");if(sheath.bounds.size.z<22||sheath.bounds.size.x<9)throw new Exception("Plasma must be a broad continuous envelope");UnityEngine.Object.DestroyImmediate(sheath);
  foreach(string map in new[]{"HullAlbedo","HullNormal","CarbonAlbedo","CarbonNormal"}){var tex=Resources.Load<Texture2D>(map);if(!tex||tex.width<1024)throw new Exception("Missing detailed surface map "+map);}
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Ember.fbx");if(!prefab)throw new Exception("Missing spacecraft");
  var root=UnityEngine.Object.Instantiate(prefab);Transform nose=null,tail=null;foreach(var t in root.GetComponentsInChildren<Transform>()){if(t.name=="NoseAxis")nose=t;if(t.name=="TailAxis")tail=t;}
  if(!nose||!tail||Vector3.Dot((nose.position-tail.position).normalized,Vector3.back)<.99f)throw new Exception("Model nose must point along physical travel -Z");
  if(Vector3.Dot(Vector3.forward,(nose.position-tail.position))>=0)throw new Exception("Wake must travel away from nose");
  var site=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Aster.fbx"));bool runway=false;foreach(var r in site.GetComponentsInChildren<Renderer>())if(r.name=="2500m runway"){runway=true;if(Math.Abs(r.bounds.size.z-2500)>1||Math.Abs(r.bounds.size.x-60)>1)throw new Exception("Airport physical dimensions invalid");}
  if(!runway)throw new Exception("Missing runway geometry");UnityEngine.Object.DestroyImmediate(site);UnityEngine.Object.DestroyImmediate(root);Debug.Log("ASSET_AXIS_AND_SCALE_TESTS_PASSED / nose -Z, wake +Z, runway 2500m x 60m");
 }
}
