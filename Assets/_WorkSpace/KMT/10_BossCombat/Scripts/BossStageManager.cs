using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class BossStageManager : StageManager, IDamageAddable
{
    [Header("Boss Stage Info")]
    [SerializeField]
    TextMeshProUGUI scoreText;
    [SerializeField]
    Slider leftTimeBar;

    [Header("Reward UI")]
    [SerializeField]
    TextMeshProUGUI newCountText;
    [SerializeField]
    TextMeshProUGUI newScoreText;
    [SerializeField]
    TextMeshProUGUI prevScore;
    [SerializeField]
    TextMeshProUGUI curScore;

    float maxTimeLimit;
    float score;


    protected override void StartGame()
    {
        base.StartGame();

        maxTimeLimit = timeLimit;
        scoreText.text = "0";
        score = 0;
    }


    public void IDamageAdd(float damage)
    {
        if (IsCombatEnd)
            return;

        score += damage;
        scoreText.text = ((long)score).ToString();
    }

    protected override IEnumerator StartTimerCO()
    {
        TimeSpan leftTimeSpan = TimeSpan.FromSeconds(timeLimit);
        leftTimeText.text = $"{leftTimeSpan.Minutes:D2} : {leftTimeSpan.Seconds:D2}";

        while (timeLimit > 0)
        {
            leftTimeBar.value = timeLimit / maxTimeLimit;

            yield return new WaitForSeconds(1);
            timeLimit -= 1;
            leftTimeSpan = TimeSpan.FromSeconds(timeLimit);
            leftTimeText.text = $"{leftTimeSpan.Minutes:D2} : {leftTimeSpan.Seconds:D2}";
        }

        if (IsCombatEnd)
        {
            Debug.Log("이미 전투가 종료됨");
            yield break;
        }

        IsCombatEnd = true;
        Debug.Log("타임 오버!");
        
        RankApplier.ApplyRank("boss", UserData.myUid, GameManager.UserData.Profile.Name.Value, (long)(score + timeLimit), (isBestRecord, bestScore, rankCount) =>
        {

            // 아이템 획득 팝업 + 확인 클릭시 메인 화면으로
            List<CharacterData> chDataL = new List<CharacterData>(batchDictionary.Values);
            int randIdx = UnityEngine.Random.Range(0, chDataL.Count);

            newCountText.gameObject.SetActive(isBestRecord);
            newScoreText.gameObject.SetActive(isBestRecord);
            prevScore.text = bestScore.ToString();
            curScore.text = ((long)(score + timeLimit)).ToString();

            resultPopupWindow.OpenDoubleButtonWithResult(//todo : 순위?
                $"",
                null,
                "확인", LoadPreviousScene,
                "재도전", () => {

                    GameManager.OverlayUIManager.OpenDoubleInfoPopup("재도전하시겠습니까?", "아니요", "네",
                        null, () => {
                            //TODO : 용도에 따라서 지우거나 이용
                            //prevSceneData.stageData = prevSceneData.stageData;
                            //prevSceneData.prevScene = prevSceneData.prevScene;
                            GameManager.Instance.LoadBattleFormationScene(prevSceneData);
                        });

                },
                true, false,
                $"현재 등수: { rankCount }", chDataL[randIdx].FaceIconSprite,
                AdvencedPopupInCombatResult.ColorType.VICTORY
            );

        });
    }

    protected override void OnClear()
    {
        // TODO: 보스 몬스터 처치 구현 필요시 여기서 결과 처리
        Debug.LogWarning("아직 정의되지 않은 동작");

        if (IsCombatEnd)
        {
            Debug.Log("이미 전투가 종료됨");
            return;
        }

        IsCombatEnd = true;

/*        RankApplier.ApplyRank("boss", UserData.myUid, GameManager.UserData.Profile.Name.Value, (long)(score + timeLimit), (isBestRecord) =>
        {
            // 클리어 팝업 + 확인 클릭시 메인 화면으로
            ItemGainPopup popupInstance = Instantiate(itemGainPopupPrefab, GameManager.PopupCanvas);
            popupInstance.Title.text = "보스전 종료!";
            popupInstance.onPopupClosed += () => GameManager.Instance.LoadMenuScene(PrevScene);

        });*/
    }

}
