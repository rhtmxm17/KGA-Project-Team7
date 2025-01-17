using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Collections.ObjectModel;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Search;
using Unity.EditorCoroutines.Editor;
#endif

public interface ITsvRowParseable
{
#if UNITY_EDITOR
    public void ParseTsvRow(string[] cells);
#endif
}

public interface ITsvMultiRowParseable
{
#if UNITY_EDITOR
    public void ParseTsvMultiRow(string[] lines, ref int line);
#endif
}

public interface ITsvSheetParseable
{
#if UNITY_EDITOR
    public void ParseTsvSheet(int id, string title, string tsv);
#endif
}


public class DataTableManager : SingletonScriptable<DataTableManager>
{
    [SerializeField] List<CharacterData> characterDataList;
    private Dictionary<int, CharacterData> characterDataIdDic; // id 기반 검색용

    [SerializeField] List<ItemData> itemDataList;
    private Dictionary<int, ItemData> itemDataIdDic; // id 기반 검색용

    [SerializeField] List<StageData> stageDataList;
    private Dictionary<int, StageData> stageDataIdDic; // id 기반 검색용
    public ReadOnlyCollection<StageData> StageDataList => stageDataList.AsReadOnly();

    [SerializeField] List<StoryDirectingData> storyDirectingDataList;
    private Dictionary<int, StoryDirectingData> storyDirectingDataIdDic; // id 기반 검색용
    public ReadOnlyCollection<StoryDirectingData> StoryDirectingDataList => storyDirectingDataList.AsReadOnly();

    [SerializeField] List<ShopItemData> shopItemDataList;
    private Dictionary<int, ShopItemData> shopItemDataIdDic;

    public CharacterData GetCharacterData(int id)
    {
        if (false == characterDataIdDic.ContainsKey(id))
        {
            Debug.LogWarning($"존재하지 않는 캐릭터 ID({id})가 요청됨");
            return null;
        }

        return characterDataIdDic[id];
    }

    public ItemData GetItemData(int id)
    {
        if (false == itemDataIdDic.ContainsKey(id))
        {
            Debug.LogWarning($"존재하지 않는 아이템 ID({id})가 요청됨");
            return null;
        }

        return itemDataIdDic[id];
    }

    /// <summary>
    /// 해당 id를 갖는 스테이지를 반환합니다. id가 존재하지 않으면 null이 반환됩니다
    /// </summary>
    /// <param name="id">스테이지의 id</param>
    /// <returns>스테이지(null 가능)</returns>
    public StageData GetStageData(int id)
    {
        if (false == stageDataIdDic.ContainsKey(id))
        {
            return null;
        }

        return stageDataIdDic[id];
    }

    public ShopItemData GetShopItemData(int id)
    {
        if (false == shopItemDataIdDic.ContainsKey(id))
        {
            Debug.LogWarning($"존재하지 않는 상점 ID({id})가 요청됨");
            return null;
        }

        return shopItemDataIdDic[id];
    }

    private void OnEnable() => IndexData();

    /// <summary>
    /// 색인 생성
    /// </summary>
    private void IndexData()
    {
        characterDataIdDic = characterDataList.ToDictionary(item => item.Id);
        itemDataIdDic = itemDataList.ToDictionary(item => item.Id);
        stageDataIdDic = stageDataList.ToDictionary(item => item.Id);
        storyDirectingDataIdDic = storyDirectingDataList.ToDictionary(item => item.Id);
        shopItemDataIdDic = shopItemDataList.ToDictionary(item => item.Id);
    }

#if UNITY_EDITOR
    public const string SoundsAssetFolder = "Assets/Imports/Sounds";
    public const string PrefabsAssetFolder = "Assets/_WorkSpace/Prefabs";
    public const string SpritesAssetFolder = "Assets/_WorkSpace/Sprites";
    public const string SkillAssetFolder = "Assets/_WorkSpace/Datas/Skills";
    public const string CharacterAssetFolder = "Assets/_WorkSpace/Datas/Character";
    public const string ItemAssetFolder = "Assets/_WorkSpace/Datas/Items";
    public const string PackageAssetFolder = "Assets/_WorkSpace/Datas/Packages";
    public const string StoryAssetFolder = "Assets/_WorkSpace/Datas/StoryDirectings";

    [SerializeField] Sprite dummySprite;
    public Sprite DummySprite => dummySprite;


    [SerializeField] Object characterDataFolder;
    [SerializeField] Object itemDataFolder;
    [SerializeField] Object stageDataFolder;
    [SerializeField] Object packageDataFolder;
    [SerializeField] Object storyDirectingDataFolder;

    private const string documentID = "1mshKeAWkTmozk0snaJPWp7Jizs3pSeLhlFU-982BqHA";
    private const string storyDocumentID = "1mCbO7Xdg0DLPY-J9YjVriGHRueg1PSFvvlKqPZp8pVY";
    private const string characterSheetId = "0";
    private const string itemSheetId = "1467425655";
    private const string packageSheetId = "1751436041";
    private const string stageSheetId = "504606070";


    [ContextMenu("시트에서 캐릭터 데이터 불러오기")]
    private void GetCharacterDataFromSheet()
    {
        GetRowDataFromSheet<CharacterData>(characterSheetId, characterDataFolder, characterDataList);
        IndexData();
    }

    [ContextMenu("시트에서 아이템 데이터 불러오기")]
    private void GetItemDataFromSheet()
    {
        GetRowDataFromSheet<ItemData>(itemSheetId, itemDataFolder, itemDataList);
        IndexData();
    }

    [ContextMenu("시트에서 스테이지 데이터 불러오기")]
    private void GetStageDataFromSheet()
    {
        GetMultiRowDataFromSheet(stageSheetId, stageDataFolder, stageDataList);
        IndexData();
    }

    [ContextMenu("시트에서 상점 패키지 데이터 불러오기")]
    private void GetShopItem()
    {
        GetMultiRowDataFromSheet(packageSheetId, packageDataFolder, shopItemDataList);
        IndexData();
    }

    [ContextMenu("시트에서 스토리 데이터 불러오기")]
    private void GetStroyDataTest()
    {
        GetSheetDataFromDocument<StoryDirectingData>(storyDocumentID, storyDirectingDataFolder, storyDirectingDataList);
        IndexData();
    }


    private void GetRowDataFromSheet<T>(string sheetId, Object dataFolder, List<T> dataList) where T : ScriptableObject, ITsvRowParseable
    {
        GoogleSheet.GetSheetData(documentID, sheetId, this, (succeed, result) =>
        {
            if (succeed == true)
            {
                dataList.Clear();
                string soFolderPath = AssetDatabase.GetAssetPath(dataFolder);

                string[] lines = result.Split("\r\n");
                foreach (string line in lines)
                {
                    string[] cells = line.Split('\t');

                    // 0번열은 ID
                    // ID가 정수가 아니라면 데이터행이 아닌 것으로 간주(주석 등)
                    if (false == int.TryParse(cells[0], out int id))
                        continue;

                    // 1번열은 파일명
                    string soPath = $"{soFolderPath}/{cells[1]}.asset";

                    T soAsset;
                    if (System.IO.File.Exists(soPath))
                    {
                        soAsset = AssetDatabase.LoadAssetAtPath(soPath, typeof(T)) as T;
                        soAsset.ParseTsvRow(cells);

                        EditorUtility.SetDirty(soAsset);
                    }
                    else
                    {
                        soAsset = ScriptableObject.CreateInstance<T>();

                        soAsset.ParseTsvRow(cells);

                        AssetDatabase.CreateAsset(soAsset, soPath);
                    }

                    // 매니저에서 관리하기 위한 목록에 추가
                    dataList.Add(soAsset);
                }

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            }
            else
            {
                Debug.LogWarning("<color=red>읽기 실패!</color>");
            }

        });
    }

    private void GetMultiRowDataFromSheet<T>(string sheetId, Object dataFolder, List<T> dataList) where T : ScriptableObject, ITsvMultiRowParseable
    {
        GoogleSheet.GetSheetData(documentID, sheetId, this, (succeed, result) =>
        {
            if (succeed == true)
            {
                dataList.Clear();
                string soFolderPath = AssetDatabase.GetAssetPath(dataFolder);

                string[] lines = result.Split("\r\n");
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] cells = lines[i].Split('\t');

                    // 0번열은 ID
                    // ID가 정수가 아니라면 데이터행이 아닌 것으로 간주(주석 등)
                    if (false == int.TryParse(cells[0], out int id))
                        continue;

                    // 1번열은 파일명
                    string soPath = $"{soFolderPath}/{cells[1]}.asset";

                    T soAsset;
                    if (System.IO.File.Exists(soPath))
                    {
                        soAsset = AssetDatabase.LoadAssetAtPath(soPath, typeof(T)) as T;
                        soAsset.ParseTsvMultiRow(lines, ref i);

                        EditorUtility.SetDirty(soAsset);
                    }
                    else
                    {
                        soAsset = ScriptableObject.CreateInstance<T>();

                        soAsset.ParseTsvMultiRow(lines, ref i);

                        AssetDatabase.CreateAsset(soAsset, soPath);
                    }

                    // 매니저에서 관리하기 위한 목록에 추가
                    dataList.Add(soAsset);
                }

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            }
            else
            {
                Debug.LogWarning("<color=red>읽기 실패!</color>");
            }

        });
    }

    private struct SheetIndexer
    {
        public int id;
        public string title;
        public string soPath;
        public string sheetId;
    }

    private void GetSheetDataFromDocument<T>(string documentID, Object dataFolder, List<T> dataList) where T : ScriptableObject, ITsvSheetParseable
    {
        GoogleSheet.GetSheetData(documentID, "0", this, (succeed, result) =>
        {
            if (false == succeed)
            {
                Debug.LogWarning("<color=red>색인 정보를 불러오는데 실패했습니다</color>");
                return;
            }

            string soFolderPath = AssetDatabase.GetAssetPath(dataFolder);

            string[] lines = result.Split("\r\n");
            List<SheetIndexer> sheetIndex = new List<SheetIndexer>();
            foreach (string line in lines)
            {
                string[] cells = line.Split('\t');

                // 0번열은 ID
                // ID가 정수가 아니라면 데이터행이 아닌 것으로 간주(주석 등)
                if (false == int.TryParse(cells[0], out int id))
                    continue;

                sheetIndex.Add(new SheetIndexer()
                {
                    id = id,
                    soPath = $"{soFolderPath}/{cells[1]}.asset",
                    title = cells[2],
                    sheetId = cells[3],
                });
            }

            Debug.Log("색인 생성 완료");
            EditorCoroutineUtility.StartCoroutine(ParseDocumentRoutine(documentID, sheetIndex, dataList), this);
        });
    }

    private IEnumerator ParseDocumentRoutine<T>(string documentID, List<SheetIndexer> sheetIndex, List<T> dataList) where T : ScriptableObject, ITsvSheetParseable
    {
        dataList.Clear();
        bool complete;
        for (int i = 0; i < sheetIndex.Count; i++)
        {
            complete = false;
            Debug.Log($"데이터 생성중 ({i}/{sheetIndex.Count})");
            GoogleSheet.GetSheetData(documentID, sheetIndex[i].sheetId, this, (succeed, result) =>
            {
                if (false == succeed)
                {
                    Debug.LogWarning($"<color=red>시트 정보를 불러오는데 실패했습니다(색인 ID:{sheetIndex[i].sheetId})</color>");
                    return;
                }

                T soAsset;
                if (System.IO.File.Exists(sheetIndex[i].soPath))
                {
                    soAsset = AssetDatabase.LoadAssetAtPath(sheetIndex[i].soPath, typeof(T)) as T;
                    soAsset.ParseTsvSheet(sheetIndex[i].id, sheetIndex[i].title, result);

                    EditorUtility.SetDirty(soAsset);
                }
                else
                {
                    soAsset = ScriptableObject.CreateInstance<T>();

                    soAsset.ParseTsvSheet(sheetIndex[i].id, sheetIndex[i].title, result);

                    AssetDatabase.CreateAsset(soAsset, sheetIndex[i].soPath);
                }

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                dataList.Add(soAsset);

                complete = true;
            });

            yield return new WaitUntil(() => complete);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"<color=#bfffbb>데이터 생성 완료 ({sheetIndex.Count}/{sheetIndex.Count})</color>");
    }
#endif

}
