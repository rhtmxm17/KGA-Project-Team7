using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IndexedButton : MonoBehaviour
{
    public int Id { get; set; }

    public StageData StageData {  get; private set; }

    public void SetStageData(StageData data) => StageData = data;


    public Button Button => button;
    public TMP_Text Text => text;

    [SerializeField] Button button;
    [SerializeField] TMP_Text text;
}