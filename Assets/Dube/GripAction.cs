using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dube
{
    public class GripAction : MonoBehaviour
    {
        public InputActionReference gripAction;

        private void Awake()
        {
            if (gripAction != null)
                gripAction.action.Enable();
        }
    }
}