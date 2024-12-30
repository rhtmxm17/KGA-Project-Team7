using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class ShopPopupController : BaseUI
{
    [SerializeField] PlayerInput input;
    [SerializeField] GameObject popup;

    private ShopItem shopItem;
    public TMP_Text shopPopupText;
    public Image shopPopupImage;
    void Start()
    {
        input = GameManager.Input;
        shopPopupText = GetUI<TMP_Text>("ShopPopupText");
        shopPopupImage = GetUI<Image>("ShopPopupImage");

    }

    private void OnEnable()
    {


    }

    void Update()
    {
        // 팝업의 외부를 터치할 경우 화면을 닫는 시스템
        if (input.actions["Click"].WasPressedThisFrame())
        {
            if (EventSystem.current.currentSelectedGameObject == true)
                return;

            Debug.Log("화면 클릭 & 팝업 종료");
            popup.SetActive(false);
        }
    }
}
