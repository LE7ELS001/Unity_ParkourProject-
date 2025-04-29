using UnityEngine;



[CreateAssetMenu(menuName = "Parkour System/Custom Actions/new vault action")]



public class VaultAction : ParkourAction
{
    [Header("Offset")]
    [Range(0f, 1f)]
    [SerializeField] float centerOffsetPercent = 0.2f;

    [Header("Animation Names")]
    [SerializeField] string[] outerVaultAnims = { "VaultFence", "VaultFence1-2" };
    [SerializeField] string[] middleVaultAnims = { "VaultFence2" };
    public override bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        if (!base.CheckIfPossible(hitData, player))
        {
            return false;
        }

        var hitPoint = hitData.forwardHit.transform.InverseTransformPoint(hitData.forwardHit.point);
        float obstacleWidth = hitData.forwardHit.collider.bounds.size.x;

        float xOffsetPercent = Mathf.Abs(hitPoint.x) / (obstacleWidth * 0.5f);

        //middle range 
        if (xOffsetPercent <= centerOffsetPercent)
        {
            int rand = UnityEngine.Random.Range(0, middleVaultAnims.Length);
            string selectName = middleVaultAnims[rand];
            animName = selectName;
            switch (selectName)
            {
                case "VaultFence2":
                    matchStartTime = 0.03f;
                    matchTargetTime = 0.52f;
                    matchBodyPart = AvatarTarget.LeftHand;

                    break;

            }
        }
        else
        {
            int rand = UnityEngine.Random.Range(0, outerVaultAnims.Length);
            string selectName = outerVaultAnims[rand];
            animName = selectName;
            if (hitPoint.z < 0 && hitPoint.x < 0 || hitPoint.z > 0 && hitPoint.x > 0)
            {

                //mirror animation 
                Mirror = true;
                switch (animName)
                {
                    case "VaultFence":
                        matchStartTime = 0.09f;
                        matchTargetTime = 0.347f;
                        matchBodyPart = AvatarTarget.RightHand;
                        break;
                    case "VaultFence1-2":
                        matchStartTime = 0.056f;
                        matchTargetTime = 0.214f;
                        matchBodyPart = AvatarTarget.RightHand;
                        break;
                }
            }
            else
            {
                //normal animation
                Mirror = false;


                switch (animName)
                {
                    case "VaultFence":

                        matchStartTime = 0.09f;
                        matchTargetTime = 0.347f;
                        matchBodyPart = AvatarTarget.LeftHand;
                        break;

                    case "VaultFence1-2":
                        matchStartTime = 0.056f;
                        matchTargetTime = 0.214f;
                        matchBodyPart = AvatarTarget.LeftHand;

                        break;
                }
            }


        }



        return true;
    }
}
