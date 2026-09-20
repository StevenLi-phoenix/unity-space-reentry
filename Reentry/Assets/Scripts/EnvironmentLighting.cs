using UnityEngine;
using UnityEngine.Rendering;
/// <summary>Altitude-aware reflection environment. Authored sky/Earth radiance, with
/// increasingly diffuse mip levels for rough surfaces; no per-frame probe cameras.</summary>
public sealed class EnvironmentLighting {
 readonly Cubemap reflection;readonly Color[][] orbital,daylight,buffer;float previous=-10;
 public EnvironmentLighting(){int size=64;reflection=new Cubemap(size,TextureFormat.RGBAHalf,true);reflection.name="Earth sky reflection radiance";reflection.wrapMode=TextureWrapMode.Clamp;reflection.filterMode=FilterMode.Trilinear;
  int levels=reflection.mipmapCount;orbital=new Color[levels*6][];daylight=new Color[levels*6][];buffer=new Color[levels*6][];
  for(int mip=0;mip<levels;mip++){int n=Mathf.Max(1,size>>mip);float diffuse=(float)mip/(levels-1);for(int face=0;face<6;face++){int k=mip*6+face;orbital[k]=new Color[n*n];daylight[k]=new Color[n*n];buffer[k]=new Color[n*n];for(int y=0;y<n;y++)for(int x=0;x<n;x++){float u=2*(x+.5f)/n-1,v=2*(y+.5f)/n-1;Vector3 d=Direction(face,u,v).normalized;orbital[k][y*n+x]=Color.Lerp(Radiance(d,false),new Color(.12f,.18f,.24f),diffuse*diffuse);daylight[k][y*n+x]=Color.Lerp(Radiance(d,true),new Color(.29f,.35f,.39f),diffuse*diffuse);}}}
  RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;RenderSettings.customReflectionTexture=reflection;RenderSettings.reflectionIntensity=.85f;RenderSettings.ambientMode=AmbientMode.Trilight;Tick(70);
 }
 static Vector3 Direction(int face,float u,float v){switch(face){case 0:return new Vector3(1,-v,-u);case 1:return new Vector3(-1,-v,u);case 2:return new Vector3(u,1,v);case 3:return new Vector3(u,-1,-v);case 4:return new Vector3(u,-v,1);default:return new Vector3(-u,-v,-1);}}
 public static Color Radiance(Vector3 d,bool day){float cloud=Mathf.SmoothStep(.25f,.85f,.5f+.23f*Mathf.Sin(d.x*17+d.z*8)*Mathf.Sin(d.z*23-d.y*9)+.16f*Mathf.Sin(d.x*43+d.z*32));
  if(d.y<0)return Color.Lerp(new Color(.045f,.15f,.26f),new Color(.85f,.88f,.87f),cloud*.85f);
  Color sky=day?Color.Lerp(new Color(.65f,.77f,.87f),new Color(.15f,.35f,.64f),Mathf.Sqrt(d.y)):Color.Lerp(new Color(.10f,.27f,.50f),new Color(.002f,.004f,.012f),Mathf.Clamp01(d.y*5));
  return day?Color.Lerp(sky,new Color(.9f,.92f,.94f),cloud*Mathf.Pow(1-d.y,2)*.45f):sky;
 }
 public void Tick(float altitude){float u=Mathf.SmoothStep(0,1,Mathf.InverseLerp(38,8,altitude));if(Mathf.Abs(previous-u)<.02f)return;previous=u;
  for(int mip=0;mip<reflection.mipmapCount;mip++)for(int face=0;face<6;face++){int k=mip*6+face;for(int i=0;i<buffer[k].Length;i++)buffer[k][i]=Color.Lerp(orbital[k][i],daylight[k][i],u);reflection.SetPixels(buffer[k],(CubemapFace)face,mip);}reflection.Apply(false,false);
  RenderSettings.ambientSkyColor=Color.Lerp(new Color(.035f,.05f,.075f),new Color(.26f,.35f,.48f),u);RenderSettings.ambientEquatorColor=Color.Lerp(new Color(.10f,.15f,.20f),new Color(.24f,.29f,.33f),u);RenderSettings.ambientGroundColor=new Color(.10f,.14f,.17f);
 }
}
