using UnityEngine;
using System.Collections.Generic;
/// <summary>All movable geometry is authored around named Blender hinges. No mesh scaling.</summary>
public sealed class VehicleAnimation {
 sealed class Joint {public Transform Transform;public Quaternion Rest;public Vector3 Axis;}
 readonly List<Renderer> gearRenderers=new List<Renderer>();
 readonly Dictionary<string,Joint> joints=new Dictionary<string,Joint>();float elevon,gear,brake;double airborneWheelDistance;
 public VehicleAnimation(Transform ship){foreach(var t in ship.GetComponentsInChildren<Transform>()){
  string n=t.name;if(n=="GearL"||n=="GearR"||n=="GearN")gearRenderers.AddRange(t.GetComponentsInChildren<Renderer>());if(n.StartsWith("Elevon")||n=="BodyFlap"||n.StartsWith("Rudder")||n.StartsWith("Gear")&&n!="Gear tire"||n.StartsWith("Wheel")){
   Vector3 axis=n.StartsWith("Rudder")?Vector3.up:n.StartsWith("GearDoor")?Vector3.forward:Vector3.right;
   joints[n]=new Joint{Transform=t,Rest=t.localRotation,Axis=t.parent.InverseTransformDirection(ship.TransformDirection(axis))};
  }
 }Debug.Log("VEHICLE_RIG_READY / "+joints.Count+" articulated transforms");}
 void Rotate(string name,float degrees){if(joints.TryGetValue(name,out var j))j.Transform.localRotation=Quaternion.AngleAxis(degrees,j.Axis)*j.Rest;}
 public void Tick(float altitude,float attack,float pitch,float dt,LandingFrame? landing){
  bool contact=landing.HasValue&&landing.Value.Stage>=LandingStage.Touchdown;
  float target=VehiclePose.Elevon(attack,pitch,landing.HasValue&&landing.Value.Stage==LandingStage.Flare);
  elevon=Mathf.MoveTowards(elevon,target,dt*35);gear=Mathf.MoveTowards(gear,Mathf.Clamp01((1400-altitude)/700),dt*.55f);brake=Mathf.MoveTowards(brake,contact?1:0,dt*.7f);
  Rotate("ElevonL",-elevon);Rotate("ElevonR",-elevon);Rotate("BodyFlap",elevon*.42f);
  Rotate("RudderL",-brake*38);Rotate("RudderR",brake*38);
  foreach(string side in new[]{"L","R","N"}){Rotate("Gear"+side,(1-gear)*(side=="N"?-96:96));Rotate("GearDoor"+side,-gear*85);}
  foreach(var renderer in gearRenderers)renderer.enabled=gear>.04f;
  if(contact)airborneWheelDistance=landing.Value.WheelDistance;else if(!landing.HasValue)airborneWheelDistance=0;
  float wheelAngle=(float)(airborneWheelDistance/.24*180/System.Math.PI%360);
  foreach(string name in new[]{"WheelL0","WheelL1","WheelR0","WheelR1"})Rotate(name,-wheelAngle);
  Rotate("WheelN0",-(float)(VehiclePose.NoseWheelDistance(airborneWheelDistance)/.24*180/System.Math.PI%360));
 }
}
