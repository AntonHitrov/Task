using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlaneRender : MonoBehaviour
{
    public MeshFilter Area;
    public Vector2 size;
    public float border,offset,angle;
    public int collumn, line, startCollumn, startLine;
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
        var area = new Area(Vector2.zero, Vector2.up * 10, (Vector2.right * 10) + (Vector2.up * 10), Vector2.right * 10);
        IEnumerable<Vector2> poligons = GetPoligons(area,GetAreas(startCollumn, collumn, startLine, line, size, border, offset, angle));
        var mesh = new Mesh();
        mesh.SetVertices(poligons.Select(x => new Vector3(x.x, 0, x.y)).ToList());
        mesh.SetIndices(Enumerable.Range(0, poligons.Count()).ToArray(), MeshTopology.Triangles, 0);
        Area.mesh = mesh;
    }

    private IEnumerable<Area> GetAreas(int startCollumn, int Collumn, int startLine, int Line, Vector2 size, float border, float offset, float angle) 
        => Enumerable.Range(startCollumn, collumn).SelectMany(collumn => Enumerable.Range(startLine, Line).Select(line => Extension.GetArea(collumn, line, size, border, offset, angle)));

    private IEnumerable<Vector2> GetPoligons(Area area, IEnumerable<Area> areas)
    {
        return areas.SelectMany(target =>
        {
            var intersepts = area.GetInterseptPoints(target).ToList();
            if (intersepts.Count() == 0)
                return Enumerable.Empty<Vector2>();
            var centr = intersepts.GetCentr();
            intersepts = intersepts.Sort(centr).ToList();
            var poligons = intersepts.GetPoligons(centr);
            
            poligons.Select(x => new Vector3(x.x, 0, x.y));
            return poligons;
        });
    }

    private IEnumerable<Vector2> GetPoligons(Area area,int collumn,int line,Vector2 size,float border, float offset, float angle)
    {
        var target = Extension.GetArea(collumn, line, size, border, offset, angle);
        var intersepts = area.GetInterseptPoints(target).ToList();
        var centr = intersepts.GetCentr();
        intersepts = intersepts.Sort(centr).ToList();
        var poligons = intersepts.GetPoligons(centr);
        poligons.ToList().ForEach(x => Debug.Log(x));
        poligons.Select(x => new Vector3(x.x, 0, x.y));
        return poligons;
    }
}


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

    public bool Includet => pointInArea(Center);

    public IEnumerable<Vector2> GetInterseptPoints(Area area)
    {
        var f = points.Where(area.pointInArea);
        var s = area.points.Where(pointInArea);
        var f_intersept = lines.SelectMany(line => area.lines.Where(line.HasIntersept).Select(line.GetIntersept));
        var s_intersept = area.lines.SelectMany(line => lines.Where(line.HasIntersept).Select(line.GetIntersept));
        return f.Concat(s).Concat(f_intersept).Concat(s_intersept);
    }

    public bool pointInArea(Vector2 point) => A.Scale(point) <= 0 && B.Scale(point) <= 0 && C.Scale(point) <= 0 && D.Scale(point) <= 0;
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

    public static bool HasIntersept(this Line A, Line B) 
        => ((GetAngle(B.v1, A.v1, A.v2) >= 0.0f) == (GetAngle(B.v2, A.v1, A.v2) <= 0.0f))
        && ((GetAngle(A.v1, B.v1, B.v2) >= 0.0f) == (GetAngle(A.v2, B.v1, B.v2) <= 0.0f));

    public static Vector2 GetIntersept(this Line A, Line B)
    {
        float scale =  B.Scale(A.v1);
        return B.PointInLine(A.v1);
        Vector2 perpendicular = ((-scale) * B.Perpendicular);
        var pointInLine = A.v1 + perpendicular;
        return pointInLine + (B.normal * (perpendicular.magnitude * Mathf.Tan(GetAngle(A.v2,A.v1, pointInLine))));
    }

    public static Vector2 PointInLine(this Line line, Vector2 input)
    {
        var pointInLine = (line.Perpendicular * (-line.Scale(input))) + input;
        return Vector2.Lerp(line.Centr, pointInLine, 1.0f / (Vector2.Distance(line.Centr, pointInLine) / (line.D * 0.5f)));
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

    public static Vector2 GetCentr(this IEnumerable<Vector2> points)
    {
        var list = points.ToList();
        return list.Aggregate((a, b) => a + b) / list.Count;
    }

    public static IEnumerable<Vector2> GetPoligons(this IEnumerable<Vector2> points, Vector2 centr)
    {
        IEnumerator<Vector2> enumerator = points.GetEnumerator();
        Vector2 last = points.Last();
        while (enumerator.MoveNext())
        {
            yield return centr;
            yield return last;
            yield return last = enumerator.Current;

        }
    }

    public static Area GetArea(int column, int line, Vector2 size, float border,float offset,float angle)
    {
        var stepUP = new Vector2(size.y + border, Mathf.Repeat(offset, size.x + border));
        var stepRight = new Vector2(0, size.x + border);
        return new Area(positions(size).Select(point => point + (stepUP * line) + (stepRight * column)).Select(point => Rotate(angle, point)));
    }

    private static IEnumerable<Vector2> positions(Vector2 size)
    {
        yield return Vector2.zero;
        yield return Vector2.up * size.y;
        yield return size;
        yield return Vector2.right * size.x;
    }

    private static Vector2 Rotate(float angle, Vector2 target) 
        => new Vector2(target.x * Mathf.Cos(angle) - target.y * Mathf.Sin(angle), target.x * Mathf.Sin(angle) + target.y * Mathf.Cos(angle));
}
    