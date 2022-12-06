using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public GameObject BulletPrefab;
    public float FiringDelay = 0.1f;
    private float firingTimer = 0;
    void Start()
    {
        
    }

    void Update()
    {
        firingTimer += Time.deltaTime;
        if (Input.GetMouseButton(0) && firingTimer > FiringDelay)
        {
            firingTimer = 0;
            GameObject newBullet = Instantiate(BulletPrefab, transform.position, transform.rotation, null);
            newBullet.name = RoomHandler.Instance.OWNER_ID;
            Vector3 pos = transform.position;
            string content = pos.x.ToString("0.00") + "|" + pos.y.ToString("0.00") + "|" + pos.z.ToString("0.00") + "|" + transform.rotation.x + "|" + transform.rotation.y + "|" + transform.rotation.z + "|" + transform.rotation.w;
            ServerHandler.Instance.SendPackage("PLAYER_ATTACK#"+content, SendType.TCP);
        }
    }
}
