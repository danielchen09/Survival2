using UnityEngine;

public class PlayerLook : MonoBehaviour {
    private float xRotation;
    public float sensitivity = 100f;

    private GameObject player;

    private void Start() {
        player = GameObject.Find("Player");
    }

    void Update() {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        player.transform.Rotate(Vector3.up * mouseX);

        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation - mouseY, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
