using Photon.Pun;
using UnityEngine;

// 탄알을 충전하는 아이템
public class AmmoPack : MonoBehaviourPun, IItem
{
    public int ammo = 30; // 충전할 탄알 수

    public void Use(GameObject target) // target : 아이템과 충돌하는 대상
    {
      // 전달받은 게임 오브젝트로부터 PlayerShooter 컴포넌트 가져오기 시도
      PlayerShooter playerShooter = target.GetComponent<PlayerShooter>();

      // PlayerShooter 컴포넌트가 있고, 총 오브젝트가 존재하면, : 나중에 총이 아닌 다른 무기 사용 가능성 
      if(playerShooter != null && playerShooter.gun != null)
      {
          // 총의 남은 탄알 수를 ammo만큼 더함
          // 모든 클라이언트에서 실행
          // playerShooter.gun.ammoRemain += ammo;  
          playerShooter.gun.photonView.RPC("AddAmmo", RpcTarget.All, ammo);
      }

      // 모든 클라이언트에서 자신을 파괴
      // Destroy(gameObject); // 사용되었으므로 자신을 파괴
      // 현재 이부분 호환성 문제로 에러가 난다. 20220609 문의 넣어둠
      PhotonNetwork.Destroy(gameObject);
    }
}
