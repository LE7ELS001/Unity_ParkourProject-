using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ClimbingController : MonoBehaviour
{
    ClimbPoint currentPoint;
    EnvrionmentScanner envrionmentScanner;
    PlayerController playerController;

    private void Awake()
    {
        envrionmentScanner = GetComponent<EnvrionmentScanner>();
        playerController = GetComponent<PlayerController>();
    }
    void Update()
    {
        if (!playerController.IsHanging)
        {

            if (Input.GetButton("Jump") && !playerController.InAction)
            {
                if (envrionmentScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit))
                {

                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);
                    playerController.setControl(false);

                    StartCoroutine(JumpToLedge("IdleToHang", currentPoint.transform, 0.14f, 0.54f, handOffset: new Vector3(0.25f, 0f, 0.08f)));

                }
            }

            if (Input.GetButton("Drop") && !playerController.InAction)
            {
                if (envrionmentScanner.DropLedgeCheck(out RaycastHit ledgeHit))
                {
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);
                    playerController.setControl(false);
                    StartCoroutine(JumpToLedge("DropToHang", currentPoint.transform, 0.30f, 0.45f, handOffset: new Vector3(0.25f, 0.28f, -0.25f)));
                }
            }
        }

        else
        {
            //drop from hang
            if (Input.GetButton("Drop") && !playerController.InAction)
            {

                StartCoroutine(JumpFromHang());
                return;

            }
            //ledge to ledge jump

            float h = Mathf.Round(Input.GetAxisRaw("Horizontal"));
            float v = Mathf.Round(Input.GetAxisRaw("Vertical"));
            var inputDir = new Vector2(h, v);

            if (playerController.InAction || inputDir == Vector2.zero) return;

            //climb from hang 
            if (currentPoint.MountPoint && inputDir.y == 1)
            {
                StartCoroutine(MountFromHang());
                return;
            }

            var neighbour = currentPoint.GetNeighbour(inputDir);

            if (neighbour == null) return;

            if (neighbour.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
            {
                currentPoint = neighbour.point;

                if (neighbour.direction.y == 1)
                {
                    StartCoroutine(JumpToLedge("HopUp", currentPoint.transform, 0.34f, 0.65f, handOffset: new Vector3(0.25f, 0.06f, 0.18f)));
                }
                else if (neighbour.direction.y == -1)
                {
                    StartCoroutine(JumpToLedge("HangDrop", currentPoint.transform, 0.31f, 0.65f, handOffset: new Vector3(0.25f, 0.11f, 0.15f)));
                }
                else if (neighbour.direction.x == 1)
                {
                    StartCoroutine(JumpToLedge("HopRight", currentPoint.transform, 0.20f, 0.50f));
                }
                else if (neighbour.direction.x == -1)
                {
                    StartCoroutine(JumpToLedge("HopLeft", currentPoint.transform, 0.20f, 0.50f, handOffset: new Vector3(0.4f, 0.05f, 0.08f)));
                }

            }
            else if (neighbour.connectionType == ConnectionType.Move)
            {
                currentPoint = neighbour.point;

                if (neighbour.direction.x == 1)
                {
                    StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform, 0f, 0.38f, handOffset: new Vector3(0.25f, 0.01f, 0.08f)));
                }
                else if (neighbour.direction.x == -1)
                {
                    StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, 0f, 0.38f, AvatarTarget.LeftHand, handOffset: new Vector3(0.25f, 0.01f, 0.08f)));
                }
            }
        }

    }

    IEnumerator JumpToLedge(string anim, Transform ledge,
     float matchStartTime, float mathTargetTime,
     AvatarTarget hand = AvatarTarget.RightHand,
     Vector3? handOffset = null)
    {
        var matchParams = new MatchTargetParams()
        {
            pos = GetHandPos(ledge, hand, handOffset),
            bodyPart = hand,
            startTime = matchStartTime,
            targetTime = mathTargetTime,
            posWeight = Vector3.one
        };

        var targetRot = Quaternion.LookRotation(-ledge.forward);

        yield return playerController.DoAction(anim, matchParams, targetRot, true);

        playerController.IsHanging = true;
    }

    Vector3 GetHandPos(Transform ledge, AvatarTarget hand, Vector3? handOffset = null)
    {
        var offVal = (handOffset != null) ? handOffset.Value : new Vector3(0.25f, 0.05f, 0.08f);

        var handDir = (hand == AvatarTarget.RightHand) ? ledge.right : -ledge.right;
        return ledge.position + ledge.forward * offVal.z + Vector3.up * offVal.y - handDir * offVal.x;
    }

    IEnumerator JumpFromHang()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("JumpFromHang");
        playerController.ResetTargetRotation();
        playerController.setControl(true);
    }

    IEnumerator MountFromHang()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("MountFromHang");

        playerController.EnableCharacterController(true);

        yield return new WaitForSeconds(0.5f);

        playerController.ResetTargetRotation();
        playerController.setControl(true);

    }

    ClimbPoint GetNearestClimbPoint(Transform ledge, Vector3 hitpoint)
    {
        var points = ledge.GetComponentsInChildren<ClimbPoint>();

        ClimbPoint nearestPoint = null;
        float nearestPointDistance = Mathf.Infinity;

        foreach (var point in points)
        {
            float distance = Vector3.Distance(point.transform.position, hitpoint);

            if (distance < nearestPointDistance)
            {
                nearestPoint = point;
                nearestPointDistance = distance;
            }
        }

        return nearestPoint;


    }
}
