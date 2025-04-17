
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform followTarget;

    [SerializeField] float distance = 5;

    [SerializeField] float rotationSpeedX = 2;
    [SerializeField] float rotationSpeedY = 2;

    [SerializeField] float minVerticalAngle = -45;
    [SerializeField] float maxiVerticalAngle = 45;

    [SerializeField] Vector2 framingOffset;

    [SerializeField] bool invertX;
    [SerializeField] bool invertY;



    float rotationY;
    float rotationX;

    float inverXVal;
    float inverYval;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        inverXVal = (invertX) ? -1 : 1;
        inverYval = (invertY) ? -1 : 1;
        rotationX -= Input.GetAxis("Mouse Y") * rotationSpeedX * inverXVal;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxiVerticalAngle);

        rotationY += Input.GetAxis("Mouse X") * rotationSpeedY * inverYval;

        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
        var focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y, 0);

        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;
    }


}
