using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VelocitechVR
{
    public class SwitchOnOff : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    interface IInteractable
    {
        public void OnMouseOver();
        public void OnInteraction();
    }
}
