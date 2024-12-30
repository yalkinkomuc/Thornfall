using UnityEngine;

public class MovementRangeVisualizer : MonoBehaviour
{
    [SerializeField] private MeshRenderer rangeMeshRenderer;
    [SerializeField] private float rangeVisualizerHeight = 0.1f;
    [SerializeField] private Color rangeColor = new Color(0, 0.5f, 1f, 0.3f);
    private MoveAction moveAction;

    private void Awake()
    {
        moveAction = GetComponent<MoveAction>();
        CreateRangeMesh();
    }

    private void CreateRangeMesh()
    {
        GameObject rangeObject = new GameObject("MovementRange");
        rangeObject.transform.SetParent(transform);
        rangeObject.transform.localPosition = Vector3.zero;

        MeshFilter meshFilter = rangeObject.AddComponent<MeshFilter>();
        rangeMeshRenderer = rangeObject.AddComponent<MeshRenderer>();

        // Disk mesh oluştur
        Mesh mesh = new Mesh();
        int segments = 32;
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / segments) * Mathf.PI * 2;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        // URP/HDRP uyumlu shader ve material
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (material.shader == null)
        {
            // Fallback to built-in shader if URP is not available
            material = new Material(Shader.Find("Standard"));
        }

        material.SetFloat("_Surface", 1); // Transparent
        material.SetFloat("_Blend", 0);   // Alpha blend
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        material.color = rangeColor;

        rangeMeshRenderer.material = material;
        rangeMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rangeMeshRenderer.receiveShadows = false;
        
        rangeObject.transform.localPosition = new Vector3(0, rangeVisualizerHeight, 0);
        rangeObject.SetActive(false);

        // Layer ayarı
        rangeObject.layer = LayerMask.NameToLayer("Default") ;// veya başka uygun bir layer
    }

    public void ShowRange(float radius)
    {
        if (rangeMeshRenderer != null)
        {
            rangeMeshRenderer.transform.localScale = Vector3.one * radius * 2;
            rangeMeshRenderer.gameObject.SetActive(true);
        }
    }

    public void HideRange()
    {
        if (rangeMeshRenderer != null)
        {
            rangeMeshRenderer.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (rangeMeshRenderer != null)
        {
            Destroy(rangeMeshRenderer.material);
        }
    }
} 