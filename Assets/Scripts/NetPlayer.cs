using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetPlayer : NetworkBehaviour
{
    public float moveSpeed = 3.5f;
    public float rotateSpeed = 90.0f;


    /// <summary>
    /// 마지막 입력으로 인한 이동 및 회전 방향/ 네트워크에서 공유되는 변수
    /// </summary>
    NetworkVariable<float> netMoveDir = new NetworkVariable<float>(0.0f);

    NetworkVariable<float> netRotate = new NetworkVariable<float>(0.0f);

    /// <summary>
    /// 애니메이션 상태
    /// </summary>
    enum AnimationState
    {
        Idle,
        Walk,
        BackWalk,
        Attack1,
        Attack2,
        None
    }

    AnimationState state = AnimationState.None;

    AnimationState State
    {
        get => state;
        set
        {
            if (value != state)
            {
                state = value;
                animator.SetTrigger(state.ToString());
            }
        }
    }

    //컴포넌트들
    CharacterController controller;
    Animator animator;
    PlayerInputActions inputActions;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.MoveForward.performed += OnMoveInput;
        inputActions.Player.MoveForward.canceled += OnMoveInput;
        inputActions.Player.Rotate.performed += OnRotate;
        inputActions.Player.Rotate.canceled += OnRotate;
        inputActions.Player.Attack1.performed += OnAttack1;
        inputActions.Player.Attack2.performed += OnAttack2;
    }

    private void OnAttack1(InputAction.CallbackContext context)
    {
        State = AnimationState.Attack1;
        //animator.ResetTrigger(AnimationState.Idle.ToString());
    }

    private void OnAttack2(InputAction.CallbackContext context)
    {
        State = AnimationState.Attack2;
    }

    private void OnDisable()
    {
        inputActions.Player.Rotate.canceled -= OnRotate;
        inputActions.Player.Rotate.performed -= OnRotate;
        inputActions.Player.MoveForward.canceled -= OnMoveInput;
        inputActions.Player.MoveForward.performed -= OnMoveInput;
        inputActions.Player.Disable();
    }
    private void OnMoveInput(InputAction.CallbackContext context)
    {
        float moveInput = context.ReadValue<float>(); // 키보드라 -1, 0, 1 중 하나
        SetMoveInput(moveInput); 

    }

    void SetMoveInput(float moveInput)
    {
        float moveDir = moveInput * moveSpeed;

        if(NetworkManager.Singleton.IsServer)
        {
            netMoveDir.Value = moveDir;

        }
        else
        {
            MoveRequestServerRpc(moveDir);
        }

        //if (moveDir > 0.001f)
        //{
        //    State = AnimationState.Walk;
        //}
        //else if (moveDir < -0.001f)
        //{
        //    State = AnimationState.BackWalk;
        //}
        //else
        //{
        //    State = AnimationState.Idle;
        //}
    }

    [ServerRpc]
    void MoveRequestServerRpc(float move)
    {
        netMoveDir.Value = move;
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        float rotateInput = context.ReadValue<float>(); // 키보드라 -1, 0, 1 중 하나
       // rotate = rotateInput * rotateSpeed;
    }

    void SetRotateInput(float rotateInput)
    {

    }

    private void Update()
    {
        if(netMoveDir.Value != 0)
        {
            controller.SimpleMove(netMoveDir.Value * transform.forward);
        }
            //transform.Rotate(0, rotate * Time.deltaTime, 0, Space.World);
    }
}
