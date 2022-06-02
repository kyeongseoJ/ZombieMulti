using System.Collections;
using UnityEngine;
using UnityEngine.AI; // AI, 내비게이션 시스템 관련 코드 가져오기

// 좀비 AI 구현
public class Zombie : LivingEntity
{
    public LayerMask whatIsTarget; //추적 대상 레이어

    private LivingEntity targetEntity; // 추적 대상
    private NavMeshAgent navMeshAgent; //경로 계산 AI 에이전트

    public ParticleSystem hitEffect; //피격 시 재생할 파티클 효과
    public AudioClip deathSound; //오디오 소스 컨포넌트
    public AudioClip hitSound; // 피격 시 재생 할 소리
    
    private Animator zombieAnimator; // 애니메이터 컨포넌트
    private AudioSource zombieAudioPlayer; //오디오 소스 컴포넌트
    private Renderer zombieRenderer; // 랜더러 컴포넌트

    public float damage =20f; // 공격력
    public float timeBetAttack = 0.5f; // 공격 간격, 좀비의 공격 주기
    private float lastAttackTime; // 마지막 공격 시점

    //추적할 대상이 존재하는지 알려주는 프로퍼티
    private bool hasTarget{
        get{
            // 추적할 대상이 존재하고 대상이 사망하지 않았다면
            if(targetEntity != null && !targetEntity.dead){
                return true;
            }
            // 그렇지 않다면
            return false;
        }
    }

    private void Awake(){
        // 초기화
        // 게임 오브젝트에서 사용할 컴포넌트 가져오기
        navMeshAgent = GetComponent<NavMeshAgent>();
        zombieAnimator = GetComponent<Animator>();
        zombieAudioPlayer = GetComponent<AudioSource>();

        // 랜더러 컴포넌트는 자식 게임 오브젝트에 있으므로 GetComponentInChildren()메서드 사용
        zombieRenderer = GetComponentInChildren<Renderer>();
    }

    // 좀비 AI의 초기 스펙을 결정하는 셋업 메서드
    // ZombieData => 스크립터블 오브젝트
    public void Setup(ZombieData zombieData){
            // 체력 설정
            startingHealth = zombieData.health;
            health = zombieData.health;
            // 공격력 설정
            damage = zombieData.damage;
            // 내비메시 에이전트의 이동속도 설정
            navMeshAgent.speed = zombieData.speed;
            // 랜더러가 사용중인 머터리얼의 컬러를 변경, 외형 색이 변함
            zombieRenderer.material.color = zombieData.skinColor;
    }

    private void Start() {
        // 게임 오브젝트 활성화와 동시에 AI 추적 루틴 시작
        StartCoroutine(UpdatePath());
    }

    private void Update() {
        // 추적 대상 존재 여부에 따라 다른 애니메이션 진행
        zombieAnimator.SetBool("HasTarget", hasTarget);
    }

    // 주기적으로 추적할 대상의 위치를 찾아 경로 갱신
    private IEnumerator UpdatePath(){
        // 살아있는 동안 무한 루프
        while(!dead){
            if(hasTarget){
                // 추적대상 존재
                navMeshAgent.isStopped =false;
                navMeshAgent.SetDestination(targetEntity.transform.position);
            }
            else{
                // 추적 대상 없음 : AI 이동 중지
                navMeshAgent.isStopped = true;

                // 20유닛의 반지름을 가진 가상의 구를 그렸을 때 구와 겹치는 모든 콜라이더를 가져옴
                // 단, whatisTarget 레이어를 가진 콜라이더만 가져오도록 필터링
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);

                // 모든 콜라이더를 순회하면서 살아있는 LivingEntity 찾기
                for(int i =0; i < colliders.Length; i++){
                    // 콜라이더로부터 LivingEntity 컴포넌트 가져오기
                    // 가장 가까운 대상부터 배열에 담기게 되고 플레이어가 여럿일 때 가장 가까운 대상을 쫓게 된다.
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    // LivingEntity 컴폰넌트가 존재하며, 해당 LivingEntity가 살아있다면 
                    if(livingEntity != null && !livingEntity.dead){
                        //추적대상을 해당 LivingEntity로 설정
                        targetEntity = livingEntity;

                        // for문 즉시 정지
                        break;
                    }
                }
            }
            // 0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    // 대미지 입었을 때 실행할 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal){
        // 아직 사망하지 않은 경우에만 피격 효과 재생
        if(!dead){
            // 공격받은 지점과 방향으로 파티클 효과 재생
            hitEffect.transform.position = hitPoint;
            hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            hitEffect.Play();

            // 피격 효과음 재생
            zombieAudioPlayer.PlayOneShot(hitSound);
        }
        // LivingEntity의 OnDamage()를 실행하여 대미지 적용
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    // 사망처리
    public override void Die(){
        // LivingEntity의 Die()를 실행하여 기본 사망 처리 실행
        base.Die();

        // 다른 AI를 방해하지 않도록 자신의 모든 콜라이더를 비활성화 : 죽고 없는데 콜라이더만 남아있으면 경로에 방해가 된다.
        Collider[] zombieColliders = GetComponents<Collider>();
        for(int i = 0; i < zombieColliders.Length; i++)
        {
            zombieColliders[i].enabled = false;
        }

        // AI 추적을 중지하고 내비메시 컴포넌트를 비활성화
        navMeshAgent.isStopped = true;
        // navMeshAgent.enabled = false;

        // 사망 애니메이션 재생
        zombieAnimator.SetTrigger("Die");
        // 사망 효과음 재생
        zombieAudioPlayer.PlayOneShot(deathSound);
    }
    private void OnTriggerEnter(Collider other) {
            // 공격 애니 실행
            zombieAnimator.SetTrigger("Attack");
    }
    
    // 트리거 충돌한 상대방 게임 오브젝트가 추적 대상이라면 공격 실행되는 메서드
    private void OnTriggerStay(Collider other) {
        // 자신이 사망하지 않았으며,
        //최근 공격 시점에서 timeBetAttack(0.5f)이상 시간이 지났다면, 공격 가능
        if(!dead && Time.time >= lastAttackTime + timeBetAttack){
            // 상대방의 LivingEntity 타입 가져오기 시도
            LivingEntity attackTarget = other.GetComponent<LivingEntity>();
            
            // 상대방의 LivingEntity 가 자신의 추적 대상이라면 공격 실행
            if(attackTarget != null && attackTarget == targetEntity){

                // 최근 공격 시간 갱신
                lastAttackTime = Time.time;

                // 상대방의 피격 위치와 피갹 방향을 근삿값으로 계산 : 박스콜라이더의 영역의 위치를 정확하게 지정할 수 없기 때문
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = transform.position - other.transform.position;

                // 공격 실행
                attackTarget.OnDamage(damage, hitPoint, hitNormal);
                
            }
        }
        

    }

    
}
