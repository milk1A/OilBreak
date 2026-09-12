using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoxPickUp : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;

    [Header("Pickup")]
    [SerializeField] private float pickupDistance = 5f;
    [SerializeField] private LayerMask pickupMask;

    [Header("Holding")]
    [SerializeField] private float moveSpeed = 15f;

    [Tooltip("처음 박스를 집었을 때 거리")]
    [SerializeField] private float defaultHoldDistance = 1.5f;

    [SerializeField] private float minHoldDistance = 0.8f;
    [SerializeField] private float maxHoldDistance = 8f;

    [Tooltip("마우스 휠 거리 변화량")]
    [SerializeField] private float scrollSpeed = 0.5f;

    [Header("Landing Detection")]
    [Tooltip("박스가 착지할 수 있는 바닥/장애물 Layer")]
    [SerializeField] private LayerMask landingMask = ~0;

    [Tooltip("바닥 감지 여유 거리")]
    [SerializeField] private float landingCheckDistance = 0.15f;

    [Tooltip("바닥과 박스 사이 아주 작은 여유")]
    [SerializeField] private float landingOffset = 0.02f;

    private Rigidbody heldRigidbody;
    private Collider[] heldColliders;

    private float holdDistance;

    private Dictionary<Rigidbody, BoxOriginalTransform>
        originalTransforms =
        new Dictionary<Rigidbody, BoxOriginalTransform>();

    private bool IsHolding => heldRigidbody != null;


    private struct BoxOriginalTransform
    {
        public Vector3 position;
        public Quaternion rotation;

        public BoxOriginalTransform(
            Vector3 position,
            Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }


    // =====================================================
    // Awake
    // =====================================================

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }


    // =====================================================
    // Start
    // =====================================================

    private void Start()
    {
        holdDistance = defaultHoldDistance;

        GameObject[] boxes =
            GameObject.FindGameObjectsWithTag("PickupBox");

        foreach (GameObject boxObject in boxes)
        {
            Rigidbody boxRb =
                boxObject.GetComponent<Rigidbody>();

            if (boxRb == null)
            {
                boxRb =
                    boxObject.GetComponentInParent<Rigidbody>();
            }

            if (boxRb == null)
                continue;

            if (!originalTransforms.ContainsKey(boxRb))
            {
                originalTransforms.Add(
                    boxRb,
                    new BoxOriginalTransform(
                        boxRb.position,
                        boxRb.rotation
                    )
                );
            }
        }
    }


    // =====================================================
    // Update
    // =====================================================

    private void Update()
    {
        if (IsHolding)
        {
            HandleMouseWheel();
            MoveHeldBox();
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (IsHolding)
            {
                TryDropBox();
            }
            else
            {
                TryPickupBox();
            }
        }
    }


    // =====================================================
    // Mouse Ray
    // =====================================================

    private Ray GetMouseRay()
    {
        if (playerCamera == null)
        {
            return new Ray(
                transform.position,
                transform.forward
            );
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 screenCenter =
                new Vector2(
                    Screen.width * 0.5f,
                    Screen.height * 0.5f
                );

            return playerCamera.ScreenPointToRay(
                screenCenter
            );
        }

        if (Mouse.current != null)
        {
            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            return playerCamera.ScreenPointToRay(
                mousePosition
            );
        }
        // ggg

        return new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );
    }


    // =====================================================
    // Pickup
    // =====================================================

    private void TryPickupBox()
    {
        Ray ray = GetMouseRay();

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            pickupDistance,
            pickupMask,
            QueryTriggerInteraction.Ignore))
        {
            Transform pickupTransform =
                FindParentWithTag(
                    hit.collider.transform,
                    "PickupBox"
                );

            if (pickupTransform == null)
                return;

            Rigidbody targetRb =
                pickupTransform.GetComponent<Rigidbody>();

            if (targetRb == null)
            {
                targetRb =
                    pickupTransform.GetComponentInParent<Rigidbody>();
            }

            if (targetRb == null)
                return;

            PickupBox(targetRb);
        }
    }


    private void PickupBox(Rigidbody target)
    {
        if (target == null)
            return;

        heldRigidbody = target;
        GameAudio.Play(GameSound.Pickup);

        // 박스를 집을 때마다 거리 초기화
        holdDistance = defaultHoldDistance;

        if (!originalTransforms.ContainsKey(target))
        {
            originalTransforms.Add(
                target,
                new BoxOriginalTransform(
                    target.position,
                    target.rotation
                )
            );
        }

        // 모든 Collider 가져오기
        heldColliders =
            target.GetComponentsInChildren<Collider>(true);

        // 들고 있는 동안 Collider OFF
        SetColliders(
            heldColliders,
            false
        );

        heldRigidbody.linearVelocity =
            Vector3.zero;

        heldRigidbody.angularVelocity =
            Vector3.zero;

        heldRigidbody.useGravity = false;
        heldRigidbody.isKinematic = true;
    }


    // =====================================================
    // Mouse Wheel
    // =====================================================

    private void HandleMouseWheel()
    {
        if (Mouse.current == null)
            return;

        float scroll =
            Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) < 0.01f)
            return;

        float scrollAmount =
            scroll / 120f;

        holdDistance +=
            scrollAmount * scrollSpeed;

        holdDistance =
            Mathf.Clamp(
                holdDistance,
                minHoldDistance,
                maxHoldDistance
            );
    }


    // =====================================================
    // Move Held Box
    // =====================================================

    private void MoveHeldBox()
    {
        if (heldRigidbody == null)
            return;

        if (holdPoint == null)
            return;

        Vector3 targetPosition =
            holdPoint.position +
            holdPoint.forward *
            holdDistance;

        heldRigidbody.transform.position =
            Vector3.Lerp(
                heldRigidbody.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

        heldRigidbody.transform.rotation =
            Quaternion.Lerp(
                heldRigidbody.transform.rotation,
                holdPoint.rotation,
                moveSpeed * Time.deltaTime
            );
    }


    // =====================================================
    // Drop
    // =====================================================

    private void TryDropBox()
    {
        if (heldRigidbody == null)
            return;

        Rigidbody box = heldRigidbody;
        GameAudio.Play(GameSound.PutDown);
        Collider[] boxColliders = heldColliders;

        // Release from the current held position without moving to the cursor target.
        heldRigidbody = null;
        heldColliders = null;

        // Collider는 아직 OFF 상태!
        StartCoroutine(
            DropUntilGround(
                box,
                boxColliders
            )
        );
    }


    // =====================================================
    // Collider OFF 상태로 떨어뜨리고
    // 바닥 감지 시 Collider ON
    // =====================================================

    private IEnumerator DropUntilGround(
        Rigidbody box,
        Collider[] boxColliders)
    {
        if (box == null)
            yield break;

        // Collider는 계속 OFF
        SetColliders(
            boxColliders,
            false
        );

        // Rigidbody는 떨어질 수 있게 설정
        box.isKinematic = false;
        box.useGravity = true;

        box.linearVelocity =
            Vector3.zero;

        box.angularVelocity =
            Vector3.zero;


        // 박스의 실제 높이 계산
        float halfHeight =
            GetBoxHalfHeight(box);


        while (box != null)
        {
            // 박스 중심에서 아래로 Ray
            Vector3 rayOrigin =
                box.position;

            float rayDistance =
                halfHeight +
                landingCheckDistance;


            Debug.DrawRay(
                rayOrigin,
                Vector3.down * rayDistance,
                Color.green
            );


            if (Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit groundHit,
                rayDistance,
                landingMask,
                QueryTriggerInteraction.Ignore))
            {
                // 박스 바닥이 지면 바로 위에 오도록 위치 보정
                Vector3 position =
                    box.position;

                position.y =
                    groundHit.point.y +
                    halfHeight +
                    landingOffset;

                box.position =
                    position;


                // 속도 제거
                box.linearVelocity =
                    Vector3.zero;

                box.angularVelocity =
                    Vector3.zero;


                // =================================
                // ★ 여기서 처음 Collider ON
                // =================================

                SetColliders(
                    boxColliders,
                    true
                );


                Physics.SyncTransforms();
                GameAudio.PlayBlockedIfTrap(groundHit.collider);


                // 일반 물리 상태
                box.isKinematic = false;
                box.useGravity = true;


                yield break;
            }


            yield return new WaitForFixedUpdate();
        }
    }


    // =====================================================
    // 박스 높이 계산
    // Collider는 꺼져있으므로 Renderer 기준으로 계산
    // =====================================================

    private float GetBoxHalfHeight(
        Rigidbody box)
    {
        Renderer[] renderers =
            box.GetComponentsInChildren<Renderer>();

        if (renderers == null ||
            renderers.Length == 0)
        {
            return 0.5f;
        }

        Bounds bounds =
            renderers[0].bounds;

        for (int i = 1;
             i < renderers.Length;
             i++)
        {
            if (renderers[i] != null)
            {
                bounds.Encapsulate(
                    renderers[i].bounds
                );
            }
        }

        return bounds.extents.y;
    }


    // =====================================================
    // Collider ON/OFF
    // =====================================================

    private void SetColliders(
        Collider[] colliders,
        bool enabled)
    {
        if (colliders == null)
            return;

        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                col.enabled = enabled;
            }
        }
    }


    // =====================================================
    // Reset All Boxes
    // =====================================================

    public void ResetAllBoxes()
    {
        if (heldRigidbody != null)
        {
            SetColliders(
                heldColliders,
                true
            );
        }

        foreach (var pair
            in originalTransforms)
        {
            Rigidbody box = pair.Key;
            BoxOriginalTransform original =
                pair.Value;

            if (box == null)
                continue;

            Collider[] colliders =
                box.GetComponentsInChildren<Collider>(
                    true
                );

            // 리셋할 때 Collider 켜기
            SetColliders(
                colliders,
                true
            );

            box.isKinematic = true;
            box.useGravity = false;

            box.linearVelocity =
                Vector3.zero;

            box.angularVelocity =
                Vector3.zero;

            box.transform.SetPositionAndRotation(
                original.position,
                original.rotation
            );

            box.isKinematic = false;
            box.useGravity = true;

            box.Sleep();
        }

        heldRigidbody = null;
        heldColliders = null;

        holdDistance =
            defaultHoldDistance;
    }


    // =====================================================
    // Parent Tag Search
    // =====================================================

    private Transform FindParentWithTag(
        Transform target,
        string targetTag)
    {
        Transform current = target;

        while (current != null)
        {
            if (current.CompareTag(targetTag))
            {
                return current;
            }

            current =
                current.parent;
        }

        return null;
    }
}