using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class MaterialReplacement : EditorWindow
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

    [MenuItem("sayunana/MaterialReplacement")]
    public static void ShowExample()
    {
        MaterialReplacement wnd = GetWindow<MaterialReplacement>();
        wnd.titleContent = new GUIContent("MaterialReplacement");
    }

    public void CreateGUI()
    {
        #region UIの生成

        VisualElement root = rootVisualElement;
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/sayunana/Editor/MaterialReplacement.uss");
        root.styleSheets.Add(styleSheet);
        root.AddToClassList("Root");

        //タイトル
        VisualElement titleLabelWithStyle = new Label()
        {
            text = "マテリアルの差し替え",
        };
        titleLabelWithStyle.AddToClassList("Title");
        root.Add(titleLabelWithStyle);

        //詳細
        VisualElement detailLabelWithStyle = new Label()
        {
            text = "SkinMeshRendererに割り当てられているMaterialを\n" +
                   "SkinMeshRendererに割り当てられているMaterialの名前 + _mtoon\n" +
                   "の名前のMaterialに差し替えます"
        };
        root.Add(detailLabelWithStyle);

        //アバターオブジェクト取得
        ObjectField objField = new ObjectField()
        {
            label = "マテリアルを差し替えるアバターオブジェクト",
            objectType = typeof(GameObject),
            value = avatarGameObject
        };
        objField.AddToClassList("Margin");
        root.Add(objField);


        //マテリアルの保存先を指定するボタン
        var saveMToonFilePathButton = new Button()
        {
            text = "MToonマテリアルの保存先を指定する"
        };
        root.Add(saveMToonFilePathButton);

        //保存先を表示
        var saveFilePathText = new TextElement()
        {
            text = saveMtoonMaterialsFilePath
        };
        root.Add(saveFilePathText);

        //差し替えボタン
        var materialReplacementButton = new Button()
        {
            text = "差し替え"
        };
        root.Add(materialReplacementButton);

        #endregion


        #region 表示状態を初期化

        saveMToonFilePathButton.style.display = DisplayStyle.None;
        saveFilePathText.style.display = DisplayStyle.None;
        materialReplacementButton.style.display = DisplayStyle.None;

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
            materialReplacementButton.style.display = DisplayStyle.Flex;
        };

        //マテリアルを差し替え
        materialReplacementButton.clicked += () =>
        {
            GetAllSkinnedMeshRenderer(avatarGameObject);
        };

        #endregion
    }
    
    private SkinnedMeshRenderer[] skinnedMeshRenderers;

    private void GetAllSkinnedMeshRenderer(GameObject gameObject)
    {
        //アバターのメッシュレンダラーをまとめて取得する
        skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        //マテリアルの差し替え
        foreach (var renderer in skinnedMeshRenderers)
        {
            var mats = renderer.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var mat = GetMToonMaterial(mats[i].name);
                if (mat != null)
                {
                    mats[i] = mat;
                }
                else
                {
                    Debug.LogError($"{mats[i].name + "_mtoon"}のマテリアルが見つかりませんでした");
                }
            }
            renderer.sharedMaterials = mats;
        }
    }

    private Material GetMToonMaterial(string materialName)
    {
        string directoryPath = saveMtoonMaterialsFilePath;
        string newFileName = Path.GetFileNameWithoutExtension(materialName) + "_mtoon";
        
        string saveMaterialPath = FileUtil.GetProjectRelativePath(directoryPath) + "/" + newFileName + ".mat";
        return AssetDatabase.LoadAssetAtPath<Material>(saveMaterialPath);
    }
    private void SetSaveMaterialsFilePath()
    {
        string path = EditorUtility.OpenFolderPanel("差し替えるマテリアルの保存先を指定", "", "");
        saveMtoonMaterialsFilePath = path;
    }
}