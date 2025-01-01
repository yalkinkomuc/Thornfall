using UnityEngine;
using System;

public class MovementRangeVisualizer : MonoBehaviour
{
    [SerializeField] private MeshRenderer rangeMeshRenderer;
    [SerializeField] private float rangeVisualizerHeight = 0.1f;
    [SerializeField] private Color rangeColor = new Color(0, 0.5f, 1f, 0.3f);
    private MoveAction moveAction;

    private void Awake()
    {
        moveAction = GetComponent<MoveAction>();
        if (moveAction == null)
        {
            Debug.LogError("MoveAction component not found on " + gameObject.name);
        }
        CreateRangeMesh();
    }

    private void CreateRangeMesh()
    {
        GameObject rangeObject = new GameObject("MovementRange");
        rangeObject.transform.SetParent(null);
        rangeObject.transform.position = transform.position;

        MeshFilter meshFilter = rangeObject.AddComponent<MeshFilter>();
        rangeMeshRenderer = rangeObject.AddComponent<MeshRenderer>();

        // Disk mesh oluştur
        Mesh mesh = new Mesh();
        int segments = 32;
        
        // Üst ve alt yüzey için vertex sayısını iki katına çıkar
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        int[] triangles = new int[segments * 3 * 2];

        // Üst yüzey vertexleri - 0.5f scale ile oluştur (çünkü localScale ile 2 çarpacağız)
        vertices[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / segments) * Mathf.PI * 2;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * 0.5f, 0, Mathf.Sin(angle) * 0.5f);
        }

        // Alt yüzey vertexleri
        for (int i = 0; i <= segments; i++)
        {
            vertices[i + segments + 1] = vertices[i];
        }

        // Üst yüzey üçgenleri
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }

        // Alt yüzey üçgenleri (ters yönde)
        int offset = segments + 1;
        for (int i = 0; i < segments; i++)
        {
            triangles[(i + segments) * 3] = offset;
            triangles[(i + segments) * 3 + 1] = offset + ((i + 1) % segments + 1);
            triangles[(i + segments) * 3 + 2] = offset + i + 1;
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
        if (rangeMeshRenderer != null && moveAction != null)
        {
            float actualRange = moveAction.GetCurrentMovementPoints() / moveAction.GetMovementCostPerUnit();
            
            rangeMeshRenderer.transform.position = transform.position;
            rangeMeshRenderer.transform.localScale = Vector3.one * actualRange * 2;
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

    private void Start()
    {
        if (moveAction != null)
        {
            moveAction.OnMovementPointsChanged += MoveAction_OnMovementPointsChanged;
        }
    }

    private void OnDestroy()
    {
        if (moveAction != null)
        {
            moveAction.OnMovementPointsChanged -= MoveAction_OnMovementPointsChanged;
        }
        if (rangeMeshRenderer != null)
        {
            Destroy(rangeMeshRenderer.material);
        }
    }

    private void MoveAction_OnMovementPointsChanged(object sender, EventArgs e)
    {
        // Eğer range gösteriliyorsa güncelle
        if (rangeMeshRenderer != null && rangeMeshRenderer.gameObject.activeSelf)
        {
            ShowRange(0);
        }
    }
} 