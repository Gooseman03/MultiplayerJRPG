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
    public MessageBundle ExtrapolateMissingMessage(uint index, MessageBundle message1, MessageBundle message2)
    {

        Debug.Log("Message Was Missing... Extrapolating");
        if (message1.Input == message2.Input)
        {
            Debug.Log("New Message Created At " + index + " The Extrapolated Input was " + message1.Input);
            return new MessageBundle() { Id = index, Input = message1.Input };
        }

        return (new MessageBundle() { Id = index, Input = Vector2.zero });
    }
}
/*
public int countLostPackets(List<MessageBundle> newMessages)
{
    uint tempLastMessageId = lastMessage.Id;
    int LostPackets = 0;
    foreach (MessageBundle message in newMessages)
    {
        LostPackets += (int)(message.Id - tempLastMessageId + 1);
        tempLastMessageId = message.Id;
    }
    return LostPackets;
}
*/
