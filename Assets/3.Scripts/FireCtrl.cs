using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 반드시 필요한 컴포넌트를 삭제 안되도록 하는 어트리뷰트
[RequireComponent(typeof(AudioSource))]
public class FireCtrl : MonoBehaviour
{
    public const float BULLET_DISTANCE = 50.0f;
    // 총알 프리펩
    public GameObject bullet;
    // 총알 발사 좌표
    public Transform firePos;
    // 총소리 음원
    public AudioClip fireSfx;

    // 오디오 컴포넌트
    private new AudioSource audio;
    private MeshRenderer muzzleFlash;
    private bool isPlayerDie;
    private RaycastHit hit;

    void OnEnable()
    {
        PlayerCtrl.OnPlayerDie += this.OnPlayerDie;
    }
    void OnDisable()
    {
        PlayerCtrl.OnPlayerDie -= this.OnPlayerDie;
    }
    void Start()
    {
        audio = GetComponent<AudioSource>();
        muzzleFlash = firePos.GetComponentInChildren<MeshRenderer>();
        // 처음 시작할 때 비활성화
        muzzleFlash.enabled = false;
        isPlayerDie = false;
    }
    public void OnPlayerDie()
    {
        isPlayerDie = true;
    }
    void Update()
{
    if (isPlayerDie) return;

    Debug.DrawRay(firePos.position, firePos.forward * BULLET_DISTANCE, Color.green);

    if (Input.GetMouseButtonDown(0))
    {
        Fire();

            if (Physics.Raycast(firePos.position, firePos.forward, out hit, BULLET_DISTANCE))
            {
                // 1단계: 무엇이라도 맞았는지 로그 찍기 (로그가 뜨는지 보세요!)
                Debug.Log($"레이캐스트 충돌: {hit.transform.name}");

                // 2단계: 본인부터 부모까지 뒤져서 스크립트 찾기
                var monster = hit.transform.GetComponentInParent<MonsterCtrl>();

                if (monster != null)
                {
                    monster.OnDamage(hit.point, hit.normal);
                }
            }
        } 
}
    void Fire()
    {   
        Instantiate(bullet, firePos.position, firePos.rotation);
        // 총소리 발생
        audio.PlayOneShot(fireSfx, 1.0f);
        // 총구 화염 효과 코루틴 함수 호출
        StartCoroutine(ShowMuzzleFlash());
        //StartCoroutine("ShowMuzzleFlash");
    }

    IEnumerator ShowMuzzleFlash()
    {   
        //Vector2 offset = new Vector2(Random.Range(0, 2), Random.Range(0, 2)) * 0.5f;
        // MuzzleFlash 활성화
        /*
        muzzleFlash.material.mainTextureOffset = offset;
        float angle = Random.Range(0, 360);
        muzzleFlash.transform.localRotation = Quaternion.Euler(0, 0, angle);
        float scale = Random.Range(1.0f, 2.0f);
        muzzleFlash.transform.localScale = Vector3.one * scale;
        */

        muzzleFlash.enabled = true;
        // 0.2초동안 대기(제어권 양보)
        yield return new WaitForSeconds(0.2f);

        // MuzzleFlash 비활성화
        muzzleFlash.enabled = false;
    }
}
