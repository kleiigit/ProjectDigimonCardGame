using System.IO;
using UnityEngine;

public static class SaveUtility
{
    public static string GetPath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");
    }

    public static void SaveToSlot<T>(T data, int slotIndex)
    {
        string path = GetPath(slotIndex);

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao salvar no slot {slotIndex}: {e.Message}");
        }
    }


    public static T LoadFromSlot<T>(int slotIndex)
    {
        string path = GetPath(slotIndex);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        return default;
    }

    public static bool SlotExists(int slotIndex)
    {
        return File.Exists(GetPath(slotIndex));
    }

    public static void DeleteSlot(int slotIndex)
    {
        string path = GetPath(slotIndex);
        if (File.Exists(path))
            File.Delete(path);
    }
}
