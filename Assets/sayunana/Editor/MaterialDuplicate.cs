using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace sayunana
{
    public class MaterialDuplicate : EditorWindow
    {
        /// <summary>
        /// アバターのオブジェクト
        /// </summary>
        private GameObject avatarGameObject = null;

        /// <summary>
        /// MToonマテリアルを保存するファイルのパス
        /// </summary>
        /// <returns></returns>
        private string saveMtoonMaterialsFilePath = String.Empty;

        
        [MenuItem("sayunana/MaterialDuplicate")]
        public static void ShowExample()
        {
            MaterialDuplicate wnd = GetWindow<MaterialDuplicate>();
            wnd.titleContent = new GUIContent("MaterialDuplicate");
        }
        public void CreateGUI()
        {
            #region UIの生成

            VisualElement root = rootVisualElement;
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/sayunana/Editor/MaterialDuplicate.uss");
            root.styleSheets.Add(styleSheet);
            root.AddToClassList("Root");

            //タイトル
            VisualElement titleLabelWithStyle = new Label()
            {
                text = "マテリアルの複製",
            };
            titleLabelWithStyle.AddToClassList("Title");
            root.Add(titleLabelWithStyle);

            //詳細
            VisualElement detailLabelWithStyle = new Label()
            {
                text = "アバターに使用しているマテリアルを複製します"
            };
            root.Add(detailLabelWithStyle);

            //アバターオブジェクト取得
            ObjectField objField = new ObjectField()
            {
                label = "マテリアルを複製するアバターオブジェクト",
                objectType = typeof(GameObject),
                value = avatarGameObject
            };
            objField.AddToClassList("Margin");
            root.Add(objField);


            //マテリアルの保存先を指定するボタン
            var saveMToonFilePathButton = new Button()
            {
                text = "保存先を指定する"
            };
            root.Add(saveMToonFilePathButton);

            //保存先を表示
            var saveFilePathText = new TextElement()
            {
                text = saveMtoonMaterialsFilePath
            };
            root.Add(saveFilePathText);

            //変換ボタン
            var materialTransformButton = new Button()
            {
                text = "変換"
            };
            root.Add(materialTransformButton);

            #endregion


            #region 表示状態を初期化

            saveMToonFilePathButton.style.display = DisplayStyle.None;
            saveFilePathText.style.display = DisplayStyle.None;
            materialTransformButton.style.display = DisplayStyle.None;

            #endregion

            #region アクション

            //アバターオブジェクトを設定したときの処理
            objField.RegisterValueChangedCallback((evt) =>
            {
                avatarGameObject = (GameObject)evt.newValue;
                if (avatarGameObject != null)
                {
                    saveMToonFilePathButton.style.display = DisplayStyle.Flex;
                }
                else
                {
                    saveMToonFilePathButton.style.display = DisplayStyle.None;
                }
            });

            //保存先のパスを指定するボタンを押したときの処理
            saveMToonFilePathButton.clicked += () =>
            {
                SetSaveMaterialsFilePath();
                saveFilePathText.style.display = DisplayStyle.Flex;
                saveFilePathText.text = saveMtoonMaterialsFilePath;
                materialTransformButton.style.display = DisplayStyle.Flex;
            };

            //マテリアルを変換
            materialTransformButton.clicked += () =>
            {
                //オブジェクトを複製
                var copyAvatar = GameObject.Instantiate(avatarGameObject);
                avatarGameObject.SetActive(false);
                copyAvatar.name = $"VRM_{copyAvatar.name.Replace("(Clone)", "")}";
                GetAllSkinnedMeshRenderer(copyAvatar);
            };

            #endregion
        }

        private void SetSaveMaterialsFilePath()
        {
            string path = EditorUtility.OpenFolderPanel("マテリアルの保存先を指定", "", "");
            saveMtoonMaterialsFilePath = path;
        }

        private SkinnedMeshRenderer[] skinnedMeshRenderers;

        private void GetAllSkinnedMeshRenderer(GameObject gameObject)
        {
            //アバターのメッシュレンダラーをまとめて取得する
            skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            //マテリアルの生成
            foreach (var renderer in skinnedMeshRenderers)
            {
                var mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    CreateMToonMaterial(mats[i]);
                }

                renderer.sharedMaterials = mats;
            }

            //マテリアルの差し替え
            foreach (var renderer in skinnedMeshRenderers)
            {
                var mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var mat = GetMaterial(mats[i].name);
                    mats[i] = mat;
                }

                renderer.sharedMaterials = mats;
            }
        }

        private void CreateMToonMaterial(Material liltoonMaterial)
        {
            var mtoonMaterial = new Material(liltoonMaterial);
            string directoryPath = saveMtoonMaterialsFilePath;
            string newFileName = $"copy_" + Path.GetFileNameWithoutExtension(liltoonMaterial.name);
            string saveMaterialPath = FileUtil.GetProjectRelativePath(directoryPath) + "/" + newFileName + ".mat";

            if (!string.IsNullOrEmpty(directoryPath))
            {
                AssetDatabase.CreateAsset(mtoonMaterial, Path.Combine(saveMaterialPath));
                AssetDatabase.SaveAssets();
            }
        }

        private Material GetMaterial(string materialName)
        {
            string directoryPath = saveMtoonMaterialsFilePath;
            string newFileName = $"copy_" + Path.GetFileNameWithoutExtension(materialName);
            string saveMaterialPath = FileUtil.GetProjectRelativePath(directoryPath) + "/" + newFileName + ".mat";

            var mat = AssetDatabase.LoadAssetAtPath<Material>(saveMaterialPath);
            return mat;
        }
    }
}