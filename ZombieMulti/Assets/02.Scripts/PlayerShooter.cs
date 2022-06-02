using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 주어진 GUN오브젝트를 쏘거나 재장전
// 알맞은 애니메이션을 재생하고 IK를 사용해 캐릭터 양손이 총이 위치하도록 조정
public class PlayerShooter : MonoBehaviour
{
    /// <summary>
    /// 사용할 총
    /// </summary>
    public Gun gun;
    /// <summary>
    /// 총 배치의 기준점
    /// </summary>
    public Transform gunPivot;
    /// <summary>
    /// 총의 왼쪽 손잡이, 왼손이 위치할 지점
    /// </summary>
    public Transform leftHandMount;
    /// <summary>
    /// 총의 오른쪽 손잡이, 오른손이 위치할 지점
    /// </summary>
    public Transform rightHandMount;

    /// <summary>
    /// 플레이어의 입력
    /// </summary>
    private PlayerInput PlayerInput;
    /// <summary>
    /// 애니메이터 컴포넌트
    /// </summary>
    private Animator playerAnimator;

    // 사용할 컴포넌트 가져오기
    void Start()
    {
        PlayerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        // 슈터가 활성화 될 때 총도 같이 활성화
        gun.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        // 슈터가 비활성화 될 때 총도 같이 비활성화
        gun.gameObject.SetActive(false);
    }

    private void Update()
    {
        // 입력을 감지하고 총을 발사하거나 재장전
        if (PlayerInput.fire)
        {
            // 발사입력 감지 시 총 발사
            gun.Fire();
        }
        else if (PlayerInput.reload)
        {
            if (gun.Reload()) //메서드가 반환하는 값이 bool, true면 작동
            {
                // 재장전 성공 시에만 재장전 애니메이션 재생
                playerAnimator.SetTrigger("Reload");
            }
        }
        UpdateUI();
    }

    /// <summary>
    /// 탄알 UI 갱신
    /// </summary>
    private void UpdateUI()
    {
        // UI 매니저의 탄알 텍스트에 탄창의 탄알과 남은 전체 탄알 표시
        if(gun != null && UIManager.instance != null)
        {
            UIManager.instance.UpdateAmmoText(gun.magAmmo, gun.ammoRemain);
        }
    }

    // 애니메이터의 IK 갱신 : 총을 상체와 함께 흔들기 + 캐릭터의 양손을 손잡이에 위치시키기
    private void OnAnimatorIK(int layerIndex)
    {
        // 총의 기준점 gunPivot을 3D 모델의 오른쪽 팔꿈치로 이동
        gunPivot.position = playerAnimator.GetIKHintPosition(AvatarIKHint.RightElbow);

        // IK를 사용하여 왼손의 위치와 회전을 총의 왼쪽 손잡이에 맞춤 SET ==> 값을 사용한다. 쓴다.
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandMount.rotation);

        // IK를 사용하여 오른손의 위치와 회전을 총의 오른쪽 손잡이에 맞춤
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.RightHand, rightHandMount.rotation);

    }
}
