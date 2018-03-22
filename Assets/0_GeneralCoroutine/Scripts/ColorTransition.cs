#define COROUTINE_0

namespace Example.Coroutine.General
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    [Serializable]
    public class ColorUpdateEvent : UnityEvent<Color> { }

    public class ColorTransition : MonoBehaviour
    {
        public float colorTransPerSecond;
        public AnimationCurve curve;
        public Color[] colorArray;

        public ColorUpdateEvent colorUpdateEvent;

        [NonSerialized]
        public Color currentColor;

        [NonSerialized]
        public int colorIndex;
        [NonSerialized]
        public float colorFraction;

#if     COROUTINE_0
        IEnumerator currentTransition;
#elif   COROUTINE_1
#elif   COROUTINE_2 || COROUTINE_3
        Coroutine coroutine;
#endif

        public void StartTransition()
        {
#if     COROUTINE_0
            StartCoroutine(currentTransition = Transition());
#elif   COROUTINE_1
            StartCoroutine("Transition");
#elif   COROUTINE_2
            coroutine = StartCoroutine(Transition());
#elif   COROUTINE_3
            coroutine = StartCoroutine("Transition");
#endif
        }

        public void StopTransition()
        {
#if     COROUTINE_0
            StopCoroutine(currentTransition);
#elif   COROUTINE_1
            StopCoroutine("Transition");
#elif   COROUTINE_2 || COROUTINE_3
            StopCoroutine(coroutine);
#endif
        }

        private IEnumerator Transition()
        {
            if (colorArray.Length >= 2)
            {
                while (true)
                {
                    if (colorTransPerSecond != 0)
                    {
                        colorFraction += colorTransPerSecond * Time.deltaTime;

                        if (Mathf.CeilToInt(colorFraction) != 1)
                        {
                            float sign = Mathf.Sign(colorTransPerSecond);
                            colorFraction = colorFraction + -Mathf.Sign(sign);
                            colorIndex = (colorIndex + colorArray.Length + (int)sign * 1) % colorArray.Length;
                        }

                        int plusIndex = (colorIndex + 1) % colorArray.Length;

                        float evaluated = curve.Evaluate(colorFraction);
                        Color color = colorArray[colorIndex] * (1 - evaluated) + colorArray[plusIndex] * evaluated;
                        colorUpdateEvent.Invoke(currentColor = color);
                    }

                    yield return null;
                }
            }
            else
            {
                Debug.LogErrorFormat("ColorTransition.Transition >> ColorArray length is {0}", colorArray.Length);
            }
        }
        
        public void OnEnable()
        {
            StartTransition();
        }

        public void OnDisable()
        {
            StopTransition();
        }
    }
}
