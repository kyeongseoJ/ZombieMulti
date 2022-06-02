using UnityEngine;

public class Orc : Monster
{
     public override void Attack(){
         base.Attack(); // Monster의 attck 실행
         Debug.Log("우리는 노예가 되지 않는다!");
         }
}
