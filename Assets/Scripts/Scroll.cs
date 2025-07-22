using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Scroll : MonoBehaviour
{
    [SerializeField]
    private Image scrollImage;

    [SerializeField]
    private UILineRenderer lineRenderer;
    private bool isActive = false;

    void Awake()
    {
        UIManager.Instance.ToggleScrollEvent += ToggleScroll;
    }

    private void Start()
    {
        ToggleScroll(false);
    }

    private void ToggleScroll(bool showScroll)
    {
        isActive = showScroll;
        scrollImage.gameObject.SetActive(showScroll);
        foreach (Transform r in gameObject.GetComponentsInChildren<Transform>(true))
        {
            r.gameObject.SetActive(showScroll);
        }
    }

    public void OnNodeClick(Node node)
    {
        Vector2 nodePosition = new(node.transform.position.x, node.transform.position.y);
        lineRenderer.points.Insert(lineRenderer.points.Count - 1, nodePosition);
    }

    void Update()
    {
        if (isActive)
        {
            lineRenderer.points.RemoveAt(lineRenderer.points.Count - 1);
            lineRenderer.points.Add(Mouse.current.position.ReadValue());
            lineRenderer.SetVerticesDirty();
        }
    }
}
