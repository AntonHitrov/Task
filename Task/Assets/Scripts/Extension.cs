using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

static class Extension
{
    #region Line Intersept

    #region Has Intersept
    public static bool HasIntersept(this Line A, Line B)
        => Intercept(A.v1, A.v2,B.v1,B.v2);
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
    private static float Mult(float ax, float ay, float bx, float by) => (ax * by) - (bx * ay);
    #endregion

    public static Vector2 GetIntersept(this Line A, Line B) => GetIntersept(A.A, A.B, A.C, B.A, B.B, B.C);

    public static Vector2 GetIntersept(float a1,float b1,float c1,float a2,float b2,float c2)
    {
        Vector2 pt = new Vector2();
        var d = (a1 * b2 - b1 * a2);
        var dx = (-c1 * b2 + b1 * c2);
        var dy = (-a1 * c2 + c1 * a2);
        pt.x =(dx / d);
        pt.y = (dy / d);
        return pt;
    }

    #endregion

    #region Sort Vectors
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

        public static float GetAngle(Vector2 A, Vector2 B, Vector2 C)
        {
            Vector2 first = A - B;
            Vector2 second = C - B;
            return Mathf.Atan2(first.y, first.x) - Mathf.Atan2(second.y,second.x);
        }
    }
    #endregion

    #region CreateTriangulares
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
    #endregion

    #region Gaus
    public static float GetGauseArea(Vector2[] arg)
    => (Enumerable.Range(0, arg.Length - 1)
                 .Select(i => (arg[i].x * arg[i + 1].y) - (arg[i].y * arg[i + 1].x))
                 .Sum() + (arg.Last().x * arg[0].y) - (arg.Last().y * arg[0].x)) * 0.5f;
    #endregion

    #region AreaBuilder
    public static Area CreateArea(int column, int line, Vector2 size, float border,float offset,float angle)
    {
        var stepUP = new Vector2(Mathf.Repeat(offset, size.x + border),size.y + border);
        var stepRight = new Vector2(size.x + PlaneRender.borderY,0);
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

    internal static IEnumerable<Area> GetAreas(int startCollumn, int Collumn, int startLine, int Line, Vector2 size, float border, float offset, float angle)
    => Enumerable.Range(startCollumn, Collumn)
                 .SelectMany(collumn => Enumerable.Range(startLine, Line)
                 .Select(line => Extension.CreateArea(collumn, line, size, border, offset, angle)));

    #endregion
}
