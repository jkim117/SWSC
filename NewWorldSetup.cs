using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NewWorldSetup
{
    public static int sectorDimension = 5; // number of total sectors is this number squared
    public static int[,] mapValues;
    public static Vector3[,] airbaseCoordinates;
    public static Texture2D mapTexture;

    // Function contains randomness, run a single time when generating a world. Values need to be saved as game state to re-load.
    public static void generateWorldData()
    {
        // 2D int array, dimensions equate the number of mesh vertices in the map
        /*mapValues = new int[(int)(EndlessTerrain.chunkWorldLimit * (MeshGenerator.chunkSize + 1)), (int)(EndlessTerrain.chunkWorldLimit * (MeshGenerator.chunkSize + 1))];
        mapTexture = new Texture2D((int)(EndlessTerrain.chunkWorldLimit * MeshGenerator.chunkSize), (int)(EndlessTerrain.chunkWorldLimit * MeshGenerator.chunkSize));*/

        // 2D Vector3 array, contains a vector3 coordinate for the airbase located in each map sector
        airbaseCoordinates = new Vector3[sectorDimension, sectorDimension];

        int chunkDimPerSector = (int)EndlessTerrain.chunkWorldLimit / sectorDimension; // number of chunks per sector (lengthwise) 10. Actual number of chunks per sector would be this number squared

        // double for loop to iterate through each sector
        for (int sectorRow = 0; sectorRow < sectorDimension; sectorRow++)
        {
            for (int sectorCol = 0; sectorCol < sectorDimension; sectorCol++)
            {
                // which chunk in the sector houses the airbase
                Random.InitState(CrossSceneValues.worldSeed);
                int chunkRowAirBase = Random.Range((chunkDimPerSector / 2) - 2 , (chunkDimPerSector / 2) + 2);
                int chunkColAirBase = Random.Range((chunkDimPerSector / 2) - 2, (chunkDimPerSector / 2) + 2);
                
                // For each chunk in this sector
                for (int chunkRow = 0; chunkRow < chunkDimPerSector; chunkRow++)
                {
                    for (int chunkCol = 0; chunkCol < chunkDimPerSector; chunkCol++)
                    {
                        // Determine if this is the chunk to make an airbase
                        bool locateAirbase = false;
                        Vector3 highestValue = new Vector3(0f, -1f, 0f);
                        if (chunkRowAirBase == chunkRow && chunkColAirBase == chunkCol)
                        {
                            locateAirbase = true;
                        }
                        else
                        {
                            continue;
                        }

                        // Generates the same noise map as would be generated in MeshGenerator to calculate mesh vertex height values for this chunk
                        float[] noiseMap = Noise.GenerateNoiseMap(MeshGenerator.chunkSize, MeshGenerator.chunkSize, CrossSceneValues.worldSeed, MeshGenerator.noiseScale, MeshGenerator.octaves, MeshGenerator.persistance, MeshGenerator.lacunarity, new Vector2(0, 0), new Vector2(sectorCol * chunkDimPerSector + chunkCol, sectorRow * chunkDimPerSector + chunkRow));

                        // Iterate through the vertices of this particular chunk
                        for (int i = 0, z = 0; z <= MeshGenerator.chunkSize; z++)
                        {
                            for (int x = 0; x <= MeshGenerator.chunkSize; x++)
                            {
                                float y = noiseMap[i] * MeshGenerator.amplitude + 40; // get the y value for this vertex

                                if (locateAirbase && y > highestValue.y) // If this chunk contains the airbase and this vertex is the new highest vertex, update it.
                                {
                                    highestValue.y = y * EndlessTerrain.masterScale;
                                    highestValue.x = (sectorCol * chunkDimPerSector * MeshGenerator.chunkSize + (chunkCol * MeshGenerator.chunkSize + x)) * EndlessTerrain.masterScale;
                                    highestValue.z = (sectorRow * chunkDimPerSector * MeshGenerator.chunkSize + (chunkRow * MeshGenerator.chunkSize + z)) * EndlessTerrain.masterScale;
                                }

                                // Based on the height value, code it as water, sand, grass, or rock for the map and put it in mapValues
                                /*Color mapColor = Color.blue;
                                int heightCode = 0; // water
                                if (noiseMap[i] > 0.02)
                                {
                                    heightCode = 1; // sand
                                    mapColor = Color.yellow;
                                }
                                if (noiseMap[i] > 0.084)
                                {
                                    heightCode = 2; // green
                                    mapColor = Color.green;
                                }
                                if (noiseMap[i] > 0.36)
                                {
                                    heightCode = 3; // rock
                                    mapColor = Color.gray;
                                }
                                mapValues[(sectorCol * chunkDimPerSector * (MeshGenerator.chunkSize + 1)) + (chunkCol * MeshGenerator.chunkSize + 1) + x, (sectorRow * chunkDimPerSector * (MeshGenerator.chunkSize + 1)) + (chunkRow * MeshGenerator.chunkSize + 1) + z] = heightCode;
                                mapTexture.SetPixel((sectorCol * chunkDimPerSector * MeshGenerator.chunkSize) + chunkCol * MeshGenerator.chunkSize + x, (sectorRow * chunkDimPerSector * MeshGenerator.chunkSize) + chunkRow * MeshGenerator.chunkSize + z, mapColor);*/
                                i++;
                            }
                        }

                        // enter airbase coordinates for this sector
                        if (locateAirbase)
                        {
                            airbaseCoordinates[sectorCol, sectorRow] = highestValue;

                        }

                    }
                }
            }
        }
    }

    /*public static void generateMiniMap()
    {
        Texture2D mapTexture = new Texture2D((int)(EndlessTerrain.chunkWorldLimit * (MeshGenerator.chunkSize + 1)), (int)(EndlessTerrain.chunkWorldLimit * (MeshGenerator.chunkSize + 1)));
    }*/
}
