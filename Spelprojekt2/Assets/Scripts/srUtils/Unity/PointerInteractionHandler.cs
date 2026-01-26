using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace srUtils.Unity
{
    public class PointerInteractionHandler : MonoBehaviour
    {
        [SerializeField] private Transform bottomLeft;
        [SerializeField] private Transform topRight;
        [SerializeField] private UnityEvent<Vector2> onLeftClickInField;
        [SerializeField] private UnityEvent onRightClick;
        [SerializeField] private InputAction mousePosition;

        private void Awake()
        {
            mousePosition.Enable();
        }

        public void LeftClick(InputAction.CallbackContext c)
        {
            if (c.performed)
            {
                Vector3 clickedCoords = Camera.main.ScreenToWorldPoint(mousePosition.ReadValue<Vector2>());
                if (clickedCoords.x > bottomLeft.position.x && clickedCoords.x < topRight.position.x && clickedCoords.y > bottomLeft.position.y && clickedCoords.y < topRight.position.y)
                {
                    onLeftClickInField.Invoke(clickedCoords);
                    
                }
            }
        }
        public void RightClick(InputAction.CallbackContext c)
        {
            if (c.performed)
            {
                onRightClick.Invoke();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLineStrip(new ReadOnlySpan<Vector3>(new []
            {
                bottomLeft.position,
                new Vector3(bottomLeft.position.x, topRight.position.y,0),
                topRight.position,
                new Vector3(topRight.position.x, bottomLeft.position.y,0)
            }),true);
        }
    }
}