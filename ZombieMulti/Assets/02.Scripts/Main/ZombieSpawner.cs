using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon; // 


// 좀비 게임 오브젝트를 주기적으로 생성
public class ZombieSpawner : MonoBehaviourPun, IPunObservable
{
    public Zombie zombiePrefab; // 생성할 원본 좀비 프리팹
    // public Zombie[] zombiePrefabs; // 좀비 종류가 여러 개일 경우

    public ZombieData[] zombieDatas; // 사용할 좀비 셋업 데이터
    public Transform[] spawnPoints; // 좀비 AI 생성할 위치

    private List<Zombie> zombies = new List<Zombie>(); // 생성된 좀비를 담는 리스트

    private int zombieCount = 0; // 남은 좀비 수
    private int wave; // 현재 웨이브

    // 주기적으로 자동 실행되는 동기화 메서드
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 로컬 오브젝트라면 쓰기 부분이 실행됨
        if(stream.IsWriting)
        {
            // 남은 좀비 수를 네트워크를 통해 보내기
            stream.SendNext(zombies.Count);
            // 현재 웨이브를 네트워크를 통해 보내기
           stream.SendNext(wave);
        }
        else
        {
            // 리모트 오브젝트라면 읽기 부분이 실행 됨
            // 남은 좀비 수를 네트워크를 통해 받기
            zombieCount = (int)stream.ReceiveNext();
            // 현재 웨이브를 네트워크를 통해 받기
            wave = (int)stream.ReceiveNext();
        }
    }

    private void Awake() {
        PhotonPeer.RegisterType(typeof(Color), 128, ColorSerialization.SerializeColor, ColorSerialization.DeserializeColor);
    }

    // 플레이어가 좀비를 다 죽이기 전까지 업데이트가 계속 다시 실행이 된다.
    private void Update() 
    {
        // 호스트만 좀비를 직접 생성 할 수 있음
        // 다른 클라이언트는 호스트가 생성한 좀비를 동기화를 통해 받아옴
         if(!PhotonNetwork.IsMasterClient)
        {
            // 게임오버 상태일 때는 생성하지 않음
            if(GameManager.instance != null && GameManager.instance.isGameover)
            {
                return;
            }
            
            // 좀비를 모두 물리친 경우 다름 스폰 실행
            if(zombies.Count <= 0)
            {
                SpawnWave();
            }
        }

        // UI 갱신
        UpdateUI();
    }

    // 웨이브 정보를 UI로 갱신/표시
    private void UpdateUI()
    {
         if(!PhotonNetwork.IsMasterClient)
        {
            // 현재 웨이브와 남은 좀비 수 표시 => 호스트는 직접 갱신한 좀비 리스트를 이용해 남은 좀비 수 표시
            UIManager.instance.UpdateWaveText(wave, zombies.Count);
        }
        else
        {
            // 클라이언트는 좀비 리스트를 갱신할 수 없음으로 호스트가 보내준 zombieCount를 이용해 좀비 수 표시
            UIManager.instance.UpdateWaveText(wave, zombieCount);
        }
    }

    // 현재 웨이브에 맞춰 좀비 생성
    private void SpawnWave()
    {

        // 웨이브 1 증가 : 게임 시작 시 웨이브가 0 이라서 1 증가 시켜주고 시작
        wave++;

        // 현재 웨이브 *1.5 를 반올림한 수만큼 좀비 생성
        int spawnCount = Mathf.RoundToInt(wave*1.5f);

        // spawnCount 만큼 좀비 생성
        for(int i = 0; i < spawnCount; i++){
            // 좀비 생성 처리 실행
            CreateZombie();
        }
    }

    // 좀비를 생성하고 좀비에 추적할 대상 할당
    private void CreateZombie()
    {
        // 사용할 좀비 데이터 랜덤으로 결정
        ZombieData zombieData = zombieDatas[Random.Range(0, zombieDatas.Length)];

        // 생성할 위치를 랜덤으로 결정
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // 좀비 프리팹으로부터 좀비 생성, 네트워크 상의 모든 클라이언트에 생성됨
        GameObject createdZombie = PhotonNetwork.Instantiate(zombiePrefab.gameObject.name, spawnPoint.position, spawnPoint.rotation);

        // 생성한 좀비를 셋업하기 위해 zombie 컴포넌트를 가져옴
        Zombie zombie = createdZombie.GetComponent<Zombie>();

        // 생성한 좀비 능력치 설정
        zombie.photonView.RPC("Setup", RpcTarget.All, zombieData.health, zombieData.damage, zombieData.speed, zombieData.skinColor);
        // skinColor에서 에러가 난다. 직렬화/역직렬화 없이도 가능한 방법 알아두면 좋다

        // 동일한 4채널 배열로 변환 해서 전달가능하다.
        //Vector4 colorVector = zombieData.skinColor;
        // 직렬화 과정 없이도 아래 방식으로 변환해서 전달이 가능하다.
        //Quaternion colorVector = new Quaternion(zombieData.skinColor.r, zombieData.skinColor.g, zombieData.skinColor.b, zombieData.skinColor.a);
        //zombie.photonView.RPC("Setup", RpcTarget.All, zombieData.health, zombieData.damage, zombieData.speed, colorVector);

        // 생성된 좀비를 리스트애 추가
        zombies.Add(zombie);

        // 좀비의 onDeath 이벤트에 익명 메서드 등록 : 람다식 사용 + 이벤트 구독 
        // 사망한 좀비를 리스트에서 제거
        zombie.onDeath += () => zombies.Remove(zombie);
        // 사망한 좀비를 10초 뒤에 파괴
        zombie.onDeath += () => StartCoroutine(DestroyAfter(zombie.gameObject, 10f));
        // 좀비 사망 시 점수 상승
        zombie.onDeath += () => GameManager.instance.AddScore(100);
    }

     // 포톤의 Network.Destroy()는 지연파괴를 지원하지 않음으로 지연파괴를 직접 구현함
    IEnumerator DestroyAfter(GameObject target, float delay)
    {
        // delay만큼 대기
        yield return new WaitForSeconds(delay);

        // target이 파괴되지 않았으면 파괴 실행
        if( target != null)
        {
            // target을 모든 네트워크 상에서 파괴
            PhotonNetwork.Destroy(target);
        }
    }
}
