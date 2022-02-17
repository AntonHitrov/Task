using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlaneRender : MonoBehaviour
{
    public MeshFilter Area;
    public float sizeArea;
    public Vector2 size;
    public float border,offset,angle;
    public int collumn, line, startCollumn, startLine;
    public float Summ;

    public Text SyzeX, SyzeY, Border, Offset, S;
    public Slider Angle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void GetData()
    {
        size = Vector2.one;
       // float.TryParse(SyzeX.text.Replace('.',','), out size.x);
       // float.TryParse(SyzeY.text.Replace('.', ','), out size.y);

        float.TryParse(Border.text.Replace('.', ','), out border);
        float.TryParse(Offset.text.Replace('.', ','), out offset);

        angle = Angle.value * Mathf.PI * 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Button]
    public void Test()
    {
        GetData();
        var area = new Area(Vector2.zero, Vector2.up * 10 * sizeArea, ((Vector2.right * 10) + (Vector2.up * 10)) * sizeArea, Vector2.right * 10 * sizeArea);
        IEnumerable<Poligones> poligons = GetPoligons(area,GetAreas(startCollumn, collumn, startLine, line, size, border, offset, angle)).Where(x=> x != null).ToList();
        var mesh = new Mesh();
        mesh.SetVertices(poligons.SelectMany(x => x.point).ToList());
        mesh.SetIndices(Enumerable.Range(0, poligons.SelectMany(x => x.point).Count()).ToArray(), MeshTopology.Triangles, 0);
        mesh.SetUVs(0, poligons.SelectMany(x => x.uv).ToArray());
        Area.mesh = mesh;
        Summ = poligons.Select(x => x.S).Sum() * 0.5f;
        S.text = $"{Summ}";
    }

    private IEnumerable<Area> GetAreas(int startCollumn, int Collumn, int startLine, int Line, Vector2 size, float border, float offset, float angle) 
        => Enumerable.Range(startCollumn, collumn).SelectMany(collumn => Enumerable.Range(startLine, Line).Select(line => Extension.GetArea(collumn, line, size, border, offset, angle)));

    private IEnumerable<Poligones> GetPoligons(Area area, IEnumerable<Area> areas)
    {
        return areas.Select(target =>
        {
            var intersepts = area.GetInterseptPoints(target).ToList();
            if (intersepts.Count() == 0)
                return null;
            var centr = intersepts.GetCentr();
            intersepts = intersepts.Sort(centr).ToList();
            var arg = intersepts.ToArray();
            var S = Enumerable.Range(0, arg.Length - 1).Select(i => (arg[i].x * arg[i + 1].y) - (arg[i].y * arg[i + 1].x)).Sum() + (arg.Last().x * arg[0].y) - (arg.Last().y * arg[0].x);
            

            var poligons = intersepts.GetPoligons(centr).ToList();
            var uv = poligons.Select(target.GetUV).ToList();
            
            return new Poligones(poligons.Select(x => new Vector3(x.x, 0, x.y)).ToList(),uv,S);
        });

    }

    private class Poligones
    {
        public List<Vector3> point;
        public List<Vector2> uv;
        public float S;
        
        public Poligones(List<Vector3> point, List<Vector2> uv,float S)
        {
            this.point = point ?? throw new ArgumentNullException(nameof(point));
            this.uv = uv ?? throw new ArgumentNullException(nameof(uv));
            this.S = S;
            
        }

       
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
        var f_intersept = lines.SelectMany(line => area.lines.Where(line.HasIntersept).Select(line.GetIntersept)/*.Where(pointInArea)*/);
        var s_intersept = area.lines.SelectMany(line => lines.Where(line.HasIntersept).Select(line.GetIntersept)/*.Where(pointInArea)*/);
        return f.Concat(s).Concat(f_intersept).Concat(s_intersept);
    }

    public Vector2 GetUV(Vector2 point) => new Vector2(A.Scale(point) / D.D, D.Scale(point) / A.D);

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
        => Intercept(A.v1, A.v2,B.v1,B.v2);
    

    public static Vector2 GetIntersept(this Line A, Line B) => GetIntersept(A.A, A.B, A.C, B.A, B.B, B.C);

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
        var stepUP = new Vector2(Mathf.Repeat(offset, size.x + border),size.y + border);
        var stepRight = new Vector2(size.x + border,0);
        var points = positions(size).Select(point => point + (stepUP * line) + (stepRight * column)).ToList();
       
        return new Area(points.Select(point => Rotate(angle, point, Vector2.zero)));
    }

    private static IEnumerable<Vector2> positions(Vector2 size)
    {
        yield return Vector2.zero;
        yield return Vector2.up * size.y;
        yield return size;
        yield return Vector2.right * size.x;
    }

    private static Vector2 Rotate(float angle, Vector2 target,Vector2 pivot)
    {
        return new Vector2(((target.x - pivot.x) * Mathf.Cos(angle) - (target.y - pivot.y) * Mathf.Sin(angle)) + pivot.x, 
                           ((target.x - pivot.x) * Mathf.Sin(angle) + (target.y - pivot.y) * Mathf.Cos(angle)) + pivot.y);
    }

    static Vector2 GetIntersept(float a1,float b1,float c1,float a2,float b2,float c2)
    {
        Vector2 pt = new Vector2();
        var d = (a1 * b2 - b1 * a2);
        var dx = (-c1 * b2 + b1 * c2);
        var dy = (-a1 * c2 + c1 * a2);
        pt.x =(dx / d);
        pt.y = (dy / d);
        return pt;
    }

    private static float Mult(float ax, float ay, float bx, float by) => (ax * by) - (bx * ay);
    public static bool Intercept(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float v1 = Mult(p4.x - p3.x, p4.y - p3.y, p1.x - p3.x, p1.y - p3.y);
        float v2 = Mult(p4.x - p3.x, p4.y - p3.y, p2.x - p3.x, p2.y - p3.y);
        float v3 = Mult(p2.x - p1.x, p2.y - p1.y, p3.x - p1.x, p3.y - p1.y);
        float v4 = Mult(p2.x - p1.x, p2.y - p1.y, p4.x - p1.x, p4.y - p1.y);
        if ((v1 * v2) < 0 && (v3 * v4) < 0)
            return true;
        return false;
    }
}
    