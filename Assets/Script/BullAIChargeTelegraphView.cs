using UnityEngine;
using UnityEngine.Rendering;

public sealed class BullAIChargeTelegraphView
{
    private readonly Transform owner;
    private LineRenderer disabledLegacyLine;
    private MeshFilter chargeTelegraphFillFilter;
    private MeshRenderer chargeTelegraphFillRenderer;
    private Mesh chargeTelegraphFillMesh;

    public BullAIChargeTelegraphView(Transform owner)
    {
        this.owner = owner;
    }

    public void Hide()
    {
        if (disabledLegacyLine != null)
            disabledLegacyLine.enabled = false;

        if (chargeTelegraphFillRenderer != null)
            chargeTelegraphFillRenderer.enabled = false;
    }

    public void ShowPreview(Vector3 backLeft, Vector3 frontLeft, Vector3 frontRight, Vector3 backRight, Color outlineColor, Color fillColor)
    {
        Ensure(fillColor);
        UpdateFill(backLeft, frontLeft, frontRight, backRight, fillColor);
    }

    private void Ensure(Color fillColor)
    {
        if (chargeTelegraphFillFilter == null || chargeTelegraphFillRenderer == null)
        {
            Transform existing = owner.Find("ChargeTelegraph");
            if (existing != null)
            {
                disabledLegacyLine = existing.GetComponent<LineRenderer>();
                if (disabledLegacyLine != null)
                    disabledLegacyLine.enabled = false;
                existing.gameObject.SetActive(false);
            }

            Transform fillExisting = owner.Find("ChargeTelegraphFill");
            GameObject fillObject = fillExisting != null ? fillExisting.gameObject : new GameObject("ChargeTelegraphFill");
            if (fillObject.transform.parent != owner)
                fillObject.transform.SetParent(owner, false);
            fillObject.transform.localPosition = Vector3.zero;
            fillObject.transform.localRotation = Quaternion.identity;
            fillObject.transform.localScale = Vector3.one;

            chargeTelegraphFillFilter = fillObject.GetComponent<MeshFilter>();
            if (chargeTelegraphFillFilter == null)
                chargeTelegraphFillFilter = fillObject.AddComponent<MeshFilter>();

            chargeTelegraphFillRenderer = fillObject.GetComponent<MeshRenderer>();
            if (chargeTelegraphFillRenderer == null)
                chargeTelegraphFillRenderer = fillObject.AddComponent<MeshRenderer>();

            if (chargeTelegraphFillMesh == null)
            {
                chargeTelegraphFillMesh = new Mesh
                {
                    name = "ChargeTelegraphFillMesh"
                };
                chargeTelegraphFillMesh.MarkDynamic();
            }

            chargeTelegraphFillFilter.sharedMesh = chargeTelegraphFillMesh;
            chargeTelegraphFillRenderer.shadowCastingMode = ShadowCastingMode.Off;
            chargeTelegraphFillRenderer.receiveShadows = false;
            chargeTelegraphFillRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            chargeTelegraphFillRenderer.lightProbeUsage = LightProbeUsage.Off;
            chargeTelegraphFillRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            chargeTelegraphFillRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            chargeTelegraphFillRenderer.enabled = false;
        }

        Material fillMaterial = chargeTelegraphFillRenderer.sharedMaterial;
        if (fillMaterial != null)
            fillMaterial.color = fillColor;
    }

    private void UpdateFill(Vector3 backLeft, Vector3 frontLeft, Vector3 frontRight, Vector3 backRight, Color fillColor)
    {
        if (chargeTelegraphFillFilter == null || chargeTelegraphFillRenderer == null || chargeTelegraphFillMesh == null)
            return;

        Vector3[] vertices =
        {
            owner.InverseTransformPoint(backLeft),
            owner.InverseTransformPoint(frontLeft),
            owner.InverseTransformPoint(frontRight),
            owner.InverseTransformPoint(backRight)
        };

        int[] triangles = { 0, 1, 2, 0, 2, 3 };
        Vector2[] uvs =
        {
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0f)
        };

        chargeTelegraphFillMesh.Clear();
        chargeTelegraphFillMesh.vertices = vertices;
        chargeTelegraphFillMesh.triangles = triangles;
        chargeTelegraphFillMesh.uv = uvs;
        chargeTelegraphFillMesh.RecalculateBounds();
        chargeTelegraphFillMesh.RecalculateNormals();

        Material fillMaterial = chargeTelegraphFillRenderer.sharedMaterial;
        if (fillMaterial != null)
            fillMaterial.color = fillColor;

        chargeTelegraphFillRenderer.enabled = true;
    }
}
