using UnityEngine;
using ElevenHopem.MoonSweepers.Core;

namespace ElevenHopem.MoonSweepers.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Dynamics")]
        [SerializeField] private float walkSpeed = 6.0f;
        [SerializeField] private float runSpeed = 10.0f;
        [SerializeField] private float lowGravityAcceleration = 14.0f;
        [SerializeField] private float groundDrag = 4.0f;
        [SerializeField] private float airDrag = 0.8f;

        [Header("Lunar Jump & Jetpack")]
        [SerializeField] private float jumpForce = 5.5f; // Floaty high jump under lunar g
        [SerializeField] private float jetpackThrust = 7.0f;
        [SerializeField] private float maxFuel = 100f;
        [SerializeField] private float fuelBurnRate = 25f;
        [SerializeField] private float fuelRechargeRate = 18f;

        [Header("Camera & Orientation")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float maxLookAngle = 85.0f;

        private CharacterController characterController;
        private Vector3 currentVelocity;
        private float cameraPitch = 0.0f;
        private float currentFuel;

        public float CurrentFuel => currentFuel;
        public float MaxFuel => maxFuel;
        public bool IsGrounded => characterController.isGrounded;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            currentFuel = maxFuel;

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (GameManager.Instance != null && (GameManager.Instance.IsGamePaused || GameManager.Instance.IsShopOpen))
            {
                return;
            }

            HandleMouseLook();
            HandleMovement();
            HandleJetpackAndGravity();
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Yaw: rotate astronaut body
            transform.Rotate(Vector3.up * mouseX);

            // Pitch: rotate camera
            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);

            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 inputDirection = (transform.forward * vertical + transform.right * horizontal).normalized;

            bool isRunning = Input.GetKey(KeyCode.LeftShift) && !Input.GetButton("Fire2");
            float targetSpeed = isRunning ? runSpeed : walkSpeed;

            Vector3 targetVelocity = inputDirection * targetSpeed;

            // Low-gravity gliding interpolation
            float drag = characterController.isGrounded ? groundDrag : airDrag;
            currentVelocity.x = Mathf.Lerp(currentVelocity.x, targetVelocity.x, Time.deltaTime * lowGravityAcceleration * drag);
            currentVelocity.z = Mathf.Lerp(currentVelocity.z, targetVelocity.z, Time.deltaTime * lowGravityAcceleration * drag);
        }

        private void HandleJetpackAndGravity()
        {
            float gravity = GameManager.Instance != null ? GameManager.Instance.LunarGravity : -1.62f;

            if (characterController.isGrounded)
            {
                // Reset small downward force to keep grounded
                if (currentVelocity.y < 0)
                {
                    currentVelocity.y = -0.5f;
                }

                // Jump
                if (Input.GetButtonDown("Jump"))
                {
                    currentVelocity.y = jumpForce;
                }

                // Recharge fuel on ground
                if (currentFuel < maxFuel)
                {
                    currentFuel = Mathf.Min(maxFuel, currentFuel + fuelRechargeRate * Time.deltaTime);
                }
            }
            else
            {
                // Jetpack boost (Right Mouse or Space held in air)
                bool isBoosting = (Input.GetButton("Fire2") || (Input.GetButton("Jump") && currentVelocity.y <= 0)) && currentFuel > 0;

                if (isBoosting)
                {
                    currentFuel = Mathf.Max(0f, currentFuel - fuelBurnRate * Time.deltaTime);
                    currentVelocity.y += jetpackThrust * Time.deltaTime;

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayThruster();
                    }
                }
                else
                {
                    // Lunar gravitational pull
                    currentVelocity.y += gravity * Time.deltaTime;
                }
            }

            // Apply movement
            characterController.Move(currentVelocity * Time.deltaTime);
        }

        public void UpgradeFuelCapacity(float addedCapacity)
        {
            maxFuel += addedCapacity;
            currentFuel = maxFuel;
        }
    }
}
