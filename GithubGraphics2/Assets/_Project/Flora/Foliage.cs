using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeerGod
{
    public class Foliage : MonoBehaviour, IPointerClickHandler
    {
        private Vector3 _finalSize;
        public void OnPointerClick(PointerEventData eventData)
        {
            StartCoroutine(ShrinkAndDestroy());
        }

        void Awake()
        {
            _finalSize = transform.localScale;
        }

        // Start is called before the first frame update
        void Start()
        {
            transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            StartCoroutine(Grow());
        }

        IEnumerator ShrinkAndDestroy()
        {
            for (float scale = 1f; scale >= 0; scale -= 0.01f)
            {
                transform.localScale = transform.localScale * scale;
                yield return new WaitForSeconds(.01f);
            }
            Destroy(this);
        }

        IEnumerator Grow()
        {
            for (float scale = 0.01f; scale <= 1; scale += 0.01f)
            {
                transform.localScale = _finalSize * scale;
                yield return new WaitForSeconds(.01f);
            }
        }
    }
}