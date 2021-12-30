using UnityEngine;

public class PlayerLook : MonoBehaviour {
    private float xRotation;
    public float sensitivity = 100f;

    private GameObject player;

    public bool lockCamera = false;

    private void Start() {
        player = GameObject.Find("Player");
    }

    void Update() {
        if (!lockCamera) {
            if (Time.deltaTime > 0.05f)
                return;
            float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            player.transform.Rotate(Vector3.up * mouseX);

            float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
            xRotation = Mathf.Clamp(xRotation - mouseY, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    public void LockCamera(bool isLock) {
        lockCamera = isLock;
        Cursor.visible = lockCamera;
        Cursor.lockState = lockCamera ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
