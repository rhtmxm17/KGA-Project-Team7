using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HYJ_CharacterSelect : MonoBehaviour
{
    // 캐릭터 데이터 구조 : index(ID)
    [SerializeField] GameObject characterSelectPanel; // 캐릭터 선택 창 
    GameObject CantPosUI; // 선택 불가 팝업 -> 5개 유닛이 이미 다 배치 되었을 때의 팝업
    private int posNum; // 위치 번호

    [Header("공용 설정")]
    [SerializeField] HYJ_SelectManager SelectM; 

    [Header("위치 버튼 설정")]
    [SerializeField] GameObject posBTNImage;
    
    [Header("유닛 버튼 설정")]
    [SerializeField] HYJ_UnitInfo unitInfoScript;
    [SerializeField] int unitIndex; // 유닛 번호
    [SerializeField] Image characterImage; // 캐릭터 이미지
    [SerializeField] TMP_Text levelText; // 캐릭터 레벨 텍스트
    [SerializeField] TMP_Text nameText; // 캐릭터 이름 텍스트
    [SerializeField] TMP_Text raceText; //캐릭터 종족 텍스트
    [SerializeField] TMP_Text classText; // 캐릭터 역할 텍스트(탱커/딜러/힐러)
    [SerializeField] TMP_Text powerText; // 캐릭터 공격 타입 텍스트 (단일/광역)
    private GameObject UnitChangeUI; // 유닛 변경 확인 팝업 -> 변경하시겠습니까?

    /// <summary>
    /// 위치버튼 초기 설정
    /// </summary>
    /// <param name="posIdx"></param>위치번호
    /// <param name="CharacterSelectPanel"></param>위치버튼을 클릭 시, 사용되는 캐릭터 선택창
    /// <param name="CantPosUI"></param>이미 5개의 유닛을 배치하여 더 이상 유닛을 배치할 수 없을 때 활성화되는 팝업창
    public void InitDataPosBtn(int posIdx, GameObject CharacterSelectPanel, GameObject CantPosUI)
    {
        SelectM = gameObject.GetComponentInParent<Transform>().GetComponentInParent<HYJ_SelectManager>();
        characterSelectPanel = CharacterSelectPanel;
        this.CantPosUI = CantPosUI;
        posNum = posIdx;
        //transform.GetComponentInChildren<TextMeshProUGUI>().text = posIdx.ToString();
        
        if (CheckPos(posNum))
        {
            SetBtnChImage(true, SelectM.battleInfo[posNum]);
        }
    }

    /// <summary>
    /// 유닛 선택 버튼 초기 설정
    /// </summary>
    /// <param name="manager"></param>사용되는 중앙 매니저
    /// <param name="unitIdx"></param>유닛 고유 번호
    /// <param name="unitChangeUI"></param>이미 배치된 유닛의 자리를 변경시 활성화되는 팝업창
    public void InitDataUnitBtn(HYJ_SelectManager manager, int unitIdx, GameObject unitChangeUI)
    {
        SelectM = manager;

        CharacterData chData = GameManager.TableData.GetCharacterData(unitIdx);
        GetComponentInChildren<TextMeshProUGUI>().text = $"{unitIdx.ToString()}번 유닛"; // FIXME :GetComponentInChildren보단 필드로 받아와서 사용하도록 하기
        unitIndex = unitIdx;
        UnitChangeUI = unitChangeUI;

        characterImage.GetComponent<Image>().sprite = chData.FaceIconSprite;
        
        //표기 설정
        levelText.text = $"Lv.{chData.Level.Value}"; // 레벨
        nameText.text = chData.Name; // 유닛 이름
        switch (chData.StatusTable.type)
        {
            case ElementType.NONE:
                raceText.text = "무속성";
                break;
            case ElementType.FIRE:
                raceText.text = "화룡";
                break;
            case ElementType.WATER:
                raceText.text = "수룡";
                break;
            case ElementType.WIND:
                raceText.text = "정룡";
                break;
            case ElementType.EARTH:
                raceText.text = "토룡";
                break;
            case ElementType.METAL:
                raceText.text = "진룡";
                break;
        }

        switch (chData.StatusTable.roleType)
        {
            case RoleType.NONE:
                classText.text = "";
                break;
            case RoleType.ATTACKER:
                classText.text = "공격형";
                break;
            case RoleType.DEFENDER:
                classText.text = "방어형";
                break;
            case RoleType.SUPPORTER:
                classText.text = "지원형";
                break;
        }
        powerText.text = $"{(int)chData.PowerLevel}"; // 전투력
        
        unitInfoScript.InitUnitInfo(chData);
    }

    /// <summary>
    /// 위치 선택
    /// </summary>
    public void SelectPos()
    {
        if (SelectM.battleInfo.Count >= 5 && !CheckPos(posNum)) // 위치 리스트가 5개 초과 & 이미 선택한 위치가 아닐 경우
        {
            CantPosUI.SetActive(true);
        }
        else
        {
            SelectM.curPos = posNum; //현재의 위치 값을 저장
            characterSelectPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 선택한 위치 번호에 유닛이 이미 배치 되었는지 판별 
    /// </summary>
    /// <param name="posIdx">위치 번호</param> 
    /// <returns> true:배치o / false : 배치x</returns>
    private bool CheckPos(int posIdx) // 위치가 선택되어 있는지 확인하기
    {
        if (SelectM.battleInfo.ContainsKey(posIdx))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 유닛 선택
    /// </summary>
    public void SelectUnit()
    {
        SelectM.curUnitIndex = unitIndex; //선택한 유닛을 저장

        if (SelectM.battleInfo.ContainsKey(SelectM.curPos)) // 선택한 위치가 이미 저장되어 있을 경우 = 유닛이 이미 존재할 경우
        {
            if (SelectM.battleInfo[SelectM.curPos] == SelectM.curUnitIndex) // 저장된 유닛이 현재 유닛일 경우
            {
                Debug.Log("현재 위치에 이미 배치되어 있는 유닛입니다.");
            }
            else if(SelectM.battleInfo[SelectM.curPos] != SelectM.curUnitIndex) 
            {
                Debug.Log("aaaaaaa");
                UnitChangeUI.SetActive(true);
            }
        }
        else
        {
            //현재 선택한 유닛이 다른 곳에 배치되어 있을 때
            if (SelectM.battleInfo.ContainsValue(SelectM.curUnitIndex))
            {
                UnitChangeUI.SetActive(true);
            }
            else
            {
                SelectM.GetComponent<HYJ_SelectManager>().AddPosBtn();
                SelectM.CharacterSelectPanel.SetActive(false);
                SelectM.ChangeAllBtnColorOff();
            }
        }
    }

    /// <summary>
    /// 원하는 키(위치)와 값(유닛번호)를 딕셔너리에 추가하고 값에 맞는 이미지를 위치에 표기 
    /// </summary>
    /// <param name="key">위치</param>
    /// <param name="value">유닛번호</param>
    public void AddBatch(int key, int value)
    {
        SelectM.battleInfo.Add(key, value);
        SetBtnChImage(true,value);
    }

    /// <summary>
    /// 키(위치)에 해당하는 딕셔너리 삭제하고 위치에 있는 유닛 이미지 삭제
    /// </summary>
    /// <param name="key"></param>
    public void RemoveBatch(int key)
    {
        SelectM.battleInfo.Remove(key);     // 현재 유닛 고유번호를 갖고 있는 키와 밸류 삭제
        
        SetBtnChImage(false,unitIndex);
    }

    /// <summary>
    /// 캐릭터 이미지 설정
    /// </summary>
    /// <param name="isOn">true면 캐릭터 이미지로 변경, false면 초기화</param>
    /// <param name="unitIdx">변경할 캐릭터 이미지의 캐릭터 고유 번호</param>
    private void SetBtnChImage(bool isOn,int unitIdx)
    { 
        Image posBtnImg = posBTNImage.GetComponent<Image>();
        Color color = posBTNImage.GetComponent<Image>().color;
        if (isOn)
        {
            CharacterData chData = GameManager.TableData.GetCharacterData(unitIdx);
            posBtnImg.sprite = chData.FaceIconSprite;
            color.a = 1f;
            posBtnImg.color = color;
        }
        else
        {
            posBtnImg.sprite = null;
            color.a = 0f;
            posBtnImg.color = color;
        }
    }
}