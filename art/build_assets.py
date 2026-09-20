import bpy, math, os
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def material(name,c,metal=0,rough=.4,emission=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough
 if emission:p.inputs['Emission Color'].default_value=(*c,1);p.inputs['Emission Strength'].default_value=emission
 return m
ceramic=material('Ceramic',(0.74,.8,.83),.25);black=material('ThermalCarbon',(.027,.039,.054),.3);metal=material('Titanium',(.2,.27,.32),.8);glass=material('Cockpit',(.025,.23,.32),.8,.16);orange=material('RescueOrange',(1,.19,.035),.25);cyan=material('NavigationCyan',(.08,.8,1),.1,.2,3)
def mesh(name,verts,faces,mat):
 m=bpy.data.meshes.new(name);m.from_pydata(verts,[],faces);m.update();o=bpy.data.objects.new(name,m);bpy.context.collection.objects.link(o);o.data.materials.append(mat);return o
def bevel(o,n=.05):
 mod=o.modifiers.new('Machined edges','BEVEL');mod.width=n;mod.segments=2;o.modifiers.new('Surface normals','WEIGHTED_NORMAL')
def box(name,loc,scale,mat,be=.03):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=bpy.context.object;o.name=name;o.dimensions=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mat)
 if be:bevel(o,be)
 return o
def loft(name,sections,mat):
 v=[]
 for y,w,h,z in sections:
  for j in range(12):
   a=2*math.pi*j/12;v.append((w*math.cos(a),y,z+h*math.sin(a)))
 f=[]
 for i in range(len(sections)-1):
  for j in range(12):a=i*12+j;b=i*12+(j+1)%12;f.append((a,b,b+12,a+12))
 f.extend([tuple(reversed(range(12))),tuple(range((len(sections)-1)*12,len(v)))]);o=mesh(name,v,f,mat);bevel(o,.035);return o
loft('Lifting body', [(-3,1.02,.43,.08),(-2,1.25,.62,.1),(0,1.12,.7,.18),(1.9,.67,.45,.1),(3.5,.18,.16,-.05),(3.85,.02,.035,-.09)],ceramic)
loft('Ablative belly',[(-3,1.03,.15,-.28),(-1.7,1.25,.2,-.32),(.5,.99,.18,-.29),(2.4,.47,.12,-.22),(3.7,.04,.03,-.15)],black)
loft('Armored canopy',[(.1,.68,.22,.7),(.8,.64,.3,.63),(1.7,.38,.16,.51),(2.05,.12,.025,.44)],glass)
box('Canopy center spar',(0,1.15,.88),(.07,1.5,.045),metal)
for side in [-1,1]:
 v=[(side*.7,1.3,-.05),(side*3.6,-2.5,-.08),(side*3.5,-3.1,-.03),(side*.6,-2.7,.12)]
 vv=v+[(x,y,z-.18) for x,y,z in v];o=mesh('Delta wing',vv,[(0,1,2,3),(7,6,5,4),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)],ceramic);bevel(o)
 for k in range(8):
  x=side*(1+k*.28);y=-2.55+k*.025;box('Wing thermal seam',(x,y,.015),(.022,.65,.028),black,.005)
 o=box('Rescue livery',(side*2.85,-2.4,.04),(.25,.74,.045),orange);o.rotation_euler.z=side*.48
 v=[(side*3.3,-3,-.04),(side*3.25,-1.8,.1),(side*3.15,-2.5,1.18),(side*3.2,-3,1.3)];o=mesh('Canted winglet',v,[(0,1,2,3),(3,2,1,0)],black)
 box('Wing navigation light',(side*3.38,-2.8,.1),(.1,.3,.1),cyan)
 # engines point aft
 bpy.ops.mesh.primitive_cylinder_add(vertices=32,radius=.34,depth=.52,location=(side*.57,-3.15,.06),rotation=(math.pi/2,0,0));bpy.context.object.data.materials.append(metal)
 bpy.ops.mesh.primitive_torus_add(major_radius=.28,minor_radius=.065,major_segments=24,minor_segments=8,location=(side*.57,-3.43,.06),rotation=(math.pi/2,0,0));bpy.context.object.data.materials.append(orange)
 bpy.ops.mesh.primitive_cylinder_add(vertices=24,radius=.23,depth=.03,location=(side*.57,-3.44,.06),rotation=(math.pi/2,0,0));bpy.context.object.data.materials.append(black)
 for j in range(5):box('RCS vent',(side*1.05,-1+j*.25,.58),(.12,.09,.055),black,.01)
for y in range(12):
 for x in range(-3,4):
  if abs(x)*.23<1.05-max(0,y-7)*.12:box('Individual heat shield tile',(x*.25,-2.7+y*.43,-.485),(.23,.4,.035),black,.01)
# Export applied model with material names; retain editable source in Blender.
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'art','Ember.blend'))
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.fbx(filepath=os.path.join(ROOT,'Reentry/Assets/Resources/Ember.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=False,add_leaf_bones=False)
print('BLENDER_ASSET_EXPORTED',len(bpy.data.objects),'objects')
# Hangar visual reference
world=bpy.context.scene.world or bpy.data.worlds.new('World');bpy.context.scene.world=world;world.color=(.05,.05,.05)
bpy.ops.object.camera_add(location=(11,13,10));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,0))-cam.location).to_track_quat('-Z','Y').to_euler();bpy.context.scene.camera=cam
for loc,power,size in [((3,4,10),2200,7),((-5,0,4),1600,6),((0,-6,5),1800,4)]:
 bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.data.energy=power;o.data.shape='DISK';o.data.size=size;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
s=bpy.context.scene;s.render.engine='CYCLES';s.cycles.samples=24;s.render.resolution_x=1100;s.render.resolution_y=750;s.render.resolution_percentage=100;s.render.image_settings.file_format='PNG';s.render.filepath=os.path.join(ROOT,'art','spacecraft.png');s.render.film_transparent=True;bpy.ops.render.render(write_still=True)
