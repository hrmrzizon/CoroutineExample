namespace Example.Coroutine.Enumerator
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [ExecuteInEditMode]
    public class ListEnumeratorBehaviour : MonoBehaviour
    {
        [SerializeField]
        public List<int> integerList;
        
        public void OnEnable()
        {
            IEnumerator<int> enumerator = integerList.GetEnumerator();

            while (enumerator.MoveNext())
                Debug.Log(enumerator.Current);
        }
    }
}
