using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomUI : BaseUI
{
    MyroomInitializer initializer;
    
    [SerializeField] Sprite[] roomSprites;
    [SerializeField] MyRoomCData[] roomCData;
    private int roomIndex;
    private int charaIndex;
    [SerializeField] OutskirtsUI outskirtsUI;
    
    // 현재 위치 표시
    [SerializeField] TMP_Text roomName;
    // 캐릭터 상호작용
    [SerializeField] CharacterInteract characterInteract;
    // 채팅창 내방 확인용
    [SerializeField] DialogueUI dialogueUI;

    private void Start()
    {
        initializer = GetComponent<MyroomInitializer>();
        GetComponent<MyroomInitializer>().Initialize(this);
        
        // 더미로그인
        GameManager.UserData.TryInitDummyUserAsync(28, () =>
        {
            Debug.Log("완료");
            LoadImage();
        });
        SetMyRoomUI();
    }
    
    private void OnEnable()
    {
        // 마이룸 입장 시 되돌아가기 버튼 비활성
        GetUI("ReturnMyRoomButton").SetActive(false);
        // 마이룸 나갔다 왔을 때 안보이는 버그 
        LoadImage();
    }

    private void SetMyRoomUI()
    {
        // 방뒷배경 바꾸기 팝업 띄우기
        GetUI<Button>("RoomChangeButton").onClick.AddListener(()=>
        {
            OpenSetRoomPopup("RoomChangePopup");
            AddStack("RoomChangePopup");
        });
        // 배경 바꾸기 팝업 닫기
        GetUI<Button>("CloseChangeRoomPopup").onClick.AddListener(()=>CloseSetRoomPopup("RoomChangePopup"));
        
        // 캐릭터 바꾸기 팝업 띄우기
        GetUI<Button>("CharacterChangeButton").onClick.AddListener(()=>OpenSetRoomPopup("CharacterChange"));
        // 캐릭터 바꾸기 팝업 닫기
        GetUI<Button>("CloseChangeCharacter").onClick.AddListener(()=> CloseSetRoomPopup("CharacterChange"));

        // 채팅 열기 버튼
        GetUI<Button>("ChatButton").onClick.AddListener(()=> GetUI<OpenableWindow>("ChatCanvas").OpenWindow());
        // 채팅 닫기
        GetUI<Button>("CloseChat").onClick.AddListener(()=> GetUI<OpenableWindow>("ChatCanvas").CloseWindow());

        // 놀러가기 버튼
        GetUI<Button>("VisitButton").onClick.AddListener(
            () =>
            {
                OpenSetRoomPopup("FriendTapCanvas");
                AddStack("FriendTapCanvas");
            }
        );
        
       
        // TODO: 도감완료 시 추가할 것 
    }

    public void LoadImage()
    {
        roomIndex = GameManager.UserData.Profile.MyroomBgIdx.Value;
        GetUI<Image>("BackImage").sprite = roomSprites[roomIndex];
        charaIndex = GameManager.UserData.Profile.MyroomCharaIdx.Value-1;
        GetUI<Image>("MyRoomCharacter").sprite = roomCData[charaIndex].image;
    }
    
    private void OpenSetRoomPopup(string _name)
    {
        GetUI(_name).SetActive(true);
    }

    private void CloseSetRoomPopup(string _name)
    {
        GetUI(_name).SetActive(false);
    }

    public void ChangeCharacter(int _index)
    {
        if (_index < 0 || roomCData.Length <= _index)
        {
            Debug.LogError("마이룸 캐릭터 커스텀 번호가 잘못됨");
            return;
        }
        GetUI<Image>("MyRoomCharacter").sprite = roomCData[_index].image;
    }

    public void ChangeBackground(int _index)
    {
        if (_index < 0 || roomSprites.Length <= _index)
        {
            Debug.LogError("마이룸 배경 커스텀 번호가 잘못됨");
            return;
        }
        GetUI<Image>("BackImage").sprite = roomSprites[_index];
    }

    // 스택 추가용
    public void AddStack(string _name)
    {
        GameObject addStack = GetUI(_name).gameObject;
        outskirtsUI.UIStack.Push(addStack);
    }
    
    // 내방으로 돌아오기(임시)
    public void ReturnMyRoomUI()
    {
        LoadImage();
        roomName.text = "나만의 공간";
        outskirtsUI.UIStack.Pop();
        GetUI<Button>("MyRoomCharacter").enabled = true;
        GetUI("VisitButton").SetActive(true);
        GetUI("CharacterChangeButton").SetActive(true);
        GetUI("RoomChangeButton").SetActive(true);
        GetUI("TimerBox").SetActive(true);
        GetUI("SpawnerButton").SetActive(true);
        GetUI("ReturnMyRoomButton").SetActive(false);
        // 채팅용
        dialogueUI.isMyroom = true;
        // 뒤로가기 버튼 비활성
        outskirtsUI.ReturnButton.enabled = true;
        // 캐릭터 상호작용 되돌리기
        characterInteract.isFriendRoom = false; 
    }
}