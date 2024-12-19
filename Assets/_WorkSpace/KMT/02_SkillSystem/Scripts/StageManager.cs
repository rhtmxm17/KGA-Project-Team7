using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField]
    CombManager characterManager;

    [SerializeField]
    List<CombManager> monsetWaveQueue = new List<CombManager>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("S눌림");
            characterManager.StartCombat(monsetWaveQueue[0]);
            monsetWaveQueue[0].StartCombat(characterManager);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F눌림");

            characterManager.EndCombat();
            monsetWaveQueue[0].EndCombat();
        }
    }
}
