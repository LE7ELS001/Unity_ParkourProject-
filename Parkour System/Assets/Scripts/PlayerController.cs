using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] float MoveSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;

    Animator animator;
    Quaternion targetRotation;
    CharacterController characterController;

    CameraController cameraController;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }
    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        var moveInput = (new Vector3(h, 0, v)).normalized;

        var moveDirection = cameraController.PlanarRotation * moveInput;

        if (moveAmount > 0)
        {
            characterController.Move(moveDirection * MoveSpeed * Time.deltaTime);
            targetRotation = Quaternion.LookRotation(moveDirection);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
        rotationSpeed * Time.deltaTime);

        animator.SetFloat("moveAmount", moveAmount, 0.2f, Time.deltaTime);

    }
}
