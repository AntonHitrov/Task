using System.Collections.Generic;
using UnityEngine;

class Line
{
    internal readonly float A, B, C, D;
    internal readonly Vector2  v1, v2, Perpendicular, normal;

    internal Line(Vector2 v1, Vector2 v2)
    {
        this.v1 = v1;
        this.v2 = v2;

        D = Vector2.Distance(v1, v2);
        A = (v1.y - v2.y) / D;
        B = (v2.x - v1.x) / D;
        C = ((v1.x * v2.y) - (v2.x * v1.y)) / D;
        normal = (v2 - v1).normalized;
        Perpendicular = Vector2.Perpendicular(v2 - v1).normalized;
    }


    internal IEnumerable<Vector2> Positions() { yield return v1; yield return v2; }

    internal float Scale(Vector2 position) => (A * position.x) + (B * position.y) + C;

    public override string ToString() => $"({v1.x} , {v1.y})  ({v2.x} , {v2.y})";
}
    