using UnityEngine;
using UnityEditor;
using System.IO;

public class CameraInfomationCreator : EditorWindow
{
    private bool _isEnabled = false;
    private string _saveDir = "SOs/CameraInfos"; // Assetsフォルダからの相対パス
    private float _defaultSize = 4f;
    private bool _showPreview = true;

    [MenuItem("Tools/CameraInfomation Creator")]
    public static void ShowWindow()
    {
        GetWindow<CameraInfomationCreator>("Camera Info Creator");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Camera Information 作成ツール", EditorStyles.boldLabel);

        bool newEnabled = EditorGUILayout.Toggle("作成モード有効化", _isEnabled);
        if (newEnabled != _isEnabled)
        {
            _isEnabled = newEnabled;
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();

        _saveDir = EditorGUILayout.TextField("保存先 (Assets/以下)", _saveDir);
        _defaultSize = EditorGUILayout.FloatField("デフォルトサイズ", _defaultSize);
        _showPreview = EditorGUILayout.Toggle("プレビュー表示", _showPreview);

        EditorGUILayout.Space();

        if (_isEnabled)
        {
            EditorGUILayout.HelpBox("シーンビューで Shift + 左クリック すると、マウス位置にCameraInformationを作成します。", MessageType.Info);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!_isEnabled) return;

        Event e = Event.current;
        Vector3 mousePosition = e.mousePosition;

        // レイキャストロジック
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        Vector3 worldPos = Vector3.zero;
        bool foundPos = false;

        // まず2Dコライダーとの衝突をチェック
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider != null)
        {
            worldPos = hit.point;
            foundPos = true;
        }
        else
        {
            // 平面 (Z=0) との交差判定
            Plane plane = new Plane(Vector3.forward, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                worldPos = ray.GetPoint(enter);
                foundPos = true;
            }
        }

        // 格子点にスナップ
        if (foundPos)
        {
            worldPos = new Vector3(Mathf.Round(worldPos.x), Mathf.Round(worldPos.y), worldPos.z);
        }

        if (foundPos && _showPreview)
        {
            Handles.color = Color.cyan;

            float aspect = 16f / 9f;
            if (Camera.main != null)
            {
                aspect = Camera.main.aspect;
            }

            float height = _defaultSize * 2f;
            float width = height * aspect;
            Vector3 size = new Vector3(width, height, 0f);

            Handles.DrawWireCube(worldPos, size);
            Handles.Label(worldPos + Vector3.up * (_defaultSize + 0.5f), $"作成位置 ({worldPos.x}, {worldPos.y})");
            SceneView.RepaintAll();
        }

        if (e.type == EventType.MouseDown && e.button == 0 && e.shift && foundPos)
        {
            // 他のツールが反応しないようにイベントを消費
            e.Use();
            CreateCameraInformation(worldPos);
        }
    }

    private void CreateCameraInformation(Vector3 position)
    {
        string fullSavePath = Path.Combine("Assets", _saveDir);

        if (!AssetDatabase.IsValidFolder(fullSavePath))
        {
            // フォルダが存在しない場合は作成
            string absPath = Path.Combine(Application.dataPath, _saveDir);
            Directory.CreateDirectory(absPath);
            AssetDatabase.Refresh();
        }

        CameraInformation asset = ScriptableObject.CreateInstance<CameraInformation>();

        // privateフィールドなどにアクセスするためにリフレクションを使用
        System.Type type = typeof(CameraInformation);
        System.Reflection.FieldInfo centerPosField = type.GetField("_centerPos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo sizeField = type.GetField("_size", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (centerPosField != null)
        {
            centerPosField.SetValue(asset, new Vector3(position.x, position.y, -10));
        }
        else
        {
            Debug.LogError("CameraInformationにフィールド '_centerPos' が見つかりませんでした。");
        }

        if (sizeField != null)
        {
            sizeField.SetValue(asset, _defaultSize);
        }

        // ユニークなパスを生成
        string fileName = $"CameraPos_{position.x:F2}_{position.y:F2}.asset";
        // ファイル名のサニタイズ
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }

        string assetPath = Path.Combine(fullSavePath, fileName);
        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        AssetDatabase.CreateAsset(asset, uniquePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"{position} に CameraInformation を作成し、 {uniquePath} に保存しました。");

        // 作成したアセットをハイライト
        EditorGUIUtility.PingObject(asset);
        Selection.activeObject = asset;
    }
}
