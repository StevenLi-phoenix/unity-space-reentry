using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.TextCore.LowLevel;
public static class BuildReentry {
 public static void Test(){FlightTests.Run();PresentationTests.Run();LandingTests.Run();DebriefTests.Run();AssetChecks.Run();}
 public static void Web(){Build(BuildTarget.WebGL,"Build/WebGL");}
 public static void Desktop(){EditorUserBuildSettings.SetPlatformSettings("OSXUniversal","Architecture","ARM64");Build(BuildTarget.StandaloneOSX,"Build/Ember.app");}
 public static void MacOS(){
  string previous=EditorUserBuildSettings.GetPlatformSettings("OSXUniversal","Architecture");
  try{EditorUserBuildSettings.SetPlatformSettings("OSXUniversal","Architecture","x64ARM64");Build(BuildTarget.StandaloneOSX,"Build/macOS/Ember.app");}
  finally{EditorUserBuildSettings.SetPlatformSettings("OSXUniversal","Architecture",previous);}
 }
 public static void Windows(){Build(BuildTarget.StandaloneWindows64,"Build/Windows/Ember.exe");}
 public static void Linux(){Build(BuildTarget.StandaloneLinux64,"Build/Linux/Ember.x86_64");}
 static void Build(BuildTarget target,string output){
  FlightTests.Run();PresentationTests.Run();LandingTests.Run();DebriefTests.Run();AssetChecks.Run();
  if(!AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Fonts/BodySDF.asset")){
   var f=TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/Body.ttf"),64,8,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
   string chars="";for(int i=32;i<127;i++)chars+=(char)i;f.TryAddCharacters(chars,out _);f.atlasPopulationMode=AtlasPopulationMode.Static;AssetDatabase.CreateAsset(f,"Assets/Resources/Fonts/BodySDF.asset");foreach(var t in f.atlasTextures)AssetDatabase.AddObjectToAsset(t,f);AssetDatabase.AddObjectToAsset(f.material,f);
  }
  var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);settings.FindProperty("activeInputHandler").intValue=1;settings.ApplyModifiedPropertiesWithoutUndo();
  var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Resources/Renderer.asset");if(!renderer){renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,"Assets/Resources/Renderer.asset");}
  var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Resources/Pipeline.asset");if(!pipeline){pipeline=UniversalRenderPipelineAsset.Create(renderer);AssetDatabase.CreateAsset(pipeline,"Assets/Resources/Pipeline.asset");}
  pipeline.supportsCameraDepthTexture=true;pipeline.supportsHDR=true;pipeline.msaaSampleCount=2;pipeline.shadowDistance=80;
  GraphicsSettings.defaultRenderPipeline=pipeline;for(int i=0;i<QualitySettings.names.Length;i++){QualitySettings.SetQualityLevel(i,false);QualitySettings.renderPipeline=pipeline;}
  foreach(string shader in new[]{"Universal Render Pipeline/Lit","Universal Render Pipeline/Unlit","Ember/Atmosphere","Ember/Planet","Ember/Plasma","Ember/Clouds","Ember/Ground","Ember/Sky","Ember/SoftMark"}){string p="Assets/Resources/"+shader.Substring(shader.LastIndexOf('/')+1)+".mat";if(!AssetDatabase.LoadAssetAtPath<Material>(p))AssetDatabase.CreateAsset(new Material(Shader.Find(shader)),p);}
  foreach(string kind in new[]{"Hull","Carbon"}){
   string normalPath="Assets/Resources/"+kind+"Normal.png";var importer=(TextureImporter)AssetImporter.GetAtPath(normalPath);importer.textureType=TextureImporterType.NormalMap;importer.SaveAndReimport();
   string surfacePath="Assets/Resources/"+kind+"Surface.png";var surfaceImporter=(TextureImporter)AssetImporter.GetAtPath(surfacePath);surfaceImporter.sRGBTexture=false;surfaceImporter.alphaSource=TextureImporterAlphaSource.FromInput;surfaceImporter.SaveAndReimport();
   string path="Assets/Resources/"+kind+"PBR.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
   material.SetTexture("_BaseMap",Resources.Load<Texture2D>(kind+"Albedo"));material.SetTexture("_BumpMap",Resources.Load<Texture2D>(kind+"Normal"));material.EnableKeyword("_NORMALMAP");material.SetTexture("_MetallicGlossMap",Resources.Load<Texture2D>(kind+"Surface"));material.EnableKeyword("_METALLICSPECGLOSSMAP");material.SetFloat("_BumpScale",.45f);material.SetFloat("_Smoothness",1);material.SetFloat("_Metallic",kind=="Hull"?.02f:.01f);EditorUtility.SetDirty(material);
  }
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("EMBER - Flight Director").AddComponent<ReentryGame>();EditorSceneManager.SaveScene(scene,"Assets/Ember.unity");
  PlayerSettings.productName="EMBER - Return to Earth";PlayerSettings.companyName="Steven Li";PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
  PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;PlayerSettings.WebGL.decompressionFallback=true;PlayerSettings.WebGL.template="PROJECT:Reentry";
  PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL,new[]{GraphicsDeviceType.OpenGLES3});
  if(target!=BuildTarget.WebGL)PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
  System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(output));
  Debug.Log($"Building {target}, version {PlayerSettings.bundleVersion}, output {output}");
  AssetDatabase.SaveAssets();var report=BuildPipeline.BuildPlayer(new[]{"Assets/Ember.unity"},output,target,BuildOptions.None);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed");Debug.Log("EMBER_BUILD_PASSED "+output);
 }
}
