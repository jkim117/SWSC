using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public const int chunkSize = 200;
    private int xSize = chunkSize;
    private int zSize = chunkSize;

    public const float amplitude = 65f;

    //public bool autoUpdate;

    public const float noiseScale = 100f;
    public const int octaves = 4;

    [Range(0,1)]
    public const float persistance = 0.2f;
    public const float lacunarity = 3f;

    private int seed = CrossSceneValues.worldSeed;
    public Vector2 offset = new Vector2(0, 0);

    [SerializeField]
    GameObject palmTree;


    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
    {
    }

    public void Generate(Vector2 chunkCoord)
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        float[] noiseMap = Noise.GenerateNoiseMap(xSize, zSize, seed, noiseScale, octaves, persistance, lacunarity, offset, chunkCoord);
        CreateShape(noiseMap);
        UpdateMesh();
        GetComponent<MeshCollider>().sharedMesh = mesh;

        // Map MeshColors
        /*Color[] meshColors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            if (noiseMap[i] > 0.8f)
            {
                meshColors[i] = new Color(0.87f, 0.87f, 0.87f);
            }
            else if (noiseMap[i] > 0.6f)
            {
                meshColors[i] = new Color(0.24f, 0.15f, 0.01f);
            }
            else if (noiseMap[i] > 0.3f)
            {
                meshColors[i] = new Color(0.2f, 0.4f, 0);
            }
            else if (noiseMap[i] > 0.2f)
            {
                meshColors[i] = new Color(0.69f, 0.701f, 0.525f);
            }
            else if (noiseMap[i] <= 0.2f)
            {
                meshColors[i] = new Color(0.4f, 0.69f, 1);
            }
            //meshColors[i] = Color.Lerp(Color.black, Color.white, noiseMap[i]);
        }
        mesh.colors = meshColors;*/

    }

    void CreateShape(float[] noiseMap)
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = noiseMap[i] * amplitude;
                if (y <= 0)
                {
                    y = 0.01f;
                }
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        /*foreach (Vector3 v in vertices)
        {
            Random.InitState(v.GetHashCode());
            float rand = Random.Range(0f, 1f);
            float rand2 = Random.Range(0f, 1f);
            if (v.y > 7 && rand > 0.8)
            {
                GameObject pt = GameObject.Instantiate(palmTree, v, this.transform.rotation, this.transform);
                pt.transform.localScale = new Vector3(rand2 / 5, rand2 / 5, rand2 / 5);
                pt.transform.Rotate(new Vector3(0, rand2 * 360, 0));
            }
                
        }*/
    }

    /*private void OnValidate()
    {
        if (xSize < 1)
        {
            xSize = 1;
        }
        if (zSize < 1)
        {
            zSize = 1;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
        if (persistance < 0)
        {
            persistance = 0;
        }
        if (persistance > 1)
        {
            persistance = 1;
        }
    }*/

    /*private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
    }*/
}
