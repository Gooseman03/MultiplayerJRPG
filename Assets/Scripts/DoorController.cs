using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public enum DoorState
{
    open,
    close
}
[Serializable]
public struct Door
{
    public Transform door;
    public Transform doorStart;
    public Transform doorEnd;
}
public class DoorController : MonoBehaviour
{
    [SerializeField] private short doorCount;
    [SerializeField] private List<Door> doors = new List<Door>();

    DoorState state = DoorState.close;
    private bool isOpen
    {
        get
        {
            return state == DoorState.open;
        }
        set
        {
            state = value ? DoorState.open : DoorState.close;
        }
    }
    private bool isMoving;

    private float openTimer = 0;

    public void SwitchDoor()
    {
        isOpen = !isOpen;
        ChangeState(state);
    }
    public void ChangeState(DoorState newState)
    {
        StopAllCoroutines();
        state = newState;
        if (!isMoving)
        {
            openTimer = (int)state;
        }
        StartCoroutine(MoveDoor(isOpen));
    }
    public void OpenDoor()
    {
        ChangeState(DoorState.open);
    }
    public void CloseDoor()
    {
        ChangeState(DoorState.close);
    }

    private IEnumerator MoveDoor(bool isOpening)
    {
        isMoving = true;
        
        while (isMoving)
        {
            openTimer = isOpening ? openTimer + Time.deltaTime : openTimer - Time.deltaTime;

            if (openTimer > 1 || openTimer < 0)
            {
                isMoving = false;
                openTimer = Mathf.RoundToInt(openTimer);
            }
            
            foreach (Door door in doors)
            {
                door.door.localScale = Vector3.Lerp(door.doorStart.localScale, door.doorEnd.localScale, openTimer);
                door.door.localPosition = Vector3.Lerp(door.doorStart.localPosition, door.doorEnd.localPosition, openTimer);
                door.door.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(door.doorStart.localRotation.eulerAngles.z, door.doorEnd.localRotation.eulerAngles.z, openTimer));
            }

            yield return null;
        }
    }
}