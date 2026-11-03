using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 6f;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.2f, -10f);

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
}
