﻿using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

// 점수와 게임오버 여부, 게임 UI를 관리하는 게임 매니저
public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{   
    // 외부에서 싱글턴 오브젝트를 가져올 떄 사용할 프로퍼티
    public static GameManager instance{
        get
        {
            // 만약 싱글턴 변수에 아직 할당되지 않앗다면
            if(m_instance == null){
                // 씬에서 GameManager 오브젝트를 찾아서 할당
                m_instance = FindObjectOfType<GameManager>();
            }
            // 싱글턴 오브젝트 반환
            return m_instance;
        }
    }

    // 싱글턴이 할당된 static 변수
    public static GameManager m_instance;

    public GameObject playerPrefab; // 생성할 플레이어 프리팹

    private int score =0 ; // 현재 게임 점수
    public bool isGameover{get; private set;} // 게임오버 상태

    // 주기적으로 자동 실행되는 동기화 메서드
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 로컬 오브젝트라면 쓰기 부분이 실행됨
        if(stream.IsWriting)
        {
            // 네트워크를 통해 score 보내기
            stream.SendNext(score);
        }
        else
        {
            // 리모트 오브젝트라면 읽기 부분이 실행 됨

            // 네트워크를 통해 score 값 받기
            score = (int)stream.ReceiveNext();
            // 동기화하여 받은 점수를 UI로 표시
            UIManager.instance.UpdateScoreText(score);
        }
    }

    private void Awake() 
    {
        // 씬에 싱글턴 오브젝트가 된 다른 GameMAnager 오브젝트가 있다면
        if(instance != this)
        {   // 자신을 파괴
            Destroy(gameObject);
        }
    }

    // 게임 시작과 동시에 플레이어가 될 게임 오브젝트 생성 
    private void Start()
    {
        // 생성할 랜덤 위치 지정
        Vector3 randomSpawnPos = Random.insideUnitSphere * 5f;
        // 위치 y값은 0으로 변경
        randomSpawnPos.y = 0f;

        // 네트워크 상의 모든 클라이언트에서 생성 실행
        // 해당 게임 오브젝트의 주도권은 생성 메서드를 직접 실행한 클라이언트에 있음
        PhotonNetwork.Instantiate(playerPrefab.name, randomSpawnPos, Quaternion.identity);

        // 기존 코드 
        // 플레이어 캐릭터의 사망 이벤트 발생 시 게임 오버
        // onDeath 메서드를 EndGame 메서드가 구독하는 처리 : 플레이어 사망 시 
        // onDeath가 발동되면 onDeath를 구독중인 EndGame메서드도 함께 실행 되어 게임오버 처리 된다.
        // FindObjectOfType<PlayerHealth>().onDeath += EndGame;// 죽는다는 개념이 사라지고 부활 Respawn()이/가 추가되었다
    }

    // 점수를 추가하고 UI 갱신
public void AddScore(int newScore){
    // 게임오버가 아닌 상태에서만 점수 추가 가능
    if(!isGameover){
        // 점수 추가
        score += newScore;
        // 점수 UI 텍스트 갱신
        UIManager.instance.UpdateScoreText(score);
        }
    }

    // 게임 오버 처리 onDeath()를 구독중
    public void EndGame(){
        // 게임오버 상태를 참으로 변경
        isGameover = true;
        // 게임오버 UI 활성화
        UIManager.instance.SetActiveGameoverUI(true);
    }

    // 키보드를 감지하고 룸을 나가게 함
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){
            PhotonNetwork.LeaveRoom();
        }
    }

    // 룸을 나갈 때 실행되는 메서드
    public override void OnLeftRoom()
    {
        // 룸을 나가면 로비 씬으로 돌아감
        SceneManager.LoadScene("Lobby");
    }


}
