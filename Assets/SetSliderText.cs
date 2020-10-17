using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetSliderText : MonoBehaviour
{
    public Text text;
    public float value;
    public HoopSpawner hoopSpawner;

    private void Start()
    {
        value = GetComponent<Slider>().value;
    }
    public void SetSlider()
    {
        text.text = GetComponent<Slider>().value.ToString();
        value = GetComponent<Slider>().value;
    }
}
