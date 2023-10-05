using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveSystem {
    public static string saveFolder;


    public static bool initialized;

    public static void Init() {
        saveFolder = Application.persistentDataPath +  "/Saves/" + MapInfo.mapName;
        if (!Directory.Exists(saveFolder)) {
            Directory.CreateDirectory(saveFolder);
        }
        initialized = true;
    }

    public static void Terminate() {
        initialized = false;
    }

    public static void Save(string saveString, string fileName) {
        if (!initialized) return;
        File.WriteAllText(saveFolder + "/" + fileName + ".dat", saveString);
    }

    public static string Load(string fileName) {
        if (!initialized) return null;
        if (File.Exists(saveFolder + "/" + fileName + ".dat")) {
            return File.ReadAllText(saveFolder + "/" + fileName + ".dat");
        }
        return null;
    }

    public static List<string> GetMapNames() {
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath +  "/Saves/");
        DirectoryInfo[] mapFolders = directoryInfo.GetDirectories();
        List<string> mapNames = new List<string>();
        for (int i = 0; i < mapFolders.Length; i++) {
            mapNames.Add(mapFolders[i].Name);
        }
        return mapNames;
    }

    public static void DeleteMapFolder(string mapName) {
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath +  "/Saves/" + mapName);
        foreach (FileInfo file in directoryInfo.GetFiles()) {
            file.Delete(); 
        }
        Directory.Delete(Application.persistentDataPath +  "/Saves/" + mapName);
    }
}
