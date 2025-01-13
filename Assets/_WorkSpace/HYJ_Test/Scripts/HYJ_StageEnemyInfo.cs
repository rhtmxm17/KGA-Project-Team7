using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HYJ_StageEnemyInfo : MonoBehaviour
{
    [SerializeField] TMP_Text stageNameText;
    [SerializeField] private TMP_Text powerText;
    [SerializeField] GameObject monsterPrefab;
    private void Start()
    {
        InitStageData();
    }

    private void InitStageData()
    {
        StageData curStageData = GameManager.Instance.sceneChangeArgs.stageData;
        
        stageNameText.text = curStageData.StageName;

        foreach (var iWave in curStageData.Waves)
        {
            foreach (var iWaveMonster in iWave.monsters)
            {
                GameObject iMonster = Instantiate(monsterPrefab, transform);
                iMonster.GetComponent<HYJ_MonsterInfo>().InitMonsterData(iWaveMonster);
                if (GameManager.Instance.sceneChangeArgs.stageType == StageType.BOSS)
                {
                    iMonster.GetComponent<HYJ_MonsterInfo>().SetBoss();
                }
            }
        }
        
    }

    private void SetRewardList()
    {
        StageData curStageData = GameManager.Instance.sceneChangeArgs.stageData;
        
        //curStageData.Reward
    }
}