using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

class Area
{
    public readonly Line A, B, C, D;
    public readonly Vector2 a, b, c, d;
    public Vector2 Center => (a + b + c + d) / 4;

    public IEnumerable<Vector2> points { get { yield return a; yield return b; yield return c; yield return d; } }
    public IEnumerable<Line> lines { get { yield return A; yield return B; yield return C; yield return D; } }

    public Area(IEnumerable<Vector2> points)
    {
        var list = points.ToArray();
        this.a = list[0];
        this.b = list[1];
        this.c = list[2];
        this.d = list[3];
        A = new Line(a, b);
        B = new Line(b, c);
        C = new Line(c, d);
        D = new Line(d, a);
    }

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

    public IEnumerable<Vector2> GetInterseptPoints(Area area) 
        => points.Where(area.pointInArea)
                 .Concat(area.points.Where(pointInArea))
                 .Concat(lines.SelectMany(line => area.lines.Where(line.HasIntersept).Select(line.GetIntersept)))
                 .Concat(area.lines.SelectMany(line => lines.Where(line.HasIntersept).Select(line.GetIntersept)));

    public Vector2 GetUV(Vector2 point) => new Vector2(A.Scale(point) / D.D, D.Scale(point) / A.D);

    public bool pointInArea(Vector2 point) => !lines.Any(line => line.Scale(point) > 0);
}
    