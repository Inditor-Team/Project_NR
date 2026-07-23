using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ShakeText : MonoBehaviour
{
    [SerializeField] private float intensity = 2f;
    [SerializeField] private float speed = 25f;

    private TMP_Text text;
    private TMP_MeshInfo[] cachedMeshInfo;
    private float timer;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        RefreshTextMesh();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (text.havePropertiesChanged)
            RefreshTextMesh();

        ShakeCharacters();
    }

    private void RefreshTextMesh()
    {
        text.ForceMeshUpdate();
        cachedMeshInfo = text.textInfo.CopyMeshInfoVertexData();
    }

    private void ShakeCharacters()
    {
        TMP_TextInfo textInfo = text.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];

            if (!characterInfo.isVisible)
                continue;

            int materialIndex = characterInfo.materialReferenceIndex;
            int vertexIndex = characterInfo.vertexIndex;

            Vector3[] sourceVertices =
                cachedMeshInfo[materialIndex].vertices;

            Vector3[] destinationVertices =
                textInfo.meshInfo[materialIndex].vertices;

            float noiseTime = timer * speed + i * 10f;

            float x =
                (Mathf.PerlinNoise(noiseTime, i) - 0.5f) * 2f;

            float y =
                (Mathf.PerlinNoise(i, noiseTime) - 0.5f) * 2f;

            Vector3 offset = new Vector3(x, y, 0f) * intensity;

            for (int j = 0; j < 4; j++)
            {
                destinationVertices[vertexIndex + j] =
                    sourceVertices[vertexIndex + j] + offset;
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo = textInfo.meshInfo[i];

            meshInfo.mesh.vertices = meshInfo.vertices;
            text.UpdateGeometry(meshInfo.mesh, i);
        }
    }

    private void OnDisable()
    {
        if (text == null)
            return;

        text.ForceMeshUpdate();
    }
}
