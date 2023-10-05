using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour {

    public static PlayerData instance;

    public Vector3 playerPosition;
    public bool hasABoat;
    public bool usingBoat;

    void Awake() {
        instance = this;
        playerPosition = new Vector3(0, 0, -1);
    }
    
    public void SaveData() {
        string save = playerPosition[0].ToString() + " " + playerPosition[1];
        string filename = "playerdata";
        SaveSystem.Save(save, filename);
    }

    public void LoadData() {
        string filename = "playerdata";
        string save = SaveSystem.Load(filename);
        if (save != null) {
            string[] coord = save.Split(' ');
            playerPosition[0] = float.Parse(coord[0]);
            playerPosition[1] = float.Parse(coord[1]);
        }
    }
}
