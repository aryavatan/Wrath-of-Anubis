using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperScope : MonoBehaviour
{
    [Range(1,60)]
    public int scopeZoom;
    
    [Range(0,1)]
    public float scopeSensitivity;

    [Header("Scope Sway")]
    private Quaternion OrigAimPos;  //original aim position recorded for the scope sway so we know where to return to
    public float ScopeSwayMin = -1.5f; //the min/max amount in degrees the scope can sway around when aiming, 
    public float ScopeSwayMax = 1.5f;
    public float ScopeSwaySpeed = 0.1f; //speed of target sway

    static int defaultFov = 60;
    Camera weaponCamera;
    GameUI ui;
    PlayerMovement playerCamera;
    Gun gun;

    bool isScoped = false;


    void Awake()
    {
        weaponCamera = Camera.main.transform.GetChild(0).GetComponent<Camera>();
        ui = FindObjectOfType<GameUI>();
        playerCamera = FindObjectOfType<PlayerMovement>();
        OrigAimPos = Camera.main.transform.localRotation;
        gun = GetComponent<Gun>();
    }

    private void Start()
    {
        defaultFov = PlayerPrefs.GetInt("FovOption", -1);
        if (defaultFov == -1)
            defaultFov = 60;
    }

    //Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire2") && !gun.IsReloading())
        {
            StartCoroutine(OnScope());
        }
        else
        {
            Unscope();
        }
    }

    public IEnumerator OnScope()
    {
        isScoped = true;
        yield return new WaitForSeconds(0.15f);
        
        if (isScoped)
        {
            ui.SetSniperScope(true);

            weaponCamera.enabled = false;
            Camera.main.fieldOfView = scopeZoom;

            playerCamera.SetCameraSensitivity(scopeSensitivity);

            SniperSway();
        }
    }

    public void Unscope()
    {
        isScoped = false;
        ui.SetSniperScope(false);
        weaponCamera.enabled = true;
        Camera.main.fieldOfView = defaultFov;
        playerCamera.SetCameraSensitivity(1f);
        Camera.main.transform.localRotation = OrigAimPos;
    }

    //perfom the scope sway
    void SniperSway()
    {
        float x, y;
        x = Random.Range(ScopeSwayMin, ScopeSwayMax);
        y = Random.Range(ScopeSwayMin, ScopeSwayMax);
        Quaternion q = Quaternion.Euler(new Vector3(x, y, 0));
        Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, q, Time.deltaTime * ScopeSwaySpeed);
        Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, OrigAimPos, Time.deltaTime * ScopeSwaySpeed);
    }

    public static void UpdateFOV(int value)
    {
        defaultFov = value;
    }
}
