using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    public float damage = 20.0f;
    public float force = 1500.0f;
    private Rigidbody rb;
    void Start()
    {
        //Rigidbody 연결
        rb = GetComponent<Rigidbody>();

        //전진방향으로 힘을 가함
        rb.AddForce(transform.forward * force);
    }
}
