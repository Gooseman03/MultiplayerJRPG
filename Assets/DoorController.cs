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

    [SerializeField] private float openTimer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //if (leftSlat == null || rightSlat == null)
        //{
        //    Debug.LogError("Doors Require 2 slats");
        //    this.enabled = false;
        //}
        //doorWidth = leftSlat.transform.localScale.x + rightSlat.transform.localScale.x;
    }

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

//float newScale = Mathf.Lerp(doorWidth / 2, 0, openTimer);
//float positionOffset = Mathf.Lerp(0, doorWidth / 4, openTimer);
//GameObject slat;
//while (true)
//{
//    float newPostition = (float)(doorWidth / 4) + positionOffset;
//    if (doorSide == DoorSide.left)
//    {
//        slat = leftSlat;
//        newPostition = -newPostition;
//    }
//    else
//    {
//        slat = rightSlat;
//    }
//    slat.transform.localScale = new Vector3(newScale, slat.transform.localScale.y, slat.transform.localScale.z); ;
//    slat.transform.localPosition = new Vector3(newPostition, 0, 0);
//    if (doorSide == DoorSide.right)
//    { 
//        break; 
//    }
//    doorSide = DoorSide.right;
//}