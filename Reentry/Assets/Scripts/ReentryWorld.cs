using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
/// <summary>Two cameras share orientation and exact 1:1000 scale. Environment units are km;
/// foreground spacecraft units are metres. This avoids both orbital precision loss and
/// inventing an oversized nearby airport. The destination is never toggled or rescaled.</summary>
public sealed class ReentryWorld {
 public Camera Camera {get;private set;}Camera environment;
 public Transform Ship {get;private set;}Transform planet,runway,clouds,terrain;ReentryPlasma plasma;
 Material skyMaterial;int gearAxis;Transform gear;public float DisplayAltitude,DisplayRange,LandingProgress;ParticleSystem sparks;Light sun,heatLight;
 public bool ReduceMotion;float clock,endClock;
 public Vector3 RunwayViewport=>environment.WorldToViewportPoint(runway.position);
 const float Radius=6371;const int PlanetLayer=8,ShipLayer=9;

 static Material Mat(string name,Color c){var m=new Material(Resources.Load<Material>(name));m.SetColor("_BaseColor",c);return m;}
 static void Layer(GameObject g,int layer){foreach(var t in g.GetComponentsInChildren<Transform>(true))t.gameObject.layer=layer;}
 static GameObject Sphere(string name,float radius,Material material){var g=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));g.GetComponent<MeshFilter>().sharedMesh=Globe();g.GetComponent<MeshRenderer>().sharedMaterial=material;g.transform.localScale=Vector3.one*radius;return g;}
 public ReentryWorld(){
  environment=new GameObject("Planetary camera / kilometres").AddComponent<Camera>();environment.nearClipPlane=.001f;environment.farClipPlane=20000;environment.cullingMask=1<<PlanetLayer;environment.clearFlags=CameraClearFlags.SolidColor;
  Camera=new GameObject("Vehicle camera / metres").AddComponent<Camera>();Camera.nearClipPlane=.05f;Camera.farClipPlane=200;Camera.cullingMask=1<<ShipLayer;Camera.gameObject.AddComponent<AudioListener>();var overlay=Camera.GetUniversalAdditionalCameraData();overlay.renderType=CameraRenderType.Overlay;overlay.renderPostProcessing=true;overlay.requiresDepthTexture=true;environment.GetUniversalAdditionalCameraData().cameraStack.Add(Camera);
  var sky=new GameObject("Atmospheric horizon",typeof(MeshFilter),typeof(MeshRenderer));sky.layer=PlanetLayer;sky.transform.SetParent(environment.transform,false);sky.transform.localPosition=Vector3.forward;var skyMesh=new Mesh();skyMesh.vertices=new[]{new Vector3(-1,-1,0),new Vector3(1,-1,0),new Vector3(1,1,0),new Vector3(-1,1,0)};skyMesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};skyMesh.triangles=new[]{0,1,2,0,2,3};skyMesh.RecalculateBounds();sky.GetComponent<MeshFilter>().sharedMesh=skyMesh;skyMaterial=new Material(Resources.Load<Material>("Sky"));sky.GetComponent<MeshRenderer>().sharedMaterial=skyMaterial;
  sun=new GameObject("Sunlight").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.8f;sun.transform.rotation=Quaternion.Euler(30,-45,0);sun.color=new Color(.9f,.94f,1);
  var bounce=new GameObject("Earthshine").AddComponent<Light>();bounce.type=LightType.Directional;bounce.color=new Color(.3f,.48f,.7f);bounce.intensity=.5f;bounce.transform.rotation=Quaternion.Euler(-35,160,0);
  RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.13f,.17f,.22f);
  var volume=new GameObject("Optics").AddComponent<Volume>();volume.isGlobal=true;volume.profile=ScriptableObject.CreateInstance<VolumeProfile>();var bloom=volume.profile.Add<Bloom>();bloom.intensity.Override(.65f);bloom.threshold.Override(1.1f);var vignette=volume.profile.Add<Vignette>();vignette.intensity.Override(.18f);var tone=volume.profile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.ACES);
  var earth=Mat("Planet",Color.white);earth.SetTexture("_MainTex",Resources.Load<Texture2D>("EarthNASA"));planet=Sphere("Earth / 6371 km radius",Radius,earth).transform;planet.position=new Vector3(0,-Radius,0);Layer(planet.gameObject,PlanetLayer);
  clouds=Sphere("Cloud layer / 8 km",Radius+8,Mat("Clouds",Color.white)).transform;clouds.position=planet.position;Layer(clouds.gameObject,PlanetLayer);
  Ship=Object.Instantiate(Resources.Load<GameObject>("Ember")).transform;Ship.name="EMBER / verified nose -Z";Layer(Ship.gameObject,ShipLayer);Rematerial(Ship,false);
  runway=Object.Instantiate(Resources.Load<GameObject>("Aster")).transform;runway.name="Aster / permanent 2500 metre runway";runway.localScale=Vector3.one*.001f;Layer(runway.gameObject,PlanetLayer);Rematerial(runway,true);
  var coast=new GameObject("Aster coastline and ground reference",typeof(MeshFilter),typeof(MeshRenderer));terrain=coast.transform;coast.layer=PlanetLayer;coast.GetComponent<MeshFilter>().sharedMesh=CoastalTerrain.Build();coast.GetComponent<MeshRenderer>().sharedMaterial=new Material(Resources.Load<Material>("Ground"));
  heatLight=new GameObject("Heat-shield glow").AddComponent<Light>();heatLight.type=LightType.Point;heatLight.color=new Color(1,.2f,.025f);heatLight.range=12;heatLight.cullingMask=1<<ShipLayer;
  plasma=new ReentryPlasma(Ship);
  foreach(var t in Ship.GetComponentsInChildren<Transform>())if(t.name=="LandingGear"){gear=t;Vector3 up=gear.InverseTransformDirection(Ship.up);gearAxis=Mathf.Abs(up.x)>Mathf.Abs(up.y)?0:1;if(Mathf.Abs(up.z)>Mathf.Abs(up[gearAxis]))gearAxis=2;}
  sparks=new GameObject("Ablation / flow +Z").AddComponent<ParticleSystem>();sparks.gameObject.layer=ShipLayer;var main=sparks.main;main.maxParticles=600;main.startLifetime=.45f;main.startSize=.025f;main.startSpeed=25;main.startColor=new Color(1,.55f,.12f);main.simulationSpace=ParticleSystemSimulationSpace.World;var shape=sparks.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.radius=.8f;shape.angle=8;var emission=sparks.emission;emission.rateOverTime=0;sparks.GetComponent<ParticleSystemRenderer>().material=Mat("Unlit",new Color(2,.5f,.07f));
  var random=new System.Random(79);var starMaterial=Mat("Unlit",new Color(.55f,.7f,.8f));var starMesh=new Mesh();var vertices=new Vector3[1400];var triangles=new int[2100];for(int i=0;i<350;i++){Vector3 p=new Vector3((float)random.NextDouble()*2-1,(float)random.NextDouble(),(float)random.NextDouble()*2-1).normalized*14000;float r=1.2f+(float)random.NextDouble()*2;vertices[i*4]=p+new Vector3(-r,-r,0);vertices[i*4+1]=p+new Vector3(r,-r,0);vertices[i*4+2]=p+new Vector3(r,r,0);vertices[i*4+3]=p+new Vector3(-r,r,0);int k=i*6,n=i*4;triangles[k]=n;triangles[k+1]=n+2;triangles[k+2]=n+1;triangles[k+3]=n;triangles[k+4]=n+3;triangles[k+5]=n+2;}starMesh.vertices=vertices;starMesh.triangles=triangles;var stars=new GameObject("Distant stars",typeof(MeshFilter),typeof(MeshRenderer));stars.layer=PlanetLayer;stars.GetComponent<MeshFilter>().sharedMesh=starMesh;stars.GetComponent<MeshRenderer>().sharedMaterial=starMaterial;
 }
 void Rematerial(Transform root,bool site){foreach(var r in root.GetComponentsInChildren<Renderer>()){if(site&&r.name=="Reclaimed coastal base"){r.enabled=false;continue;}var source=r.sharedMaterials;for(int i=0;i<source.Length;i++){string n=source[i]?source[i].name:"";Color c=new Color(.52f,.57f,.61f);if(site){c=new Color(.18f,.2f,.17f);if(n.Contains("Asphalt"))c=new Color(.035f,.04f,.05f);if(n.Contains("Concrete"))c=new Color(.2f,.22f,.24f);if(n.Contains("Markings"))c=new Color(.7f,.72f,.67f);if(n.Contains("Glass"))c=new Color(.04f,.15f,.2f);if(n.Contains("Metal"))c=new Color(.3f,.34f,.38f);if(n.Contains("Lights"))c=new Color(.1f,.4f,.75f);}else{if(n.Contains("Carbon"))c=new Color(.018f,.025f,.035f);if(n.Contains("Cockpit"))c=new Color(.012f,.08f,.13f);if(n.Contains("Titanium"))c=new Color(.16f,.22f,.27f);if(n.Contains("Orange"))c=new Color(.8f,.18f,.025f);if(n.Contains("Cyan"))c=new Color(.06f,.7f,1);}bool emissive=n.Contains("Lights")||n.Contains("Cyan");var m=Mat(emissive?"Unlit":"Lit",emissive?c*2:c);if(!site){m.SetFloat("_Smoothness",n.Contains("Cockpit")?.85f:.25f);m.SetFloat("_Metallic",n.Contains("Titanium")?.7f:.05f);}if(!site&&(n.Contains("Ceramic")||n.Contains("Carbon"))){Object.Destroy(m);m=new Material(Resources.Load<Material>(n.Contains("Carbon")?"CarbonPBR":"HullPBR"));}source[i]=m;}r.sharedMaterials=source;}}
 public void Tick(FlightModel f,bool playing,bool ended,float dt){
  clock+=dt;if(ended)endClock+=dt;else endClock=0;
  float h=f?.Altitude??70,range=f?.Range??1354,gamma=f==null?-7:(float)(f.Gamma*Mathf.Rad2Deg),alpha=f?.Attack??10;
  bool landed=ended&&f.Outcome==FlightOutcome.Landed;
  if(landed){float u=Mathf.SmoothStep(0,1,Mathf.Clamp01(endClock/9));h=Mathf.Lerp(h,.0032f,u);range=Mathf.Lerp(range,0,u);gamma=Mathf.Lerp(gamma,0,u);alpha=Mathf.Lerp(alpha,2,u);}
  DisplayAltitude=h;DisplayRange=range;LandingProgress=landed?Mathf.Clamp01(endClock/9):0;
  Ship.position=Vector3.zero;Ship.rotation=Quaternion.Euler(gamma+alpha,0,ReduceMotion?0:Mathf.Sin(clock*.8f)*.35f);
  Vector3 offset=(playing||ended)?Vector3.Lerp(new Vector3(9,5,16),new Vector3(0,3,15),Mathf.Clamp01(1-h/10)):new Vector3(10,5,-15);
  Vector3 look=(playing||ended)?new Vector3(0,0,-1.5f):-Vector3.Cross(Vector3.up,-offset.normalized)*4;
  float flux=f==null?0:(float)Mathf.Clamp01((float)f.HeatFlux/1700000);float vibration=ReduceMotion?0:Mathf.Clamp01((f?.G??0)/8)*.035f;
  Camera.transform.position=offset+new Vector3(Mathf.Sin(clock*31),Mathf.Cos(clock*37),0)*vibration;Camera.transform.LookAt(look);Camera.fieldOfView=46;
  environment.transform.position=new Vector3(offset.x*.001f,h+offset.y*.001f,offset.z*.001f);environment.transform.rotation=Camera.transform.rotation;environment.fieldOfView=Camera.fieldOfView;skyMaterial.SetFloat("_AltitudeKm",h);float depression=Mathf.Acos(Radius/(Radius+Mathf.Max(.001f,h)));float down=Mathf.Asin(-Camera.transform.forward.y);skyMaterial.SetFloat("_Horizon",.5f+Mathf.Tan(down-depression)/(2*Mathf.Tan(Camera.fieldOfView*Mathf.Deg2Rad*.5f)));
  float sky=Mathf.Clamp01((35-h)/35);environment.backgroundColor=Color.Lerp(new Color(.001f,.003f,.01f),new Color(.19f,.39f,.59f),sky);sun.intensity=f?.Mission==2?1.5f:2.3f;
  float angle=range/Radius;runway.position=new Vector3(0,Radius*(Mathf.Cos(angle)-1),-Radius*Mathf.Sin(angle));runway.rotation=Quaternion.Euler(-angle*Mathf.Rad2Deg,0,0);terrain.SetPositionAndRotation(runway.position,runway.rotation);
  planet.rotation=Quaternion.AngleAxis((float)(f?.Downrange??0)/1000/Radius*Mathf.Rad2Deg,Vector3.right)*Quaternion.Euler(0,0,-45);clouds.rotation=planet.rotation;var earth=planet.GetComponent<Renderer>().sharedMaterial;earth.SetFloat("_Altitude",h);
  Quaternion flightRotation=Quaternion.Euler(gamma,0,0);Vector3 origin=Ship.TransformPoint(new Vector3(0,-.13f,-3.8f));
  heatLight.transform.position=origin+Vector3.up*.3f;heatLight.intensity=playing?flux*10:0;
  plasma.Tick(flightRotation,flux,clock,playing);
  if(dt==0&&!sparks.isPaused)sparks.Pause();else if(dt>0&&sparks.isPaused)sparks.Play();
  if(gear){Vector3 size=Vector3.one;size[gearAxis]=Mathf.SmoothStep(.001f,1,Mathf.Clamp01((1500-h*1000)/1000));gear.localScale=size;}
  sparks.transform.position=origin;sparks.transform.rotation=flightRotation;var emission=sparks.emission;emission.rateOverTime=playing?flux*12:0;
 }
 public void Burst(){sparks.Emit(160);}
 static Mesh Globe(){int rows=256,cols=512;var v=new Vector3[(rows+1)*(cols+1)];var uv=new Vector2[v.Length];var idx=new int[rows*cols*6];int k=0;for(int y=0;y<=rows;y++)for(int x=0;x<=cols;x++){float a=Mathf.PI*y/rows,b=Mathf.PI*2*x/cols;int i=y*(cols+1)+x;v[i]=new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b));uv[i]=new Vector2((float)x/cols,1-(float)y/rows);if(y<rows&&x<cols){idx[k++]=i;idx[k++]=i+1;idx[k++]=i+cols+1;idx[k++]=i+1;idx[k++]=i+cols+2;idx[k++]=i+cols+1;}}var m=new Mesh();m.indexFormat=IndexFormat.UInt32;m.vertices=v;m.normals=v;m.uv=uv;m.triangles=idx;m.RecalculateBounds();return m;}
}
