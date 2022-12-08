using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeerGod
{
    // Have to make the particle system loop so we can prewarm so we can keep up with running
    // Now we have to destroy the particle system ourselves
    public class Ripple : MonoBehaviour
    {
        [SerializeField] private Vector3 _rot = new Vector3(-90f, 0f, 0f);

        private float _timeToLive;
        private float _timeAlive = 0f;

        void Awake()
        {
            _timeToLive = GetComponent<ParticleSystem>().main.duration;
        }

        // Update is called once per frame
        void Update()
        {
            transform.LookAt(_rot, Vector3.up);
            _timeAlive += Time.deltaTime;
            if (_timeAlive > _timeToLive)
            {
                Destroy(this.gameObject);
            }
        }
    }
}