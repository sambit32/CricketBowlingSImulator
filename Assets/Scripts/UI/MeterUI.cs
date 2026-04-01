using UnityEngine;
using UnityEngine.UI;

public class MeterUI : MonoBehaviour
{
    [SerializeField] private Image meterFillImage;

    public void UpdateMeter(float value)
    {
        meterFillImage.fillAmount = value;
    }
}
