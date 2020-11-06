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

        if(!controls.freecam){

            // If is Grounded
        	if(isGrounded){
        		velocity.y = -1f;
        	}
            // If not, gravity affects
            else{
                velocity.y += gravity * Time.deltaTime;
            }

            float x = controls.movementX;
            float z = controls.movementZ;

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

            // If gravity hack is toggled
            if(controls.gravityHack){
                velocity.y = 10f;
            }

            // Gravity
            controller.Move(velocity * Time.deltaTime);
        }

        // If on Freecam
        else{

            float x = controls.movementX;
            float z = controls.movementZ;

            move = transform.right * x + transform.forward * z;
            controller.Move(move * speed * Time.deltaTime);

            if(controls.jumping){
                velocity.y = 5;
                controller.Move(velocity * Time.deltaTime);
                velocity.y = 0;
            }
            else if(controls.shifting){
                velocity.y = -5;
                controller.Move(velocity * Time.deltaTime);
                velocity.y = 0;
            }

            

        }
    }

}
