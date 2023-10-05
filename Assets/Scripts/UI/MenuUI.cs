using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuUI : MonoBehaviour {

    Button newMapButton;
    Button loadMapButton;
    Button deleteMapButton;
    Button deleteAllMapsButton;

    TMP_InputField mapNameInput;
    TMP_Dropdown mapNamesDropdown;

    void Awake() {
        newMapButton = transform.Find("NewMapButton").GetComponent<Button>();
        newMapButton.onClick.AddListener(NewMap);
        loadMapButton = transform.Find("LoadMapButton").GetComponent<Button>();
        loadMapButton.onClick.AddListener(LoadMap);
        deleteMapButton = transform.Find("DeleteMapButton").GetComponent<Button>();
        deleteMapButton.onClick.AddListener(DeleteMap);
        deleteAllMapsButton = transform.Find("DeleteAllMapsButton").GetComponent<Button>();
        deleteAllMapsButton.onClick.AddListener(DeleteAllMaps);

        mapNameInput = transform.Find("MapNameInput").GetComponent<TMP_InputField>();
        mapNamesDropdown = transform.Find("MapNamesDropdown").GetComponent<TMP_Dropdown>();
        mapNamesDropdown.AddOptions(SaveSystem.GetMapNames());
    }

    void NewMap() {
        string mapName = mapNameInput.text;
        if (mapName != "") {
            MapInfo.mapName = mapName;
            // Loader.Load(Loader.Scene.GameScene);
            GameManager.instance.LoadGame();
        }
    }

    void LoadMap() {
        int option = mapNamesDropdown.value;
        if (mapNamesDropdown.options.Count > 0) {
            string mapName = mapNamesDropdown.options[option].text;
            MapInfo.mapName = mapName;
            // Loader.Load(Loader.Scene.GameScene);
            GameManager.instance.LoadGame();
        }
    }

    void DeleteMap() {
        int option = mapNamesDropdown.value;
        if (mapNamesDropdown.options.Count > 0) {
            string mapName = mapNamesDropdown.options[option].text;
            SaveSystem.DeleteMapFolder(mapName);
            mapNamesDropdown.options.Clear();
            mapNamesDropdown.AddOptions(SaveSystem.GetMapNames());
        }
    }

    void DeleteAllMaps() {
        while (mapNamesDropdown.options.Count > 0) {
            string mapName = mapNamesDropdown.options[0].text;
            SaveSystem.DeleteMapFolder(mapName);
            mapNamesDropdown.options.Clear();
            mapNamesDropdown.AddOptions(SaveSystem.GetMapNames());
        }
    }
}
