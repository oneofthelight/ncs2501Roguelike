using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MonsterCtrl : MonoBehaviour
{
    private const int MAX_MONSTER_HP = 100;
    private const int HIT_MONSTER_HP = 10;
    #region Hash
    // 해시값 추출
    private readonly int hashTrace = Animator.StringToHash("IsTrace");
    private readonly int hashAttack = Animator.StringToHash("IsAttack");
    private readonly int hashhit = Animator.StringToHash("Hit");
    private readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");
    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashDie = Animator.StringToHash("Die");
    private readonly int hashVictory = Animator.StringToHash("GangnamStyle"); // 파라미터 이름 확인!
    #endregion
    public const float TIMER_CHECK = 0.3f;
    public enum State
    {
        IDLE,
        TRACE,
        ATTACK,
        DIE
    }
    public State state = State.IDLE;
    public float traceDist = 10.0f;
    public float attackDist = 2.0f;
    public const int SCORE_KILL = 50;
    public bool isDie = false;
    private Transform monsterTr;
    private Transform playerTr;
    private NavMeshAgent agent;
    private Animator animator;
    private GameObject bloodEffect;  // 혈흔 효과 프리팹
    private int hp = 100;
    // 스크립트가 활성화 될때마다 호출되는 함수
    void OnEnable()
    {
        // 이벤트 발생 시 수행할 함수 연결
        PlayerCtrl.OnPlayerDie += this.OnPlayerDie;
        // 몬스터의 상태를 체크하는 코루틴 함수 호출
        StartCoroutine(CheckMonsterState());
        // 상태에 따라 몬스터의 행돌을 수행하는 코루틴 함수 호출
        StartCoroutine(MonsterAction());
    }
    // 스크립트가 비활성 될 때마다 호출되는 함수
    void OnDisable()
    {
        // 기존에 연결된 함수 해제
        PlayerCtrl.OnPlayerDie -= this.OnPlayerDie;

    }
    void Awake()
    {
        monsterTr = GetComponent<Transform>();
        playerTr = GameObject.FindWithTag("PLAYER").GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        animator = GetComponent<Animator>();
        bloodEffect = Resources.Load<GameObject>("BloodSprayEffect");
    }
    IEnumerator CheckMonsterState()
    {
        while (!isDie)
        {
            // 0.3ch 대기하는 동안 제어권 넘김
            yield return new WaitForSeconds(TIMER_CHECK);
            //몬스터의 상태가 Die 일때 코루틴 종료
            if (state == State.DIE) yield break;
            float distance = Vector3.Distance(playerTr.position, monsterTr.position);

            if (distance <= attackDist)
            {
                state = State.ATTACK;
            }
            else if (distance <= traceDist)
            {
                state = State.TRACE;
            }
            else
            {
                state = State.IDLE;
            }
        }
    }

    // Update is called once per frame
    void OnDrawGizmos()
    {
        if (state == State.TRACE)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, traceDist);
        }
        if (state == State.ATTACK)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackDist);
        }
    }
    IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            switch (state)
            {
                case State.IDLE:
                    agent.isStopped = true;
                    animator.SetBool(hashTrace, false);
                    break;
                case State.TRACE:
                    agent.SetDestination(playerTr.position);
                    agent.isStopped = false;
                    animator.SetBool(hashTrace, true);
                    animator.SetBool(hashAttack, false);
                    break;
                case State.ATTACK:
                    animator.SetBool(hashAttack, true);
                    break;
                case State.DIE:
                    isDie = true;
                    agent.isStopped = true;
                    animator.SetTrigger(hashDie);
                    // 몬스터의 Collider 비활성화
                    GetComponent<CapsuleCollider>().enabled = false;
                    // 몬스터의 손에 달려있는 Collider 비활성화
                    SphereCollider[] sc = GetComponentsInChildren<SphereCollider>();
                    foreach (var item in sc)
                    {
                        item.enabled = false;
                    }
                    // 일정시간 대기 후 오브젝트 풀링으로 환원
                    yield return new WaitForSeconds(3.0f);
                    // 사망 후 다시 사용될 때를 위해 hp값 초기화
                    isDie = false;
                    GetComponent<CapsuleCollider>().enabled = true;
                    foreach (var item in sc)
                    {
                        item.enabled = true;
                    }
                    state = State.IDLE;
                    // 몬스터를 비활성화
                    this.gameObject.SetActive(false);
                    break;

            }
            yield return new WaitForSeconds(TIMER_CHECK);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("BULLET"))
        {
            Destroy(collision.gameObject);
        }
    }
    public void OnDamage(Vector3 pos, Vector3 normal)
    {
        if (isDie || state == State.DIE) return;

        // HP 차감
        hp -= HIT_MONSTER_HP;
        Debug.Log($"<color=red>몬스터 피격! 현재 HP: {hp}</color>");

        // 피격 애니메이션 트리거
        animator.SetTrigger(hashhit);

        // 사망 판정
        if (hp <= 0)
        {
            Debug.Log("<color=yellow>몬스터 HP 0 달성! 사망 로직 진입</color>");
            Die(); // 🚨 즉시 사망 함수 호출
        }
    }
    private void ShowBloodEffect(Vector3 pos, Quaternion rot)
    {
        GameObject blood = Instantiate<GameObject>(bloodEffect, pos, rot, monsterTr);
        Destroy(blood, 1.0f);
    }
    void OnTriggerEnter(Collider coll)
    {
        Debug.Log(coll.gameObject.name);
    }
    private void OnPlayerDie()
    {
        Debug.Log("<color=orange>1. MonsterCtrl: OnPlayerDie 실행됨</color>");

        if (this == null || !gameObject.activeInHierarchy) return;

        StopAllCoroutines();
        if (agent != null) agent.isStopped = true;

        animator.Play("GangnamStyle", -1, 0f);

        // 🚨 GameManager 호출 전 체크
        if (GameManager.instance != null)
        {
            Debug.Log("<color=orange>2. MonsterCtrl: GameManager instance 찾음. UI 호출 시도!</color>");
            GameManager.instance.ShowGameOverUI();
        }
        else
        {
            Debug.LogError("<color=red>🚨 MonsterCtrl: GameManager instance가 null입니다!</color>");
        }
    }

    void Update()
    {
        if (agent.remainingDistance >= 2.0f)
        {
            Vector3 direction = agent.desiredVelocity;
            Quaternion rot = Quaternion.LookRotation(direction);
            monsterTr.rotation = Quaternion.Slerp(monsterTr.rotation, rot, Time.deltaTime * 10.0f);
        }
    }
    private void Die()
    {
        if (isDie) return;
        isDie = true;
        state = State.DIE;

        // 1. 모든 행동 중지
        StopAllCoroutines();
        if (agent != null) agent.isStopped = true;

        // 2. 물리 판정 제거 (사체에 총알이 더 이상 안 맞게)
        var collider = GetComponent<CapsuleCollider>();
        if (collider != null) collider.enabled = false;

        // 3. 사망 애니메이션 재생
        animator.SetTrigger(hashDie);

        // 4. 🚨 GameManager를 통해 GameClear UI 호출
        if (GameManager.instance != null)
        {
            Debug.Log("GameManager에게 GameClear UI 요청");
            GameManager.instance.ShowGameClearUI();
        }
        else
        {
            Debug.LogError("GameManager 인스턴스를 찾을 수 없어 UI를 띄우지 못했습니다.");
        }

        // 5. 시체 제거 (옵션: 3초 후)
        // Invoke("ReturnToPool", 3.0f);
    }
}


