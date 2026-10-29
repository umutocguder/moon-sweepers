/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using UnityEngine;
using ElevenHopem.MoonSweepers.Core;
using ElevenHopem.MoonSweepers.Environment;

namespace ElevenHopem.MoonSweepers.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 7.4f;
        [SerializeField] private float runSpeed = 12.2f;
        [SerializeField] private float groundAccel = 46.0f;
        [SerializeField] private float airAccel = 12.0f;
        [SerializeField] private float jumpVelocity = 4.9f;
        [SerializeField] private float playerGravityScale = 1.95f;
        [SerializeField] private float jetpackThrust = 9.2f;
        [SerializeField] private float maxFuel = 100f;
        [SerializeField] private float fuelBurnRate = 24f;
        [SerializeField] private float fuelRechargeRate = 30f;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivity = 2.2f;
        [SerializeField] private float maxLookAngle = 86.0f;

        private CharacterController characterController;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float cameraPitch = 8.0f;
        private float currentFuel;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private float walkCycleTime;
        private Vector3 baseCameraLocalPos = new Vector3(0f, 0.72f, 0.06f);
        private float baseFov = 68f;
        private float vehicleSpeedMultiplier = 1.0f;
        private bool isBoostingNow;
        private bool floorSnappedThisFrame;

        public float CurrentFuel => currentFuel;
        public float MaxFuel => maxFuel;
        public float MouseSensitivity => mouseSensitivity;
        public bool IsGrounded => (characterController != null && characterController.isGrounded) || floorSnappedThisFrame;
        public bool IsBoosting => isBoostingNow;
        public Camera PlayerCamera => playerCamera;
        public float PlanarSpeed => horizontalVelocity.magnitude;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            currentFuel = maxFuel;

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }

            if (playerCamera != null)
            {
                baseCameraLocalPos = playerCamera.transform.localPosition;
                playerCamera.fieldOfView = baseFov;
                playerCamera.nearClipPlane = 0.06f;
                playerCamera.farClipPlane = 900f;
            }
        }

        private void Start()
        {
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            }
            EnforceTerrainFloorAndBounds();
        }

        private void Update()
        {
            if (GameManager.Instance != null && (GameManager.Instance.IsGamePaused || GameManager.Instance.IsShopOpen))
            {
                if (isBoostingNow)
                {
                    isBoostingNow = false;
                    AudioManager.Instance?.SetThrusterActive(false);
                }
                return;
            }

            HandleSensitivityHotkeys();

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                HandleMouseLook();
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                TeleportToBasePad();
                return;
            }

            HandleMovementAndPhysics();
            AnimateCameraFeedback();
        }

        public void TeleportToBasePad()
        {
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            float baseGroundY = LunarSurfaceGenerator.SampleHeight(0f, -5.0f);
            characterController.enabled = false;
            transform.position = new Vector3(0f, baseGroundY + 1.1f, -5.0f);
            transform.rotation = Quaternion.identity;
            characterController.enabled = true;
            GameManager.Instance?.NotifyToast("Returned to Base Landing Pad");
        }

        private void HandleSensitivityHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                mouseSensitivity = Mathf.Max(0.5f, mouseSensitivity - 0.25f);
                GameManager.Instance?.NotifyToast($"Mouse Sensitivity: {mouseSensitivity:F2}");
            }
            else if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                mouseSensitivity = Mathf.Min(6.0f, mouseSensitivity + 0.25f);
                GameManager.Instance?.NotifyToast($"Mouse Sensitivity: {mouseSensitivity:F2}");
            }
        }

        private void HandleMouseLook()
        {
            float rawX = Input.GetAxisRaw("Mouse X");
            float rawY = Input.GetAxisRaw("Mouse Y");

            float yawDelta = rawX * mouseSensitivity;
            float pitchDelta = rawY * mouseSensitivity;

            transform.Rotate(0f, yawDelta, 0f, Space.Self);

            cameraPitch = Mathf.Clamp(cameraPitch - pitchDelta, -maxLookAngle, maxLookAngle);
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            }
        }

        private void HandleMovementAndPhysics()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.04f);

            float moveX = 0f;
            float moveZ = 0f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveX += 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveX -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveZ += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveZ -= 1f;

            Vector3 wishDir = transform.right * moveX + transform.forward * moveZ;
            if (wishDir.sqrMagnitude > 1f)
            {
                wishDir.Normalize();
            }

            bool sprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float maxSpeed = (sprinting ? runSpeed : walkSpeed) * vehicleSpeedMultiplier;
            Vector3 targetPlanar = wishDir * maxSpeed;

            bool grounded = characterController.isGrounded || floorSnappedThisFrame;
            floorSnappedThisFrame = false;

            if (grounded)
            {
                coyoteTimer = 0.18f;
            }
            else
            {
                coyoteTimer -= dt;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                jumpBufferTimer = 0.15f;
            }
            else
            {
                jumpBufferTimer -= dt;
            }

            float accel = grounded ? groundAccel : airAccel;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetPlanar, accel * dt);

            float lunarG = (GameManager.Instance != null ? GameManager.Instance.LunarGravity : -1.62f) * playerGravityScale;

            if (coyoteTimer > 0f && jumpBufferTimer > 0f)
            {
                verticalVelocity = jumpVelocity;
                coyoteTimer = 0f;
                jumpBufferTimer = 0f;
                grounded = false;
                AudioManager.Instance?.PlayJump();
            }

            bool wantJetpack = Input.GetMouseButton(1) || (!grounded && Input.GetKey(KeyCode.Space) && verticalVelocity < 1.2f);

            if (wantJetpack && currentFuel > 0.5f)
            {
                isBoostingNow = true;
                currentFuel = Mathf.Max(0f, currentFuel - fuelBurnRate * dt);
                verticalVelocity = Mathf.Min(7.8f, verticalVelocity + jetpackThrust * dt);
                if (wishDir.sqrMagnitude > 0.01f)
                {
                    horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, wishDir * (maxSpeed * 1.22f), 18f * dt);
                }
                AudioManager.Instance?.SetThrusterActive(true);
            }
            else
            {
                if (isBoostingNow)
                {
                    isBoostingNow = false;
                    AudioManager.Instance?.SetThrusterActive(false);
                }

                if (grounded && verticalVelocity < 0f)
                {
                    verticalVelocity = -1.2f;
                }
                else
                {
                    verticalVelocity = Mathf.Max(-18f, verticalVelocity + lunarG * dt);
                }

                if (grounded && currentFuel < maxFuel)
                {
                    currentFuel = Mathf.Min(maxFuel, currentFuel + fuelRechargeRate * dt);
                }
            }

            Vector3 finalDisplacement = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z) * dt;
            characterController.Move(finalDisplacement);

            EnforceTerrainFloorAndBounds();
        }

        private void EnforceTerrainFloorAndBounds()
        {
            Vector3 pos = transform.position;
            Vector2 planarPos = new Vector2(pos.x, pos.z);
            bool clampedHorizontally = false;

            if (planarPos.magnitude > 98f)
            {
                Vector2 clamped = planarPos.normalized * 98f;
                pos.x = clamped.x;
                pos.z = clamped.y;
                clampedHorizontally = true;
            }

            float groundY = LunarSurfaceGenerator.SampleHeight(pos.x, pos.z);
            float halfHeight = characterController != null ? characterController.height * 0.5f : 0.9f;
            float minCenterY = groundY + halfHeight;

            if (pos.y < minCenterY - 0.02f || clampedHorizontally)
            {
                if (pos.y < minCenterY + 0.05f)
                {
                    pos.y = minCenterY + 0.06f;
                    if (verticalVelocity < 0f)
                    {
                        verticalVelocity = -0.5f;
                    }
                    floorSnappedThisFrame = true;
                }

                characterController.enabled = false;
                transform.position = pos;
                characterController.enabled = true;
            }
        }

        private void AnimateCameraFeedback()
        {
            if (playerCamera == null) return;

            float speed = horizontalVelocity.magnitude;
            bool movingOnGround = characterController.isGrounded && speed > 0.4f;

            if (movingOnGround)
            {
                walkCycleTime += Time.deltaTime * Mathf.Lerp(5.5f, 9.5f, Mathf.Clamp01(speed / 12f));
                float bobY = Mathf.Sin(walkCycleTime * 2f) * 0.032f;
                float bobX = Mathf.Cos(walkCycleTime) * 0.018f;
                Vector3 targetPos = baseCameraLocalPos + new Vector3(bobX, bobY, 0f);
                playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, targetPos, Time.deltaTime * 12f);
            }
            else
            {
                playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, baseCameraLocalPos, Time.deltaTime * 8f);
            }

            float targetFov = baseFov;
            if (isBoostingNow || speed > 9.5f)
            {
                targetFov = baseFov + 5.5f;
            }
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, Time.deltaTime * 7f);
        }

        public void SetVehicleSpeedMultiplier(float multiplier)
        {
            vehicleSpeedMultiplier = Mathf.Clamp(multiplier, 1.0f, 2.2f);
        }

        public void UpgradeFuelCapacity(float addedCapacity)
        {
            maxFuel += addedCapacity;
            currentFuel = maxFuel;
        }

        public void SetMaxFuel(float fuel)
        {
            maxFuel = Mathf.Max(100f, fuel);
            currentFuel = maxFuel;
        }
    }
}
