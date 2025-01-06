using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 100f;
    
    [Header("Position Constraints")]
    [SerializeField] private float minHeight = 10f;
    [SerializeField] private float boundaryRange = 50f;  // Başlangıç pozisyonundan ne kadar uzaklaşabileceğimiz

    private float minX, maxX, minZ, maxZ;  // Sınırları runtime'da hesaplayacağız
    private bool isRightClickHeld = false;

    private void Start()
    {
        // Sınırları başlangıç pozisyonuna göre ayarla
        minX = transform.position.x - boundaryRange;
        maxX = transform.position.x + boundaryRange;
        minZ = transform.position.z - boundaryRange;
        maxZ = transform.position.z + boundaryRange;
    }

    private void Update()
    {
        HandleInput();
        ClampPosition();  // ClampHeight yerine daha kapsamlı bir metod
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isRightClickHeld = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isRightClickHeld = false;
        }

        if (isRightClickHeld)
        {
            MoveCamera();
            RotateCamera();
        }
    }

    private void MoveCamera()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0, vertical);
        moveDirection = transform.TransformDirection(moveDirection);
        
        Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        
        // Pozisyonu sınırlar içinde tut
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = transform.position.y;  // Y'yi koru
        newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);
        
        transform.position = newPosition;
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime, Space.World);

        Vector3 currentRotation = transform.eulerAngles;
        float potentialRotation = currentRotation.x + (-mouseY * rotationSpeed * Time.deltaTime);
        
        if (!WouldRotationCauseTooLowHeight(potentialRotation))
        {
            transform.Rotate(Vector3.left, mouseY * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void ClampPosition()
    {
        Vector3 position = transform.position;
        
        // Tüm eksenleri sınırla
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Max(position.y, minHeight);  // Y için sadece minimum
        position.z = Mathf.Clamp(position.z, minZ, maxZ);
        
        transform.position = position;
    }

    private bool WouldRotationCauseTooLowHeight(float newXRotation)
    {
        if (newXRotation > 180) newXRotation -= 360;
        return newXRotation > 80;
    }

    // Debug için görsel sınırlar (isteğe bağlı)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) / 2f, minHeight, (minZ + maxZ) / 2f);
        Vector3 size = new Vector3(maxX - minX, 10f, maxZ - minZ);
        Gizmos.DrawWireCube(center, size);
    }
}