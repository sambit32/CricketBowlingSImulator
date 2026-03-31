using UnityEngine;
using System.Collections.Generic;

public class BounceMarkerController : MonoBehaviour
{
    [SerializeField] private Renderer m_Renderer;
    [SerializeField] private List<Material> materials;
    private int currentIndex = 0;

    private void Start()
    {
        currentIndex = 0;
        m_Renderer.material = materials[currentIndex];
    }

    private void ChangeMaterial()
    {
        if (currentIndex == 0)
            currentIndex = 1;
        else
            currentIndex = 0;

        m_Renderer.material = materials[currentIndex];
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            ChangeMaterial();
        }
    }
}