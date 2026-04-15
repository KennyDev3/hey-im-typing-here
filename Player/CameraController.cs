using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using System;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ParticleSystem  speedupParticleSystem;

    [Header("Camera Settings")]
    [SerializeField] float minFOV = 20f;
    [SerializeField] float maxFov = 120f;
    [SerializeField] float zoomDuration = 0.5f;
    [SerializeField] float zoomSpeedModifier = 3f;



    CinemachineCamera cinemachineCamera;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
    }


    public void ChangeCameraFov(float speedAmount)
    {
        StopAllCoroutines();
        StartCoroutine(ChangeFovRoutine(speedAmount));

        if(speedAmount > 0)
        {
            speedupParticleSystem.Play();
        }
    }

    IEnumerator ChangeFovRoutine(float speedAmount)
    {
        float startFOV = cinemachineCamera.Lens.FieldOfView;
        float targetFOV = Mathf.Clamp(startFOV + speedAmount * zoomSpeedModifier, minFOV,maxFov);

        float elapsedTime = 0f;

        while (elapsedTime < zoomDuration)
        {
            float t = elapsedTime / zoomDuration;
            elapsedTime += Time.deltaTime;

            cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;

        }

        cinemachineCamera.Lens.FieldOfView = targetFOV;



    }
}
