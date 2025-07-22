using System;
using NUnit.Framework.Internal;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    [SerializeField]
    private Scroll scroll;

    [SerializeField]
    private Button button;

    [SerializeField]
    private bool isSelected = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (scroll == null)
        {
            scroll = GetComponentInParent<Scroll>();
        }
    }

    private void Start()
    {
        button.onClick.AddListener(OnClick);
        button.onClick.AddListener(() => scroll.OnNodeClick(this));
    }

    private void OnClick()
    {
        Debug.Log("test");
    }

    // Update is called once per frame
    void Update() { }
}
