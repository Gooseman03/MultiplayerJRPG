using System;
using System.Net;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject uiConnectionModePanel;

    [SerializeField] private UnityEngine.UI.Button clientButton;
    [SerializeField] private UnityEngine.UI.Button hostButton;
    [SerializeField] private TMP_InputField ipField;
    private void Start()
    {
        // Makes a new Function that is called when the buttons are clicked
        clientButton.onClick.AddListener(() =>
        {

            if (IsValidIp (ipField.text))
            {
                Debug.Log(ipField.text + " is a valid IP address.");
            }
            else
            {
                Debug.Log(ipField.text + " is not a valid IP address... Cancelling Client Startup");
                return;
            }

            // Initialize the client with values set in the ip field
            // Copied from https://docs-multiplayer.unity3d.com/netcode/current/components/networkmanager/   
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData
                (
                ipField.text, // IP
                (ushort)7777  // Port
                );
            NetworkManager.Singleton.StartClient();
            uiConnectionModePanel.SetActive(false);
        });
        hostButton.onClick.AddListener(() =>
        {
            // Initialize the host with default listen port and address
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData
                ( 
                "127.0.0.1",
                (ushort)7777,
                "0.0.0.0"
                );
            NetworkManager.Singleton.StartHost();
            uiConnectionModePanel.SetActive(false);
        });
    }
    bool IsValidIp(string ipString)
    {
        return IPAddress.TryParse(ipString, out _);
    }
}
