using UnityEngine;

public class Monster : MonoBehaviour
{

    private void Start() {
        Orc orc = FindObjectOfType<Orc>();
        Monster monster = orc;
        monster.Attack();
    }

    public virtual void Attack(){
         Debug.Log("공격!");
    }
}  
