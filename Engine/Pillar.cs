using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class Pillar : MonoBehaviour
{
    //1D engine properties
    private Color _color;
    public Color Col => _color;

    /// <summary>
    /// List of the rays hitting the pillar, is used to get the distance from character.
    /// </summary>
    public List<RaycastHit2D> hittingRays = new();
    private float Distance => hittingRays.Count > 0 ? hittingRays.Min(r => r.distance) : float.MaxValue;

    /// <summary>
    /// This list should keep the screen proportions of the strip, always having even count.
    /// </summary>
    public List<float> stripProportions;

    //General properties specific to the Voybit Manuscript
    public bool active;
    public string content;

    //Rendering and UI
    private SpriteRenderer _renderer;
    private List<TMP_Text> _displayTexts = new();
    private const string ARROW_STRING = " | \n | \n";

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void InstantiateTexts(GameObject prefab, Transform parent, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject pf = Instantiate(prefab, parent);
            _displayTexts.Add(pf.GetComponent<TMP_Text>());
        }
    }

    public void ResetProportions()
    {
        hittingRays = new();
        stripProportions = new();
    }

    public void Paint()
    {
        if (active)
            _color = GameManager.Instance.activationColor;
        else
            _color = GameManager.Instance.passiveColor;
        _renderer.color = Col;
    }

    public void SetDisplays(int appearances)
    {
        if (appearances > _displayTexts.Count)
        {
            InstantiateTexts(GameManager.Instance.displayPF, GameManager.Instance.displayParent, appearances - _displayTexts.Count);
        }
        float width = Screen.width;
        float height = Screen.height;
        for (int i = 0; i < _displayTexts.Count; i++)
        {
            if (i < appearances)
            {
                _displayTexts[i].text = ARROW_STRING + $" {content}  \n {Distance:F1} ";
                float xProp = (stripProportions[2 * i] + stripProportions[2 * i + 1]) * 0.5f;
                _displayTexts[i].rectTransform.anchoredPosition = new Vector2(width * (xProp - 0.5f), 0.5f);
            }
            else
            {
                _displayTexts[i].text = "";
            }

        }
    }
}
