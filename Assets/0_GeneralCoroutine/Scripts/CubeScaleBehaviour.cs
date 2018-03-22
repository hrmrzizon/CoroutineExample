namespace Example.Coroutine.General
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CubeScaleBehaviour : MonoBehaviour
    {
        WaitForSeconds justWaitInstruction;
        ScaleOverYieldInstruction scaleLimit;

        Coroutine scaleCorountine, stopIdleCoroutine;

        public float waitTime;
        public float scaleFactor;
        public float limitScale;

        public void StartScaling()
        {
            scaleCorountine = StartCoroutine(ChangeScaleGradually(scaleFactor));
        }

        public void StopScaling()
        {
            StopCoroutine(scaleCorountine);
            StopCoroutine(stopIdleCoroutine);

            scaleLimit.Stop();
        }

        public void ResetScale()
        {
            transform.localScale = Vector3.one;
        }

        IEnumerator ChangeScaleGradually(float scaleMultiplier)
        {
            stopIdleCoroutine = StartCoroutine(AutoStopScale());

            while (true)
            {
                yield return justWaitInstruction;
                transform.localScale = transform.localScale * scaleMultiplier;
            }
        }

        IEnumerator AutoStopScale()
        {
            //*
            yield return scaleLimit;
            /*/
            yield return new WaitWhile(
                () => 
                {
                    Vector3 scale = transform.localScale;
                    return limitScale > scale.x && limitScale > scale.y && limitScale > scale.z;
                }
                );
            //*/

            StopCoroutine(scaleCorountine);
            transform.localScale = Vector3.one;
        }

        private void Awake()
        {
            justWaitInstruction = new WaitForSeconds(waitTime);
            scaleLimit = new ScaleOverYieldInstruction(transform, new Vector3(limitScale, limitScale, limitScale));
        }
    }
}