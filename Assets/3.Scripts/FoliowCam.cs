using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoliowCam : MonoBehaviour
{
    // 따라갈 대상
    public Transform targetTr;
    // MainCamera 자신의 Transform
    private Transform camTr;
    // 대상으로부터 거리
    [Range(2.0f, 20.0f)]
    public float distance = 10.0f;
    // y축 높이
    [Range(0.0f, 10.0f)]
    public float height = 2.0f;
    // 반응 속도
    public float damping = 10.0f;
    public float targetTrOffset = 2.0f; 
    private Vector3 velocity = Vector3.zero;
    void Start()
    {   
        camTr = GetComponent<Transform>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //추적할 대상 뒤쪽으로 distance만큼, 높이는 height만큼 이동
        Vector3 pos = targetTr.position
                        + (-targetTr.forward * distance)
                        + (Vector3.up * height);

        camTr.position = Vector3.SmoothDamp(camTr.position,
                                            pos,
                                            ref velocity,
                                            damping);
                                            
        // 피벗좌표를 향해 회전
        camTr.LookAt(targetTr.position + (targetTr.up * targetTrOffset));
    }
}
