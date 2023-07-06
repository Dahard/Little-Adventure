using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour
{
    private CharacterController cc;
    public float MovementSpeed = 5f;
    private Vector3 movementVelocity;
    private PlayerInput playerInput;
    private float verticalVelocity;
    public float Gravity = -9.8f;
    private Animator animator;
    public int Coin;
    private float attackAnimationDuration;
    public float SlideSpeed = 9f;

    //Enemy
    public bool isPlayer = true;
    private NavMeshAgent navMeshAgent;
    private Transform targetPlayer;

    //DamageCaster
    private DamageCaster damageCaster;

    //Health
    private Health health;

    //Material Animation
    private MaterialPropertyBlock materialPropertyBlock;
    private SkinnedMeshRenderer skinnedMeshRenderer;

    public GameObject ItemToDrop;

    //PlayerSlides
    private float attackStartTime;
    public float AttackSlideDuration = 0.4f;
    public float AttackSlideSpeed = 0.06f;

    private Vector3 impactOnCharacter;

    public bool IsInvincible;
    public float invincibilityDuration = 2f;
    
    //State Machine
    public enum CharacterState
    {
        Normal, Attacking, Dead, BeingHit, Slide, Spawn
    }

    public CharacterState CurrentState;

    public float SpawnDuration = 2f;
    private float currentSpawnTime;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        damageCaster = GetComponentInChildren<DamageCaster>();
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        materialPropertyBlock = new MaterialPropertyBlock();
        skinnedMeshRenderer.GetPropertyBlock(materialPropertyBlock);

        if (!isPlayer)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            targetPlayer = GameObject.FindWithTag("Player").transform;
            navMeshAgent.speed = MovementSpeed;
            SwitchStateTo(CharacterState.Spawn);
        } 
        else
        {
            playerInput = GetComponent<PlayerInput>();
        }
    }

    private void CalculateEnemyMovement()
    {
        if(Vector3.Distance(targetPlayer.position, transform.position) >= navMeshAgent.stoppingDistance)
        {
            navMeshAgent.SetDestination(targetPlayer.position);
            animator.SetFloat("Speed", 0.2f);
        }
        else
        {
            navMeshAgent.SetDestination(transform.position);
            animator.SetFloat("Speed", 0f);
            SwitchStateTo(CharacterState.Attacking);
        }
    }

    private void CalculatePlayerMovement()
    {
        if(playerInput.MouseButtonDown && cc.isGrounded)
        {

            SwitchStateTo(CharacterState.Attacking);
            return;

        } else if (playerInput.SpaceKeyDown && cc.isGrounded)
        {

            SwitchStateTo(CharacterState.Slide);
            return;

        }

        movementVelocity.Set(playerInput.HorizontalInput, 0f, playerInput.VerticalInput);
        movementVelocity.Normalize();
        movementVelocity = Quaternion.Euler(0, -45f, 0) * movementVelocity;

        animator.SetFloat("Speed", movementVelocity.magnitude);

        movementVelocity *= MovementSpeed * Time.deltaTime;

        if(movementVelocity != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(movementVelocity); 
        }

        animator.SetBool("isAirBorne", !cc.isGrounded);

    }

    private void FixedUpdate()
    {
        switch (CurrentState)
        {
            case CharacterState.Normal:
                if (isPlayer)
                {
                    CalculatePlayerMovement();
                }
                else
                {
                    CalculateEnemyMovement();
                }
                break;

            case CharacterState.Attacking:

                if(isPlayer)
                {

                    if(Time.time < attackStartTime + AttackSlideDuration)
                    {
                        float timePassed = Time.time - attackStartTime;
                        float lerpTime = timePassed / AttackSlideDuration;
                        movementVelocity = Vector3.Lerp(transform.forward * AttackSlideSpeed, Vector3.zero, lerpTime);
                    }

                    if(playerInput.MouseButtonDown && cc.isGrounded) 
                    {
                        string currenClipName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                        attackAnimationDuration = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                        if(currenClipName != "LittleAdventurerAndie_ATTACK_03" && attackAnimationDuration > 0.5f && attackAnimationDuration < 0.7f)
                        {
                            playerInput.MouseButtonDown = false;
                            SwitchStateTo(CharacterState.Attacking);
                            CalculatePlayerMovement();
                        }
                    }

                }

                break;

            case CharacterState.Dead:
                return;

            case CharacterState.BeingHit:

                if(impactOnCharacter.magnitude > 0.2f)
                {
                    movementVelocity = impactOnCharacter * Time.deltaTime;
                }

                impactOnCharacter = Vector3.Lerp(impactOnCharacter, Vector3.zero, Time.deltaTime * 5);

                break;

            case CharacterState.Slide:

                movementVelocity = transform.forward * SlideSpeed * Time.deltaTime;
                break;

            case CharacterState.Spawn:

                currentSpawnTime -= Time.deltaTime;
                if (currentSpawnTime <= 0)
                {
                    SwitchStateTo(CharacterState.Normal);
                }
                break;
        }

        if (isPlayer)
        {
            if (cc.isGrounded == false)
            {
                verticalVelocity = Gravity;
            }
            else
            {
                verticalVelocity = Gravity * 0.3f;
            }

            movementVelocity += verticalVelocity * Vector3.up * Time.deltaTime;

            cc.Move(movementVelocity);
            movementVelocity = Vector3.zero;
        }
    }

    public void SwitchStateTo(CharacterState newState)
    {
        //Clear
        if(isPlayer)
        {
            playerInput.ClearCache();
        }

        //Exiting State
        switch (CurrentState)
        {
            case CharacterState.Normal:
                break;
            case CharacterState.Attacking:
                
                if(damageCaster != null)
                    DisableDamageCaster();

                if(isPlayer)
                    GetComponent<PlayerVFXManger>().StopBlade();

                break;
            case CharacterState.Dead:
                return;
            case CharacterState.BeingHit:
                break;
            case CharacterState.Slide:
                break;
            case CharacterState.Spawn:
                IsInvincible = false;
                break;
        }

        //Entering State
        switch (newState)
        {
            case CharacterState.Normal:
                break;
            case CharacterState.Attacking:

                if(!isPlayer)
                {
                    Quaternion newRotation = Quaternion.LookRotation(targetPlayer.position - transform.position);
                    transform.rotation = newRotation;
                }

                animator.SetTrigger("Attack");

                if (isPlayer)
                    attackStartTime = Time.time;

                break;
            case CharacterState.Dead:

                cc.enabled = false;
                animator.SetTrigger("Dead");
                StartCoroutine(MaterialDissolve());
                break;

            case CharacterState.BeingHit:

                animator.SetTrigger("BeingHit");

                if(isPlayer)
                {
                    IsInvincible = true;
                    StartCoroutine(DelayCancelInvincibility());
                }
                break;

            case CharacterState.Slide:
                animator.SetTrigger("Slide");
                break;
            case CharacterState.Spawn:
                IsInvincible = true;
                currentSpawnTime = SpawnDuration;
                StartCoroutine(MaterialAppear());
                break;
        }

        CurrentState = newState;

        Debug.Log("Switched to: " + CurrentState);
    }

    public void AttackAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void BeingHitAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void SlideAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void ApplyDamage(int damage, Vector3 attackerPos = new Vector3())
    {

        if(IsInvincible)
        {
            return;
        }

        if (health != null)
        {
            Debug.Log("Damage applied");
            health.ApplyDamage(damage);
        }

        if(!isPlayer)
        {
            GetComponent<EnemyVFXManager>().PlayBeingHit(attackerPos);
        }

        StartCoroutine(MaterialBlink());

        if(isPlayer)
        {
            SwitchStateTo(CharacterState.BeingHit);
            AddImpact(attackerPos, 10f);
        }

    }

    IEnumerator DelayCancelInvincibility ()
    {
        yield return new WaitForSeconds(invincibilityDuration);
        IsInvincible = false;
    }

    private void AddImpact(Vector3 attackerPos, float force)
    {
        Vector3 impactDir = transform.position - attackerPos;
        impactDir.Normalize();
        impactDir.y = 0;
        impactOnCharacter = impactDir * force;
    }

    public void EnableDamageCaster()
    {
        damageCaster.EnableDamageCaster();
    }

    public void DisableDamageCaster()
    {
        damageCaster.DisableDamageCaster();
    }

    IEnumerator MaterialBlink()
    {
        materialPropertyBlock.SetFloat("_blink", 0.4f);
        skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);

        yield return new WaitForSeconds(0.2f);

        materialPropertyBlock.SetFloat("_blink", 0);
        skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    IEnumerator MaterialDissolve()
    {
        yield return new WaitForSeconds(2);

        float dissolveTimeDuration = 2f;
        float currentDissolveTime = 0;
        float dissolveHeightStart = 20f;
        float dissolveHeightTarget = -10f;
        float dissolveHeight;

        materialPropertyBlock.SetFloat("_enableDissolve", 1);
        skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);

        while (currentDissolveTime < dissolveTimeDuration)
        {
            currentDissolveTime += Time.deltaTime;
            dissolveHeight = Mathf.Lerp(dissolveHeightStart, dissolveHeightTarget, currentDissolveTime / dissolveTimeDuration);
            materialPropertyBlock.SetFloat("_dissolve_height", dissolveHeight);
            skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);
            yield return null;
        }

        DropItem();
    }

    public void DropItem()
    {
        if (ItemToDrop != null)
        {
            Instantiate(ItemToDrop, transform.position, Quaternion.identity);
        }
    }

    public void PickUpItem(PickUp item)
    {
        switch(item.type)
        {
            case PickUp.PickUpType.Heal:

                AddHealth(item.Value);

                break;

            case PickUp.PickUpType.Coin:

                AddCoin(item.Value);

                break;
        }
    }

    private void AddCoin(int coin)
    {
        Coin += coin;
    }

    private void AddHealth(int healthValue)
    {
        health.AddHealth(healthValue);
        GetComponent<PlayerVFXManger>().PlayHealVFX();
    }

    public void RotateToTarget()
    {
        if (CurrentState != CharacterState.Dead)
        {
            transform.LookAt(targetPlayer, Vector3.up);
        }
    }

    IEnumerator MaterialAppear()
    {
        float dissolveTimeDuration = SpawnDuration;
        float currentDissolveTime = 0;
        float dissolveHeightStart = -10f;
        float dissolveHeightTarget = 20f;
        float dissolveHeight;

        materialPropertyBlock.SetFloat("_enableDissolve", 1);
        skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);

        while (currentDissolveTime < dissolveTimeDuration)
        {
            currentDissolveTime += Time.deltaTime;
            dissolveHeight = Mathf.Lerp(dissolveHeightStart, dissolveHeightTarget, currentDissolveTime / dissolveTimeDuration);
            materialPropertyBlock.SetFloat("_dissolve_height", dissolveHeight);
            skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);
            yield return null;
        }

        materialPropertyBlock.SetFloat("_enableDissolve", 0);
        skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);
    }
}
