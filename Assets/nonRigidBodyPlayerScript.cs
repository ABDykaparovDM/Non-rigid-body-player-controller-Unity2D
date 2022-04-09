using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nonRigidBodyPlayerScript : MonoBehaviour
{
    Vector2 PzeroPos; //Player ZERO POSition, contains the position of the player and is being used to modify it
    float speed; //walk speed

    public float g = 9.81f; //free fall acceleration
    public float fallSpeed;
    public float fallSpeedCap; //fall speed limitations
    public float freeFallDistanceCap; // limits the distance where fall speed could be aplied, needed to prevent any risks of colliders clip

    public float jumpForce; // jump velocity, increases with the space key being held
    public float jumpForceCap; // jump velocity cap (also prevents player from hold space for too long and flying to the moon)
    public float jumpAcceleration; // acceleration of jumpForce 
    public bool canJump; // can the player jump?
    public bool jump; // did the player jump?
    public bool midJump; // the player is still in their current jump?
    public float noGravityTime = 0f; // time spent with limited gravitation during the jump
    public float noGravityCap; // limits the duration of the noGravityTime
    public int apexSpeedMod; //walk speed modifier during the nogravity part of the jump
    public float apexSpeed;  //walk speed during the nogravity part of the jump

    public LayerMask Roof; // finds a ground above the player (lets player to clip through platforms right above them)
    public LayerMask Floor; // finds a ground/platform below the player
    public LayerMask Wall; // finds a ground/platform on a side of the player

    public float RoofY; //Y axis of ground above the player
    public float FloorY; //Y axis of ground/platform below the player
    public float L_floorY; //x axis of ground/platform below the player
    public float R_floorY; //X axis of ground/platform above the player
    public float L_WallX; //x axis of ground/platform below the player
    public float R_WallX; //X axis of ground/platform above the player

    public float distanceRoof; //distance between the player and the ground/platform above the player
    public float distanceFloor; //distance between the player and the ground/platform below the player
    public float distanceWallL; //distance between the player and the ground/platform on the left of the player
    public float distanceWallR; //distance between the player and the ground/platform on the right of the player

    public float distanceFloorL; //distance between the player and the ground/platform on the left diagonal of the player
    public float distanceFloorR; //distance between the player and the ground/platform on the right diagonal of the player

    //public Vector3 raycast_pos;

    void Start()
    {
        PzeroPos = transform.position;
        jump = false;
        fallSpeed = 0;
        fallSpeedCap = 0.5f;
        jumpForce = 0;
        speed = 0.2f;
        apexSpeedMod = 2;
        apexSpeed = speed * apexSpeedMod;
        jumpAcceleration = 6f;
        jumpForceCap = 1f;
        noGravityCap = 0.25f;
        Floor = LayerMask.GetMask("Ground") | LayerMask.GetMask("Platform");
        Wall = LayerMask.GetMask("Ground") | LayerMask.GetMask("Platform");
        Roof = LayerMask.GetMask("Ground") | LayerMask.GetMask("Platform");


    }

    void FixedUpdate()
    {
        RaycastHit2D hitUp = Physics2D.Raycast(transform.position, Vector2.up, 2, Roof); // raycast upwards
        RaycastHit2D hitDown = Physics2D.Raycast(transform.position, Vector2.down, 2, Floor); // raycast downwards
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, 2, Wall); // raycast to the left
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, 2, Wall); // raycast to the right

        RaycastHit2D hitDiagonalLD = Physics2D.Raycast(transform.position - new Vector3(0.48f, 0, 0), Vector2.down, 2, Floor); // raycast to the left-down diagonaly 
        RaycastHit2D hitDiagonalRD = Physics2D.Raycast(transform.position + new Vector3(0.48f, 0, 0), Vector2.down, 2, Floor); // raycast to the right-down diagonaly 
        //raycast_pos = hitDiagonalRD.transform.position;

        if (hitUp.collider != null) //calcultes distance between the player foot and the ground
        {
            RoofY = hitUp.point.y;
            distanceRoof = Mathf.Abs(RoofY - transform.position.y) - 0.5f;
        }
        if (hitDown.collider != null) //calcultes distance between the player and the ground
        {
            FloorY = hitDown.point.y;
            distanceFloor = Mathf.Abs(FloorY - transform.position.y) - 0.5f;
        }
        if (hitLeft.collider != null) //calcultes distance between the player foot and the ground
        {
            L_WallX = hitLeft.point.x;
            distanceWallL = Mathf.Abs(L_WallX - transform.position.x) - 0.5f;
        }
        if (hitRight.collider != null) //calcultes distance between the player foot and the ground
        {
            R_WallX = hitRight.point.x;
            distanceWallR = Mathf.Abs(R_WallX - transform.position.x) - 0.5f;
        }
        if (hitDiagonalLD.collider != null) //calcultes distance between the player foot and the ground
        {
            L_floorY = hitDiagonalLD.point.y;
            distanceFloorL = Mathf.Abs(L_floorY - transform.position.y) - 0.5f;
        }
        if (hitDiagonalRD.collider != null) //calcultes distance between the player and the ground
        {
            R_floorY = hitDiagonalRD.point.y;
            distanceFloorR = Mathf.Abs(R_floorY - transform.position.y) - 0.5f;
        }

        if (distanceFloor > 0.01f && distanceFloorL > 0.01f && distanceFloorR > 0.01f) // increases cap depending on the fall speed 
        {
            freeFallDistanceCap = 0.5f + fallSpeed * 0.5f;
        }
        else
        {
            freeFallDistanceCap = 0.5f;
        }

        if (distanceFloor >= freeFallDistanceCap && distanceFloorL >= freeFallDistanceCap && distanceFloorR >= freeFallDistanceCap) // accelerates fall speed until the player hits the ground or the fall speed cap reached
        {
            if (fallSpeed <= fallSpeedCap)  //makes sure that fall speed didn't reach its cap
            {     
                    canJump = false;
                    fallSpeed += Time.deltaTime * g / 3;
            }
            PzeroPos.y -= fallSpeed;
        }
        else if (!midJump) //ends fall and jump once the player "hits" the ground by entering the free Fall Distance Cap
        {
            fallSpeed = 0;
            jumpForce = 0;
            PzeroPos.y = Mathf.Max(FloorY, Mathf.Max(L_floorY, R_floorY)) + 0.52f;
            canJump = true;
            jump = false;
        }

        if (Input.GetKey("d") && distanceWallR >= 0) //x axis movement (needed to be in the fixed update for smooth movement)
        {
            PzeroPos.x += speed;
        }
        if (distanceWallR < 0.02f && !Input.GetKey("a"))
        {
            PzeroPos.x = R_WallX - 0.5f; 
        }
        if (Input.GetKey("a") && distanceWallL >= 0)
        {
            PzeroPos.x -= speed;
        }
        if (distanceWallL < 0.02f && !Input.GetKey("d"))
        {
            PzeroPos.x = L_WallX + 0.5f;
        }


        if (distanceRoof >= 0.2562f)
        {
            if (jumpForce <= jumpForceCap && jump && Input.GetKey("space")) // detects wether the space key is being held during the jump, allowing the player to vary their jump height
            {
                jump = true;
                jumpForce += Time.deltaTime * jumpAcceleration;
                PzeroPos.y += jumpForce;
                transform.position = PzeroPos;
                fallSpeed = 0;
            }
            else // decreases gravity at the end of the full height jump (for a short period of time)
            {
                if (noGravityTime <= noGravityCap && midJump && distanceFloor >= 1f)
                {
                    noGravityTime += Time.deltaTime;
                    speed = apexSpeed;
                    fallSpeed = 0;
                }
                else
                {
                    noGravityTime = 0;
                    speed = apexSpeed / apexSpeedMod;
                    midJump = false;
                }
            }
        } else if (RoofY != 0)
        {
            canJump = false;
            jump = false;
            midJump = false;
        }
        

        transform.position = PzeroPos; // changes player position
    }

    void Update()
    {
        if (!jump && Input.GetKeyDown("space") && canJump && !midJump) //detects space key input and starts the jump process if allowed
        {
            jump = true;
            midJump = true;
            canJump = false;
        }
        

        if (Input.GetKeyUp("space")) // detects space key release and shortens the jump
        {
            midJump = false;
            jumpForce = jumpForceCap;
        }
        
    }
}
