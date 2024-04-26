using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using System;


//!!!!TEMPORARY SCRIPT FOR FEATURE TESTING!!!!
public class ShowStatusUpgrade : MonoBehaviour
{
    [Serializable]
    private struct StatusImage
    {
        public PlayerStatus status;
        public GameObject obj;
    }
    [SerializeField] private StatusImage[] statusImages;

    private RectTransform rectTransform;
    private float xx = 0, target = 0;

    [SerializeField] private float timeShow, smoothRatio;
    private float timer;

    private Vector3 initialPos;

    void Start ()
    {
        rectTransform = GetComponent<RectTransform>();
        target = -rectTransform.rect.width;
        xx = target;

        initialPos = rectTransform.localPosition;

        Vector3 tmp = initialPos;
        tmp.x += xx;
        rectTransform.localPosition = tmp;
    }
    void Update()
    {
        timer -= Time.deltaTime;

        float _t = 0;

        if (timer <= 0)
        {
            _t = target;
        }
        xx += (_t - xx) / (smoothRatio / Time.deltaTime);

        Vector3 tmp = initialPos;
        tmp.x += xx;
        rectTransform.localPosition = tmp;
    }

    public void ShowStatus(PlayerStatus status)
    {
        foreach (StatusImage statusObject in statusImages)
        {
            statusObject.obj.SetActive(false);
            if (statusObject.status == status)
            {
                statusObject.obj.SetActive(true);
            }
        }

        timer = timeShow;
    }
}