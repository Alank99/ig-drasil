using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour
{
    [Header("Referencias")]
    Rigidbody2D playerRB;
    public Transform playerSprites;
    public Animator playerAnim;

    [Header("Movimiento lateral")]

    public float maxSpeedX;

    public float airtimeControlReduction;
    public Vector2 sensitivity;
    public Vector2 initialPushWhenGrounded;

    public float spriteScale;

    [Header("Cosas para el brinco")]

    /// <summary>
    /// Explica la velocidad que se le aplica al jugador después de presionar brincar
    /// </summary>
    public float jumpForce;
    /// <summary>
    /// La gravedad que se le va a aplicar cuando precione espacio
    /// </summary>
    public float initialGravity;
    /// <summary>
    /// La gravedad el resto del tiempo
    /// </summary>
    public float finalGravity;
    /// <summary>
    /// Cual es el tiempo máximo que el jugador puede brincar 
    /// </summary>
    public float maxJumpTime;

    public AnimationCurve jumpCurve;

    [Header("Cosas para el dash")]
    [Tooltip("If the player has unlocked the dash ability")]
    public int has_dash;
    public Vector2 dashForce;
    //If the player currently can dash
    private bool hasDash = true;
    //1=left, 0=right
    private int moving_left = 0;

    [Header("Estadísticas del sistema")]

    public bool grounded;
    public bool jumping;
    public float elapsed;
    public Vector2 movement;

    [Header("Cosas para buffs")]
    public float buffSpeed = 1f;
    public float buffJump = 1f;
    public float buffAttackDamage = 1f;
    public float buffMaxSpeed = 0f;
    public float buffDash = 1f;
    public float buffAttackSpeed = 0f;

    public List<buff> buffs = new List<buff>();

    public static playerController playerSingleton;
    public static HealthManager manager;

    private void Start() {
        if (playerSingleton == null){
            playerSingleton = this;
            manager = this.GetComponent<HealthManager>();
        }else{
            Destroy(gameObject);
            return;
        }
        playerRB = gameObject.GetComponent<Rigidbody2D>();
        grounded = true;
    }

    public void TouchGrass(){
        grounded = true;
        playerAnim.SetTrigger("fall");
        stopJump();
        hasDash = true;
    }
    public void StopTouchGrass(){
        grounded = false;
    }

    private void Update() {
        var cacheSens = grounded ? sensitivity : sensitivity * airtimeControlReduction;
        playerRB.AddForce(new Vector2(movement.x * cacheSens.x * Time.deltaTime * buffSpeed, 
                                      movement.y * cacheSens.y * Time.deltaTime * buffSpeed));

        if (playerRB.velocity.x >  maxSpeedX + buffMaxSpeed){
            playerRB.velocity = new Vector2(maxSpeedX, playerRB.velocity.y);
            playerSprites.localScale = new Vector3(-spriteScale,spriteScale,spriteScale);
        }

        if (playerRB.velocity.x <  -maxSpeedX - buffMaxSpeed){
            playerRB.velocity = new Vector2(-maxSpeedX, playerRB.velocity.y);
            playerSprites.localScale = new Vector3(spriteScale,spriteScale,spriteScale);
        }
    }

    /// <summary>
    /// Se ejecuta cuando se presiona el botón de Dodge y se regenera al tocar el piso
    /// </summary>
    /// <param name="value"></param>
    public void OnDoge(){
        if (has_dash == 0) return;
        Vector2 force = new Vector2(0, 0.3f);
        //Checa si toco el piso antes del dash
        if (hasDash){
            if (moving_left == 0 && playerRB.velocity.x != 0)
                force.x = dashForce.x * buffDash;
            else if (moving_left == 1 && playerRB.velocity.x != 0)
                force.x = -dashForce.x * buffDash;

            //Checa si hay algo en la direccion de la fuerza
            RaycastHit2D hit = Physics2D.Raycast(transform.position, force, Mathf.Abs(force.x), LayerMask.GetMask("Ground"));
            if (hit.collider!= null)
                StartCoroutine(MoveFunction(hit.point));
            else
                StartCoroutine(MoveFunction(playerRB.position + force));

            hasDash = false;
        }
    }

    IEnumerator MoveFunction(Vector2 newPosition)
    {
        float timeSinceStarted = 0f;
        while (true)
        {
            timeSinceStarted += Time.deltaTime;
            playerRB.MovePosition(Vector3.Lerp(playerRB.position, newPosition, timeSinceStarted));

            // If the object has arrived, stop the coroutine
            if ((Vector3.Distance(playerRB.position, newPosition) < 1f) || timeSinceStarted > 0.5f)
            {
                yield break;
            }

            // Otherwise, continue next frame
            yield return null;
        }
    }

    /// <summary>
    /// Utilizado por el player controller, regresa que tanto esta movido algo
    /// </summary>
    /// <param name="value"></param>
    public void OnMove(InputValue value){
        movement = value.Get<Vector2>();

        if (movement.x > 0)
            moving_left = 0;
        else if (movement.x < 0)
            moving_left = 1;

        if (movement.x == 0){
            playerRB.velocity = new Vector2(playerRB.velocity.x/2, playerRB.velocity.y);
            playerAnim.SetBool("walkLeft", false);
        }
        else {
            playerAnim.SetBool("walkLeft", true);
        }

        if (grounded){
            playerRB.velocity = new Vector2(movement.x * initialPushWhenGrounded.x, playerRB.velocity.y);
        }
    }

    /// <summary>
    /// Esta madre se corre asincrona. Se inicia con startCoroutine y termina cuando se sale o stopcoroutine
    /// </summary>
    /// <returns></returns>
    IEnumerator jumpController(){
        jumping = true;
        playerAnim.SetTrigger("jump");
        // la x se mantiene para que no interferimos con ella
        playerRB.velocity = new Vector2(playerRB.velocity.x, jumpForce * buffJump);
        playerRB.gravityScale = initialGravity;

        // Cálculos generales de tiempo
        var startTime = Time.time;
        elapsed = Time.time - startTime;
        // relga de 3 para que sepamos que porcentaje llevamos (va de 0 a 1)
        var percentageElapsed = elapsed/maxJumpTime;

        var localGravityScale = initialGravity;

        while (elapsed < maxJumpTime){
            elapsed = Time.time - startTime;
            percentageElapsed = elapsed/maxJumpTime;

            // vamos a hacer una interpolación lineal de donde estamos, a donde debemos de estar
            localGravityScale = Mathf.Lerp(initialGravity, finalGravity, jumpCurve.Evaluate(percentageElapsed)); // el jump curve es para conseguir el y
            playerRB.gravityScale = localGravityScale;

            // Aquí esperamos al siguiente calculo de física
            yield return new WaitForFixedUpdate();
        }

        playerRB.gravityScale = initialGravity;
        jumping = false;
    }


    /// <summary>
    /// Termina la corutina y baja al personaje
    /// </summary>
    private void stopJump(){
        StopCoroutine("jumpController");
        playerAnim.ResetTrigger("jump");
        playerRB.gravityScale = finalGravity;
    }

    /// <summary>
    /// Se corre cuando se preciona espacio y cuando se termina de precionar
    /// </summary>
    /// <param name="state"></param>
    public void OnJump(InputValue state){
        if (state.Get<float>() > 0.5f){ // diferencia entre preciona y deja de
            // Nota: este es cuando se inicia el brinco
            if (grounded)
                StartCoroutine("jumpController");
        }
        else{
            // Aquí es cuando se termina el brinco
            stopJump();
        }
    }
    
    /// <summary>
    /// gets the current mouse pos, and returns the vector from the point of reference to the mouse
    /// </summary>
    /// <param name="pointOfReference">The point that is being looked at</param>
    /// <returns></returns>
    public static Vector2 mousePosVector(Vector2 pointOfReference){
        return (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - pointOfReference;
    }

    public void addBuff(buff buffToAdd){
        StartCoroutine(buffToAdd.buffTimer());
    }
}

/// <summary>
/// Class that defines a buff type. it needs a buffType, a value and a duration
/// </summary>
[System.Serializable]
public class buff {
    [Tooltip("The type of buff that this is")]
    public buffTypes type;
    [Tooltip("The amount of type that will get affected")]
    public float value;
    [Tooltip("How long will the buff last")]
    public float duration;

    public buff(buffTypes type, float value, float duration){
        this.type = type;
        this.value = value;
        this.duration = duration;
    }

    private void testAndRemoveBuffs(){
        playerController.playerSingleton.buffs.Remove(this);

        if (playerController.playerSingleton.buffs.Count == 0)
        {
            // remove buff
            playerController.playerSingleton.buffJump = 1f;
            playerController.playerSingleton.buffSpeed = 1f;
            playerController.playerSingleton.buffAttackDamage = 1f;
            playerController.playerSingleton.buffMaxSpeed = 0f;
        }
    }

    /// <summary>
    /// Call this when the player picks up the buff!!!
    /// </summary>
    /// <returns></returns>
    public IEnumerator buffTimer(){
        playerController.playerSingleton.buffs.Add(this);
        // apply buff

        var buffApplied = true;

        switch (type)
        {
            case buffTypes.speed:
                if (playerController.playerSingleton.buffSpeed < 5f){
                    playerController.playerSingleton.buffSpeed += value;
                    buffApplied = true;
                }
                break;
            case buffTypes.maxSpeed:
                if (playerController.playerSingleton.buffMaxSpeed < 5f){
                    playerController.playerSingleton.buffMaxSpeed += value;
                    buffApplied = true;
                }
                break;
            case buffTypes.jump:
                if (playerController.playerSingleton.buffJump < 5f){
                    playerController.playerSingleton.buffJump += value;
                    buffApplied = true;
                }
                break;
            case buffTypes.dash:
                if (playerController.playerSingleton.buffDash < 5f){
                    playerController.playerSingleton.buffDash += value;
                    buffApplied = true;
                }
                break;
            case buffTypes.damage:
                if (playerController.playerSingleton.buffAttackDamage < 5f){
                    playerController.playerSingleton.buffAttackDamage += value;
                    playerController.manager.player_info.attack *= playerController.playerSingleton.buffAttackDamage;
                    buffApplied = true;
                }
                break;
            case buffTypes.attackSpeed:
                if (playerController.playerSingleton.buffAttackSpeed < 5f){
                    playerController.playerSingleton.buffAttackSpeed += value;
                    buffApplied = true;
                }
                break;
            case buffTypes.health:
                HealthManager.healthSingleton.HealInternal((int)value);
                playerController.playerSingleton.buffs.Remove(this);
                testAndRemoveBuffs();
                yield break;
            default:
                break;
        }

        // stop if no buff was applied
        if (!buffApplied){
            playerController.playerSingleton.buffs.Remove(this);
            testAndRemoveBuffs();
            yield break;
        }

        // delay for duration
        yield return new WaitForSeconds(duration);

        // remove buff
        switch (type)
        {
            case buffTypes.speed:
                playerController.playerSingleton.buffSpeed -= value;
                break;
            case buffTypes.maxSpeed:
                playerController.playerSingleton.buffMaxSpeed -= value;
                break;
            case buffTypes.jump:
                playerController.playerSingleton.buffJump -= value;
                break;
            case buffTypes.dash:
                playerController.playerSingleton.buffDash -= value;
                break;
            case buffTypes.damage:
                playerController.manager.player_info.attack /= playerController.playerSingleton.buffAttackDamage;
                playerController.playerSingleton.buffAttackDamage -= value;
                break;
            case buffTypes.attackSpeed:
                playerController.playerSingleton.buffAttackSpeed -= value;
                break;
            default:
                break;
        }

        // return to default state
        testAndRemoveBuffs();
    }
}

/// <summary>
/// Types of buffs that a droppable can have
/// </summary>
public enum buffTypes {
    
    speed = 0,
    maxSpeed = 1,

    jump = 10,
    dash = 20,
    damage = 30,
    attackSpeed = 40,
    health = 50,
    coin = 1
}