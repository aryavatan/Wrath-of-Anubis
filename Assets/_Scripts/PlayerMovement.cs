﻿using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Assingables
    public Transform playerCam;
    public Transform orientation;

    //Other
    private Rigidbody rb;

    //Rotation and look
    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;
    public static float mouseSensitivity = 1f;

    //Movement
    public float moveSpeed = 4500f;
    public float maxSpeed = 20f;
    public float movementMultiplier = 1f;
    public bool grounded;
    public LayerMask whatIsGround;
    public AudioClip footsteps;
    public AudioClip landingJumpAudio;
    private AudioSource footstepsSource;
    private bool footstepPlaying = false;
    private AudioSource jumpAudioSource;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    //Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;
    public MilkShake.ShakePreset cameraShake;

    //Input
    float x, y;
    bool jumping, sprinting, crouching, inAir;

    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    // Default FOV
    private static int fov = 60;

    // Debuff Variables
    bool debuffActive = false;
    float debuffTimer = 0f;
    float debuffDuration = 0f;
    float debuffMultiplier = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        jumpAudioSource = gameObject.AddComponent<AudioSource>();
        jumpAudioSource.playOnAwake = false;
        jumpAudioSource.loop = false;
        jumpAudioSource.clip = landingJumpAudio;
    }

    void Start()
    {
        playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        fov = PlayerPrefs.GetInt("FovOption", -1);
        if (fov == -1)
            fov = 60;
        Camera.main.fieldOfView = fov;

        InitializeFootstepsAudioSource();
        inAir = false;
    }


    private void FixedUpdate()
    {
        Movement();
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;
        MyInput();
        Look();

        debuffTimer += Time.deltaTime;
        if (debuffActive)
        {
            debuffMultiplier = 0.4f;
        }
        if (debuffTimer > debuffDuration)
        {
            debuffMultiplier = 1f;
            debuffActive = false;
        }
    }

    /// <summary>
    /// Find user input. Should put this in its own class but im lazy
    /// </summary>
    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        crouching = Input.GetKey(KeyCode.LeftControl);

        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch();
    }

    private void StartCrouch()
    {
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (rb.velocity.magnitude > 0.5f)
        {
            if (grounded)
            {
                rb.AddForce(orientation.transform.forward * slideForce);
            }
        }
    }

    private void StopCrouch()
    {
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private void Movement()
    {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        // When landing jump, play impact sound
        if (inAir && grounded && readyToJump)
        {
            inAir = false;
            jumpAudioSource.Play();
            Camera.main.GetComponent<MilkShake.Shaker>().Shake(cameraShake);
        }

        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping) Jump();

        //Set max speed
        float maxSpeed = this.maxSpeed * movementMultiplier * debuffMultiplier;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        // Footstep sounds
        if ((Math.Abs(x) > 0 || Math.Abs(y) > 0) && grounded)
        {
            PlayFootsteps(true);
        }
        else
        {
            PlayFootsteps(false);
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (grounded && crouching) multiplierV = 0f;
        if (inAir) multiplierV = 0.2f;

        //Apply forces to move player        
        Vector3 movementForce = ((orientation.transform.forward * y) + (orientation.transform.right * x)).normalized;
        movementForce *= moveSpeed * Time.deltaTime * multiplier * multiplierV;

        rb.AddForce(movementForce);
    }

    private void Jump()
    {
        if (grounded && readyToJump)
        {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            inAir = true;
            jumping = false;

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private float desiredX;
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier * mouseSensitivity;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    // Simulates gun recoil by shifting the camera rotation
    public void Recoil(float verticleRecoil, float horizontalRecoil)
    {
        //Find current look rotation
        Quaternion original = playerCam.transform.localRotation;
        Vector3 rot = original.eulerAngles;

        // Verticle Recoil
        xRotation -=  UnityEngine.Random.Range(verticleRecoil/2, verticleRecoil);
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Horizontal Recoil
        float magnitude = UnityEngine.Random.Range(horizontalRecoil/2, horizontalRecoil);
        if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
            magnitude *= -1f;
        rot.y -= magnitude;

        Quaternion recoil = Quaternion.Euler(xRotation, rot.y, 0);
        playerCam.transform.localRotation = Quaternion.Slerp(original, recoil, Time.deltaTime);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        //Slow down sliding
        if (crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }

    public void SetCameraSensitivity(float multiplier)
    {
        sensMultiplier = multiplier;
    }

    public static void UpdateFOV(int value)
    {
        fov = value;
    }

    void InitializeFootstepsAudioSource()
    {
        if (footsteps)
        {
            footstepsSource = gameObject.AddComponent<AudioSource>();
            footstepsSource.clip = footsteps;
            footstepsSource.playOnAwake = false;
            footstepsSource.loop = true;
            footstepsSource.pitch = 1.1f;
            footstepsSource.volume = 0.7f;
        }
    }

    void PlayFootsteps(bool state)
    {
        if (footsteps)
        {
            if (state == true && footstepPlaying == false)
            {
                footstepPlaying = true;
                footstepsSource.Play();
            }
            else if (state == false)
            {
                footstepsSource.Pause();
                footstepPlaying = false;
            }
        }
    }

    public void AddSlownessDebuff(float duration)
    {
        debuffDuration = duration;
        debuffTimer = 0f;
        debuffActive = true;
    }

}
