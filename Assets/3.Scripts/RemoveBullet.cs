using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveBullet : MonoBehaviour
{
    //스파크 파티클 프리펩 연결할 변수
    public GameObject sparkEffect;
    // 충돌 시작 시 발생하는 이벤트
    void OnCollisionEnter(Collision coll)
    {
        // tag값 비교
        //if (coll.collider.tag == "BULLET")
        if (coll.collider.CompareTag("BULLET"))
        {
            // 충돌지점 정보 추출
            ContactPoint cp = coll.GetContact(0);
            // 충돌한 총알의 normal vector를 Qwatarnion 타입으로 변환
            Quaternion rot = Quaternion.LookRotation(-cp.normal);

            // 스파크 파티클을 동적으로 생성
            GameObject spark = Instantiate(sparkEffect, cp.point, rot);
            // 일정 시간 지난 후 스파크 파티클 삭제
            Destroy(spark, 0.5f);
            
            // 충돌한 gameObject 삭제
            Destroy(coll.gameObject);
        }
    }
}
