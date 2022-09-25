using UnityEngine;

public static class Utils {

    public static float DistanceLinePoint(Vector2 lp1, Vector2 lp2, Vector2 p) {
        float d = (Mathf.Abs((lp2.x - lp1.x) * (lp1.y - p.y) - (lp1.x - p.x) * (lp2.y - lp1.y))) / Mathf.Sqrt((float)(Mathf.Pow(lp2.x - lp1.x, 2) + Mathf.Pow(lp2.y - lp1.y, 2)));
        float dlp1lp2 = Vector2.Distance(lp1, lp2);
        float dlp1p = Vector2.Distance(lp1, p);
        float dlp2p = Vector2.Distance(lp2, p);
        if (dlp1p > dlp1lp2 || dlp2p > dlp1lp2) {
            return Mathf.Min(dlp1p, dlp2p);
        } else {
            return d;
        }
    }

    public static Vector3 PointDistanceLinePoint(Vector2 lp1, Vector2 lp2, Vector2 p) {
        float a = lp1.y - lp2.y;
        float b = lp2.x - lp1.x;
        float c = (lp1.x - lp2.x) * lp1.y + (lp2.y - lp1.y) * lp1.x;

        float d = Mathf.Abs(a * p.x + b * p.y + c) / Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
        float x = (b * (b * p.x - a * p.y) - a * c) / (Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
        float y = (a * (-b * p.x + a * p.y) - b * c) / (Mathf.Pow(a, 2) + Mathf.Pow(b, 2));

        float dlp1lp2 = Vector2.Distance(lp1, lp2);
        float dlp1p = Vector2.Distance(lp1, p);
        float dlp2p = Vector2.Distance(lp2, p);

        if (dlp1p > dlp1lp2 || dlp2p > dlp1lp2) {
            if (dlp1p < dlp2p) {
                d = dlp1p;
                x = lp1.x;
                y = lp1.y;
            } else {
                d = dlp2p;
                x = lp2.x;
                y = lp2.y;
            }
        }

        return new Vector3(x, y, d);
    }

    public static bool BoxSphereIntersection(Vector3 boxCorder1, Vector3 boxCorner2, Vector3 spherePosition, float sphereRadius) {
        float x = Mathf.Max(boxCorder1.x, Mathf.Min(spherePosition.x, boxCorner2.x));
        float y = Mathf.Max(boxCorder1.y, Mathf.Min(spherePosition.y, boxCorner2.y));
        float z = Mathf.Max(boxCorder1.z, Mathf.Min(spherePosition.z, boxCorner2.z));

        float distance = Mathf.Sqrt(
            (x - spherePosition.x) * (x - spherePosition.x) +
            (y - spherePosition.y) * (y - spherePosition.y) +
            (z - spherePosition.z) * (z - spherePosition.z)
        );

        return distance < sphereRadius; 
    }

    public static bool SquareCircleIntersection(Vector2 squareCorder1, Vector2 squareCorner2, Vector2 circlePosition, float circleRadius) {
        float x = Mathf.Max(squareCorder1.x, Mathf.Min(circlePosition.x, squareCorner2.x));
        float y = Mathf.Max(squareCorder1.y, Mathf.Min(circlePosition.y, squareCorner2.y));

        float distance = Mathf.Sqrt(
            (x - circlePosition.x) * (x - circlePosition.x) +
            (y - circlePosition.y) * (y - circlePosition.y)
        );

        return distance < circleRadius; 
    }

    public static float DistancePointPlane(Vector3 point, Vector4 plane) {
        return (Mathf.Abs(plane.x * point.x + plane.y * point.y + plane.z * point.z - plane.w)) / (Mathf.Sqrt(Mathf.Pow(plane.x, 2) + Mathf.Pow(plane.y, 2) + Mathf.Pow(plane.z, 2)));
    }

    public static Vector3 PointPointPlane(Vector3 point, Vector4 plane) {
        float k = (plane.x * point.x + plane.y * point.y + plane.z * point.z - plane.w) / (Mathf.Pow(plane.x, 2) + Mathf.Pow(plane.y, 2) + Mathf.Pow(plane.z, 2));
        return new Vector3(point.x - k * plane.x, point.y - k * plane.y, point.z - k * plane.z);
    }

    public static bool AgentInAgentFieldOfView(Agent agent1, Agent agent2, Vector3 r, Vector3 u) {
        if (!agent1.Equals(agent2) && Vector3.Distance(agent1.Position, agent2.Position) <= agent1.PerceptionRadius) {

            float hangle = Mathf.Rad2Deg * Mathf.Acos(
                Vector3.Dot(agent1.Direction, Vector3.ProjectOnPlane(agent2.Position - agent1.Position, u)) / 
                (agent1.Direction.magnitude * (Vector3.ProjectOnPlane(agent2.Position - agent1.Position, u)).magnitude)
            );
            
            float vangle = Mathf.Rad2Deg * Mathf.Acos(
                Vector3.Dot(agent1.Direction, Vector3.ProjectOnPlane(agent2.Position - agent1.Position, r)) / 
                (agent1.Direction.magnitude * (Vector3.ProjectOnPlane(agent2.Position - agent1.Position, r)).magnitude)
            );

            if (hangle <= agent1.HorizontalFOV / 2 && vangle <= agent1.VerticalFOV / 2) {
                return true;
            }
        }

        return false;
    }

}