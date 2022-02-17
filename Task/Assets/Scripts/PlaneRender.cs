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
    public float sizeArea, border, offset, angle, Summ;
    #endregion
    #region private

    private IEnumerable<Vector2> DefaultPointsToArea
    {
        get
        {
            yield return Vector2.zero;
            yield return Vector2.up * 10 * sizeArea;
            yield return ((Vector2.right * 10) + (Vector2.up * 10)) * sizeArea;
            yield return Vector2.right * 10 * sizeArea;
        }
    }

    private IEnumerable<Area> DefaultAreas 
        => GetAreas(startCollumn, collumn, startLine, line, size, border, offset, angle);
    #endregion

    private void Start()
    {
        Border.ObserveEveryValueChanged(x => x.text).Subscribe(_ => Handler());
        Offset.ObserveEveryValueChanged(x => x.text).Subscribe(_ => Handler());
        Angle.ObserveEveryValueChanged(x => x.value).Subscribe(_ => Handler());
        BorderY.ObserveEveryValueChanged(x => x.text).Subscribe(_ => Handler());
    }

   
    private void Handler()
    {
        GetParams();
        UpDate(Poligones.GetPoligons(new Area(DefaultPointsToArea), DefaultAreas).ToList());
    }

    private void UpDate(List<Poligones> poligons)
    {
        AddMesh(poligons);
        SetResult(poligons);
    }

    #region Func
    private void GetParams()
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

    private void SetResult(IEnumerable<Poligones> poligons) => S.text = $"{poligons.Select(x => x.S).Sum()}";
    #endregion
}
