namespace Example.Coroutine.General
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    [System.Serializable]
    public class WaitMessageEvent : 
        UnityEvent<string> { }

    public class WaitBehaviour : MonoBehaviour
    {
        Coroutine coroutine;
        WaitForSeconds justWaitInstruction;
        IEnumerator enumerator;

        public int num = 0;
        public float waitTime = 3f;
        public WaitMessageEvent endMessageEvent;

        private void Awake()
        {
            justWaitInstruction 
                = new WaitForSeconds(waitTime);
        }
        IEnumerator Wait(int num)
        {   // new WaitForSecods(waitTime);
            yield return justWaitInstruction; 
            endMessageEvent.Invoke("TIME EXPIRED");
            coroutine = null;
            enumerator = null;
        }

        public void StartEnumWithCoroutine()
        {
            enumerator = Wait(num++);
            coroutine = StartCoroutine(enumerator);
        }

        public void StartEnumOnly()
        {
            enumerator = Wait(num++);
            StartCoroutine(enumerator);
            coroutine = null;
        }

        public void StartNameWithCoroutine()
        {
            coroutine = StartCoroutine("Wait", num++);
        }

        public void StopAsEnumerator()
        {
            StopCoroutine(enumerator);
            coroutine = null;
            enumerator = null;
        }

        public void StopAsCoroutine()
        {
            StopCoroutine(coroutine);
            coroutine = null;
            enumerator = null;
        }

        public void StopAsName()
        {
            StopCoroutine("Wait");
            coroutine = null;
            enumerator = null;
        }

        public void StopAll()
        {
            StopAllCoroutines();
            coroutine = null;
            enumerator = null;
        }
    }
}