using UnityEngine;
using UnityEngine.Rendering;
public static class CoastalTerrain {
 public static Mesh Build(){
  const int n=192;var vertices=new Vector3[(n+1)*(n+1)];var uv=new Vector2[vertices.Length];var tris=new int[n*n*6];int k=0;
  // Nonuniform sampling keeps metre-scale runway surroundings without a camera-following patch.
  float Coordinate(int i,float extent){float t=i*2f/n-1;return Mathf.Sign(t)*Mathf.Pow(Mathf.Abs(t),2.2f)*extent;}
  for(int z=0;z<=n;z++)for(int x=0;x<=n;x++){int i=z*(n+1)+x;float xx=Coordinate(x,220),zz=Coordinate(z,320);float curve=(xx*xx+zz*zz)/(2*6371);vertices[i]=new Vector3(xx,(float)GroundReference.Height(xx,zz)-curve+.0002f,zz);uv[i]=new Vector2(xx,zz);if(x<n&&z<n){tris[k++]=i;tris[k++]=i+n+1;tris[k++]=i+1;tris[k++]=i+1;tris[k++]=i+n+1;tris[k++]=i+n+2;}}
  var mesh=new Mesh{name="Permanent Aster coastal terrain / km",indexFormat=IndexFormat.UInt32};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=tris;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
 }
}
