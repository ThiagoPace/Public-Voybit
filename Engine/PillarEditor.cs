using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Pillar))]
public class PillarEditor : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Pillar _pillar;
    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _pillar = GetComponent<Pillar>();
    }
    private void Update()
    {
        if (_pillar.active)
        {
            _renderer.color = Color.black;
        }
        else
        {
            _renderer.color = Color.white;
        }
    }

}
