import bpy,numpy as np,os
# Deterministic, tileable authored material maps. No external texture assets.
def generate_maps(root):
 out=os.path.join(root,'Reentry/Assets/Resources');n=1024;y,x=np.mgrid[0:n,0:n];rng=np.random.default_rng(824)
 for name,size,base in [('Hull',256,np.array([.64,.67,.69])),('Carbon',128,np.array([.045,.052,.064]))]:
  tx=x%size;ty=y%size;edge=np.minimum(np.minimum(tx,size-1-tx),np.minimum(ty,size-1-ty))
  tiles=rng.uniform(-.028,.028,(n//size,n//size));variation=tiles[y//size,x//size]
  noise=rng.normal(0,.006,(n,n));seam=np.exp(-edge*edge/3.5);rgb=base[None,None,:]*(1+variation[:,:,None])+noise[:,:,None];rgb*=1-.55*seam[:,:,None]
  # Flush fasteners at panel corners; carbon keeps a ceramic-grain surface.
  if name=='Hull':
   for cx in [12,size-13]:
    for cy in [12,size-13]:
     d=np.sqrt((tx-cx)**2+(ty-cy)**2);ring=(d<3.5)&(d>1.8);rgb[ring]*=.45
   # Fine brushed finish and a restrained alternating plate tone.
   rgb+=np.sin(x*.8)[:,:,None]*.0015
  else:rgb+=rng.normal(0,.004,(n,n,1))
  height=-seam*.13+noise*.1;gy,gx=np.gradient(height);normal=np.stack([-gx*2,-gy*2,np.ones((n,n))],axis=-1);normal/=np.linalg.norm(normal,axis=-1)[:,:,None]
  for suffix,pixels in [('Albedo',np.clip(rgb,0,1)),('Normal',normal*.5+.5)]:
   image=bpy.data.images.new(name+suffix,width=n,height=n,alpha=True);rgba=np.concatenate([pixels,np.ones((n,n,1))],axis=-1).astype(np.float32);image.pixels.foreach_set(rgba.ravel());image.filepath_raw=os.path.join(out,name+suffix+'.png');image.file_format='PNG';image.save()
  # Smoothness is independent of color: weathered ceramic and satin carbon have
  # softer reflections than the framed glazing and machined engine hardware.
  smooth=np.clip((.38 if name=='Hull' else .22)+variation*.8-noise*2-seam*.14,.08,.52)
  surface=np.ones((n,n,4),dtype=np.float32);surface[:,:,:3]=.02 if name=='Hull' else .01;surface[:,:,3]=smooth
  image=bpy.data.images.new(name+'Surface',width=n,height=n,alpha=True);image.colorspace_settings.name='Non-Color';image.pixels.foreach_set(surface.ravel());image.filepath_raw=os.path.join(out,name+'Surface.png');image.file_format='PNG';image.save()
 print('PBR_MAPS_GENERATED hull/carbon albedo and normal')
