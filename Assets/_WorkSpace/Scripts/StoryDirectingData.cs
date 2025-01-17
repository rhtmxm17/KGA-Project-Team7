using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(StoryDirectingData))]
public class StoryDirectingDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("테이블 매니저 바로가기"))
        {
            UnityEditor.Selection.activeObject = DataTableManager.Instance;
        }

        base.OnInspectorGUI();
    }
}
#endif

public class StoryDirectingData : ScriptableObject, ITsvSheetParseable
{
    public enum TransitionType
    {
        NONE,  // 미정의
        BLINK, // 순간이동, 기본값
        NORMAL, // 선형적 이동
        BOUNCE, // 통통
        SHAKE, // 부들부들
        SHAKE_HORIZONTAL, // 부들수평
        SPIRAL,
        UPSIZE, // 확대
        DOWNSIZE, // 축소
    }

    [SerializeField] int id;
    public int Id => id;

    [SerializeField] string title;
    public string Title => title;

    [System.Serializable]
    public struct Dialogue
    {
        public string Loaction;
        public string Speaker;
        [TextArea] public string Script;
        public float timeMult;

        // 해당 다이얼로그 재생과 함께 진행되는 갱신사항들
        // null이라면 해당 내용은 갱신하지 않음을 의미
        public List<TransitionInfo> Transitions; // 스탠딩 이미지 갱신사항
        public List<EffectInfo> Effects; // 이펙트 목록
        public Vector2 CameraPosition;
        public float CameraSize;
        public float CamereaTransitionTime;
        public AudioClip Bgm;
        public AudioClip Sfx;
        public Sprite BackgroundSprite;
    }

    [System.Serializable]
    public struct TransitionInfo
    {
        public int StandingImageId; // 어느 이미지를 갱신할 것인지
        public bool Active; // 출현 또는 숨기기
        public bool Flip;
        public float ColorMultiply; // 1(일반)~0(어두움), rgb에 각각 곱할 값
        public Vector2 Position; // 유효 영역 기준, 좌하단을 (0, 0) 우상단을 (20, 10)로 하는 위치 좌표
        public float Scale; // 크기, 1이 기본
        public TransitionType Type;
        public float Time;
    }

    [System.Serializable]
    public struct EffectInfo
    {
        public StoryEffect Effect;
        public Vector2 Position; // 유효 영역 기준, 좌하단을 (0, 0) 우상단을 (20, 10)로 하는 위치 좌표
    }

    [System.Serializable]
    public struct StandingImage
    {
        public int ActorId; // 해당 스토리에서 사용되는 ID(직렬화된 배열 인덱스)
        public Sprite ImageSprite;
    }

    [SerializeField] Dialogue[] dialogues;
    public Dialogue[] Dialogues => dialogues;

    [SerializeField] StandingImage[] standingImages;
    public StandingImage[] StandingImages => standingImages;

#if UNITY_EDITOR
    private enum Column
    {
        ID, // 구분자

        LOCATION,   // 다이얼로그
        SPEAKER,    // 다이얼로그
        SCRIPT,     // 다이얼로그
        TIME_MULT,  // 다이얼로그

        STANDING_IMAGE_ID,  // 트랜지션
        EFFECT_NAME,    // 이펙트

        FADE,               // 트랜지션
        FLIP,               // 트랜지션
        COLOR_MULT,         // 트랜지션
        POS_X,              // 트랜지션 + 이펙트
        POS_Y,              // 트랜지션 + 이펙트
        SCALE,              // 트랜지션
        DROPDOWN_TRANSITION,
        TRANSITION,         // 트랜지션

        TIME,               // 트랜지션 + 카메라

        FOCUS_X,    // 카메라
        FOCUS_Y,    // 카메라
        ZOOM,       // 카메라

        BGM,        // 기타 연출
        SFX,        // 기타 연출
        BG_IMG,     // 기타 연출
    }

    public void ParseTsvSheet(int sheetId, string title, string csv)
    {
        this.id = sheetId;
        this.title = title;

        // 1. 장면 ID 찾기
        // 2. 그 행의 장면데이터 입력
        // 3. 다음 행부터 ID열이 비어있다면 스탠딩이미지 데이터로 취급해 추가
        // 4. ID열이 채워져 있다면 장면 데이터로 보고 다음 장면 데이터 입력으로 이동

        Dictionary<string, StandingImage> tempStandingImages = new Dictionary<string, StandingImage>();
        List<Dialogue> tempDialogues = new List<Dialogue>();

        string[] lines = csv.Split("\r\n");
        foreach (string line in lines)
        {
            string[] cells = line.Split('\t');

            // 0번열은 다이얼로그 ID
            if (int.TryParse(cells[0], out int _))
            {
                Dialogue parsed = new Dialogue();
                parsed.Transitions = new List<TransitionInfo>();
                parsed.Effects = new List<EffectInfo>();

                // LOCATION
                parsed.Loaction = cells[(int)Column.LOCATION];

                // SPEAKER
                parsed.Speaker = cells[(int)Column.SPEAKER];

                // SCRIPT
                parsed.Script = cells[(int)Column.SCRIPT]
                    .Trim('\"') // 줄바꿈 등 적용시 자동으로 앞뒤에 씌워짐
                    .Trim('|') // 큰 따옴표 씌우기용
                    .Replace("\"\"", "\"") // 큰 따옴표가 2개씩 들어오는거 해결
                    .Replace("\\n", "\n"); // 줄바꿈 문자 치환

                // TIME_MULT
                if (string.IsNullOrEmpty(cells[(int)Column.TIME_MULT]))
                {
                    // 비어있을 경우 기본값
                    parsed.timeMult = 1f;
                }
                else if (false == float.TryParse(cells[(int)Column.TIME_MULT], out parsed.timeMult))
                {
                    Debug.LogWarning($"잘못된 자료형이 입력됨(요구사항:float, 입력된 데이터라인:{line}");
                }

                // BGM
                parsed.Bgm = SearchAsset.SearchAudioClipAsset(cells[(int)Column.BGM]);

                // SFX
                parsed.Sfx = SearchAsset.SearchAudioClipAsset(cells[(int)Column.SFX]);

                // BG_IMG
                parsed.BackgroundSprite = SearchAsset.SearchSpriteAsset(cells[(int)Column.BG_IMG]);

                // FOCUS_X
                if (string.IsNullOrEmpty(cells[(int)Column.FOCUS_X]))
                {
                    // 비어있을 경우 기본값
                    parsed.CameraPosition.x = 10;
                }
                else if (false == float.TryParse(cells[(int)Column.FOCUS_X], out parsed.CameraPosition.x))
                {
                    Debug.LogWarning($"잘못된 자료형이 입력됨(요구사항:float, 입력된 데이터라인:{line}");
                }

                // FOCUS_Y
                if (string.IsNullOrEmpty(cells[(int)Column.FOCUS_Y]))
                {
                    // 비어있을 경우 기본값
                    parsed.CameraPosition.y = 5;
                }
                else if (false == float.TryParse(cells[(int)Column.FOCUS_Y], out parsed.CameraPosition.y))
                {
                    Debug.LogWarning($"잘못된 자료형이 입력됨(요구사항:float, 입력된 데이터라인:{line}");
                }

                // ZOOM
                if (string.IsNullOrEmpty(cells[(int)Column.ZOOM]))
                {
                    // 비어있을 경우 기본값
                    parsed.CameraSize = 1f;
                }
                else if (false == float.TryParse(cells[(int)Column.ZOOM], out parsed.CameraSize))
                {
                    Debug.LogWarning($"잘못된 자료형이 입력됨(요구사항:float, 입력된 데이터라인:{line}");
                }

                // TIME
                if (string.IsNullOrEmpty(cells[(int)Column.TIME]))
                {
                    parsed.CamereaTransitionTime = 0.5f; // 기본값
                }
                else if (false == float.TryParse(cells[(int)Column.TIME], out parsed.CamereaTransitionTime))
                {
                    Debug.LogWarning($"잘못된 자료형이 입력됨(요구사항:float, 입력된 데이터라인:{line}");
                    continue;
                }

                tempDialogues.Add(parsed);
            }
            // ID가 정수가 아니라면 다이얼로그행이 아님(주석 또는 트랜지션)
            else
            {
                // 다이얼로그도 비어있다면 주석행
                if (tempDialogues.Count == 0)
                    continue;

                // STANDING_IMAGE_ID: 등장인물 번호 조회 또는 신규 등장인물 가져오기
                string imageKey = cells[(int)Column.STANDING_IMAGE_ID];
                // EFFECT_NAME: 이펙트 프리펩 등록
                string effectKey = cells[(int)Column.EFFECT_NAME];
                // 스탠딩 이미지가 등록되어 있다면 트랜지션 행
                if (false == string.IsNullOrEmpty(imageKey))
                {
                    TransitionInfo parsedTransition = new TransitionInfo();

                    if (tempStandingImages.ContainsKey(imageKey))
                    {
                        parsedTransition.StandingImageId = tempStandingImages[imageKey].ActorId;
                    }
                    else
                    {
                        parsedTransition.StandingImageId = tempStandingImages.Count;

                        StandingImage loaded = new StandingImage();
                        loaded.ActorId = tempStandingImages.Count;

                        loaded.ImageSprite = SearchAsset.SearchSpriteAsset(imageKey);

                        tempStandingImages.Add(imageKey, loaded);
                    }

                    // LEAVE: 테이블값이 T면 해당 캐릭터 페이드아웃
                    parsedTransition.Active = ("T" != cells[(int)Column.FADE]);

                    // FLIP: 테이블값이 T면 해당 캐릭터는 좌우 반전 상태
                    parsedTransition.Flip = ("T" == cells[(int)Column.FLIP]);

                    // COLOR_MULT
                    if (string.IsNullOrEmpty(cells[(int)Column.COLOR_MULT]))
                    {
                        parsedTransition.ColorMultiply = 1f; // 기본값
                    }
                    else if (false == float.TryParse(cells[(int)Column.COLOR_MULT], out parsedTransition.ColorMultiply))
                    {
                        Debug.LogWarning($"잘못된 자료형이 입력됨(요구사항:float, 입력된 데이터라인:{line}");
                        continue;
                    }

                    // POS_X
                    if (false == float.TryParse(cells[(int)Column.POS_X], out parsedTransition.Position.x))
                    {
                        Debug.LogWarning($"잘못된 자료형이 입력됨(요구사항:float, 입력된 데이터라인:{line}");
                        continue;
                    }

                    // POS_Y
                    if (false == float.TryParse(cells[(int)Column.POS_Y], out parsedTransition.Position.y))
                    {
                        Debug.LogWarning($"잘못된 자료형이 입력됨(요구사항:float, 입력된 데이터라인:{line}");
                        continue;
                    }

                    // SCALE
                    if (string.IsNullOrEmpty(cells[(int)Column.SCALE]))
                    {
                        parsedTransition.Scale = 1f; // 기본값
                    }
                    else if (false == float.TryParse(cells[(int)Column.SCALE], out parsedTransition.Scale))
                    {
                        Debug.LogWarning($"잘못된 자료형이 입력됨(요구사항:float, 입력된 데이터라인:{line}");
                        continue;
                    }

                    // TRANSITION
                    if (string.IsNullOrEmpty(cells[(int)Column.TRANSITION]))
                    {
                        parsedTransition.Type = TransitionType.BLINK;
                    }
                    else if (int.TryParse(cells[(int)Column.TRANSITION], out int type))
                    {
                        parsedTransition.Type = (TransitionType)type;
                    }
                    else
                    {
                        Debug.LogWarning($"잘못된 자료형이 입력됨(요구사항:int, 입력된 데이터라인:{line}");
                        continue;
                    }

                    // TIME
                    if (string.IsNullOrEmpty(cells[(int)Column.TIME]))
                    {
                        parsedTransition.Time = 0.5f; // 기본값
                    }
                    else if (false == float.TryParse(cells[(int)Column.TIME], out parsedTransition.Time))
                    {
                        Debug.LogWarning($"잘못된 자료형이 입력됨(요구사항:float, 입력된 데이터라인:{line}");
                        continue;
                    }

                    // 파싱된 연출 정보를 현재 대사에 등록
                    tempDialogues[^1].Transitions.Add(parsedTransition);
                }
                // 이펙트가 등록되어 있다면 이펙트 행
                else if (false == string.IsNullOrEmpty(effectKey))
                {
                    EffectInfo parsedEffect = new EffectInfo();

                    parsedEffect.Effect = SearchAsset.SearchPrefabAsset<StoryEffect>(effectKey);
                    if (parsedEffect.Effect == null)
                    {
                        Debug.LogWarning($"이펙트 데이터를 찾지 못함, 입력된 데이터라인:{line}");
                        continue;
                    }

                    // POS_X
                    if (false == float.TryParse(cells[(int)Column.POS_X], out parsedEffect.Position.x))
                    {
                        Debug.LogWarning($"잘못된 자료형이 입력됨(요구사항:float, 입력된 데이터라인:{line}");
                        continue;
                    }

                    // POS_Y
                    if (false == float.TryParse(cells[(int)Column.POS_Y], out parsedEffect.Position.y))
                    {
                        Debug.LogWarning($"잘못된 자료형이 입력됨(요구사항:float, 입력된 데이터라인:{line}");
                        continue;
                    }
                    // 파싱된 이펙트 정보를 현재 대사에 등록
                    tempDialogues[^1].Effects.Add(parsedEffect);
                }
            }
        }

        // 임시 데이터를 직렬화 필드에 저장
        standingImages = new StandingImage[tempStandingImages.Count];
        foreach (var pair in tempStandingImages)
        {
            standingImages[pair.Value.ActorId] = pair.Value;
        }

        dialogues = tempDialogues.ToArray();
    }
#endif
}
