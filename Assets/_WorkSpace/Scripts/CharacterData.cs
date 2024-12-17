using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "ScriptableObjects/CharacterData")]
public class CharacterData : ScriptableObject, ISheetManageable
{
    #region 유저 데이터
    /// <summary>
    /// 유저 데이터. DataManager의 LoadUserData()가 호출된 적이 있어야 정상적인 값을 갖는다<br/>
    /// 주의: 에디터의 Enter Play Mode Settings에서 도메인 리로드가 비활성화 되어있을 경우 이전 실행시의 값이 남아있을 수 있음
    /// 주의2: 로그아웃을 구현해야 한다면 마찬가지로 이전 유저의 값이 남아있으므로 인증 정보 변경시 정리하는 메서드 추가할것
    /// </summary>
    [field: System.NonSerialized] public ReactiveProperty<int> Level { get; private set; } = new ReactiveProperty<int>();
    #endregion

    // 능력치 표 내지는 계산식으로 변경 필요함
    [System.Serializable]
    public struct Status
    {
        public float Range;

    }

    [SerializeField] int id;
    public int Id => id;

    [SerializeField] new string name;
    public string Name => name;

    [SerializeField] GameObject modelPrefab;
    public GameObject ModelPrefab => modelPrefab;

    [SerializeField] Status statusTable;
    public Status StatusTable => statusTable;

    [SerializeField] Sprite skillSprite;
    public Sprite SkillSprite => skillSprite;

    [SerializeField] Sprite faceIconSprite;
    public Sprite FaceIconSprite => faceIconSprite;

#if UNITY_EDITOR
    private enum Column
    {
        ID,
        /// <summary>
        /// 사용되지 않음(DataManager에서 사용)
        /// </summary>
        FILE_NAME,
        NAME,
        RANGE,
        SHAPE,
        SKILL_SPRITE,
    }

    public void ParseCsvLine(string[] cells)
    {
        // ID
        if (false == int.TryParse(cells[(int)Column.ID], out id))
        {
            Debug.LogError($"잘못된 데이터로 갱신 시도됨");
            return;
        }

        // NAME
        name = cells[(int)Column.NAME];

        // RANGE
        if (false == float.TryParse(cells[(int)Column.RANGE], out statusTable.Range))
        {
            Debug.LogError($"잘못된 데이터로 갱신 시도됨");
            return;
        }

        // SHAPE
        modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_WorkSpace/Prefabs/{cells[(int)Column.SHAPE]}.prefab");

        // SKILL_SPRITE
        skillSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_WorkSpace/Sprites/{cells[(int)Column.SKILL_SPRITE]}.asset");
    }
#endif

}