using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float moveSpeed;
    public Rigidbody rBody;

    Vector2 moveInputLateral;
    float moveInputVertical;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        moveInputLateral = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        float up = Input.GetKey(KeyCode.Space) ? 1f : 0f;
        float down = Input.GetKey(KeyCode.LeftShift) ? -1f : 0f;
        moveInputVertical = up + down;
	}

    void FixedUpdate()
    {
        //Vector3 directionVertical = (rBody.transform.forward * moveInput.y);
        //Vector3 directionHorizontal = (rBody.transform.right * moveInput.x);
        Vector3 moveDirection = new Vector3(moveInputLateral.x, moveInputVertical, moveInputLateral.y).normalized;
        Vector3 moveVector = rBody.transform.TransformDirection(moveDirection * moveSpeed * Time.deltaTime);
        rBody.MovePosition(rBody.position + moveVector);
    }
}
