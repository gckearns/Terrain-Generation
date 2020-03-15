using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Vector2 sensitivity;

    public bool invertXAxis;
    public bool invertYAxis;

    public float horizontalRotateMin = -80;
    public float horizontalRotateMax = 80;

    public float verticalRotateMin = -70;
    public float verticalRotateMax = 75;

    public Rigidbody playerRigidbody;
    public Camera playerCamera;

    Vector2 mouseInput;
    float horizontalRotate = 0;
    float verticalRotate = 0;


    // Use this for initialization
    void Start()
    {
        //Debug.Log(playerCamera.transform.localEulerAngles);
    }

    // Update is called once per frame
    void Update()
    {
        mouseInput = new Vector2(invertXAxis ? -Input.GetAxisRaw("Mouse X") : Input.GetAxisRaw("Mouse X"), invertYAxis ? Input.GetAxisRaw("Mouse Y") : -Input.GetAxisRaw("Mouse Y"));

        //horizontalRotate += mouseInput.x * sensitivity.x * Time.deltaTime;
        //horizontalRotate = Mathf.Clamp(horizontalRotate, horizontalRotateMin, horizontalRotateMax);

        verticalRotate += mouseInput.y * sensitivity.y;
        verticalRotate = Mathf.Clamp(verticalRotate, verticalRotateMin, verticalRotateMax);

        playerCamera.transform.localEulerAngles = new Vector3(verticalRotate, horizontalRotate, 0);
    }

    void FixedUpdate()
    {
        Quaternion targetRotation = playerRigidbody.rotation * Quaternion.AngleAxis(mouseInput.x * sensitivity.x, Vector3.up);
        playerRigidbody.MoveRotation(targetRotation);
    }
}
