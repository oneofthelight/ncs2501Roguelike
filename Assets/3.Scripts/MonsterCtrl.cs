using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MonsterCtrl : MonoBehaviour
{
    // 상태 열거형
    public enum State { IDLE, TRACE, ATTACK, DIE }
    public State state = State.IDLE;

    private Transform monsterTr;
    private Transform playerTr;
    private NavMeshAgent agent;
    private Animator animator;

    // 애니메이션 해시값
    private readonly int hashTrace = Animator.StringToHash("IsTrace");
    private readonly int hashAttack = Animator.StringToHash("IsAttack");
    private readonly int hashDie = Animator.StringToHash("Die");
    private readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");
    private readonly int hashSpeed = Animator.StringToHash("Speed");

    [Header("Monster Settings")]
    public float traceDist = 10.0f;
    public float attackDist = 2.0f;
    private bool isDie = false;

    void Awake()
    {
        monsterTr = GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // 플레이어 찾기 (보스 씬 기준)
        GameObject playerObj = GameObject.FindWithTag("PLAYER");
        if (playerObj != null) playerTr = playerObj.transform;
    }

    // 🚨 [핵심 수정] 오브젝트가 활성화될 때마다 실행
    void OnEnable()
    {
        isDie = false;
        state = State.IDLE;

        // 플레이어 사망 이벤트 구독
        PlayerCtrl.OnPlayerDie += this.OnPlayerDie;

        StartCoroutine(CheckMonsterState());
        StartCoroutine(MonsterAction());
    }

    // 🚨 [핵심 수정] 오브젝트가 비활성화(풀에 회수)될 때 실행
    void OnDisable()
    {
        // 🚨 중요: 반드시 이벤트를 해제해야 MissingReferenceException이 발생하지 않습니다.
        PlayerCtrl.OnPlayerDie -= this.OnPlayerDie;
        StopAllCoroutines();
    }

    IEnumerator CheckMonsterState()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.3f);

            if (playerTr == null) continue;

            float distance = Vector3.Distance(playerTr.position, monsterTr.position);

            if (distance <= attackDist)
                state = State.ATTACK;
            else if (distance <= traceDist)
                state = State.TRACE;
            else
                state = State.IDLE;
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
                    GetComponent<CapsuleCollider>().enabled = false;
                    break;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    // 🚨 [에러 해결 버전] 플레이어 사망 시 호출되는 함수
    private void OnPlayerDie()
    {
        // 1. 자기 자신이 이미 파괴되었거나 비활성 상태인지 확인
        if (this == null || !gameObject.activeInHierarchy) return;

        // 모든 행동 중지
        StopAllCoroutines();
        if (agent != null && agent.enabled) agent.isStopped = true;

        // 플레이어의 죽음을 비웃는(?) 애니메이션 등 연출
        animator.SetFloat(hashSpeed, UnityEngine.Random.Range(0.8f, 1.2f));
        animator.SetTrigger(hashPlayerDie);
    }
    // 🚨 완전히 파괴될 때를 대비한 2중 안전장치
    void OnDestroy()
    {
        PlayerCtrl.OnPlayerDie -= this.OnPlayerDie;
    }
    // FireCtrl에서 보내는 2개의 인자(Vector3, Vector3)를 받도록 수정
    public void OnDamage(Vector3 pos, Vector3 normal)
    {
        if (isDie) return;

        Debug.Log($"몬스터 피격! 위치: {pos}");

        // 필요하다면 여기서 피격 이펙트(혈흔 등)를 생성할 수 있습니다.
        // CreateBloodEffect(pos, normal); 

        // 현재는 한 대 맞으면 바로 죽는 로직
        Die();
    }

    // 사망 로직
    private void Die()
    {
        if (isDie) return;

        isDie = true;
        state = State.DIE;

        if (agent != null) agent.isStopped = true;

        animator.SetTrigger(hashDie);

        // 더 이상 총에 맞지 않게 콜라이더 끔
        GetComponent<CapsuleCollider>().enabled = false;

        Debug.Log("보스 몬스터 사망!");

        // 2초 뒤에 다시 소환될 수 있도록 풀로 회수
        Invoke("ReturnToPool", 2.0f);
    }

    private void ReturnToPool()
    {
        // 다시 켜줄 것들 정리 (재소환 대비)
        GetComponent<CapsuleCollider>().enabled = true;
        gameObject.SetActive(false);
    }
}