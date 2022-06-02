using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 좀비 게임 오브젝트를 주기적으로 생성
public class ZombieSpawner : MonoBehaviour
{
    public Zombie zombiePrefab; // 생성할 원본 좀비 프리팹
    // public Zombie[] zombiePrefabs; // 좀비 종류가 여러 개일 경우

    public ZombieData[] zombieDatas; // 사용할 좀비 셋업 데이터
    public Transform[] spawnPoints; // 좀비 AI 생성할 위치

    private List<Zombie> zombies = new List<Zombie>(); // 생성된 좀비를 담는 리스트
    private int wave; // 현재 웨이브

    // 플레잉어가 좀비를 다 죽이기 전까지 업데이트가 계속 다시 실행이 된다.
    private void Update() 
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
        // UI 갱신
        UpdateUI();
    }

    // 웨이브 정보를 UI로 갱신
    private void UpdateUI()
    {
        // 현재 웨이브와 남은 좀비 수 표시
        UIManager.instance.UpdateWaveText(wave, zombies.Count);
    }

    // 현재 웨이브에 맞춰 좀비 생성
    private void SpawnWave()
    {

        // 웨이브 1 증가 : 게임 시작 시 웨이브가 0 이라서 1 증가 시켜주고 시작
        wave++;

        // 현재 웨이브 *1.5 를 반올림한 수만큼 좀비 생성
        int spawnCount = Mathf.RoundToInt(wave*1.5f);

        // spawnCount 만큼 좀비 생성
        for(int i =0; i < spawnCount; i++){
            // 좀비 생성 처리 실행
            CreateZombie();
        }
    }

    // 좀비를 생성하고 좀비에 추적할 대상 할당
    private void CreateZombie()
    {
        // 사용할 좀비 데이터 랜덤으로 결정
        ZombieData zombieData= zombieDatas[Random.Range(0, zombieDatas.Length)];

        // 생성할 위치를 랜덤으로 결정
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // 좀비가 여러개일 경우 위와 동일하게 받아와서 쓰면 된다.
        // Zombie zombieObject = zombiePrefabs[Random.Range(0,zombiePrefabs.Length)];

        // 좀비 프리팹으로부터 좀비 생성
        Zombie zombie = Instantiate(zombiePrefab, spawnPoint.position, spawnPoint.rotation);

        // 생성한 좀비 능력치 설정
        zombie.Setup(zombieData);

        // 생성된 좀비를 리스트애 추가
        zombies.Add(zombie);

        // 좀비의 onDeath 이벤트에 익명 메서드 등록
        // 사망한 좀비를 리스트에서 제거
        zombie.onDeath += () => zombies.Remove(zombie);
        // 사망한 좀비를 10초 뒤에 파괴
        zombie.onDeath += () => Destroy(zombie.gameObject, 10f);
        // 좀비 사망 시 점수 상승
        zombie.onDeath += () => GameManager.instance.AddScore(100);
    }
}
