using UnityEditor;
using UnityEngine;
public static class AssetChecks {
 public static void Inspect(){var root=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Ember.fbx"));Debug.Log("MODEL_ROOT "+root.transform.localEulerAngles+" scale="+root.transform.localScale);foreach(var t in root.GetComponentsInChildren<Transform>())if(t.name.Contains("canopy")||t.name.Contains("body")||t.name.Contains("Cylinder")||t.name.Contains("Torus"))Debug.Log("MODEL_AXIS "+t.name+" world="+t.position+" local="+t.localPosition+" bounds="+(t.GetComponent<Renderer>()?t.GetComponent<Renderer>().bounds.center.ToString():"none"));Object.DestroyImmediate(root);}
}
