using UnityEngine;
/// <summary>Gas density is integrated through a bounded volume. Five windward source regions
/// follow the airframe; their wakes follow airflow independently of angle of attack.</summary>
public sealed class ReentryPlasma {
 readonly Transform volume,ship;readonly Material material;readonly Vector4[] sources=new Vector4[5];
 static readonly Vector3[] LocalSources={new Vector3(0,-.12f,-3.72f),new Vector3(-.9f,-.15f,-.5f),new Vector3(.9f,-.15f,-.5f),new Vector3(-3.25f,-.12f,2.2f),new Vector3(3.25f,-.12f,2.2f)};
 public ReentryPlasma(Transform vessel){ship=vessel;var g=new GameObject("Windward plasma volume",typeof(MeshFilter),typeof(MeshRenderer));g.layer=9;volume=g.transform;g.GetComponent<MeshFilter>().sharedMesh=VolumeMesh();material=new Material(Resources.Load<Material>("Plasma"));g.GetComponent<MeshRenderer>().sharedMaterial=material;}
 public void Tick(Quaternion flightRotation,float heat,float clock,bool visible){volume.rotation=flightRotation;volume.gameObject.SetActive(visible&&heat>.015f);var inverse=Quaternion.Inverse(flightRotation);for(int i=0;i<sources.Length;i++){var p=inverse*ship.TransformPoint(LocalSources[i]);sources[i]=new Vector4(p.x,p.y,p.z,i==0?1.2f:i<3?.7f:.55f);}material.SetVectorArray("_Sources",sources);material.SetFloat("_Heat",heat);material.SetFloat("_FlowTime",clock);}
 public static Mesh VolumeMesh(){
  // Cube faces have outward winding; render only the back face and intersect the box in shader.
  Vector3[] p={new Vector3(-6,-5,-6),new Vector3(6,-5,-6),new Vector3(6,7,-6),new Vector3(-6,7,-6),new Vector3(-6,-5,28),new Vector3(6,-5,28),new Vector3(6,7,28),new Vector3(-6,7,28)};
  int[] t={0,2,1,0,3,2,4,5,6,4,6,7,0,4,7,0,7,3,1,2,6,1,6,5,0,1,5,0,5,4,3,7,6,3,6,2};var mesh=new Mesh{name="Closed plasma raymarch bounds"};mesh.vertices=p;mesh.triangles=t;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
 }
}
