using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HP_barController : MonoBehaviour
{
    
public float maxHP = 100f;
    public float currentHP = 100f;

    private VisualElement hpFill;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        hpFill = root.Q<VisualElement>("HPFill");
        UpdateHPBar();
    }

    public void UpdateHPBar()
    {
        float hpPercent = currentHP / maxHP;
        hpFill.style.width = new Length(hpPercent * 100, LengthUnit.Percent);
    }

}
