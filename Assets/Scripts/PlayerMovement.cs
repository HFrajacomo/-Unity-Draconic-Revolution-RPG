﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

	public CharacterController controller;
	public float speed = 5f;
	public float gravity = -19.62f;
	public float jumpHeight = 5f;
    public LayerMask layerMask;
    public bool isGrounded;
	public Vector3 velocity;

    public Vector3 move;
    private int jumpticks = 6; // Amount of ticks the skinWidth will stick to new blocks
    public MainControllerManager controls;


    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;

        // If is Grounded
    	if(isGrounded){
    		velocity.y = -1f;
    	}
        // If not, gravity affects
        else{
            velocity.y += gravity * Time.deltaTime;
        }

        float x = controls.movementX;//playerControls.Player.Movement.ReadValue<Vector2>().x;
        float z = controls.movementZ;//playerControls.Player.Movement.ReadValue<Vector2>().y;

        move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);


        if(controls.jumping && isGrounded){
            velocity.y = jumpHeight;
            jumpticks = 10;
            controller.skinWidth = 0.4f;
        }

        // Block Sticking
        if(jumpticks > 0){
            jumpticks--;
        }
        else{
            controller.skinWidth = 0f;
        }

        // Gravity
        controller.Move(velocity * Time.deltaTime);
    }

}
