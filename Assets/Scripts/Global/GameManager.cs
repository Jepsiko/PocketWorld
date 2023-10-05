using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour {

    public static GameManager instance;
    public GameObject loadingScreen;

    public TMP_Text loading;
    public TMP_Text progress;
    public Slider bar;
    float totalSceneProgress;
    float totalSpawnProgress;

    public float timeAutoUpdate;

    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
    bool finished;

    void Awake() {
        instance = this;

        SceneManager.LoadSceneAsync((int) SceneIndexes.TITLE_SCREEN, LoadSceneMode.Additive);
    }

    void Start() {
        AutoSave();
    }


    public void LoadScene(SceneIndexes currentScene, SceneIndexes nextScene) {
        loadingScreen.SetActive(true);
        finished = false;

        scenesLoading.Add(SceneManager.UnloadSceneAsync((int) currentScene));
        scenesLoading.Add(SceneManager.LoadSceneAsync((int) nextScene, LoadSceneMode.Additive));
        
        GetTotalProgress();
        UpdateLoadingText();
    }

    public void LoadGame() {
        LoadScene(SceneIndexes.TITLE_SCREEN, SceneIndexes.GAME);
    }

    public void LoadMenu() {
        LoadScene(SceneIndexes.GAME, SceneIndexes.TITLE_SCREEN);
        PlayerData.instance.SaveData();
        SaveSystem.Terminate();
    }

    public async void GetTotalProgress() {
        float totalProgress = 0;

        while (ChunkLoader.instance == null || !ChunkLoader.instance.isDone) {
            if (ChunkLoader.instance == null) {
                totalSpawnProgress = 0;
            }
            else {
                totalSpawnProgress = ChunkLoader.instance.progress;
            }

            totalProgress = totalSpawnProgress;
            progress.text = Mathf.Round(totalProgress * 100f).ToString() + "%";
            bar.value = totalProgress;

            await Task.Yield();
        }

        finished = true;
        loadingScreen.SetActive(false);
    }

    public async void UpdateLoadingText() {
        while (!finished) {
            for (int i = 0; i < 4; i++) {
                loading.text = "LOADING" + new String('.', i);
                await Task.Delay(300);
            }
        }
    }

    public async void AutoSave() {
        while (true) {
            PlayerData.instance.SaveData();
            await Task.Delay(Mathf.RoundToInt(timeAutoUpdate*1000));
        }
    }
}
