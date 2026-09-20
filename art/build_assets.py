import bpy, math, os, sys
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0,os.path.dirname(__file__))
from material_maps import generate_maps
generate_maps(ROOT)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def material(name,c,metal=0,rough=.4,emission=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough
 if emission:p.inputs['Emission Color'].default_value=(*c,1);p.inputs['Emission Strength'].default_value=emission
 return m
ceramic=material('Ceramic',(0.74,.8,.83),.02);black=material('ThermalCarbon',(.027,.039,.054),.3);metal=material('Titanium',(.2,.27,.32),.8);glass=material('Cockpit',(.045,.10,.14),.35,.10);orange=material('RescueOrange',(1,.19,.035),.25);cyan=material('NavigationCyan',(.08,.8,1),.1,.2,3)
for mat,kind in [(ceramic,'Hull'),(black,'Carbon')]:
 nodes=mat.node_tree.nodes;links=mat.node_tree.links;shader=nodes.get('Principled BSDF')
 albedo=nodes.new('ShaderNodeTexImage');albedo.image=bpy.data.images.get(kind+'Albedo');links.new(albedo.outputs['Color'],shader.inputs['Base Color'])
 normal=nodes.new('ShaderNodeTexImage');normal.image=bpy.data.images.get(kind+'Normal');normal.image.colorspace_settings.name='Non-Color';bump=nodes.new('ShaderNodeNormalMap');bump.inputs['Strength'].default_value=.45;links.new(normal.outputs['Color'],bump.inputs['Color']);links.new(bump.outputs['Normal'],shader.inputs['Normal'])
 surface=nodes.new('ShaderNodeTexImage');surface.image=bpy.data.images.get(kind+'Surface');surface.image.colorspace_settings.name='Non-Color';rough=nodes.new('ShaderNodeMath');rough.operation='SUBTRACT';rough.inputs[0].default_value=1;links.new(surface.outputs['Alpha'],rough.inputs[1]);links.new(rough.outputs[0],shader.inputs['Roughness'])
 for img in [albedo.image,normal.image,surface.image]:img.pack()
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
 f.extend([tuple(reversed(range(12))),tuple(range((len(sections)-1)*12,len(v)))]);o=mesh(name,v,f,mat);sub=o.modifiers.new("Smooth aerospace shell","SUBSURF");sub.levels=2;sub.render_levels=2
 for poly in o.data.polygons:poly.use_smooth=True
 bevel(o,.02);return o
def pivot(name,loc):
 o=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(o);o.location=loc;bpy.context.view_layer.update();return o
def attach(o,p):
 bpy.context.view_layer.update();w=o.matrix_world.copy();o.parent=p;o.matrix_world=w;return o
def rod(name,a,b,r,mat):
 a,b=Vector(a),Vector(b);bpy.ops.mesh.primitive_cylinder_add(vertices=16,radius=r,depth=(b-a).length,location=(a+b)/2);o=bpy.context.object;o.name=name;o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();o.data.materials.append(mat);bevel(o,.009);return o
def panel(name,v,mat,depth=.09):
 n=len(v);o=mesh(name,v+[(x,y,z-depth) for x,y,z in v],[tuple(range(n)),tuple(reversed(range(n,2*n)))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)],mat);bevel(o,.025);return o
def label(text,loc,size=.1,mat=None):
 bpy.ops.object.text_add(location=loc);o=bpy.context.object;o.name='Stencil '+text;o.data.body=text;o.data.size=size;o.data.extrude=.0005;o.data.materials.append(mat or black);bpy.ops.object.convert(target='MESH');return o
loft('Pressure vessel ceramic aeroshell',[(-3,1.02,.43,.08),(-2,1.25,.62,.1),(0,1.12,.7,.18),(1.9,.67,.45,.1),(3.35,.24,.18,-.04),(3.65,.10,.09,-.09)],ceramic)
loft('RCC blunt nose cap',[(3.15,.31,.23,-.03),(3.45,.20,.15,-.06),(3.77,.065,.065,-.08),(3.83,.015,.02,-.08)],black)
loft('Ventral thermal protection',[(-3,1.03,.15,-.28),(-1.7,1.25,.2,-.32),(.5,.99,.18,-.29),(2.4,.47,.12,-.22),(3.6,.09,.045,-.13)],black)
# A pressure-supported cockpit with separate flat panes, thick ceramic framing and seals.
loft('Cockpit pressure fairing',[(-.05,.64,.18,.73),(.55,.68,.27,.73),(1.2,.50,.18,.68),(1.95,.22,.05,.48)],ceramic)
for side in [-1,1]:
 panes=[[(side*.045,1.75,.60),(side*.39,1.42,.68),(side*.43,.86,.94),(side*.045,.92,1.005)],[(side*.48,.84,.91),(side*.43,1.37,.68),(side*.63,.88,.73),(side*.65,.24,.83)]]
 for i,v in enumerate(panes):
  panel('Window pressure frame',v,black,.04);center=sum((Vector(p) for p in v),Vector())/4
  vv=[tuple(center+(Vector(p)-center)*.83+Vector((0,0,.012))) for p in v];panel('Fused silica window '+str(i),vv,glass,.018)
# Dorsal avionics access hatch and restrained identification.
box('Dorsal access gasket',(0,-.78,.829),(.82,1.05,.035),black,.08)
box('Flush avionics cover',(0,-.78,.853),(.77,1,.025),ceramic,.07)
for x in [-.30,.30]:
 for y in [-1.15,-.42]:box('Quarter turn hatch latch',(x,y,.87),(.038,.095,.013),metal,.008)
label('07',(0,-1.05,.872),.22);label('AVIONICS',(-.27,-.62,.875),.068)
for side in [-1,1]:
 # Thick load-bearing root and swept leading-edge RCC, with a separate trailing elevon.
 loft('Wing root carry through fairing', [(-2.85,.38,.12,.0),(-1.7,.48,.22,.07),(-.5,.30,.19,.13),(1,.09,.06,.0)],ceramic).location.x=side*.97
 v=[(side*.78,1.1,-.035),(side*3.6,-2.5,-.055),(side*3.52,-2.66,-.015),(side*.76,-2.44,.10)]
 panel('Structural delta wing',v,ceramic,.21)
 edge=[(side*.80,1.14,-.008),(side*3.65,-2.47,-.027),(side*3.49,-2.53,.005),(side*.89,.98,.015)]
 panel('RCC leading edge',edge,black,.14)
 elevon=pivot('ElevonL' if side<0 else 'ElevonR',(side*2.05,-2.59,.015))
 attach(panel('Elevon thermal sandwich',[(side*.90,-2.49,.09),(side*3.46,-2.70,-.012),(side*3.40,-3.14,-.035),(side*.9,-2.99,.05)],ceramic,.15),elevon)
 for x in [1.1,2.0,3.0]:
  rod('Elevon hinge bearing',(side*(x-.075),-2.58,.01),(side*(x+.075),-2.58,.01),.045,metal)
 for x in [1.20,2.9]:
  box('Actuator fairing',(side*x,-2.42,.10),(.13,.35,.12),ceramic,.05)
 o=box('Rescue livery',(side*2.82,-2.33,.035),(.20,.39,.025),orange);o.rotation_euler.z=side*.48
 # Forward winglet box and genuinely articulated trailing rudder/speedbrake.
 panel('Winglet torsion box',[(side*3.28,-2.65,.01),(side*3.25,-1.85,.10),(side*3.12,-2.35,1.25),(side*3.16,-2.67,1.3)],ceramic,.07)
 rudder=pivot('RudderL' if side<0 else 'RudderR',(side*3.19,-2.67,.12))
 attach(panel('Split wingtip drag rudder',[(side*3.28,-2.70,.02),(side*3.16,-2.70,1.3),(side*3.18,-3.08,1.27),(side*3.32,-3.10,.03)],black,.055),rudder)
 box('Navigation light housing',(side*3.39,-2.60,.11),(.15,.28,.09),black)
 box('Wing navigation light',(side*3.41,-2.55,.135),(.08,.12,.05),cyan)
 # OMS nacelle with a supported, visibly hollow nozzle and concentric throat.
 loft('OMS structural blister',[(-3.20,.43,.29,.27),(-2.70,.48,.37,.31),(-1.4,.31,.22,.43),(-.85,.05,.055,.45)],ceramic).location.x=side*.67
 cx=side*.62;cz=.16
 sections=[(-2.95,.15),(-3.10,.18),(-3.35,.31),(-3.58,.39),(-3.61,.39),(-3.59,.35),(-3.35,.275),(-3.12,.14)]
 vv=[(cx+r*math.cos(j*2*math.pi/48),y,cz+r*math.sin(j*2*math.pi/48)) for y,r in sections for j in range(48)]
 faces=[(i*48+j,i*48+(j+1)%48,(i+1)*48+(j+1)%48,(i+1)*48+j) for i in range(len(sections)-1) for j in range(48)]
 nozzle=mesh('Regeneratively cooled OMS bell',vv,faces,metal)
 for p in nozzle.data.polygons:p.use_smooth=True
 bpy.ops.mesh.primitive_cylinder_add(vertices=32,radius=.14,depth=.02,location=(cx,-3.13,cz),rotation=(math.pi/2,0,0));bpy.context.object.data.materials.append(black)
 for a in range(0,360,30):
  an=math.radians(a);rod('Bell cooling rib',(cx+.20*math.cos(an),-3.14,cz+.20*math.sin(an)),(cx+.386*math.cos(an),-3.58,cz+.386*math.sin(an)),.008,metal)
 for j in range(3):
  # Small recessed reaction-control ports rather than decorative surface spikes.
  box('RCS ceramic insert',(side*.99,-.4+j*.20,.49),(.12,.13,.09),black,.025)
  box('RCS nozzle recess',(side*1.006,-.4+j*.20,.533),(.045,.065,.015),metal,.014)
 label('EMBER', (side*1.52,-2.15,.13),.16)
 label('THERMAL / NO STEP',(side*1.42,-2.38,.13),.060)
# Articulated aft body flap.
body=pivot('BodyFlap',(0,-2.98,-.32))
attach(box('Ventral body flap',(0,-3.33,-.33),(1.12,.66,.13),black,.035),body)
# Tricycle landing gear: pivoting oleos, torque links, twin main wheels, inset hubs.
gear=pivot('LandingGear',(0,0,0))
for suffix,x,y in [('L',-.84,-1.8),('R',.84,-1.8),('N',0,1.95)]:
 root=pivot('Gear'+suffix,(x,y,-.34));attach(root,gear)
 attach(rod('Oleo outer cylinder',(x,y,-.34),(x,y,-.76),.064,metal),root)
 attach(rod('Chrome piston',(x,y,-.72),(x,y,-1.00),.038,metal),root)
 attach(rod('Torque link upper',(x,y-.07,-.58),(x,y-.20,-.79),.025,metal),root)
 attach(rod('Torque link lower',(x,y-.20,-.79),(x,y-.07,-.96),.025,metal),root)
 attach(rod('Drag brace',(x,y+.28,-.36),(x,y,-.87),.032,metal),root)
 for k,dx in enumerate([-.115,.115] if suffix!='N' else [0]):
  wheel=pivot('Wheel'+suffix+str(k),(x+dx,y,-1.00));attach(wheel,root)
  bpy.ops.mesh.primitive_torus_add(major_radius=.175,minor_radius=.065,major_segments=32,minor_segments=12,location=(x+dx,y,-1),rotation=(0,math.pi/2,0));o=bpy.context.object;o.name='Grooved tire';o.data.materials.append(black);attach(o,wheel)
  attach(rod('Wheel hub',(x+dx-.067,y,-1),(x+dx+.067,y,-1),.115,metal),wheel)
  for a in range(0,360,60):
   an=math.radians(a);attach(rod('Wheel spoke',(x+dx+.075,y,-1),(x+dx+.075,y+.105*math.cos(an),-1+.105*math.sin(an)),.012,ceramic),wheel)
 door=pivot('GearDoor'+suffix,(x-.18,y,-.36));attach(door,gear)
 attach(box('Insulated gear bay door',(x,y,-.39),(.35,.72,.045),black,.022),door)

# Correct and verify outward-facing closed surfaces before export.
import bmesh
for obj in list(bpy.context.scene.objects):
 if obj.type=='MESH':
  bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
  uv=obj.data.uv_layers.new(name="Surface panels") if not obj.data.uv_layers else obj.data.uv_layers.active
  for face in obj.data.polygons:
   axis=max(range(3),key=lambda k:abs(face.normal[k]));axes=[k for k in range(3) if k!=axis]
   for li in face.loop_indices:
    co=obj.data.vertices[obj.data.loops[li].vertex_index].co;uv.data[li].uv=(co[axes[0]]/3+.5,co[axes[1]]/3+.5)
# Named physical direction anchors, exported with the geometry.
for name,loc in [('NoseAxis',(0,3.85,0)),('TailAxis',(0,-3.45,0))]:
 ob=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(ob);ob.location=loc
# Export applied model with material names; retain editable source in Blender.
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'art','Ember.blend'))
# Export optimized material groups while preserving editable Blender source and gear hierarchy.
for mat in [ceramic,black,metal,glass,orange,cyan]:
 group=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.parent is None and len(o.data.materials)==1 and o.data.materials[0]==mat]
 if not group:continue
 bpy.ops.object.select_all(action='DESELECT')
 for o in group:
  o.select_set(True);bpy.context.view_layer.objects.active=o
  for mod in list(o.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
 bpy.context.view_layer.objects.active=group[0];bpy.ops.object.join();group[0].name='Airframe '+mat.name
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.fbx(filepath=os.path.join(ROOT,'Reentry/Assets/Resources/Ember.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=False,add_leaf_bones=False)
print('BLENDER_ASSET_EXPORTED',len(bpy.data.objects),'objects')
# Hangar visual reference
world=bpy.context.scene.world or bpy.data.worlds.new('World');bpy.context.scene.world=world;world.color=(.05,.05,.05)
bpy.ops.object.camera_add(location=(11,13,10));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,0))-cam.location).to_track_quat('-Z','Y').to_euler();bpy.context.scene.camera=cam
for loc,power,size in [((3,4,10),2200,7),((-5,0,4),1600,6),((0,-6,5),1800,4)]:
 bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.data.energy=power;o.data.shape='DISK';o.data.size=size;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
s=bpy.context.scene;s.render.engine='CYCLES';s.cycles.samples=24;s.render.resolution_x=1100;s.render.resolution_y=750;s.render.resolution_percentage=100;s.render.image_settings.file_format='PNG';s.render.filepath=os.path.join(ROOT,'art','spacecraft.png');s.render.film_transparent=True;bpy.ops.render.render(write_still=True)
