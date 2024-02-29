using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

[RequireComponent(typeof(PlayerSetting))]
public class PlayerController : MonoBehaviour
{
     // get input and control PlayerChara

     // public
     public static Vector3 mouseHit { get; private set; } //if mouse ray didn't hit anything, pretend it hit the skybox
     public bool enableMoveInput = true; //lose control when eg. knocked away / stunned
     public float groundedSphereCastRadius = 0.8f; //smaller then chara so bumping into wall don't count


     void Start()
     {
          TEST.OnConnect += OnConnect;
          TEST.OnDisconnect += OnDisconnect;

          SetupStandAlone();
     }
     void Update()
     {
          if (rb != null && col != null)
          {
               if (Input.GetKeyDown(KeyCode.Alpha5)) TEST_Blink(); //TEST

               UpdateInput();
          }
     }

     void FixedUpdate()
     {
          _checkGrounded = false; //reset flag first

          if (Time.time > tSlowEnd) //slow timer
               speedMultiple = 100;

          BeSlippery(!isGrounded); //prevent sticking to the wall in air

          UpdateRotation();
          UpdatePosition();
          UpdateJump();
     }


     // init  ---------------------------------------------------------------------

     // for network
     Rigidbody net_rb;
     Collider net_col;
     bool isConnected { get => TEST.isConnected; }

     void OnConnect()
     {
          net_rb = NetworkChara.myChara.GetComponent<Rigidbody>();
          net_col = NetworkChara.myChara.GetComponent<Collider>();
     }
     void OnDisconnect()
     {
          //
     }

     // for standalone 
     Rigidbody this_rb; //for standalone
     Collider this_col; //for standalone
     void SetupStandAlone()
     {
          this_rb = GetComponentInChildren<Rigidbody>();
          this_col = GetComponentInChildren<Collider>();

          if (this_rb == null)
               this_rb = gameObject.AddComponent<Rigidbody>();
          this_rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; //freeze rotation

          if (this_col == null)
               this_col = gameObject.AddComponent<CapsuleCollider>();
          var capsule = this_col as CapsuleCollider;
          capsule.center = new Vector3(0, 1, 0);
          capsule.height = 2;
     }


     // TEST ---------------------------------------------------------------------

     float blinkDist = 20;
     void TEST_Blink()
     {
          var dir = cam.transform.rotation * Vector3.forward;
          dir.y = 0;

          NetworkChara.myChara.transform.position += dir * blinkDist;

          Debug.Log("BLINK");
     }



     // input ----------------------------------------------------------------------------------
     float inputH; //left right
     float inputV; //forward
     bool inputJump;
     bool isRunning = false;

     void UpdateInput()
     {
          if (enableMoveInput)
          {
               inputH = Input.GetAxisRaw("Horizontal");
               inputV = Input.GetAxisRaw("Vertical");
               isRunning = Input.GetKey(KeyCode.LeftShift);
               if (!inputJump) inputJump = Input.GetKeyDown(KeyCode.Space);
          }
          else
          {
               inputH = 0;
               inputV = 0;
               isRunning = false;
               inputJump = false;
          }
     }


     // move -------------------------------------------------------------------------------------
     Rigidbody rb { get => isConnected ? net_rb : this_rb; }
     Collider col { get => isConnected ? net_col : this_col; }
     Camera cam { get => Camera.main; }

     PlayerSetting status { get => PlayerSetting.inst; }
     float maxSpeed { get => speedMultiple / 100f * (isGrounded ? isRunning ? status.maxSpeedRun : status.maxSpeed : status.maxSpeed); }
     float accStrength { get => isGrounded ? status.accStrength : status.accStrengthAir; } //acceleration as a ratio to maxSpeed. 4=car, 10=ninja (physic friction=5)
     float magic = 5.5f; //since we are lerping, and with friction, +1 is needed to reach maxSpeed
     float rotateLerp { get => status.rotateLerp; }
     float jumpVelocity { get => status.jumpVelocity; }
     int jumpCount { get => status.jumpCount; } //2 = double jump
     int jumpRemain;


     public float fun;
     public float maxSPD;
     void UpdatePosition()
     {
          fun = speedMultiple;
          maxSPD = maxSpeed;

          if (inputH == 0 && inputV == 0)
               return;

          var dir = (inputH * cam.transform.right + inputV * cam.transform.forward).normalized;
          var desired_velocity = dir * (maxSpeed + magic);
          var acc = (desired_velocity - rb.velocity) / Time.fixedDeltaTime;
          acc.y = 0;
          acc = Vector3.ClampMagnitude(acc, maxSpeed * accStrength);

          rb.AddForce(acc, ForceMode.Acceleration);
     }

     void UpdateJump()
     {
          if (isGrounded)
               jumpRemain = jumpCount;

          if (inputJump)
          {
               inputJump = false; //comsume input
               if (jumpRemain > 1)
               {
                    jumpRemain--;
                    isGrounded = false;

                    rb.AddForce(jumpVelocity * Vector3.up, ForceMode.VelocityChange);
               }
          }

     }

     void UpdateRotation()
     {
          var ray = cam.ScreenPointToRay(Input.mousePosition);
          var me = rb.transform.position;

          if (Physics.Raycast(ray, out var hit))
          {
               mouseHit = hit.point;
          }
          else
          {
               mouseHit = ray.origin + ray.direction * 1000f; //pretend we hit the skybox

               //this does not equal to raycast, since player/camera pos/rot are all different
               //but the behaviour is entertaining, as if player suddenly look into some far far away place
          }

          var rot = Quaternion.LookRotation(new Vector3(mouseHit.x - me.x, 0, mouseHit.z - me.z)); //remove y tilt
          rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, rot, rotateLerp * Time.deltaTime);

     }


     // slow debuff  ---------------------------------------------------------------------------------
     float speedMultiple = 100;
     float tSlowEnd;
     public void Slow(float _speedMultiple, float sec)
     {
          if (_speedMultiple < speedMultiple) //only apply the stronger slow
          {
               speedMultiple = Mathf.Clamp(_speedMultiple, 0, 100);
               tSlowEnd = Time.time + sec;
          }
     }


     // check if grounded ----------------------------------------------------------------------------
     bool isGrounded { get => UpdateIsGrounded(); set => _isGrounded = value; }
     bool _isGrounded;
     bool _checkGrounded;
     float _small = 0.01f;

     bool UpdateIsGrounded()
     {
          if (!_checkGrounded)
          {
               _checkGrounded = true;

               var origin = rb.transform.position + Vector3.up * (groundedSphereCastRadius + _small); //+a small number to avoid sphere touching the ground initially (it will ignore these obj)
               var dist = groundedSphereCastRadius + _small * 2; //add 1 back, add another 1 as allowed error
                                                                 // var dist =  _small * 2; //add 1 back, add another 1 as allowed error
               _isGrounded = Physics.SphereCast(origin, groundedSphereCastRadius, Vector3.down, out _, dist, LayerMaskUtil.wall_mask);
          }

          return _isGrounded;
     }


     // prevent sticking to wall in air ----------------------------------------------------------------------------
     float friction = 5f;
     void BeSlippery(bool slip)
     {
          col.material.dynamicFriction = slip ? 0 : friction;
          col.material.staticFriction = slip ? 0 : friction;
          col.material.frictionCombine = slip ? PhysicMaterialCombine.Minimum : PhysicMaterialCombine.Average;
     }


}
