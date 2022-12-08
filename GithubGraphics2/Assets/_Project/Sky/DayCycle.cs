using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeerGod
{
    // This script should be attached to the main directional light for the scene
    [RequireComponent(typeof(Light))]
    public class DayCycle : MonoBehaviour
    {
        // Sun rises and sets over the course of an in game day, there isn't really a night/moon phase since the skybox just blacks out
        [Tooltip("Length of an in game day specified in seconds")]
        [SerializeField]  private float _dayLengthSeconds;
        [Tooltip("Angle sun starts at")]
        [SerializeField] private float _sunStartAngle;
        [Tooltip("Angle sun ends at")]
        [SerializeField] private float _sunEndAngle;

        private float _secondsPassed = 0;    // How much time has passed in seconds since start of cycle
        private Light _sun;

        private void Awake()
        {
            _sun = GetComponent<Light>();
            transform.localRotation = Quaternion.Euler(_sunStartAngle, 0, 0);
        }

        // Update is called once per frame
        private void Update()
        {
            float percentTimePassed = _secondsPassed / _dayLengthSeconds;
            float currentAngle = Mathf.Lerp(_sunStartAngle, _sunEndAngle, percentTimePassed);
            transform.localRotation = Quaternion.Euler(currentAngle, 0, 0);


            _secondsPassed += Time.deltaTime;
            // Reset Timer
            if (_secondsPassed > _dayLengthSeconds)
            {
                _secondsPassed -= _dayLengthSeconds;
            }
        }
    }
}
