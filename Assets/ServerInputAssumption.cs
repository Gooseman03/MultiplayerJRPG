using Unity.Netcode;
using UnityEngine;
using Ladder.PlayerMovementHelpers;
using System.Collections.Generic;

public class ServerInputAssumption : NetworkBehaviour
{
    private List<MessageBundle> messages = new List<MessageBundle>();
    public MessageBundle lastMessage;
    // this will fill out missing messages if the difference is 2 otherwise it will leave the missing message out
    public List<MessageBundle> FindMissingMessage(List<MessageBundle> newMessages)
    {
        List < MessageBundle> ExtrapolatedMessages = new(newMessages);
        int c = 0;
        int insertedMessages = 0;
        foreach (MessageBundle message in newMessages)
        {
            if(message.Id - lastMessage.Id == 2 && lastMessage.Id != 0)
            {
                MessageBundle ExtrapolatedMessage = ExtrapolateMissingMessage(message.Id - 1, new(lastMessage), new(message));
                //Everytime we insert a message the list will shift "insertedMessages" accounts for that shift by increasing every time we insert a message
                ExtrapolatedMessages.Insert(c+ insertedMessages, ExtrapolatedMessage);
                insertedMessages++;
            }
            lastMessage = message;
            c++;
        }
        newMessages = null;
        return ExtrapolatedMessages;
    }
    /* 
     * This will ALWAYS Create a Message  
     * if the Message before and after are the same It will Extrapolate the new message as the same 
     * otherwise it will return a message of no input
    */
    private MessageBundle ExtrapolateMissingMessage(ulong index, MessageBundle message1,MessageBundle message2)
    {
        Debug.Log("Message Was Missing... Extrapolating");
        if (message1.Input == message2.Input)
        {
            Debug.Log("New Message Created At " + (ulong)index + " The Extrapolated Input was " + message1.Input);
            return new MessageBundle() { Id = (ulong)index, Input = message1.Input};
        }
        Debug.Log("Input was Changed Between Packets Cancelling");
        return (new MessageBundle() { Id = (ulong)index, Input = Vector2.zero });
    }
}
/*
public int countLostPackets(List<MessageBundle> newMessages)
{
    ulong tempLastMessageId = lastMessage.Id;
    int LostPackets = 0;
    foreach (MessageBundle message in newMessages)
    {
        LostPackets += (int)(message.Id - tempLastMessageId + 1);
        tempLastMessageId = message.Id;
    }
    return LostPackets;
}
*/
