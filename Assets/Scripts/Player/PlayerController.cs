//Script by Anthony C.

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamageable
{
    //setup
    private Vector2 input;
    public CharacterController characterController;
    public HealthController playerHealth;
    public StuffingController playerStuffing;
    public PlayerHealthScriptableObject savedPlayerHealth;
    private Vector3 direction;
    private Camera mainCamera;
    [SerializeField] bool ragdolling = false;

    //player movement values
    [SerializeField] public float speed;
    [SerializeField] public float rotationSpeed = 500f; //smoothtime
    private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    [SerializeField] private float jumpPower;
    private float velocity;

    //interaction
    public delegate void Interact();
    public event Interact OnInteraction;
    public bool isDragging;

    public UnityEvent<int> onDamage;


    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerHealth = GetComponent<HealthController>();
        playerStuffing = GetComponent<StuffingController>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        playerHealth.health = savedPlayerHealth.currentHealth;
        playerStuffing.stuffingCount = savedPlayerHealth.currentStuffing;
        Debug.Log("Player Health is: " + playerHealth.health);
    }

    private void Update()
    {
        ApplyRotation();
        ApplyGravity();
        ApplyMovement();

        if(ragdolling)
        {
            characterController.enabled = false;
            input = Vector2.zero;
            GetComponent<Rigidbody>().isKinematic = false;
        }
        if(!ragdolling)
        {
            characterController.enabled = true;
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void ApplyRotation()
    {
        if (input.sqrMagnitude == 0) return;
        direction = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0.0f) * new Vector3(input.x, 0.0f, input.y);
        var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed *Time.deltaTime);
        //var targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        //var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, smoothTime);
        //transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
    }

    private void ApplyMovement()
    {
        if(!ragdolling)
        {
            characterController.Move(direction * speed * Time.deltaTime);
        }
        
    }

    private void ApplyGravity()
    {
        if (IsGrounded() && velocity < 0.0f)
        {
            velocity = -1.0f;
        }
        else
        {
            velocity += gravity * gravityMultiplier * Time.deltaTime;
        }
        
        direction.y = velocity;
    }
    public void Move(InputAction.CallbackContext context)
    {
        if(characterController.enabled)
        {
            input = context.ReadValue<Vector2>();
            direction = new Vector3(input.x, 0.0f, input.y);
        }  
    }

    public void Jump(InputAction.CallbackContext context)
    {
        //Debug.Log("Jump");
        if (!context.started) return;
        if (!IsGrounded()) return;
        if (ragdolling) return;
        if (isDragging) return;

        velocity += jumpPower;
    }

    private bool IsGrounded() => characterController.isGrounded;

    public void Damage(int damageValue)
    {
        playerHealth.Damage(damageValue);
        onDamage.Invoke(damageValue);
    }

    public void Die()
    {
        Debug.Log("Player Dies");
        this.gameObject.SetActive(false);
    }
    

    public void Interaction(InputAction.CallbackContext context)
    {
        
        OnInteraction?.Invoke();
        if(!context.started) return;
        Debug.Log("Interaction");
    }

    public void Ragdoll(InputAction.CallbackContext context)
    {
        Debug.Log("ButtonPress");
        RagdollState();
    }

    public void RagdollState()
    {
        ragdolling = !ragdolling;
    }
}
