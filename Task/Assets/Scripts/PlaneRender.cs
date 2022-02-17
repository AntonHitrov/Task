using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using static Extension;

public class PlaneRender : MonoBehaviour
{
    
    #region GUI
    public MeshFilter Area;
    public Text SyzeX, SyzeY, Border, Offset, S, BorderY;
    public Slider Angle;
    #endregion
    #region public
    public Vector2 size;
    public static float borderY;
    public int collumn, line, startCollumn, startLine;
    public float sizeArea,border, offset, angle, Summ;
    #endregion
    #region private

    private Area DefaultArea => new Area(Vector2.zero,
                                         Vector2.up * 10 * sizeArea, 
                                         ((Vector2.right * 10) + (Vector2.up * 10)) * sizeArea,
                                         Vector2.right * 10 * sizeArea);
    private List<Poligones> CreatePoligones 
        => Poligones.GetPoligons(DefaultArea, DefaultAreas)
                    .Where(x => x != null)
                    .ToList();

    private IEnumerable<Area> DefaultAreas 
        => GetAreas(startCollumn, collumn, startLine, line, size, border, offset, angle);
    #endregion

    private void Start()
    {
        Border.ObserveEveryValueChanged(x => x.text).Subscribe(_ => UpDateMesh());
        Offset.ObserveEveryValueChanged(x => x.text).Subscribe(_ => UpDateMesh());
        Angle.ObserveEveryValueChanged(x => x.value).Subscribe(_ => UpDateMesh());
        BorderY.ObserveEveryValueChanged(x => x.text).Subscribe(_ => UpDateMesh());
    }

   
    private void UpDateMesh()
    {
        GetData();
        IEnumerable<Poligones> poligons = CreatePoligones;
        AddMesh(poligons);
        Summ = poligons.Select(x => x.S).Sum();
        S.text = $"{Summ}";
    }

    private void GetData()
    {
        size = Vector2.one;
        float.TryParse(Border.text.Replace('.', ','), out border);
        float.TryParse(Offset.text.Replace('.', ','), out offset);
        float.TryParse(BorderY.text.Replace('.', ','), out borderY);
        angle = Angle.value * Mathf.PI * 2;
    }


    private void AddMesh(IEnumerable<Poligones> poligons)
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(poligons.SelectMany(x => x.point).ToList());
        mesh.SetIndices(Enumerable.Range(0, poligons.SelectMany(x => x.point).Count()).ToArray(), MeshTopology.Triangles, 0);
        mesh.SetUVs(0, poligons.SelectMany(x => x.uv).ToArray());
        Area.mesh = mesh;
    }

 /*   internal IEnumerable<Area> GetAreas(int startCollumn, int Collumn, int startLine, int Line, Vector2 size, float border, float offset, float angle) 
        => Enumerable.Range(startCollumn, collumn)
                     .SelectMany(collumn => Enumerable.Range(startLine, Line)
                     .Select(line => Extension.CreateArea(collumn, line, size, border, offset, angle)));*/
}
