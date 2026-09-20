using UnityEngine;
using UnityEngine.UI;
/// <summary>Instrument remains readable when clouds/ocean provide no visual horizon.</summary>
public sealed class AttitudeReference:MaskableGraphic {
 public double Pitch,Gamma;
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();Vector2 center=new Vector2(110,-115);float radius=98;
  void Tri(Vector2 a,Vector2 b,Vector2 c,Color color){int i=vh.currentVertCount;vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.zero);vh.AddVert(c,color,Vector2.zero);vh.AddTriangle(i,i+1,i+2);}
  void Line(Vector2 a,Vector2 b,float width,Color color){Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*width;Tri(a+n,b+n,a-n,color);Tri(a-n,b+n,b-n,color);}
  float horizon=center.y-(float)Pitch*1.5f;
  for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64,b=(i+1)*Mathf.PI*2/64;Vector2 p=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,q=center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius;Tri(center,p,q,new Color(.01f,.04f,.07f,.9f));Line(p,q,.8f,new Color(.23f,.47f,.57f));}
  for(int y=-90;y<90;y+=3){float width=Mathf.Sqrt(radius*radius-y*y);float yy=center.y+y;Line(new Vector2(center.x-width,yy),new Vector2(center.x+width,yy),1.4f,yy>horizon?new Color(.08f,.24f,.36f,.9f):new Color(.22f,.17f,.1f,.9f));}
  for(int degrees=-60;degrees<=60;degrees+=10){float yy=horizon+degrees*1.5f;if(Mathf.Abs(yy-center.y)>85)continue;float w=degrees==0?Mathf.Sqrt(radius*radius-(yy-center.y)*(yy-center.y))-4:degrees%20==0?28:16;Line(new Vector2(center.x-w,yy),new Vector2(center.x+w,yy),degrees==0?1.2f:.5f,new Color(.7f,.85f,.9f));}
  Color gold=new Color(1,.68f,.24f);Line(center+new Vector2(-44,0),center+new Vector2(-12,0),1.8f,gold);Line(center+new Vector2(12,0),center+new Vector2(44,0),1.8f,gold);Line(center+new Vector2(-12,0),center+new Vector2(0,-7),1.8f,gold);Line(center+new Vector2(0,-7),center+new Vector2(12,0),1.8f,gold);
  Vector2 velocity=new Vector2(center.x,horizon+(float)Gamma*1.5f);velocity.y=Mathf.Clamp(velocity.y,center.y-82,center.y+82);Color green=new Color(.3f,1,.67f);for(int i=0;i<16;i++){float a=i*Mathf.PI/8,b=(i+1)*Mathf.PI/8;Line(velocity+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*6,velocity+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*6,.8f,green);}Line(velocity+Vector2.left*15,velocity+Vector2.left*6,.8f,green);Line(velocity+Vector2.right*6,velocity+Vector2.right*15,.8f,green);
 }
}
