using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class PulseLight2D : MonoBehaviour
{
    public float minIntensity = 1.0f;
    public float maxIntensity = 1.8f;
    public float pulseSpeed   = 1.5f;

    private Light2D light2d;

    void Awake()
    {
        light2d = GetComponent<Light2D>();
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        light2d.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
    }
}
