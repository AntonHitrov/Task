using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class PlaneRender : MonoBehaviour
{
    public MeshFilter Area;
    public Text SyzeX, SyzeY, Border, Offset, S, BorderY;
    public Slider Angle;

    public Vector2 size;
    public static float borderY;
    public int collumn, line, startCollumn, startLine;
    public float sizeArea,border, offset, angle, Summ;

    private Area DefaultArea => new Area(Vector2.zero,
                                         Vector2.up * 10 * sizeArea, 
                                         ((Vector2.right * 10) + (Vector2.up * 10)) * sizeArea,
                                         Vector2.right * 10 * sizeArea);


    private void Start()
    {
        Border.ObserveEveryValueChanged(x => x.text).Subscribe(_ => UpDateMesh());
        Offset.ObserveEveryValueChanged(x => x.text).Subscribe(_ => UpDateMesh());
        Angle.ObserveEveryValueChanged(x => x.value).Subscribe(_ => UpDateMesh());
        BorderY.ObserveEveryValueChanged(x => x.text).Subscribe(_ => UpDateMesh());
    }

    private void GetData()
    {
        size = Vector2.one;
        float.TryParse(Border.text.Replace('.', ','), out border);
        float.TryParse(Offset.text.Replace('.', ','), out offset);
        float.TryParse(BorderY.text.Replace('.', ','), out borderY);
        angle = Angle.value * Mathf.PI * 2;
    }
   
    public void UpDateMesh()
    {
        GetData();
        IEnumerable<Poligones> poligons = GetPoligons(DefaultArea, GetAreas(startCollumn, collumn, startLine, line, size, border, offset, angle)).Where(x => x != null).ToList();
        AddMesh(poligons);
        Summ = poligons.Select(x => x.S).Sum();
        S.text = $"{Summ}";
    }


    private void AddMesh(IEnumerable<Poligones> poligons)
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(poligons.SelectMany(x => x.point).ToList());
        mesh.SetIndices(Enumerable.Range(0, poligons.SelectMany(x => x.point).Count()).ToArray(), MeshTopology.Triangles, 0);
        mesh.SetUVs(0, poligons.SelectMany(x => x.uv).ToArray());
        Area.mesh = mesh;
    }

    private IEnumerable<Area> GetAreas(int startCollumn, int Collumn, int startLine, int Line, Vector2 size, float border, float offset, float angle) 
        => Enumerable.Range(startCollumn, collumn)
                     .SelectMany(collumn => Enumerable.Range(startLine, Line)
                     .Select(line => Extension.CreateArea(collumn, line, size, border, offset, angle)));

    private IEnumerable<Poligones> GetPoligons(Area area, IEnumerable<Area> areas)
        => areas.Select(target => ExtractPoligons(area, target));

    private static Poligones ExtractPoligons(Area area, Area target)
    {
        List<Vector2> intersepts = area.GetInterseptPoints(target).ToList();
        if (intersepts.Count() == 0)
            return null;
        Vector2 centr = intersepts.GetCentr();
        intersepts = intersepts.Sort(centr).ToList();
        float S = GetGauseArea(intersepts.ToArray());
        List<Vector2> poligons = intersepts.GetPoligons(centr).ToList();
        List<Vector2> uv = poligons.Select(target.GetUV).ToList();
        return new Poligones(poligons.Select(x => new Vector3(x.x, 0, x.y)).ToList(), uv, S);
    }

    private static float GetGauseArea(Vector2[] arg) 
        => (Enumerable.Range(0, arg.Length - 1)
                     .Select(i => (arg[i].x * arg[i + 1].y) - (arg[i].y * arg[i + 1].x))
                     .Sum() + (arg.Last().x * arg[0].y) - (arg.Last().y * arg[0].x))*0.5f ;

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
    