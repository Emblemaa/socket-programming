using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private float lifeTime = 0;
    private static float MAX_TIME = 3;
    public GameObject ExplosionPrefab;

    private void Despawn()
    {
        Instantiate(ExplosionPrefab, transform.position + transform.up*-10, Quaternion.identity, null);
        Destroy(gameObject);
    }

    private void Start()
    {
        GetComponent<Rigidbody>().AddForce(150 * transform.up, ForceMode.Impulse);       
    }
    private void Update()
    {
        lifeTime += Time.deltaTime;
        if(lifeTime > MAX_TIME)
        {
            Despawn();
            return;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other) return;
        if ((other.gameObject.name.Equals(gameObject.name)) || (other.gameObject.tag.Equals("Spawn"))) return;
        Plane plane = other.gameObject.GetComponent<Plane>();
        if (plane && !plane.isDead)
        {
            ServerHandler.Instance.SendPackage("ADD_POINT#" + other.gameObject.name + "|" + 100, SendType.TCP);
            plane.Die();
        }
        Despawn();
    }
}
