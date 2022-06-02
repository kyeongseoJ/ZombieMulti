using UnityEngine;
using UnityEngine.UI; // UI 관련 코드
using UnityEngine.SceneManagement; // 씬 관리자 관련 코드

// UI에 즉시 접근하는 통로만 제공 => 변경
// HUD Canvasd UI에 즉시 접근하고 변경할 수 있도록 허용하는 매니저
public class UIManager : MonoBehaviour
{   
    // 싱글턴 접근용 프로퍼티 : 읽기전용 
    public static UIManager instance{
        get
        {
            if(m_instance == null){
                m_instance = FindObjectOfType<UIManager>();
            }
            return m_instance;
        }
    }

    // 싱글턴이 할당될 변수
    public static UIManager m_instance;

    public Text ammoText; // 탄알 표시용 테스트
    public Text scoreText; // 점수 표시용 텍스트
    public Text waveText; // 적 웨이브 표시 텍스트

    public GameObject gameoverUI; // 게임오버 시 활성화 할 UI

    // 탄알 텍스트 갱신
    public void UpdateAmmoText(int magAmmo, int remainAmmo){
        ammoText.text = magAmmo + "/"+ remainAmmo;
    }
    // 점수 텍스트 갱신
    public void UpdateScoreText(int newScore){
        scoreText.text = "Score : " + newScore;
    }

    // 남은 적 수와 웨이브 갱신 |n ==> 개행문자
    public void UpdateWaveText(int waves, int count){
        waveText.text = "Wave : "+ waves + "\nEnemy Left : " + count;
    }

    // 게임 오버 UI 활성화
    public void SetActiveGameoverUI(bool active){
        gameoverUI.SetActive(active);
    }

    // 게임 재시작
    public void GameRestart(){
        // 현재 실행중인 씬의 이름을 가져오는 프로퍼티
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
