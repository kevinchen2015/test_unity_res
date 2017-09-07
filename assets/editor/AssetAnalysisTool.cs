using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;


//分析资源引用和一些关注数据
// 如： prefab引用的 贴图，shader
//    贴图的尺寸，格式

//todo... 针对性的进行处理

namespace tpf
{

   
    public class AssetAnalysisTool
    {
        static AssetInfo s_AssetInfo = new AssetInfo();
 

        [Serializable]
        public class PrefabInfo
        {
            public string name;
            public List<string> dependAsset = new List<string>();

        }



        [Serializable]
        public class AssetInfo
        {
            public List<TextureInfo> textureInfo = new List<TextureInfo>();
            public List<ShaderInfo> shaderInfo = new List<ShaderInfo>();
            public List<MeshInfo> meshInfo = new List<MeshInfo>();
            public void Clear()
            {
                textureInfo.Clear();
                meshInfo.Clear();
            }
        }

        [Serializable]
        public class ShaderInfo
        {
            public string name;
            public int refCount;
        }

        [Serializable]
        public class TextureInfo
        {
            public string name;
            public int refCount;
            public Vector2 size;
            public TextureFormat format;
            public int mipmap;
          
        }

        [Serializable]
        public class MeshInfo
        {
            public string name;
            public string meshFilter;
            public int refCount;
            public int vertexCount;
            public int triangleCount;
            public int subMeshCount;
        }

        [Serializable]
        public class AnimationClipInfo
        {
            public string name;
            public float frameRate;
            public float timeLength;
        }

        [Serializable]
        public class AudioClipInfo
        {
            public string name;
            public int channels;
            public int frequency;
            public float timeLength;
            public AudioClipLoadType loadType;
        }
        
        
        [MenuItem("AssetTool/AnalysisEffect")]
        public static void AnalysisEffect()
        {
            s_AssetInfo.Clear();

            List<PrefabInfo> prefabInfos = new List<PrefabInfo>();
            Dictionary<string, int> refCount = new Dictionary<string, int>();

            string path = Application.dataPath + "/resources/effect/";
            List<string> files = new List<string>();
            List<string> prefabs = new List<string>();
            PathUtils.GetFileList(ref files, path, true);
            PathUtils.GetFileListByEnds(ref files, ".prefab", ref prefabs);

            for (int i = 0; i < prefabs.Count; ++i)
            {
                string assets = "/assets/";
                int idx = prefabs[i].ToLower().IndexOf(assets);
                int startPos = idx + 1;// assets.Length;
                prefabs[i] = prefabs[i].Substring(startPos, prefabs[i].Length- startPos);
            }

            List<GameObject> allPrefab = new List<GameObject>();
            for (int i = 0;i < prefabs.Count;++i)
            {
                EditorUtility.DisplayProgressBar("load prefab", "handle...", (float)i / prefabs.Count);

                GameObject prefab = AssetDatabase.LoadAssetAtPath(prefabs[i], typeof(GameObject)) as GameObject;
                allPrefab.Add(prefab);

                UnityEngine.Object[] roots = new UnityEngine.Object[] { prefab };
                UnityEngine.Object[] dependObjs = EditorUtility.CollectDependencies(roots);

                PrefabInfo info = new PrefabInfo();
                info.name = prefabs[i];

                for(int k=0;k < dependObjs.Length;++k )
                {
                    string p = AssetDatabase.GetAssetPath(dependObjs[k]);
                    info.dependAsset.Add(p);

                    int count = 0;
                    if (dependObjs[k] == null) continue;
                    if(refCount.TryGetValue(p, out count))
                    {
                        refCount[p] += 1;
                    }
                    else
                    {
                        refCount[p] = 1;
                    }
                }
                prefabInfos.Add(info);
            }
            EditorUtility.ClearProgressBar();

            UnityEngine.Object[] all = allPrefab.ToArray();
            UnityEngine.Object[] allDependObjs = EditorUtility.CollectDependencies(all);
            for (int k = 0; k < allDependObjs.Length; ++k)
            {
                EditorUtility.DisplayProgressBar("foreach dependobjs", "handle...", (float)k / allDependObjs.Length);

                UnityEngine.Object obj = allDependObjs[k];
                string p = AssetDatabase.GetAssetPath(obj);

                MeshFilter meshFilter = obj as MeshFilter;
                if(meshFilter != null)
                {
                    Mesh meshObj = meshFilter.sharedMesh;
                    MeshInfo info = new MeshInfo();
                    info.name = p;
                    info.meshFilter = meshFilter.name;
                    info.refCount = refCount[p];
                    if(meshObj != null)
                    {
                        info.subMeshCount = meshObj.subMeshCount;
                        info.vertexCount = meshObj.vertexCount;
                        info.triangleCount = meshObj.triangles.Length;
                    }
                    s_AssetInfo.meshInfo.Add(info);
                }

                Texture2D texObj = obj as Texture2D;
                if (texObj != null)
                {
                    TextureInfo info = new TextureInfo();
                    info.name = p;
                    info.refCount = refCount[p];
                    info.size.x = texObj.width;
                    info.size.y = texObj.height;
                    info.mipmap = texObj.mipmapCount;
                    info.format = texObj.format;
                    s_AssetInfo.textureInfo.Add(info);
                }

                Shader shaderObj = obj as Shader;
                if (shaderObj != null)
                {
                    ShaderInfo info = new ShaderInfo();
                    info.name = p;
                    info.refCount = refCount[p];
                    s_AssetInfo.shaderInfo.Add(info);
                }
            }
            EditorUtility.ClearProgressBar();
            EditorUtility.UnloadUnusedAssetsImmediate();

            string outputFile = Application.dataPath + "AssetAnalysis.json";
            SerializeHelper.SaveToFile(s_AssetInfo, outputFile, true);


            {
                string fileName = Application.dataPath + "texture.csv";
                if (File.Exists(fileName)) File.Delete(fileName);

                FileStream file = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(file);
                string line = "path,width,height,format,mipmap,refCount";
                sw.WriteLine(line);
                for (int i = 0;i < s_AssetInfo.textureInfo.Count;++i)
                {
                    TextureInfo info = s_AssetInfo.textureInfo[i];
                    //if(info.size.x > 128 || info.size.y > 128)
                    {
                        line = info.name+","+info.size.x+","+info.size.y + "," +info.format.ToString()+","+info.mipmap+","+info.refCount;
                        sw.WriteLine(line);
                    }
                }

                sw.Flush();
                file.Flush();
                file.Close();
            }

            Debug.Log("分析完毕!");

        }

    }
}