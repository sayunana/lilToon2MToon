#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if ENABLE_LILTOON
using lilToon;
#endif

namespace sayunana
{
    public class lilToon2MToon : EditorWindow
    {
        private Editor _editor;

        private Animator root;

        /// <summary>
        /// MToonマテリアルを保存するファイルのパス
        /// </summary>
        private string saveMtoonMaterialsFilePath = String.Empty;

        [MenuItem("sayunana/lilToon2MToon")]
        static void Open()
        {
            var window = GetWindow<lilToon2MToon>();
            window.titleContent = new GUIContent("lilToon2MToon");
        }

        /// <Summary>
        /// ウィンドウのパーツを表示する
        /// </Summary>
        void OnGUI()
        {
            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.wordWrap = true;

            GUIStyle errorTextStyle = new GUIStyle(GUI.skin.label);
            errorTextStyle.wordWrap = true;
            errorTextStyle.normal.textColor = Color.red;
            errorTextStyle.hover.textColor = Color.red;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.wordWrap = true;

            GUILayout.Label("lilToon2MToon\n" +
                            "このエディターではアバターに登録されているlilToonのマテリアルをMToonに変換し差し替えます。", textStyle);

            if (IsCheckImportlilToon() == false)
            {
                GUILayout.Label("lilToonがインポートされていません", errorTextStyle);
                if (GUILayout.Button("lilToonをインポートしてください", buttonStyle))
                {
                    Application.OpenURL("https://github.com/lilxyzw/lilToon");
                }
                return;
            }
#if !ENABLE_LILTOON
            GUILayout.Label("lilToonのバージョンが足りていません",errorTextStyle);
            // todo:lilMaterialBakerが一般公開されたら削除する
            GUILayout.Label("devをインポートしてください",errorTextStyle);
            return;
#endif

            
            if(IsCheckImportMToon() == false)
            {
                GUILayout.Label("MToonがインポートされていません", errorTextStyle);
                if (GUILayout.Button("MToonをインポートしてください",buttonStyle))
                {
                    Application.OpenURL("https://vrm.dev/");
                }
                return;
            }
            
            GUILayout.Space(30);

            if (GUILayout.Button("マテリアルを保存するファイルパスを選択", buttonStyle))
            {
                SetSaveMaterialsFilePath();
            }

            if (InUnityProjectPath(saveMtoonMaterialsFilePath))
            {
                GUILayout.Label("保存先：" + GetProjectRelativePath(saveMtoonMaterialsFilePath), textStyle);
            }
            else
            {
                GUILayout.Label("保存先：", textStyle);
            }

            root = (Animator)EditorGUILayout.ObjectField("アバターオブジェクト", root, typeof(Animator), true);

            if (saveMtoonMaterialsFilePath == String.Empty)
            {
                GUILayout.Label("保存先を指定してください", errorTextStyle);
            }

            if (!InUnityProjectPath(saveMtoonMaterialsFilePath) && saveMtoonMaterialsFilePath != String.Empty)
            {
                GUILayout.Label("保存先がAssets外です\n" +
                                "保存先をAssets内に変更してください", errorTextStyle);
            }

            if (root == null)
            {
                GUILayout.Label("アニメーターをセットしてください", errorTextStyle);
            }

            if (root != null && saveMtoonMaterialsFilePath != String.Empty &&
                InUnityProjectPath(saveMtoonMaterialsFilePath))
            {
                if (GUILayout.Button("lilToonをMToonに変換する", buttonStyle))
                {
                    //一旦すべての非表示オブジェクトを表示する
                    var setActiveFalseList = SetActiveTrueInAllChildren(root.gameObject);

                    var skinnedMeshRenderers = root.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                    List<Material> referenceMaterials = new List<Material>();

                    //使用しているマテリアルをリストに追加
                    foreach (var skinnedMesh in skinnedMeshRenderers)
                    {
                        foreach (var sMaterial in skinnedMesh.sharedMaterials)
                        {
                            //lilToonのマテリアルか判定
                            if (IslilToonShader(sMaterial))
                            {
                                //マテリアルがリストに登録されているか判定
                                if (!referenceMaterials.Contains(sMaterial))
                                {
                                    referenceMaterials.Add(sMaterial);
                                }
                            }
                            else
                            {
                                Debug.LogError($"{sMaterial.name}はlitToonのマテリアルではありません", sMaterial);
                            }
                        }
                    }

                    //変換処理
                    Dictionary<Material, Material> convertedMaterials = new Dictionary<Material, Material>();
                    foreach (var material in referenceMaterials)
                    {
                        //MToonにマテリアルを変換
                        var path = Path.Combine(GetProjectRelativePath(saveMtoonMaterialsFilePath),
                            material.name + ".mat");
                        try
                        {
                            ConvertlilToon(material, path);
                        }
                        catch (Exception e)
                        {
                            //Debug.Log(material.name);
                        }

                        Material m = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
                        convertedMaterials.Add(material, m);
                    }

                    //マテリアルの差し替え
                    foreach (var skinnedMesh in skinnedMeshRenderers)
                    {
                        var materials = skinnedMesh.sharedMaterials;
                        for (int i = 0; i < materials.Length; i++)
                        {
                            //lilToonのマテリアルか判定
                            if (IslilToonShader(materials[i]))
                            {
                                if (convertedMaterials.ContainsKey(materials[i]))
                                {
                                    materials[i] = convertedMaterials[materials[i]];
                                }
                            }
                        }

                        skinnedMesh.sharedMaterials = materials;
                    }

                    //非表示オブジェクトを非表示に戻す
                    setActiveFalseList.ToList().ForEach(obj => obj.SetActive(false));
                }
            }
        }

        void SetSaveMaterialsFilePath()
        {
            string path = EditorUtility.OpenFolderPanel("マテリアルの保存先を指定", "", "");
            saveMtoonMaterialsFilePath = path;
        }

        void ConvertlilToon(Material mat, string path)
        {
#if ENABLE_LILTOON
            lilMaterialBaker.CreateMToonMaterial(mat, path);
#endif
        }

        //引数のパスがUnityプロジェクト内にあるか判定
        bool InUnityProjectPath(string fullPath)
        {
            var path = fullPath.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries);

            return path.Any(t => t == "Assets");
        }

        //引数のフルパスからプロジェクト内の相対パスを取得
        string GetProjectRelativePath(string fullPath)
        {
            var path = fullPath.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries);

            int index = 0;
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] == "Assets")
                {
                    index = i;
                    break;
                }
            }

            string projectRelativePath = "";
            for (int i = index; i < path.Length; i++)
            {
                projectRelativePath += path[i];
                if (i != path.Length - 1)
                {
                    projectRelativePath += "/";
                }
            }

            return projectRelativePath;
        }

        //引数のオブジェクトの子オブジェクトをすべてアクティブにする
        GameObject[] SetActiveTrueInAllChildren(GameObject obj)
        {
            List<GameObject> list = new List<GameObject>();
            foreach (Transform n in obj.transform)
            {
                if (n.gameObject.activeSelf == false)
                {
                    n.gameObject.SetActive(true);
                    list.Add(n.gameObject);
                }

                list.AddRange(SetActiveTrueInAllChildren(n.gameObject));
            }

            return list.ToArray();
        }

        static bool IsCheckImportMToon()
        {
            Shader shader = Shader.Find("VRM/MToon");
            return shader != null;
        }

        static bool IsCheckImportlilToon()
        {
            Shader shader = Shader.Find("lilToon");
            return shader != null;
        }
        
        //lilToonのマテリアルか判定
        bool IslilToonShader(Material material)
        {
            if (material == null)
                return false;

            return material.shader.name.Contains("lilToon");
        }
    }
}
#endif