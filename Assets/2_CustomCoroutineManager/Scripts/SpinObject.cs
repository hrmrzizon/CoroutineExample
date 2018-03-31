namespace Example.Coroutine.Custom
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SpinObject : MonoBehaviour
    {
        void Update()
        {
            transform.eulerAngles += new Vector3(0f, 180f * Time.deltaTime, 0f);
        }
    }
}