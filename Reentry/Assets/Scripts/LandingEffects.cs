using UnityEngine;
public sealed class LandingEffects {
 readonly Transform shadow;readonly Material shade;readonly ParticleSystem smoke;bool contact;double previousDistance;
 public LandingEffects(){
  var g=new GameObject("Vehicle contact shadow",typeof(MeshFilter),typeof(MeshRenderer));g.layer=9;shadow=g.transform;var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-1,0,-1),new Vector3(1,0,-1),new Vector3(1,0,1),new Vector3(-1,0,1)};mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};mesh.colors=new[]{Color.white,Color.white,Color.white,Color.white};mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateBounds();g.GetComponent<MeshFilter>().sharedMesh=mesh;shade=new Material(Resources.Load<Material>("SoftMark"));g.GetComponent<MeshRenderer>().sharedMaterial=shade;
  smoke=new GameObject("Main tire contact smoke").AddComponent<ParticleSystem>();smoke.gameObject.layer=9;var main=smoke.main;main.loop=false;main.playOnAwake=false;main.maxParticles=90;main.startLifetime=.9f;main.startSize=new ParticleSystem.MinMaxCurve(.12f,.4f);main.startSpeed=.6f;main.startColor=new Color(.8f,.84f,.86f,.6f);main.simulationSpace=ParticleSystemSimulationSpace.Local;var emission=smoke.emission;emission.rateOverTime=0;var shape=smoke.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(1.7f,.03f,.1f);var velocity=smoke.velocityOverLifetime;velocity.enabled=true;velocity.y=.3f;var size=smoke.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.3f,1,3));var color=smoke.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(.8f,0),new GradientAlphaKey(0,1)});color.color=gradient;var material=new Material(Resources.Load<Material>("SoftMark"));material.SetColor("_BaseColor",Color.white);smoke.GetComponent<ParticleSystemRenderer>().sharedMaterial=material;smoke.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
 }
 public void Tick(float altitude,float range,LandingFrame? frame){
  bool near=altitude<35&&Mathf.Abs(range)<1.25f;shadow.gameObject.SetActive(near);if(near){float spread=1+altitude*.06f;shadow.position=new Vector3(0,-altitude+2.13f,0);shadow.localScale=new Vector3(3.7f*spread,1,4.5f*spread);shade.SetColor("_BaseColor",new Color(.01f,.015f,.022f,.65f/(1+altitude*.10f)));}
  bool touching=frame.HasValue&&frame.Value.Stage>=LandingStage.Touchdown;
  if(touching&&!contact){smoke.transform.position=new Vector3(0,-1.15f,1.8f);smoke.Play();smoke.Emit(44);previousDistance=frame.Value.WheelDistance;}
  if(touching){smoke.transform.position+=Vector3.forward*(float)(frame.Value.WheelDistance-previousDistance);previousDistance=frame.Value.WheelDistance;}
  if(!frame.HasValue&&contact){smoke.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);previousDistance=0;}contact=touching;
 }
}
