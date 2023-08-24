using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static UniversalGameData universalData;
    public static GameData data0;
    public static GameData data1;
    public static GameData data2;
    public static GameData data3;
    public static int currentLoadedGame;

    public static void save(int gameSlot)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string[] pathSections = { Application.persistentDataPath, "game" + gameSlot.ToString() + ".save" };
        string path = Path.Combine(pathSections);
        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            switch (gameSlot)
            {
                case 0:
                    formatter.Serialize(stream, data0);
                    break;
                case 1:
                    formatter.Serialize(stream, data1);
                    break;
                case 2:
                    formatter.Serialize(stream, data2);
                    break;
                case 3:
                    formatter.Serialize(stream, data3);
                    break;
                default:
                    formatter.Serialize(stream, data0);
                    break;
            }
        }
    }

    public static void saveUniversalData()
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string[] pathSections = { Application.persistentDataPath, "universalgamedata.save" };
        string path = Path.Combine(pathSections);
        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            formatter.Serialize(stream, universalData);
        }
    }

    public static void load(int gameSlot)
    {
        string[] pathSections = { Application.persistentDataPath, "game" + gameSlot.ToString() + ".save" };
        string path = Path.Combine(pathSections);

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                switch (gameSlot)
                {
                    case 0:
                        data0 = formatter.Deserialize(stream) as GameData;
                        break;
                    case 1:
                        data1 = formatter.Deserialize(stream) as GameData;
                        break;
                    case 2:
                        data2 = formatter.Deserialize(stream) as GameData;
                        break;
                    case 3:
                        data3 = formatter.Deserialize(stream) as GameData;
                        break;
                    default:
                        data0 = formatter.Deserialize(stream) as GameData;
                        break;
                }
            }
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
        }
    }

    public static void loadAllGameSlots()
    {
        string[] pathSections = { Application.persistentDataPath, "universalgamedata.save" };
        string path = Path.Combine(pathSections);

        BinaryFormatter formatter = new BinaryFormatter();
        if (File.Exists(path))
        {
            
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                universalData = formatter.Deserialize(stream) as UniversalGameData;
                //UniversalGameData uv = formatter.Deserialize(stream) as UniversalGameData;
                //universalData = new UniversalGameData(uv.numGaveSaves);
            }
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            universalData = new UniversalGameData(0);
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(stream, universalData);
            }

        }

        // load number of game slots and load respective slots
        for (int i = 0; i < universalData.numGaveSaves; i++)
        {
            load(i);
        }
        CrossSceneValues.invertY = universalData.yInvert;
        CrossSceneValues.invertX = universalData.xInvert;
        CrossSceneValues.sensitivity = universalData.sensitivity;
    }
}
