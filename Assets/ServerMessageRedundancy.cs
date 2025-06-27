using Ladder.PlayerMovementHelpers;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerMessageRedundancy : NetworkBehaviour
{
    public PlayerMovement playerMovement;
    private ulong LastMessageId = 0;
    private int CurrentRedunancy = 0;
    public void ApplyRedundancyToClient(List<MessageBundle> messages)
    {
        int messagesLost = FindMissingMessages(messages);
        CalculateNewRedundancy(messagesLost);
    }

    private int FindMissingMessages(List<MessageBundle> messages)
    {
        ulong missingMessages = 0;
        foreach (MessageBundle message in messages)
        {
            if (message.Id - 1 < LastMessageId)
            {
                missingMessages += message.Id - LastMessageId + 1;
            }
            LastMessageId = message.Id;
        }
        return (int)missingMessages;
    }
    private void CalculateNewRedundancy(int messagesLost)
    {
        if (messagesLost >= CurrentRedunancy)
        {
            CurrentRedunancy++;
        }
        if (messagesLost < CurrentRedunancy)
        {
            CurrentRedunancy--;
        }
    }
}
