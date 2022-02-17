using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlaneRender : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Button]
    void Test()
    {
        var first = new Line(Vector2.up,Vector2.down);
        var second = new Line(Vector2.left + Vector2.down, Vector2.right + Vector2.down);

        Debug.Log(Extension.GetAngle(Vector2.left, Vector2.up, Vector2.right));
        Debug.Log(Extension.GetAngle(Vector2.up, Vector2.left, Vector2.down));

        Debug.Log(Extension.HasIntersept(first,second));
        Debug.Log(Extension.GetIntersept(first, second));
    }
}


class Area
{
    public readonly Line A, B, C, D;
    public readonly Vector2 a, b, c, d;
    public Vector2 Center => (a + b + c + d) / 4;

    public Area(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
        A = new Line(a, b);
        B = new Line(b, c);
        C = new Line(c, d);
        D = new Line(d, a);
    }

    public bool Includet => pointInArea(Center);

    public bool pointInArea(Vector2 point) => A.Scale(point) <= 0 && B.Scale(point) <= 0 && C.Scale(point) <= 0;

   

}

class Line
{
    public readonly float A, B, C, D;
    public readonly Vector2 Centr, v1, v2, Perpendicular, normal;

    public Line(Vector2 v1, Vector2 v2)
    {
        this.v1 = v1;
        this.v2 = v2;

        D = Vector2.Distance(v1, v2);
        A = (v1.y - v2.y) / D;
        B = (v2.x - v1.x) / D;
        C = ((v1.x * v2.y) - (v2.x * v1.y)) / D;
        Centr = Vector2.Lerp(v1, v2, 0.5f);
        normal = (v2 - v1).normalized;
        Perpendicular = Vector2.Perpendicular(v2 - v1).normalized;
    }
   

    public IEnumerable<Vector2> Positions() { yield return v1; yield return v2; }

    public float Scale(Vector2 position) => (A * position.x) + (B * position.y) + C;

    public override string ToString() => $"({v1.x} , {v1.y})  ({v2.x} , {v2.y})";
}

static class Extension
{
    public static float GetAngle(Vector2 A, Vector2 B, Vector2 C)
    {
        Vector2 first = A - B;
        Vector2 second = C - B;
        return Mathf.Atan2(first.y, first.x) - Mathf.Atan2(second.y,second.x);
    }

    public static bool HasIntersept(Line A, Line B) 
        => ((GetAngle(B.v1, A.v1, A.v2) >= 0.0f) == (GetAngle(B.v2, A.v1, A.v2) <= 0.0f))
        && ((GetAngle(A.v1, B.v1, B.v2) >= 0.0f) == (GetAngle(A.v2, B.v1, B.v2) <= 0.0f));

    public static Vector2 GetIntersept(Line A, Line B)
    {
        float scale =  B.Scale(A.v1);
        Vector2 perpendicular = ((-scale) * B.Perpendicular);
        var pointInLine = A.v1 + perpendicular;
        return pointInLine + (B.normal * (perpendicular.magnitude * Mathf.Tan(GetAngle(A.v2,A.v1, pointInLine))));
    }

    public static IEnumerable<Vector2> Sort(this List<Vector2> points, Vector2 centr)
    {
        points.Sort(new Comparer(centr));
        return points;
    }

    private class Comparer: IComparer<Vector2>
    {
        readonly Vector2 centr;
        public Comparer(Vector2 centr) => this.centr = centr;
        int IComparer<Vector2>.Compare(Vector2 x, Vector2 y) => GetAngle(x, centr, y) >= 0 ? 1 : -1;
    }

}
    