using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoldStageManager : StageManager, IDamageAddable, IProgressable
{
    [Header("Gold Stage Info")]
    [SerializeField]
    Slider leftTimeBar;

    [SerializeField]
    int clearRewardGold;

    string curLevel;
    float maxTimeLimit;
    float gaveDamage;

    Combatable bossCharacters;

    public override void Initialize(StageSceneChangeArgs sceneChangeArgs)
    {
        base.Initialize(sceneChangeArgs);

        curLevel = sceneChangeArgs.dungeonLevel.ToString();
    }


    protected override void StartGame()
    {
        base.StartGame();

        maxTimeLimit = timeLimit;
        gaveDamage = 0;
    }


    public void IDamageAdd(float damage)
    {
        if (IsCombatEnd)
            return;

        gaveDamage += damage;
    }

    public void IPrograssable(Combatable monster)
    {
        bossCharacters = monster;
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

        //초과데미지를 주었더라도 최대 보상보다는 적게 주도록 강제
        Rewarding(gaveDamage, false);

    }


    void Rewarding(float socre, bool isClear)
    {

        float resultRate = Mathf.Clamp01(socre / bossCharacters.MaxHp.Value);//0~1 사이로 고정시키기
        long resultLong = (long)(resultRate * 100);

        if (isClear)//클리어인경우, 클리어률을 100으로 지정.
        {
            resultLong = 100;
            resultRate = 1;
        }
        else//클리어가 아닌 경우라면 99%로 명시.
        {
            resultLong = Math.Min(resultLong, 99);
        }

        int rewardGold = (int)(stageDataOnLoad.Reward[0].gain * resultRate);

        Debug.Log("클리어!");

        ItemGain reward = new ItemGain()
        {
            item = GameManager.TableData.GetItemData(1),
            gain = rewardGold
        };

        var stream = GameManager.UserData.StartUpdateStream();

        var goldClearRateDic = GameManager.UserData.PlayData.GoldDungeonClearRate.Value;

        if (!goldClearRateDic.ContainsKey(curLevel))//첫도전인 경우
        {
            stream.SetDBDictionaryInnerValue(GameManager.UserData.PlayData.GoldDungeonClearRate, curLevel, resultLong);
        }
        else if (goldClearRateDic[curLevel] < resultLong)//재도전인데 이전 클리어률보다 큰 경우
        {
            stream.SetDBDictionaryInnerValue(GameManager.UserData.PlayData.GoldDungeonClearRate, curLevel, resultLong);
        }


        stream
            .AddDBValue(reward.item.Number, rewardGold)
            .AddDBValue(DataTableManager.Instance.GetItemData(9/*골드티켓*/).Number, -1)
            .Submit(result =>
            {
                if (false == result)
                {
                    Debug.Log("요청 전송에 실패했습니다");
                    return;
                }

                Debug.Log("와! 골드!");

                List<CharacterData> chDataL = new List<CharacterData>(batchDictionary.Values);
                int randIdx = UnityEngine.Random.Range(0, chDataL.Count);

                resultPopupWindow.OpenDoubleButtonWithResult(
                    stageDataOnLoad.StageName,
                    new List<ItemGain>() { reward },
                    "확인", LoadPreviousScene,
                    "다음 스테이지로", null,//TODO : 다음스테이지로 가는 로직 만들기.
                    true, true,
                    "승리!", chDataL[randIdx].FaceIconSprite,
                    AdvencedPopupInCombatResult.ColorType.VICTORY
                );

            });

    }

    protected override void OnClear()
    {
        if (IsCombatEnd)
        {
            Debug.Log("이미 전투가 종료됨");
            return;
        }

        IsCombatEnd = true;

        Debug.Log("클리어!");

        Rewarding(float.MaxValue - 10, true);
    }

}
