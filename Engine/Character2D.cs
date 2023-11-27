using System;
using System.Collections;
using UnityEngine;


public class Character2D : MonoBehaviour, IMessageSender, IVisualCaller
{
    //Position properties for convenience. Also safer than going like character.transform as it is
    //easier to find references
    public Vector2 Position => transform.position;
    public Vector2 Forward => transform.up;
    public Vector2 Right => transform.right;

    //Layer
    public LayerMask characterLayer;

    //Movement properties
    [SerializeField] private float _speed = 1f;
    //Sensitivity is actually set by a manager in the game
    [HideInInspector]
    public float mouseSensitivity = 120f;

    private float _horizontal;
    private float _vertical;
    private float _mouseX;

    private bool MoveCondition => _horizontal != 0f || _vertical != 0 || _mouseX != 0;

    //Events
    public event EventHandler VisualEvent;
    public event EventHandler<SentMessageEventArgs> MessageEvent;

    private void Update()
    {
        if (GameManager.Instance.IsPaused)
            return;

        ReadInput();

        if (MoveCondition)
        {
            MoveCharacter();
            VisualEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        MessageEvent?.Invoke(this, new SentMessageEventArgs(new PromptMessage(GameManager.Instance.messageSettings.collision)));
    }

    private void ReadInput()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
        _vertical = Input.GetAxisRaw("Vertical");
        //Minus sign for correct orientation
        _mouseX = -Input.GetAxis("Mouse X");
    }

    private void MoveCharacter()
    {
        Vector3 moveDirection = (Vector3.up * _vertical + Vector3.right * _horizontal).normalized;
        transform.Translate(Time.deltaTime * _speed * moveDirection);
        transform.Rotate(new Vector3(0f, 0f, Time.deltaTime * mouseSensitivity * _mouseX));
    }
}
