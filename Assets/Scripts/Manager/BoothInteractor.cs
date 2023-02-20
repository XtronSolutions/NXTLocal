using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoothInteractor : MonoBehaviour
{
    public Transform InteractTransform;
    private string linkedBoothURL;
    private bool interactEnabled = false;
    bool isTriggered = false;
    bool checkGoingBack = false;
    Transform playerTransform;

    float minDistance = 1.8f;
    float maxDistance = 1.9f;
    float calculatedDistance;
    public void SetBoothData(string _url,bool _state)
    {
        linkedBoothURL = _url;
        interactEnabled = _state;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (interactEnabled && other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            BoothManager.Instance.SetSelectedBoothLink(linkedBoothURL);
            BoothManager.Instance.ToggleInteractMessage(true);
            playerTransform = other.transform;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (interactEnabled && isTriggered && other.CompareTag("Player"))
        {
            isTriggered = false;
            BoothManager.Instance.ToggleInteractMessage(false);
            BoothManager.Instance.ToggleInteract(false);
        }
    }

    private void FixedUpdate()
    {
        if (interactEnabled && isTriggered && !Constants.InteractionRunning && InteractTransform != null)
        {
            calculatedDistance = Vector3.Distance(InteractTransform.position, playerTransform.transform.position);
            if (calculatedDistance < minDistance && checkGoingBack == false)
            {
                checkGoingBack = true;
                BoothManager.Instance.ToggleInteract(true);
                BoothManager.Instance.ToggleInteractMessage(false);
            }
            else if (calculatedDistance > maxDistance && checkGoingBack == true)
            {
                BoothManager.Instance.SetSelectedBoothLink(linkedBoothURL);
                BoothManager.Instance.ToggleInteract(false);
                BoothManager.Instance.ToggleInteractMessage(true);
                checkGoingBack = false;
            }
            Debug.Log(calculatedDistance);
        }
    }
}

