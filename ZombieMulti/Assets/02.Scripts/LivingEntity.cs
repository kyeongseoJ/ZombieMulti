using System;
using UnityEngine;

// 생명체로 동작할 게임 오브젝트들을 위한 뼈대 제공
// 체력, 대미지 받아들이기, 사망 기능, 사망 이벤트를 제공
public class LivingEntity : MonoBehaviour, IDamageable
{
    /// <summary>
    /// 시작 체력
    ///</summary>
    public float startingHealth = 200f;
    /// <summary>
    /// 현재체력
    ///</summary>
    public float health{get; protected set;}
    /// <summary>
    /// 사망 상태
    ///</summary>
    public bool dead{get; protected set;}
    /// <summary>
    /// 사망 시 발동할 이벤트 
    ///</summary>
    public event Action onDeath;

    // 생명체가 활성화 될 때 상태를 리셋
    protected virtual void OnEnable() {
        // 사망하지 않은 상태로 시작
        dead =false;
        // 체력을 시작 체력으로 초기화
        health = startingHealth;
    }


    /// <summary>
    /// 대미지를 입는 기능
    ///</summary>
    public virtual void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal){
        // 대미지 만큼 체력 감소
        health -= damage;

        // 체력이 0 이하 && 아직 죽지 않았다면 사망처리 실행
        if( health <= 0 && !dead){
            Die();
        }
    }
    
    /// <summary>
    /// 체력을 회복하는 기능
    ///</summary>
    public virtual void RestoreHealth(float newHealth){
        if(dead){
            //이미 사망한 경우 체력을 회복할 수 없음
            return;
        }
        // 체력 추가
        health += newHealth;
    }


    /// <summary>
    /// 사망 처리
    ///</summary>
    public virtual void Die(){
        // onDeath이벤틍 등록된 메서드가 있다면 실행
        if(onDeath != null){
            onDeath();
        }

        // 사망상태를 참으로 변경
        dead = true;
    }
}
