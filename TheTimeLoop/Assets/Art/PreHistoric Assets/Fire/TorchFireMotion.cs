using UnityEngine;

public class TorchFireMotion : MonoBehaviour
{
    [Header("References")]
    public Rigidbody torchRb;

    [Header("Motion")]
    [SerializeField] private float positionStrength = 0.003f;
    [SerializeField] private float rotationStrength = 6f;
    [SerializeField] private float smooth = 8f;

    private Vector3 startLocalPos;
    private Quaternion startLocalRot;

    void Awake()
    {
        startLocalPos = transform.localPosition;
        startLocalRot = transform.localRotation;
    }

    void LateUpdate()
    {
        if (torchRb == null)
            return;

        // Unity 6
        Vector3 velocity = torchRb.linearVelocity;

        // Unity 2022 kullanýyorsan bunu kullan:
        // Vector3 velocity = torchRb.velocity;

        Vector3 localVelocity =
            transform.parent.InverseTransformDirection(velocity);

        Vector3 targetPos =
            startLocalPos -
            new Vector3(localVelocity.x, 0f, localVelocity.z) * positionStrength;

        Quaternion targetRot =
            startLocalRot *
            Quaternion.Euler(
                localVelocity.z * rotationStrength,
                0f,
                -localVelocity.x * rotationStrength);

        transform.localPosition =
            Vector3.Lerp(
                transform.localPosition,
                targetPos,
                Time.deltaTime * smooth);

        transform.localRotation =
            Quaternion.Slerp(
                transform.localRotation,
                targetRot,
                Time.deltaTime * smooth);
    }
}