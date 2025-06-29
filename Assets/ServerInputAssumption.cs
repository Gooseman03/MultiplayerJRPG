using Ladder.PlayerMovementHelpers;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerInputAssumption : NetworkBehaviour
{
    private List<MessageBundle> messages = new List<MessageBundle>();
    public MessageBundle lastMessage;
    /* 
     * This will ALWAYS Create a Message  
     * if the Message before and after are the same It will Extrapolate the new message as the same 
     * otherwise it will return a message of no input
    */
    public Inputs ExtrapolateMissingInputs(Inputs inputs1, Inputs inputs2)
    {
        Debug.Log("Message Was Missing... Extrapolating");
        if (inputs1.MoveInput == inputs2.MoveInput)
        {
            Debug.Log("New Message Created at " + (uint)NetworkManager.ServerTime.Tick + " The Extrapolated Input was " + inputs1.MoveInput);
            return new Inputs(inputs1);
        }
        Debug.Log("Input was Changed Between Messages... Cancelling");
        return new Inputs(Vector2.zero, false);
    }
}