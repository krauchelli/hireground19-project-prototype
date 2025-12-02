using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class GerakanPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    [Header("Crosshair Settings")]
    public Image crosshair;
    private Color normalColor = Color.white;
    private Color targetColor = Color.red;
    private Color grabColor = Color.cyan;

    [Header("Grab & Throw Settings")]
    public Transform grabPoint;
    public float grabDistance = 3f;
    public float throwForce = 25f;

    private Rigidbody grabbedObject;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleActions();
        CheckCrosshairTarget();

        // Jika sedang grab, ikuti posisi depan kamera
        if (grabbedObject != null)
        {
            grabbedObject.MovePosition(Vector3.Lerp(
                grabbedObject.position,
                grabPoint.position,
                Time.deltaTime * 12f
            ));
        }
    }

    // -------------------- GERAK --------------------
    void HandleMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
            moveDirection.y = jumpPower;
        else
            moveDirection.y = movementDirectionY;

        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        if (Input.GetKey(KeyCode.R) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    // -------------------- AKSI --------------------
    void HandleActions()
    {
        if (Input.GetMouseButtonDown(0) && grabbedObject == null)
            ApplyPhysicsPush();

        if (Input.GetMouseButtonDown(1))
            HandleGrab();

        if (Input.GetKeyDown(KeyCode.Q) && grabbedObject != null)
            ThrowObject();
    }

    // -------------------- PUSH --------------------
    void ApplyPhysicsPush()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 3f))
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb != null)
                rb.AddForce(playerCamera.transform.forward * 10f, ForceMode.Impulse);
        }
    }

    // -------------------- GRAB --------------------
    void HandleGrab()
    {
        if (grabbedObject == null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, grabDistance))
            {
                Rigidbody rb = hit.collider.attachedRigidbody;
                if (rb != null)
                {
                    grabbedObject = rb;

                    // Nonaktifkan physics sementara
                    grabbedObject.useGravity = false;
                    grabbedObject.isKinematic = true;

                    // Tempelkan ke kamera (biar ngikut posisi grabPoint)
                    grabbedObject.transform.SetParent(playerCamera.transform);
                    grabbedObject.transform.position = grabPoint.position;

                    Debug.Log($"Grabbing {rb.name} at {rb.transform.position}");
                }
            }
        }
        else
        {
            // Lepaskan objek
            grabbedObject.useGravity = true;
            grabbedObject.isKinematic = false;

            // Lepas parent dari kamera
            grabbedObject.transform.SetParent(null);
            grabbedObject = null;
        }
    }

    // -------------------- THROW --------------------
    void ThrowObject()
    {
        if (grabbedObject == null) return;

        Rigidbody rb = grabbedObject;

        // Lepas dari kamera
        rb.transform.SetParent(null);

        rb.useGravity = true;
        rb.isKinematic = false;
        grabbedObject = null;

        // Hindari tabrakan awal dengan player
        Collider rbCol = rb.GetComponent<Collider>();
        Collider playerCol = GetComponent<Collider>();
        if (rbCol != null && playerCol != null)
            Physics.IgnoreCollision(rbCol, playerCol, true);

        StartCoroutine(ThrowDelayed(rb));
    }

    IEnumerator ThrowDelayed(Rigidbody obj)
    {
        yield return new WaitForSeconds(0.05f);
        obj.useGravity = true;

        // Naikkan sedikit posisi bola supaya tidak nabrak player
        obj.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 1f + Vector3.up * 0.5f;

        Debug.Log($"Throwing {obj.name} from {obj.transform.position} with direction {playerCamera.transform.forward}");

        obj.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
        grabbedObject = null;
    }

    // -------------------- CROSSHAIR --------------------
    void CheckCrosshairTarget()
    {
        if (crosshair == null) return;

        if (grabbedObject != null)
        {
            crosshair.color = grabColor;
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 3f))
        {
            if (hit.collider.attachedRigidbody != null)
            {
                crosshair.color = targetColor;
                return;
            }
        }

        crosshair.color = normalColor;
    }
}
