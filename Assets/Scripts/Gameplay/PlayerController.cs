using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public Animator animator;
    public Map map;
    public ChunkLoader chunkLoader;
    public GameUI gameUI;
    public float speed;
    public float climbingSpeedFactor;
    public float swimmingSpeedFactor;
    public float sailingSpeedFactor;
    [HideInInspector]
    public Vector3Int currentChunk;

    PlayerInputActions playerInputActions;
    SpriteRenderer spriteRenderer;

    public bool hasABoat = true;
    [HideInInspector]
    public bool usingBoat = false;
    bool wasInWater = false;

    [HideInInspector]
    public float temperature;
    [Range(0, 0.1f)]
    public float temperatureImpact;

    public float closestObjectMinDistance;
    public Prop closestObject;

    void Awake() {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
        
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (PlayerData.instance != null) {
            PlayerData.instance.LoadData();
            transform.position = PlayerData.instance.playerPosition;
            currentChunk = map.GetChunkCoordFromPlayerCoord(transform.position);
        }
    }

    void Update() {
        // INPUTS
        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>() * speed * Time.deltaTime;

        // GET DATA FROM MAP
        Vector3Int localCoord = map.GetLocalCoordFromPlayerCoord(transform.position);
        currentChunk = map.GetChunkCoordFromPlayerCoord(transform.position);
        float slope = map.GetSlope(localCoord, currentChunk);
        float height = map.GetHeight(localCoord, currentChunk);
        float mapTemperature = map.GetTemperature(localCoord, currentChunk);

        // PLAYER TEMPERATURE
        temperature = temperature + (mapTemperature - temperature) * temperatureImpact * Time.deltaTime;

        // PLAYER STATES
        bool isClimbing = slope > 0.025f;
        bool isInWater = height < map.settings.waterLevel;

        // UI CALLS
        if (hasABoat && isInWater && !wasInWater) {
            gameUI.SetBoatButtonVisibility(true);
            gameUI.SetBoatButtonInteractable(true);
        }
        else if (hasABoat && !isInWater && wasInWater) {
            gameUI.SetBoatButtonVisibility(false);
            StopUsingBoat();
        }

        // CLOSEST OBJECT OUTLINING
        TreeController gameObject = GetClosestTree();
        if (gameObject != null && gameObject != closestObject) {
            if (closestObject != null) 
                closestObject.GetComponent<SpriteRenderer>().material.SetFloat("_OutlineThickness", 0f); // Remove outline from potential previous closest
            closestObject = gameObject;
            closestObject.GetComponent<SpriteRenderer>().material.SetFloat("_OutlineThickness", 0.25f); // Add outline to new closest
        }
        else if (gameObject == null && closestObject != null) {
            closestObject.GetComponent<SpriteRenderer>().material.SetFloat("_OutlineThickness", 0f); // No close object
            closestObject = null;
        }

        // PLAYER MOVEMENTS
        float speedFactor = isClimbing ? climbingSpeedFactor : 1;
        if (!usingBoat) speedFactor = isInWater ? swimmingSpeedFactor : speedFactor;
        else speedFactor = isInWater ? sailingSpeedFactor : speedFactor;

        SetSpeed(inputVector.sqrMagnitude);
        SetDirectionX(inputVector.x);
        transform.position += (Vector3) inputVector * speedFactor;

        if (PlayerData.instance != null) {
            PlayerData.instance.playerPosition = transform.position;
        }

        wasInWater = isInWater;
    }

    public void SetSpeed(float speed) {
        animator.SetFloat("Speed", speed);
    }

    public void SetDirectionX(float x) {
        if (x < 0) spriteRenderer.flipX = true;
        if (x > 0) spriteRenderer.flipX = false;
    }

    public void UseBoat() {
        usingBoat = true;
        animator.SetBool("UsingBoat", true);
    }

    public void StopUsingBoat() {
        usingBoat = false;
        animator.SetBool("UsingBoat", false);
    }

    TreeController GetClosestTree() {
        TreeController closestTree = null;
        float closestDistance = Mathf.Infinity;
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                Vector3Int chunkCoord = currentChunk + new Vector3Int(i, j, 0);
                Chunk chunk = chunkLoader.GetChunkOrNull(chunkCoord);
                if (chunk != null) {
                    TreeController tree = chunk.GetClosestTree(transform.position);
                    if (tree != null) {
                        float distance = Vector3.Distance(transform.position, tree.transform.position);
                        if (distance < closestDistance) {
                            closestDistance = distance;
                            closestTree = tree;
                        }
                    }
                }
            }
        }
        if (closestTree != null && Vector3.Distance(transform.position, closestTree.transform.position) < closestObjectMinDistance) return closestTree;
        else return null;
    }


    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        if (closestObject != null) Gizmos.DrawLine(transform.position,  closestObject.transform.position);
    }
}
