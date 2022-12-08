using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeerGod
{
    public class PlantPlacement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private List<GameObject> _spawnables;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == 0) {
                int randnum = Random.Range(0, _spawnables.Count);
                Vector3 spawnPosition = eventData.pointerCurrentRaycast.worldPosition;
                Instantiate(_spawnables[randnum], spawnPosition, Quaternion.identity);
            }
        } }
}
