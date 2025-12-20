using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEditor.Purchasing;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    #region  Private
    private Transform tr;
    private Animation anim;
    private readonly float initHp = 100.0f; // 초기 생명
    private const float DAMAGE_HP = 10.0f;    
    private Image hpBar;
    #region Public
    public float currHp; // 현wo 생명값 
    public float moveSpeed = 10.0f;  // 이동 속도 변수
    #endregion
    public float turnSpeed = 80.0f;  // 회전속도 변수
    public delegate void PlaterDieHandler();  // 델리게이트 선언
    public static event PlaterDieHandler OnPlayerDie;   // 이벤트 선언
    #endregion

     
    // Start is called before the first frame update
    void Start()
    {
        // HP Bar 연결
        hpBar = GameObject.FindGameObjectWithTag("HP_BAR")?.GetComponent<Image>();
        tr =  GetComponent<Transform>();
        anim = GetComponent<Animation>();
        currHp = initHp;
        // 애니메이션 실행
        anim.Play("Idle");
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float r = Input.GetAxis("Mouse X");

        // Debug.Log("h=" + h);
        // Debug.Log("v=" + v);

        // Transform 컴포넌트의 position 속성값을 변경
        //transform.position += new Vector3(0, 0, 1);

        // 정규화 벡터를 사용한 코드
        //tr.position += Vector3.forward * 1;


        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

        tr.Translate(moveDir.normalized * Time.deltaTime  * moveSpeed);
        
        tr.Rotate(Vector3.up * turnSpeed * Time.deltaTime * r);
        PlayerAnim(h, v);
    }
    void PlayerAnim(float h, float v)
    {
        if (v >= 0.1f)
        {
            anim.CrossFade("RunF", 0.25f);
        }
        else if (v <= -0.1f)
        {
            anim.CrossFade("RunB", 0.25f);
        }
        else if (h >= 0.1f)
        {
            anim.CrossFade("RunR", 0.25f);
        }
        else if (h <= -0.1f)
        {
            anim.CrossFade("RunL", 01.25f);
        }
        else
        {
            anim.CrossFade("Idle", 0.25f);
        }
    }
    private void OnTriggerEnter(Collider coll)
    {
        if(currHp >= 0.0f && coll.CompareTag("PUNCH"))
        {
            currHp -= DAMAGE_HP;
            DisplayHealth();

            Debug.Log($"Player HP = {currHp/initHp}");
            if (currHp <= 0.0f)
            {
                PlayerDie();
            }
        }
    }
    private void DisplayHealth()
    {
        if(hpBar != null)
        {
            hpBar.fillAmount = currHp / initHp;
        }
    }
    private void PlayerDie()
    {
        Debug.Log("Player Die!!");
        /*
        //MONSTER 라는 태그를 가진 모든 오브젝트를 찾아옴
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");
        // 모든 몬스터의 OnPlayerDie 함수를 순차적으로 호출
        foreach (var item in monsters)
        {
            item.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
        }
        */
        // todo: UI 에서 "Game Over"라고 보여주게 하자
        // todo: ui 직접 연결 말고, 이벤트 호출을 통해서

        //GetComponent<FireCtrl>().OnPlayerDie();
        // 주인공 사망 이벤트 발생
        OnPlayerDie();
        //GameObject.Find("GameMgr").GetComponent<GameManager>().IsGameOver = true;
        GameManager.instance.IsGameOver = true;
        GameManager.instance.DisplayerGameOver();
    }
    
}