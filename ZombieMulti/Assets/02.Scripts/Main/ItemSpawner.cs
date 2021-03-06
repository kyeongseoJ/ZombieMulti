using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI; // 내비메시 관련 코드

// 주기적으로 아이템을 플레이어 근처에 생성하는 스크립트
public class ItemSpawner : MonoBehaviourPun 
{
    public GameObject[] items; // 생성할 아이템
    //public Transform palyerTransform; // 플레이어의 트랜스폼 : 위치 => 맵 중심으로 변경(0,0,0)

    public float maxDistance = 7f; // 플레이어 위치에서 아이템이 배치될 최대 반경 5-> 7로 임의 변경

    public float timeBetSpawnMax = 7f; // 최대 시간 간격
    public float timeBetSpawnMin = 2f; // 최소 시간 간격
    private float timeBetSpawn; // 생성 간격
    private float lastSpawnTime; // 마지막 생성 시점

    private void Start() {
        // 생성 간격과 마지막 생성 시점 초기화
        timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
        lastSpawnTime =0;
    }

    // 주기적으로 아이템 생성 처리 실행
    private void Update() 
    {
        // 호스트에서만 아이템 직접 생성 가능
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        // 현재 시점이 마지막 생성 시점에서 생성 주기 이상 지남
        if(Time.time >= lastSpawnTime + timeBetSpawn)
        {
            // 마지막 생성 시간 갱신
            lastSpawnTime = Time.time;
            // 생성주기를 랜덤으로 변경
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
            // 아이템 생성 실행
            Spawn();
        }
    }

    // 실제 아이템 생성 처리
    private void Spawn()
    {
        /// (0, 0, 0)을 기준으로 maxDistance 안에서 내비메시 위의 랜덤위치 지정
        // 플레이어 근처에서 내비메시 위의 랜덤한 위치 가져오기
        Vector3 spawnPosition = GetRandomPointOnNavMesh(Vector3.zero, maxDistance);
        // 바닥에서 0.5만큼 위로 올리기
        spawnPosition += Vector3.up * 0.5f;

        // 생성할 아이템을 무작위로 하나 선택
        GameObject selectedItem = items[Random.Range(0, items.Length)];

        // 아이템 중 하나를 무작위로 골라 랜덤 위치에 생성 identity: (0, 0, 0) => 네트워크의 모든 클라이언트에서 해당 아이템 생성
        GameObject item = PhotonNetwork.Instantiate(selectedItem.name, spawnPosition, Quaternion.identity);

        // 생성된 아이템을 5초뒤에 파괴 : 이 과정 때문에 윗줄 코드로 아이템을 담아준다. 
        // 만약 플레이어가 아이템을 획득을 못했다면 파괴해줘야한다. 때문에 5초 뒤 파괴시킨다.
        StartCoroutine(DestroyAfter(item, 5f)); // Destroy(item, 5f);
    }

    // 포톤의 PhotonNetwork.Destroy()를 지연 실행하는 코루틴
    IEnumerator DestroyAfter(GameObject target, float delay)
    {
        // delay만큼 대기
        yield return new WaitForSeconds(delay);

        // target이 파괴되지 않았으면 파괴 실행
        if( target != null)
        {
            PhotonNetwork.Destroy(target);
        }
    }
    

    // 내비메시위의 랜덤한 위치를 반환하는 메서드
    // center를 중심으로 distance 반경 안에서의 랜덤한 위치를 찾음
    private Vector3 GetRandomPointOnNavMesh(Vector3 center , float distance)
    {
        // center를 중심으로 반지름이 maxDistance인 구 안에서의 랜덤한 위치 하나를 저장
        // Random.insideUnirSphere는 반지름이 1인 구 안에서의 랜덤한 한 점을 반환하는 프로퍼티
        Vector3 randomPOs = Random.insideUnitSphere* distance +center;

        // 내비메시 샘플링 결과 정보를 저장하는 변수
        NavMeshHit hit;

        // maxDistance 반경 안에서 randomPos에 가장 가까운 내비메시 위의 한 점을 찾음
        // areaMask 에 해당하는 NavMesh 중에서 maxDistance 반경 내에서 sourcePositio에 가장 가까운 위치를 찾아서 그 결과를 hit에 담음
        NavMesh.SamplePosition(randomPOs, out hit, distance, NavMesh.AllAreas);

        // 찾은 점 반환
        return hit.position;
    }

}
