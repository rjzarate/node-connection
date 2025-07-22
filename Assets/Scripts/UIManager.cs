using System;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    internal PlayerInput PlayerInput { get; private set; }
    public bool ScrollShown { get; private set; } = false;

    private void Awake()
    {
        PlayerInputManager.Instance.InteractEvent += ToggleScroll;
    }

    private void Start()
    {
        ScrollShown = false;
    }

    private void ToggleScroll()
    {
        ToggleScrollEvent?.Invoke(ScrollShown);
        ScrollShown = !ScrollShown;
    }

    public delegate void ToggleScrollEventHandler(bool showScroll);
    public event ToggleScrollEventHandler ToggleScrollEvent;
}
