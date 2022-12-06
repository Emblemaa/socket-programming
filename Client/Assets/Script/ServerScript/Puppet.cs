using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puppet : MonoBehaviour
{
    private float inactiveTime = 0;

    private Vector3 targetPos;
    private float roll;
    public GameObject planeMesh;
    public GameObject smoke;

    private void Start()
    {
        targetPos = transform.position;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime*20);
        planeMesh.transform.RotateAround(planeMesh.transform.position, planeMesh.transform.forward, roll * Time.deltaTime * 20);
        inactiveTime += Time.deltaTime;
        if (inactiveTime > 10)
        {
            Destroy(gameObject);
        }
    }

    public void OnEntityUpdate(Vector3 pos, Quaternion rotation, float roll)
    {
        if (Vector2.Distance(pos, transform.position) > 10) transform.position = pos;
        inactiveTime = 0;
        targetPos = pos;
        transform.rotation = rotation;
        this.roll = roll;
    }

    public void OnEntityAttack()
    {

    }
    
    IEnumerator Die()
    {
        smoke.SetActive(true);
        yield return new WaitForSeconds(5);
        smoke.SetActive(false);
    }

    public void OnEntityDie()
    {
        StartCoroutine(Die());
    }
}
