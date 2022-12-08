using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeerGod
{
    public class RippleHoof : MonoBehaviour
    {
        [SerializeField] private GameObject _particles;
        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Water"))
            {
                Instantiate(_particles, transform.position, transform.rotation);
            }
        }
    }
}