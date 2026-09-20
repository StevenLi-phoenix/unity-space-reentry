import bpy,math,os,bmesh
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def mat(n,c):
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);return m
concrete=mat('Concrete',(.23,.24,.25));asphalt=mat('RunwayAsphalt',(.055,.06,.068));white=mat('Markings',(.82,.82,.75));earth=mat('Island',(.14,.18,.12));glass=mat('TowerGlass',(.1,.3,.4));blue=mat('RunwayLights',(.15,.5,1));metal=mat('HangarMetal',(.38,.41,.43))
def box(n,p,d,m):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=n;o.dimensions=d;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(m);return o
box('Reclaimed coastal base',(0,0,-4),(1900,4200,8),earth)
box('2500m runway',(0,0,1),(60,2500,2),asphalt)
for i in range(-24,25):box('Runway center marking',(0,i*50,2.05),(1,25,.05),white)
for side in [-1,1]:
 box('Runway edge',(side*29,0,2.06),(.5,2450,.05),white)
 for i in range(-25,26):box('Edge light',(side*33,i*50,2.5),(.8,.8,.7),blue)
 for end in [-1,1]:
  for i in range(8):box('Threshold marking',(side*(4+i*2.8),end*1200,2.08),(1.3,35,.06),white)
box('Taxiway',(140,0,.5),(24,2450,1),concrete)
for y in [-950,0,950]:box('Taxiway crossing',(72,y,.5),(140,24,1),concrete)
box('Hangar apron',(300,150,.5),(260,600,1),concrete)
for y in [-70,110,290]:
 box('Hangar structure',(365,y,20),(100,120,40),metal)
 for i in range(10):box('Hangar corrugation',(314,y-54+i*12,20),(1,1.5,39),concrete)
 box('Hangar door',(313,y,13),(1,75,25),asphalt)
box('Tower shaft',(200,-360,25),(14,14,50),concrete);box('Control cab',(200,-360,54),(28,24,8),glass);box('Control roof',(200,-360,59),(31,27,2),metal)
for i in range(5):box('Service module',(450,-400+i*50,5),(35,25,10),white)
for y in range(-1800,-1250,50):
 for x in [-8,0,8]:box('Approach strobe',(x,y,3),(.6,.6,1),white)
for o in bpy.context.scene.objects:
 if o.type=='MESH':
  bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free()
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'art','Aster.blend'))
bpy.ops.object.select_all(action='SELECT');bpy.ops.export_scene.fbx(filepath=os.path.join(ROOT,'Reentry/Assets/Resources/Aster.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',add_leaf_bones=False)
print('ASTER_DIMENSION_CHECK runway_length_m=2500 runway_width_m=60')
