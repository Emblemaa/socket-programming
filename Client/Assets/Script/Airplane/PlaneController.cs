using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public Transform planeMesh;

    public float engineThrust = 1000f;
    public float defaultThrust = 1000f;
    public float thrustSpeed = 2;
    public float pitchSpeed = 50f;
    public float yawSpeed = 50f;
    public float rollSpeed = 20f;
    public float autoTurnAngle = 30f;

    public bool autoRollBalance = false;

    private Camera cam;
    private Rigidbody rb;

    public bool enable = true;
    /*
     * Physics varibles
     */
    public float thrust;
    public float roll;
    private float pitch;
    private float yaw;
    private bool enableMouseControl;

    internal float speed;
    internal bool showCrosshairs;
    internal Vector3 crosshairsPosition;

    /**
     *Constants
     */
    private const float mToKm = 3.6f;
    private const float kmToKnots = 0.5399568035f;
    private const float aerodynamicsEffect = 0.1f;

    internal float throttle
    {
        get { return thrust; }
    }

    private void Awake()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();

        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        if (rb.mass == 1)
        {
            rb.mass = 2000;
            rb.drag = 0.75f;
            rb.angularDrag = 0.05f;
        }
    }

    private void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    private void Update()
    {
        roll = Input.GetAxis("Horizontal")*-2;

        updateThrotte();
        followTheMouse();
        
    }

    private void followTheMouse()
    {
        Vector3 localTarget = transform.InverseTransformDirection(cam.transform.forward);
        localTarget.Normalize();

        float targetRollAngle = Mathf.Lerp(0, autoTurnAngle, Mathf.Abs(localTarget.x)) * -Mathf.Sign(localTarget.x);

        pitch = -Mathf.Clamp(localTarget.y, -1f, 1f);
        yaw = Mathf.Clamp(localTarget.x, -1f, 1f);
        roll += targetRollAngle * Time.deltaTime;
    } 

    private void updateThrotte()
    {
        thrust += (Input.GetKey(KeyCode.Space) ? 1 : 0) * (engineThrust/thrustSpeed) * Time.deltaTime;
        if (thrust > defaultThrust)
        {
            thrust -= 10 * Time.deltaTime;
        }
        thrust = Mathf.Clamp(thrust, 0f, engineThrust);
    }

    private void FixedUpdate()
    {
        if (!enable) return;
        // Pitch, yaw and roll
        transform.RotateAround(transform.position, transform.right, pitch * Time.fixedDeltaTime * pitchSpeed);
        transform.RotateAround(transform.position, transform.up, yaw * Time.fixedDeltaTime * yawSpeed);

        planeMesh.transform.RotateAround(planeMesh.transform.position, planeMesh.transform.forward, roll * Time.fixedDeltaTime * rollSpeed);

        // Calculate current speed for display on UI
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        float localSpeed = Mathf.Max(0, localVelocity.z);
        speed = (localSpeed * mToKm) * kmToKnots;

        // Calculate and adjust rb.velocity
        float aerofactor = Vector3.Dot(transform.forward, rb.velocity.normalized);
        aerofactor = (float)Math.Pow(aerofactor, 2f);
        rb.velocity = Vector3.Lerp(rb.velocity, transform.forward * localSpeed, aerofactor * localSpeed * aerodynamicsEffect * Time.fixedDeltaTime);

        rb.AddForce((thrust * engineThrust) * transform.forward);
    }

    private void LateUpdate()
    {
        if (!enableMouseControl) return;

        crosshairsPosition = cam.WorldToScreenPoint(transform.position + transform.forward * speed); // Try speed instead of 500f
    }
}