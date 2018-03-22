namespace Example.Coroutine.Enumerator
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [ExecuteInEditMode]
    public class CodeEnumeratorBehaviour : MonoBehaviour
    {
        public void OnEanble()
        {
            IEnumerator enumerator = CheckForEnumerator();

            while (enumerator.MoveNext())
                Debug.Log(enumerator.Current);
        }

        IEnumerator CheckForEnumerator()
        {
            Debug.Log("yield return int.MaxValue");
            yield return int.MaxValue;
            Debug.Log("yield return float.MaxValue");
            yield return float.MaxValue;
            Debug.Log("yield return double.MaxValue");
            yield return double.MaxValue;
            Debug.Log("yield return integerList.GetEnumerator()");
            yield return new List<int>().GetEnumerator();
            Debug.Log("yield return (IEnumerable)integerList");
            yield return new List<int>();
            Debug.Log("yield return WaitForSeconds");
            yield return new WaitForSeconds(1f);
        }
    }
}
