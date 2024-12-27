using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class BossStageManager : StageManager
{
    [Header("Boss Stage Info")]
    [SerializeField]
    TextMeshProUGUI scoreText;
    [SerializeField]
    Slider leftTimeBar;
    [SerializeField]
    float timeLimit;

    float maxTimeLimit;
    float score;

    bool isTimeOver;

    protected override void StartGame()
    {
        base.StartGame();

        maxTimeLimit = timeLimit;
        scoreText.text = "0";
        score = 0;
        isTimeOver = false;

        StartCoroutine(StartTimerCO());

    }

    public void AddScore(float score)
    {
        if (isTimeOver)
            return;

        this.score += score;
        scoreText.text = this.score.ToString();
    }

    IEnumerator StartTimerCO()
    {
        while (timeLimit > 0)
        {
            leftTimeBar.value = timeLimit / maxTimeLimit;
            timeLimit = Mathf.Clamp(timeLimit - Time.deltaTime, -0.01f, maxTimeLimit);
            yield return null;
        }

        Debug.Log("타임 오버!");
        isTimeOver = true;

        RankApplier.ApplyRank("boss", UserData.myUid, UserData.myNickname, (long)(score + timeLimit));
    }

}