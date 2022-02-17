using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using static Extension;

class Poligones
{
    public List<Vector3> point;
    public List<Vector2> uv;
    public float S;
        
    private Poligones(List<Vector3> point, List<Vector2> uv,float S)
    {
        this.point = point ?? throw new ArgumentNullException(nameof(point));
        this.uv = uv ?? throw new ArgumentNullException(nameof(uv));
        this.S = S;
    }

    public static IEnumerable<Poligones> GetPoligons(Area area, IEnumerable<Area> areas)
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


}
