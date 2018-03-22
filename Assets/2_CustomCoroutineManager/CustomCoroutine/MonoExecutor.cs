namespace Unity.Extension
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MonoExecutor : MonoBehaviour
    {
        public event Action update;
        public event Action lateUpdate;
        public event Action fixedUpdate;
        public event Action endOfFrameUpdate;

        private void Awake()
        {
            MonoExecutor[] executors = FindObjectsOfType<MonoExecutor>();

            if (executors.Length > 1)
                Destroy(gameObject);
            else
            {
                StartCoroutine(Updator());
            }
        }

        private void Update()
        {
            update.Invoke();
        }

        private void FixedUpdate()
        {
            fixedUpdate.Invoke();
        }

        private void LateUpdate()
        {
            lateUpdate.Invoke();
        }

        private WaitForEndOfFrame endOfFrameWat = new WaitForEndOfFrame();
        private IEnumerator Updator()
        {
            while (true)
            {
                yield return endOfFrameWat;
                endOfFrameUpdate.Invoke();
            }
        }
    }
}