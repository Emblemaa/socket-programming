using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{
    public bool isDead = false;
    public GameObject smoke;

    public void Start()
    {
        gameObject.name = RoomHandler.Instance.OWNER_ID;
    }
    public static Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
    private Vector3 getSpawnPos()
    {
        GameObject[] spawn = GameObject.FindGameObjectsWithTag("Spawn");
        GameObject currentSpawn = spawn[Random.Range(0, spawn.Length)];
        return RandomPointInBounds(currentSpawn.GetComponent<BoxCollider>().bounds);
    }

    public void Die()
    {
        StartCoroutine(Respawn());
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
        if(collision.gameObject.GetComponent<Puppet>())
        {
            //Hitting someone else
            ServerHandler.Instance.SendPackage("ADD_POINT#" + RoomHandler.Instance.OWNER_ID + "|" + 100, SendType.TCP);
        }
        else
        {
            ServerHandler.Instance.SendPackage("ADD_POINT#" + RoomHandler.Instance.OWNER_ID + "|" + -100, SendType.TCP);
        }
        Die();
    }

    IEnumerator Respawn()
    {
        if (isDead) yield break;
        GetComponent<PlaneController>().enable = false;
        GetComponent<PlaneController>().thrust = 0;
        isDead = true;
        GetComponent<Rigidbody>().useGravity = true;
        smoke.SetActive(true);
        ServerHandler.Instance.SendPackage("DIE#" + RoomHandler.Instance.OWNER_ID, SendType.TCP);
        yield return new WaitForSeconds(5);
        smoke.SetActive(false);
        transform.position = getSpawnPos();
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        GetComponent<PlaneController>().enable = true;
        isDead = false;
    }

    private void Awake()
    {
        transform.position = getSpawnPos();
    }
}
