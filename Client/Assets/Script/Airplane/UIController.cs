using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public PlaneController planeController;

    public Text speedText;
    public Text heightText;
    public Text throttleText;
    public RectTransform crosshairs;

    private void Start()
    {
        crosshairs.gameObject.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        if (planeController == null)
        {
            Debug.LogError("Plane controller not found");
            return;
        }

        speedText.text = $"Speed: {planeController.speed:n0}";
        throttleText.text = $"Throttle: {planeController.throttle:n0}";

        if (planeController.showCrosshairs)
        {
            planeController.showCrosshairs = false;
            if (!crosshairs.gameObject.activeSelf) crosshairs.gameObject.SetActive(true);
        }

        crosshairs.position = planeController.crosshairsPosition;
    }
}