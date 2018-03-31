namespace Example.Coroutine.Custom
{
    using Extension.Coroutine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CustomCoroutineExample : MonoBehaviour
    {
        private CoroutineState state;

        private void Awake()
        {
            state = Executor.Play(CoroutineCheck());
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                Debug.Log(state.empty);
                Debug.Log(state.context);
            }
        }
        
        IEnumerator<Context> CoroutineCheck()
        {
            Debug.LogWarning("Start");

            yield return Context.CreateFixedUpdate();

            Debug.LogWarning("FixedUpdate");

            yield return Context.CreateNormalTimer(3f);

            Debug.LogWarning("Time 3f");

            float startTime = Time.time;
            yield return Context.CreateWaitUntil(() => { return startTime + 3f <= Time.time; });
            Debug.LogWarning("WaitUntil");

            Time.timeScale = 0.25f;
            startTime = Time.unscaledTime;

            yield return Context.CreateWaitWhile(() => { return startTime + 3f > Time.unscaledTime; });
            Time.timeScale = 1f;
            Debug.LogWarning("WaitWhile");

            yield return Executor.Play(CoroutineCheck2());
            
            Debug.LogWarning("EndOfCoroutine");
        }

        IEnumerator<Context> CoroutineCheck2()
        {
            Debug.LogWarning("CoroutineCheck2");

            yield return Context.CreateNormalTimer(3f);

            Debug.LogWarning("Time 3f");
        }
    }
}
