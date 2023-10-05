using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour {

    public PlayerController player;

    Button loadMenuButton;

    Button useBoatButton;
    Image useBoatImage;
    Sprite boatButtonEnabled;
    Sprite boatButtonDisabled;

    Slider temperatureSlider;

    void Awake() {
        loadMenuButton = transform.Find("LoadMenu").GetComponent<Button>();
        loadMenuButton.onClick.AddListener(LoadMenu);
        
        useBoatButton = transform.Find("UseBoat").GetComponent<Button>();
        useBoatButton.onClick.AddListener(UseBoat);
        useBoatImage = transform.Find("UseBoat").GetComponent<Image>();

        boatButtonEnabled = Resources.Load<Sprite>("Sprites/UI/BoatButtonEnabled");
        boatButtonDisabled = Resources.Load<Sprite>("Sprites/UI/BoatButtonDisabled");

        temperatureSlider = transform.Find("Temperature").GetComponent<Slider>();
    }

    void Update() {
        temperatureSlider.value = player.temperature;
    }

    void LoadMenu() {
        GameManager.instance.LoadMenu();
    }

    public void SetBoatButtonVisibility(bool visibility) {
        useBoatButton.gameObject.SetActive(visibility);
    }

    public void SetBoatButtonInteractable(bool interactable) {
        if (interactable) useBoatImage.sprite = boatButtonEnabled;
        else useBoatImage.sprite = boatButtonDisabled;
    }

    void UseBoat() {
        if (player.usingBoat) {
            SetBoatButtonInteractable(true);
            player.StopUsingBoat();
        }
        else {
            SetBoatButtonInteractable(false);
            player.UseBoat();
        }
    }
}
