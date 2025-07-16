using UnityEditor;
using UnityEngine;

public class EditorCamera : MonoBehaviour
{
    [SerializeField] private GameObject m_Camera;
    private void Start()
    {
        Destroy(m_Camera);
    }
}
