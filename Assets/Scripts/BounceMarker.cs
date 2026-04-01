using UnityEngine;

public class BounceMarker : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private Renderer m_renderer;
    [SerializeField] private Material correctMat;
    [SerializeField] private Material incorrectMat;

    [Header("parameters")]
    [SerializeField] private float radius = .25f;
    [SerializeField] private LayerMask layerMask;

    bool correct;
    public bool Correct => correct;

    private void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask);

        if(colliders.Length == 0 )
        {
            correct = false;
        }
        else
        {
            correct = true;
        }

        if(correct) 
            m_renderer.material = correctMat;
        else
            m_renderer.material = incorrectMat;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = correct ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
