using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhysicsUtil
{
    public static bool ThreeRayCast(Vector3 origin, Vector3 Dir, float spacing, Transform transform,
     out List<RaycastHit> hits, float distacne, LayerMask layer, bool debugDraw = false)
    {
        bool centerHitFound = Physics.Raycast(origin, Vector3.down, out RaycastHit centerHit, distacne, layer);
        bool leftHitFound = Physics.Raycast(origin - transform.right * spacing, Vector3.down, out RaycastHit leftHit, distacne, layer);
        bool rightHitFound = Physics.Raycast(origin + transform.right * spacing, Vector3.down, out RaycastHit rightHit, distacne, layer);

        hits = new List<RaycastHit>() { centerHit, leftHit, rightHit };
        bool hitFound = centerHitFound || leftHitFound || rightHitFound;

        if (debugDraw && hitFound)
        {
            Debug.DrawLine(origin, centerHit.point, Color.red);
            Debug.DrawLine(origin + transform.right * spacing, rightHit.point, Color.red);
            Debug.DrawLine(origin - transform.right * spacing, leftHit.point, Color.red);
        }

        return hitFound;
    }

}
