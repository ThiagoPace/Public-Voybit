using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisionHandler : MonoBehaviour
{
    //Necessary components
    [SerializeField] private Character2D _character;
    private IEnumerable<IVisualCaller> _callers;
    private Material _stripeMat;
    private Pillar[] _scenePillars;

    [Header("Vision properties")]
    [SerializeField] private float _visionAmplitude = 1.5f;
    [SerializeField] private int _rayAmount = 200;

    //Output properties
    private const float MAX_RAYCAST_DIST = 100f;
    private const int REL_DEL_SIZE = 100;
    private const int COL_SIZE = 50;

    /// <summary>
    /// Array of colors in the order they appear, from left to right.
    /// </summary>
    private Color[] _colorsArray;

    /// <summary>
    /// Array of strip proportions in the screen, from left to right. Values go from 0 to 1, and each strip is represented 
    /// as [start, end]. It has a fixed size of REL_DEL_SIZE, but the actually read region must have lenght 
    /// equal to two times the read region of _colorsArray.
    /// </summary>
    private float[] _relativeDelimitations;


    private void Start()
    {
        _stripeMat = GetComponent<MeshRenderer>().material;

        _relativeDelimitations = new float[REL_DEL_SIZE];
        _colorsArray = new Color[COL_SIZE];

        _scenePillars = FindObjectsOfType<Pillar>();
        _callers = FindObjectsOfType<MonoBehaviour>().OfType<IVisualCaller>();
        foreach (IVisualCaller caller in _callers)
            caller.VisualEvent += OnUpdateEvent;

        UpdateVisuals();
    }


    #region 1D visualization methods

    private void OnUpdateEvent(object sender, EventArgs e)
    {
        UpdateVisuals();
    }
    private void UpdateVisuals()
    {
        SetPillars();
        SetShader();
    }

    private void SetPillars()
    {
        List<Pillar> nonConsec = GetNonConsecPillars();

        foreach (Pillar p in _scenePillars)
        {
            p.SetDisplays(nonConsec.Count(pillar => pillar == p));
            p.Paint();
        }
    }

    public void SetShader()
    {
        SetRelativeDelimitations();
        _stripeMat.SetFloatArray("_RelativeDels", _relativeDelimitations);
        _stripeMat.SetInt("_RelDelsLength", _relativeDelimitations.Length);
        _stripeMat.SetColorArray("_StripColors", _colorsArray);
    }

    private void SetRelativeDelimitations()
    {
        List<Color> cols = GetPillarColors();
        int colCount = cols.Count;
        List<float> relDelims = new() { 0f };
        List<Color> colDelims = new() { cols[0] };

        Color currentColor = cols[0];
        for (int i = 0; i < colCount; i++)
        {
            if (cols[i] != currentColor)
            {
                relDelims.Add((float)i / _rayAmount);
                relDelims.Add((float)i / _rayAmount);
                colDelims.Add(cols[i]);
            }
            currentColor = cols[i];
        }
        relDelims.Add(1f);
        SetRelDelims(relDelims);
        SetColDelims(colDelims);
    }

    private void SetColDelims(List<Color> colDelims)
    {
        for (int i = 0; i < COL_SIZE; i++)
        {
            if (i < colDelims.Count)
                _colorsArray[i] = colDelims[i];
            else
                _colorsArray[i] = Color.black;
        }
    }
    private void SetRelDelims(List<float> relDelims)
    {
        for (int i = 0; i < REL_DEL_SIZE; i++)
        {
            if (i < relDelims.Count)
                _relativeDelimitations[i] = relDelims[i];
            else
                _relativeDelimitations[i] = 0f;

        }
    }

    /// <summary>
    /// If possible, consider using GetNonConsecPillars instead.
    /// </summary>
    public List<Pillar> GetDistinctPillars() { return GetPillars().Distinct().ToList(); }

    /// <summary>
    /// This method allows repetition under the condition of non consecutivity. Allows for free use of pillar's scale, as in 
    /// the case of "T-blocks".
    /// </summary>
    public List<Pillar> GetNonConsecPillars()
    {
        List<Pillar> pillars = GetPillars();
        List<Pillar> nonConsec = new() { pillars[0]};
        Pillar currentPillar = pillars[0];

        for (int i = 0; i < pillars.Count; i++)
        {
            if (pillars[i] != currentPillar)
            {
                nonConsec.Add(pillars[i]);
            }
            currentPillar = pillars[i];
        }
        return nonConsec;
    }

    private List<Color> GetPillarColors() { return GetPillars().Select(p => p.Col).ToList(); }

    /// <summary>
    /// Gets all pillars in view from left to right. Each pillar takes 1/_rayAmount of the final view.
    /// </summary>
    private List<Pillar> GetPillars()
    {
        List<Pillar> delimitations = new();

        for (int i = 0; i < _rayAmount; i++)
        {
            Ray2D ray = new Ray2D(_character.Position, GetRayDir(i));
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, MAX_RAYCAST_DIST, ~_character.characterLayer);

            if (hit.collider)
            {
                Pillar hitPillar = hit.collider.GetComponent<Pillar>();
                if (hitPillar)
                {
                    if (!delimitations.Contains(hitPillar))
                    {
                        hitPillar.ResetProportions();
                    }
                    delimitations.Add(hitPillar);
                    hitPillar.hittingRays.Add(hit);
                }
            }
        }
        SetPillarsProportions(delimitations);
        return delimitations;
    }

    /// <summary>
    /// Returns the direction of the i-th ray given the chosen _rayAmount, from left to right.
    /// </summary>
    private Vector2 GetRayDir(int i)
    {
        float rightPos = i / (_rayAmount - 1f);
        float rightAmount = _visionAmplitude * (rightPos - 0.5f);
        return _character.Forward + _character.Right * rightAmount;
    }

    private void SetPillarsProportions(List<Pillar> pillars)
    {
        Pillar currentPillar = pillars[0];
        float lastStart = 0f;
        for (int i = 0; i < pillars.Count; i++)
        {
            if (pillars[i] != currentPillar)
            {
                currentPillar.stripProportions.Add(lastStart);
                currentPillar.stripProportions.Add((float)i / _rayAmount);
                lastStart = (float)i / _rayAmount;
            }
            currentPillar = pillars[i];
        }
        currentPillar.stripProportions.Add(lastStart);
        currentPillar.stripProportions.Add(1f);
    }

    #endregion

    #region Voybit methods
    /// <summary>
    /// This method returns a list of islands- i.e., consecutive active pillars. This is fundamental
    /// for the Voybit Manuscript, but not for a pure 1D engine implementation.
    /// </summary>
    public List<List<Pillar>> GetActiveIslands()
    {
        List<Pillar> ps = GetNonConsecPillars();
        List<List<Pillar>> islands = new();
        List<Pillar> currentIsland = new();
        for (int i = 0; i < ps.Count; i++)
        {
            if (ps[i].active)
            {
                currentIsland.Add(ps[i]);
                if (i < ps.Count - 1 && !ps[i + 1].active)
                {
                    islands.Add(currentIsland);
                    currentIsland = new();
                }
                if (i == ps.Count - 1 && currentIsland.Count > 0)
                {
                    islands.Add(currentIsland);
                }
            }
        }
        return islands;
    }
    #endregion

    private void OnDrawGizmos()
    {
        for (int i = 0; i <= _rayAmount; i++)
        {
            Gizmos.DrawRay(_character.Position, GetRayDir(i) * 10f);
            Gizmos.color = new Color(1, 1, 1, .1f);
        }
    }
}
