using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;
    [SerializeField] ParkourAction jumDownAction;
    [SerializeField] ParkourAction jumDownAction2;
    [SerializeField] float autoDroppedHeightLimite = 1;

    EnvrionmentScanner envrionmentScanner;
    Animator animator;
    PlayerController playerController;
    private void Awake()
    {
        envrionmentScanner = GetComponent<EnvrionmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {



        var hitData = envrionmentScanner.ObstacleCheck();
        if (Input.GetButton("Jump") && !playerController.InAction && !playerController.IsHanging)
        {
            if (hitData.forwardHitFound)
            {
                foreach (var action in parkourActions)
                {
                    if (action.CheckIfPossible(hitData, transform))
                    {
                        StartCoroutine(DoParkourACtion(action));
                        break;
                    }
                }

            }
        }

        if (playerController.isOnLedge && !playerController.InAction && !hitData.forwardHitFound)
        {
            bool shouldJump = true;

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                playerController.isOnLedge = false;
                StartCoroutine(DoParkourACtion(jumDownAction2));
            }

            if (playerController.LedgeData.height > autoDroppedHeightLimite && !Input.GetButton("Jump"))
            {
                shouldJump = false;
            }

            if (shouldJump && playerController.LedgeData.angle <= 50)
            {
                playerController.isOnLedge = false;
                StartCoroutine(DoParkourACtion(jumDownAction));
            }
        }
    }

    IEnumerator DoParkourACtion(ParkourAction action)
    {

        Debug.Log(action.AnimName);

        playerController.setControl(false);



        MatchTargetParams matchParams = null;
        if (action.EnableTargetMatching)
        {

            matchParams = new MatchTargetParams()
            {
                pos = action.MatchPos,
                bodyPart = action.MatchBodyPart,
                posWeight = action.MatchPosWeight,
                startTime = action.MatchStartTime,
                targetTime = action.MatchTargetTime
            };
        }

        yield return playerController.DoAction(action.AnimName, matchParams,
        action.TargetRotation, action.RotateToObstacle,
        action.PosActionDelay, action.Mirror);

        playerController.setControl(true);

    }

    void MatchTarget(ParkourAction action)
    {
        if (animator.isMatchingTarget) return;

        animator.MatchTarget(action.MatchPos, transform.rotation, action.MatchBodyPart, new MatchTargetWeightMask(action.MatchPosWeight, 0), action.MatchStartTime, action.MatchTargetTime);
    }
}
