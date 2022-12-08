using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Animation reference: https://www.youtube.com/watch?v=HVCsg_62xYw
// a bit of movement https://www.youtube.com/watch?v=VslgzNfibhs


public class Character : MonoBehaviour
{
	private CharacterController controller;
	private Camera cam;
	private Animator anim;

	//camera parameters
	public float cameraSpeedX = 10.0f;
	public float cameraSpeedY = 10.0f;
	public float cameraSpeedZoom = 10.0f;
	public float cameraZoomFloor = 5f;

	//behavior parameters
	public float dashSpeed = 300.0f;
	public float runSpeed = 60.0f;
	public float walkSpeed = 10.0f;
	public float cameraDistance = 15;
	public float gravity = 1f;
	public float animationJumpFallThreshhold = 3.0f;
	public float jumpHeight = .4f;
	public float turnSpeed = 100f;

	//internal control
	private bool mouseRotate = false;
	private bool jump = false;
	private bool jumpFall = false; //used to prevent unnecessary flaggings with debugging

	//other
	public float speed = 5.0f;
	private float vSpeed = 0.0f;
	private float yaw = 0.0f;
	private Vector3 v_movement;

	void Start()
	{
		controller = GetComponent<CharacterController>();
		cam = GetComponentInChildren<Camera>();
		anim = GetComponentInChildren<Animator>();
	}

	// Update is called once per frame
	void Update()
	{

		//update camera rotations===============================
		//enable camera movement on down press
		if (Input.GetMouseButtonDown(1)) mouseRotate = true;
		//disable camera movement on depress
		if (Input.GetMouseButtonUp(1)) mouseRotate = false;

		//perform rotation actions
		if (mouseRotate)
		{
			//update x
			cam.transform.RotateAround(this.transform.position, this.transform.up, cameraSpeedX * Input.GetAxis("Mouse X"));
			//update y
			cam.transform.RotateAround(this.transform.position, cam.transform.right, -cameraSpeedY * Input.GetAxis("Mouse Y"));
			//update zoom
			cam.transform.Translate(0, 0, cameraSpeedZoom * Input.GetAxis("Mouse ScrollWheel"), cam.transform);
			//floor zoom
			float distance = Vector3.Distance(cam.transform.position, this.transform.position);
			if (Mathf.Abs(distance) < cameraZoomFloor)
			{
				//if zoom is too close, back out to floor
				cam.transform.Translate(0, 0, -cameraZoomFloor + Mathf.Max(distance, 0), cam.transform);
			}
		}



		//yaw += Input.GetAxis("Mouse X");


		//update camera orientation with respect to mouse
		//this.transform.Rotate(0, Input.GetAxis("Mouse X"), 0);

		//cam.transform.RotateAround(this.transform.position, this.transform.right, -Input.GetAxis("Mouse Y"));


		//update deer location with respect to keyboard
		float forward_velocity;
		//check for movement state
		if (Input.GetAxis("Vertical") != 0)
		{
			//check for sprint
			if (Input.GetKey("left shift") || Input.GetKey("right shift") || Input.GetKey("left ctrl"))
			{
				//update animation state
				//dash
				if (Input.GetKey("left ctrl"))
				{
					anim.SetFloat("moveSpeed", 2);
					forward_velocity = Input.GetAxis("Vertical") * dashSpeed;
				}

				//regular run
				else
				{
					anim.SetFloat("moveSpeed", 2);
					forward_velocity = Input.GetAxis("Vertical") * runSpeed;
				}
			}
			else
			{
				//update animation state
				anim.SetFloat("moveSpeed", 1);
				forward_velocity = Input.GetAxis("Vertical") * walkSpeed;
			}
		}
		else
		{
			//update animation state
			anim.SetFloat("moveSpeed", 0);
			forward_velocity = 0f;
		}

		//update deer airborneness
		if (controller.isGrounded)
		{
			//test for landing
			if (vSpeed != 0.0f && !jump)
			{
				vSpeed = 0.0f;
				//update animation
				anim.SetTrigger("jumpLand");
			}
			if (Input.GetKey(KeyCode.Space) && !jump)
			{
				anim.SetTrigger("jump");
				jump = true;
			}
			//change leap velocity when animation actually leaps
			if (jump && anim.GetCurrentAnimatorStateInfo(0).IsName("Jump Up"))
			{
				jump = false;
				jumpFall = true;
				vSpeed = jumpHeight;
			}
		}
		else
		{
			//test animation falling trigger
			if (jumpFall && vSpeed < animationJumpFallThreshhold)
			{
				anim.SetTrigger("jumpFall");
				jumpFall = false;
				//Debug.Log("vSpeed " + vSpeed);
			}

			//update velocity
			vSpeed -= gravity * Time.deltaTime;
		}

		//process movement=================
		//move forward
		v_movement = anim.transform.forward * forward_velocity * Time.deltaTime;
		controller.Move(v_movement);

		//gravity / jump acceleration
		controller.Move(new Vector3(0, vSpeed, 0));

		//rotate model
		controller.transform.Rotate(Vector3.up * Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime);

		RaycastHit hit;
		if (Physics.Raycast(anim.transform.position + Vector3.up, Vector3.down, out hit))
		{
			// anim.transform.rotation = Quaternion.FromToRotation(anim.transform.up, hit.normal) * anim.transform.rotation;
			//anim.transform.rotation = Quaternion.RotateTowards(anim.transform.rotation, Quaternion.FromToRotation(anim.transform.up, hit.normal) * anim.transform.rotation, 0.025f);
		}

		//m

		//Vector3 move = new Vector3(Input.GetAxis("Horizontal") * speed, vSpeed, Input.GetAxis("Vertical") * speed);

		//controller.Move(Quaternion.Euler(0, yaw, 0) * move * Time.deltaTime);
	}
}
