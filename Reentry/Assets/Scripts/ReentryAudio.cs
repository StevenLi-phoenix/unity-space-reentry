using UnityEngine;
public sealed class ReentryAudio {
 AudioSource wind,music,signal;public bool Muted;
 public ReentryAudio(GameObject host){wind=host.AddComponent<AudioSource>();music=host.AddComponent<AudioSource>();signal=host.AddComponent<AudioSource>();wind.clip=Noise();wind.loop=true;wind.volume=0;wind.Play();music.clip=Pad();music.loop=true;music.volume=.16f;music.Play();}
 static AudioClip Noise(){int n=44100*3;float[] a=new float[n];var r=new System.Random(72);float smooth=0;for(int i=0;i<n;i++){smooth=Mathf.Lerp(smooth,(float)r.NextDouble()*2-1,.08f);a[i]=smooth*.6f*Mathf.Sin(Mathf.PI*i/(n-1));}var c=AudioClip.Create("Atmospheric friction",n,1,44100,false);c.SetData(a,0);return c;}
 static AudioClip Pad(){int n=44100*8;float[] a=new float[n];for(int i=0;i<n;i++){float t=i/44100f;float env=Mathf.Pow(Mathf.Sin(Mathf.PI*i/n),2);a[i]=env*(Mathf.Sin(t*2*Mathf.PI*110)*.1f+Mathf.Sin(t*2*Mathf.PI*164.81f)*.06f+Mathf.Sin(t*2*Mathf.PI*220)*.04f);}var c=AudioClip.Create("Orbital choir",n,1,44100,false);c.SetData(a,0);return c;}
 public void Tick(float heat,bool playing){wind.volume=Muted?0:playing?.08f+heat*.55f:0;wind.pitch=.6f+heat*1.3f;music.volume=Muted?0:playing?.13f:.22f;}
 public void Tone(bool success){int n=22050;float[] a=new float[n];for(int i=0;i<n;i++){float t=i/44100f;float hz=success?440+220*(i/(n/3)):145;a[i]=Mathf.Sin(t*hz*2*Mathf.PI)*.2f*Mathf.Sin(Mathf.PI*i/n);}var clip=AudioClip.Create("Telemetry acknowledgement",n,1,44100,false);clip.SetData(a,0);if(!Muted)signal.PlayOneShot(clip);Object.Destroy(clip,2);}
}
