using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDistance = 500;
    public const float masterScale = 50f;
    public const float chunkWorldLimit = 50;
    public const float worldDim = chunkWorldLimit * MeshGenerator.chunkSize * masterScale;

    Vector2 viewerPositionOld;
    const float PlayerControllerThresholdForUpdate = 50f;
    const float PlayerControllerThresholdForUpdateSqr = PlayerControllerThresholdForUpdate * PlayerControllerThresholdForUpdate;

    public Transform viewer;
    public GameObject chunkGenerator;

    public static Vector2 viewerPosition;
    int chunkSize;
    int chunksVisibleInViewDst;

    public Material material;
    public float minHeight = 0;
    public float maxHeight = 4000;

    public Layer[] layers;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<Vector2> terrainChunksVisibleLastUpdate = new List<Vector2>();

    // world coordinates range from 0, 0 to chunkWorldLimit * chunkSize * masterScale
    public static Vector2 worldToUnityCoord(float xWorld, float zWorld)
    {
        xWorld = GenFunctions.mod(xWorld, worldDim);
        zWorld = GenFunctions.mod(zWorld, worldDim);
        float xUnity = xWorld + PlayerController.totalxShift;
        if (xUnity <= 0)
        {
            float addxUnity = xUnity + worldDim * (int)(-xUnity / worldDim);
            float addxOneUnity = addxUnity + worldDim;
            if (Mathf.Abs(addxUnity) < Mathf.Abs(xUnity))
                xUnity = addxUnity;
            if (Mathf.Abs(addxOneUnity) < Mathf.Abs(xUnity))
                xUnity = addxOneUnity;
        }
        else
        {
            float subxUnity = xUnity - worldDim * (int)(xUnity / worldDim);
            float subxOneUnity = subxUnity - worldDim;
            if (Mathf.Abs(subxUnity) < Mathf.Abs(xUnity))
                xUnity = subxUnity;
            if (Mathf.Abs(subxOneUnity) < Mathf.Abs(xUnity))
                xUnity = subxOneUnity;
        }
        float zUnity = zWorld + PlayerController.totalzShift;
        if (zUnity <= 0)
        {
            float addzUnity = zUnity + worldDim * (int)(-zUnity / worldDim);
            float addzOneUnity = addzUnity + worldDim;
            if (Mathf.Abs(addzUnity) < Mathf.Abs(zUnity))
                zUnity = addzUnity;
            if (Mathf.Abs(addzOneUnity) < Mathf.Abs(zUnity))
                zUnity = addzOneUnity;
        }
        else
        {
            float subzUnity = zUnity - worldDim * (int)(zUnity / worldDim);
            float subzOneUnity = subzUnity - worldDim;
            if (Mathf.Abs(subzUnity) < Mathf.Abs(zUnity))
                zUnity = subzUnity;
            if (Mathf.Abs(subzOneUnity) < Mathf.Abs(zUnity))
                zUnity = subzOneUnity;
        }
        return new Vector2(xUnity, zUnity);
    }

    public static Vector2 unityToWorldCoord(float xUnity, float zUnity)
    {
        float xWorld = (xUnity - PlayerController.totalxShift) % worldDim;
        float zWorld = (zUnity - PlayerController.totalzShift) % worldDim;
        return new Vector2(xWorld, zWorld);
    }

    public void subscribeToViewerEvent()
    {
        if (viewer.gameObject.GetComponent<PlayerController>() != null)
        {
            viewer.gameObject.GetComponent<PlayerController>().onReset += worldShift;
        }
            
    }

    // Start is called before the first frame update
    void Start()
    {
        subscribeToViewerEvent();

        chunkSize = MeshGenerator.chunkSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDistance / chunkSize);

        //viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / masterScale;
        viewerPosition = unityToWorldCoord(viewer.position.x, viewer.position.z) / masterScale;
        UpdateVisibleChunks();
        viewerPositionOld = worldToUnityCoord(viewerPosition.x, viewerPosition.y);

        /*material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
        material.SetInt("layerCount", layers.Length);
        material.SetColorArray("baseColors", layers.Select(x => x.tint).ToArray());
        material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
        material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
        material.SetFloatArray("baseColorStrength", layers.Select(x => x.tintStrength).ToArray());
        material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());
        Texture2DArray texturesArray = Layer.GenerateTextureArray(layers.Select(x => x.texture).ToArray());
        material.SetTexture("baseTextures", texturesArray);*/
        
    }

    void UpdateVisibleChunks()
    {
        List<Vector2> newTerrainChunksVisible = new List<Vector2>();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = - chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(GenFunctions.mod(currentChunkCoordX + xOffset, chunkWorldLimit), GenFunctions.mod(currentChunkCoordY + yOffset, chunkWorldLimit));

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    newTerrainChunksVisible.Add(viewedChunkCoord);
                }
                else
                {
                    TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, chunkSize, transform, chunkGenerator);
                    terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                    newTerrainChunksVisible.Add(viewedChunkCoord);
                }
            }
        }

        foreach (Vector2 key in terrainChunksVisibleLastUpdate)
        {
            if (!newTerrainChunksVisible.Contains(key)) // key not in the new visible chunks
            {
                TerrainChunk chunktoRemove;
                if (terrainChunkDictionary.Remove(key, out chunktoRemove))
                {
                    Destroy(chunktoRemove.meshObject);
                }

            }
        }
        terrainChunksVisibleLastUpdate = newTerrainChunksVisible;

    }

    void worldShift(float xShift, float zShift)
    {
        foreach(TerrainChunk tc in terrainChunkDictionary.Values)
        {
            tc.meshObject.transform.position += new Vector3(xShift, 0, zShift);
        }
    }

    public class TerrainChunk
    {
        public GameObject meshObject;
        Vector2 position;
        Vector2 loopedCoord;
        Vector2 loopedPosition;

        public TerrainChunk(Vector2 coord, int size, Transform parent, GameObject cg)
        {
            position = coord * size  * masterScale;
            position = worldToUnityCoord(position.x, position.y);
            loopedCoord = new Vector2(GenFunctions.mod(coord.x, chunkWorldLimit), GenFunctions.mod(coord.y, chunkWorldLimit));
            loopedPosition = loopedCoord * size;

            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = GameObject.Instantiate(cg);
            MeshGenerator mg = meshObject.GetComponent<MeshGenerator>();
            mg.offset = loopedPosition;
            
            mg.Generate(loopedCoord);

            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * masterScale;
            //meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;

        }
    }

    [System.Serializable]
    public class Layer
    {
        public Texture2D texture;
        public Color tint;
        [Range(0, 1)]
        public float tintStrength;
        [Range(0, 1)]
        public float startHeight;
        [Range(0, 1)]
        public float blendStrength;
        public float textureScale;

        const int textureSize = 512;
        const TextureFormat textureFormat = TextureFormat.RGB565;

        public static Texture2DArray GenerateTextureArray(Texture2D[] textures)
        {
            Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);
            for (int i = 0; i < textures.Length; i++)
            {
                textureArray.SetPixels(textures[i].GetPixels(), i);
            }
            textureArray.Apply();
            return textureArray;
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
        material.SetInt("layerCount", layers.Length);
        material.SetColorArray("baseColors", layers.Select(x => x.tint).ToArray());
        material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
        material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
        material.SetFloatArray("baseColorStrength", layers.Select(x => x.tintStrength).ToArray());
        material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());
        Texture2DArray texturesArray = Layer.GenerateTextureArray(layers.Select(x => x.texture).ToArray());
        material.SetTexture("baseTextures", texturesArray);*/

        //viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / masterScale;
        viewerPosition = unityToWorldCoord(viewer.position.x, viewer.position.z) / masterScale;

        Vector2 viewerPositionTemp = worldToUnityCoord(viewerPosition.x, viewerPosition.y);
        if ((viewerPositionOld - viewerPositionTemp).sqrMagnitude > PlayerControllerThresholdForUpdateSqr)
        {
            viewerPositionOld = viewerPositionTemp;
            UpdateVisibleChunks();
        }
    }

    private void FixedUpdate()
    {
        
    }
}
