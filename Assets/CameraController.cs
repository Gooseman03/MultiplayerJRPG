using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _camera;
    private GameObject cameraGameObject;
  
    void Awake()
    {
        cameraGameObject = new GameObject("PlayerCamera");
        cameraGameObject.transform.parent = transform;
        cameraGameObject.transform.localPosition = new Vector3(0,0,-10);
        _camera = cameraGameObject.AddComponent<Camera>();
        cameraGameObject.AddComponent<AudioListener>();
        _camera.orthographic = true;
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = Color.black;
    }

    private void OnDestroy()
    {
        if (cameraGameObject != null)
        {
            Destroy(cameraGameObject);
        }
    }
}
