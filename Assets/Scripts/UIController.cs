using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] private Button clientButton;
    [SerializeField] private Button hostButton;
    void Start()
    {
        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            SceneManager.LoadScene("SampleScene");
        });
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            SceneManager.LoadScene("SampleScene");
        });
    }

}
