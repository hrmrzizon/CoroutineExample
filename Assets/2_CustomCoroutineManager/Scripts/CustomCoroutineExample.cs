namespace Example.Coroutine.Custom
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Extension;
    using UnityEngine;

    public class CustomCoroutineExample : MonoBehaviour
    {
        CoroutineState state;

        private void Awake()
        {
            int id = CrtnExecutor.Play(CoroutineCheck());

            state = CrtnExecutor.GetState(id);
            StartCoroutine(Check());
        }

        private void Update()
        {
            //Debug.Log(state.context);
        }

        IEnumerator Check()
        {
            yield return new WaitUntil(() => { return true; });

            Debug.LogWarning("Check");
        }

        IEnumerator<Context> CoroutineCheck()
        {
            Debug.LogWarning("Start");

            yield return Context.CreateFixedUpdate();

            Debug.LogWarning("FixedUpdate");

            yield return Context.CreateNormalTimer(3f);

            Debug.LogWarning("Time 3f");

            yield return Context.CreateWaitUntil(() => { return true; });

            Debug.LogWarning("Delegate");
        }
    }
}
