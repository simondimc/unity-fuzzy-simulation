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

    public static (float, float) Angle(Vector3 x, Vector3 z, Vector3 y, Vector3 v) {
        Vector3 p = new Vector3(
            x.x * v.x + x.y * v.y + x.z * v.z,
            y.x * v.x + y.y * v.y + y.z * v.z,
            z.x * v.x + z.y * v.y + z.z * v.z
        );

        float a_y = Mathf.Atan2(p.z, p.x);
        a_y *= -1;

        float a_z = Mathf.Atan2(p.y, p.x);

        a_y = Mathf.Rad2Deg * a_y;
        a_z = Mathf.Rad2Deg * a_z;

        return (a_y, a_z);
    }

    public static bool AgentInAgentFieldOfView(Agent agent1, Agent agent2, Vector3 x, Vector3 z, Vector3 y) {
        if (!agent1.Equals(agent2) && Vector3.Distance(agent1.Position, agent2.Position) <= agent1.PerceptionRadius) {
            var (a_y, a_z) = Utils.Angle(x, z, y, agent2.Position - agent1.Position); 
            if (Mathf.Abs(a_y) <= agent1.HorizontalFOV / 2 && Mathf.Abs(a_z) <= agent1.VerticalFOV / 2) {
                return true;
            }
        }
        return false;
    }

}