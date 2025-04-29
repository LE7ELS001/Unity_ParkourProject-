using System;
using System.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{

    [SerializeField] float MoveSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;

    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    bool isGrounded;
    bool hasControl = true;



    public bool InAction { get; private set; }
    public bool IsHanging { get; set; }

    Vector3 desiredMoveDir;
    Vector3 moveDir;
    Vector3 Velocity;
    public bool isOnLedge { get; set; }
    public LedgeData LedgeData { get; set; }

    float ySpeed;

    Animator animator;
    Quaternion targetRotation;
    CharacterController characterController;

    CameraController cameraController;

    EnvrionmentScanner envrionmentScanner;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        envrionmentScanner = GetComponent<EnvrionmentScanner>();


    }
    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        var moveInput = (new Vector3(h, 0, v)).normalized;

        desiredMoveDir = cameraController.PlanarRotation * moveInput;
        moveDir = desiredMoveDir;

        if (!hasControl) { return; }

        if (IsHanging) { return; }

        Velocity = Vector3.zero;

        // Debug.Log(hasControl);
        if (hasControl)
        {

            GroundCheck();
            animator.SetBool("isGrounded", isGrounded);
            if (isGrounded)
            {
                ySpeed = -0.5f;
                Velocity = desiredMoveDir * MoveSpeed;

                isOnLedge = envrionmentScanner.LedgeCheck(desiredMoveDir, out LedgeData ledgeData);

                if (isOnLedge)
                {
                    LedgeData = ledgeData;
                    LedgeMovement();
                }
                animator.SetFloat("moveAmount", Velocity.magnitude / MoveSpeed, 0.2f, Time.deltaTime);
            }
            else
            {
                ySpeed += Physics.gravity.y * Time.deltaTime;

                Velocity = transform.forward * MoveSpeed / 2;
            }
            Velocity.y = ySpeed;

            characterController.Move(Velocity * Time.deltaTime);

            if (moveAmount > 0 && moveDir.magnitude > 0.2f)
            {
                targetRotation = Quaternion.LookRotation(moveDir);
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
            rotationSpeed * Time.deltaTime);


        }

    }

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);

    }

    public void setControl(bool hasControl)
    {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;

        if (!hasControl)
        {
            animator.SetFloat("moveAmount", 0f);
            targetRotation = transform.rotation;
        }
    }

    public bool HasControl
    {
        get => hasControl;
        set => hasControl = value;
    }

    void LedgeMovement()
    {
        if (Vector3.Angle(desiredMoveDir, transform.forward) >= 80)
        {
            Velocity = Vector3.zero;
            return;
        }

        float SignedAngle = Vector3.SignedAngle(LedgeData.surfaceHit.normal, desiredMoveDir, Vector3.up);
        float angle = Mathf.Abs(SignedAngle);
        if (angle < 60)
        {
            Velocity = Vector3.zero;
            moveDir = Vector3.zero;
        }
        else if (angle < 90)
        {
            //between 60 and 90 
            var left = Vector3.Cross(Vector3.up, LedgeData.surfaceHit.normal);
            var dir = left * Mathf.Sign(SignedAngle);
            Velocity = Velocity.magnitude * dir;
            moveDir = dir;
        }
    }

    public void ResetTargetRotation()
    {
        targetRotation = transform.rotation;
    }

    public void EnableCharacterController(bool enable)
    {
        characterController.enabled = enable;
    }

    public IEnumerator DoAction(string animsName, MatchTargetParams matchParams = null, Quaternion targetRotation = new Quaternion(),
    bool rotate = false, float postDelay = 0f, bool mirror = false)
    {
        InAction = true;

        animator.SetBool("mirrorAction", mirror);
        animator.CrossFadeInFixedTime(animsName, 0.2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(animsName))
        {
            Debug.LogError("The parkour animation is wrong!");
        }

        float rotateStartTime = (matchParams != null) ? matchParams.startTime : 0f;

        float timer = 0;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / animState.length;

            if (rotate && normalizedTime > rotateStartTime)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
            }

            if (matchParams != null)
            {
                MatchTarget(matchParams);
            }

            if (animator.IsInTransition(0) && timer > 0.5f)
            {
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(postDelay);

        InAction = false;
    }

    void MatchTarget(MatchTargetParams mp)
    {
        if (animator.isMatchingTarget) return;

        animator.MatchTarget(mp.pos, transform.rotation, mp.bodyPart, new MatchTargetWeightMask(mp.posWeight, 0), mp.startTime, mp.targetTime);
    }

    public float RotationSpeed => rotationSpeed;
}

public class MatchTargetParams
{
    public Vector3 pos;
    public AvatarTarget bodyPart;
    public Vector3 posWeight;
    public float startTime;
    public float targetTime;
}
