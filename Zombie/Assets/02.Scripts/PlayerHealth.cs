using UnityEngine;
using UnityEngine.UI; // UI 관련 코드

// 플레이어 캐릭터의 생명체로서의 동작을 담당
public class PlayerHealth : LivingEntity
{
    public Slider healthSlider; // 체력을 표시할 uI슬라이더  -> 스크립터블 오브젝트로 변환해서 관리하면 좋다.
    
    public AudioClip deathClip; // 사망소리
    public AudioClip hitClip; // 피격소리
    public AudioClip itemPickupClip; // 아이템 습득소리

    private AudioSource palyerAudioPlayer; // 플레이어 소리 재생기
    private Animator playerAnimator; // 플레이어의 애니메이터

    private PlayerMovement playerMovement; // 플레이어 움직임 컴포넌트
    private PlayerShooter playerShooter; // 플레이어 슈터 컴포넌트

    private void Awake() {
        // 사용할 컴포넌트 가져오기
        playerAnimator = GetComponent<Animator>();
        palyerAudioPlayer = GetComponent<AudioSource>();

        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();

        // 자식중 해당 요소를 가진게 있다면 가져오기 연습 : Man 오브젝트(playermovement.cs넣고) 만들어서 캐릭터 자식요소로 넣기
        // PlayerMovement childMovement;
        // childMovement = GetComponentInChildren<PlayerMovement>();
    }

    // 재정의 : 오버라이드, base : 상속받은 부모 지칭
    protected override void OnEnable(){
        // LivingEntity의 OnEnable() 실행(상태초기화)
        base.OnEnable();

        // 체력 슬라이더의 활성화
        healthSlider.gameObject.SetActive(true);
        // 체력 슬라이더의 최댓값을 기본 체력값으로 변경
        healthSlider.maxValue = startingHealth;
        // 체력 슬라이더의 값을 현재 체력값으로 변경
        healthSlider.value =health;

        // 플레이어 조작을 받는 컴포넌트 활성화
        playerMovement.enabled = true;
        playerShooter.enabled = true;
    }

    // 체력회복
    public override void RestoreHealth(float newHealth)
    {
        // LivingEntity의 RestoreHealth()실행(체력증가)
        base.RestoreHealth(newHealth);

        // 갱신된 체력으로 체력 슬라이더 갱신
        healthSlider.value = health;
    }

    // 대미지 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if(!dead){
            // 사망하지 않았을 때만 효과음 재생
            palyerAudioPlayer.PlayOneShot(hitClip);
        }
        // LivingEntity의 OnDamage()실행(대미지 적용)
        base.OnDamage(damage, hitPoint, hitNormal);
        // 갱신된 체력을 체력 슬라이더에 반영
        healthSlider.value = health;
    }

    // 사망처리
    public override void Die()
    {
        // LivingEntity의 Die()실행(사망 적용)
        base.Die();

        // 체력 슬라이더 비활성화
        healthSlider.gameObject.SetActive(false);

        // 사망음 재생
        palyerAudioPlayer.PlayOneShot(deathClip);
        // 애니메이터의 Die 트리거를 발동시켜 사망 애니메이션을 재생
        playerAnimator.SetTrigger("Die");

        // 플레이어 조작을 받는 컴포넌트 비활성화
        playerMovement.enabled = false;
        playerShooter.enabled = false;
    }

    // private void OnDisable() {
    //     playerMovement.enabled = false;
    //     playerShooter.enabled = false;
    // }

    // Collider : 충돌한 대상의 정보를 담고 있는 컨테이너
    private void OnTriggerEnter(Collider other) 
    {
        // 아이템과 충돌한 경우 해당 아이템을 사용하는 처리    
        // 사망하지 않은 경우에만 아이템 사용 가능
        if(!dead){
            // 충돌한 상대방으로부터 IItem 컴포넌트 가져오기 시도
            IItem item = other.GetComponent<IItem>();
            if(item != null)
            {
                // user 메서드를 실행하여 아이템 사용
                item.Use(gameObject);

                // 아이템 습득 소리 재생 
                palyerAudioPlayer.PlayOneShot(itemPickupClip);
            }
        }
    }
}
