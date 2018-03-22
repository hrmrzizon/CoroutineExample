namespace Unity.Extension
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class ListExtention
    {
        public static void AddAsAcending<T>(this List<T> list, T item) where T : IComparable
        {
            if (list.Count < 0)
            {
                list.Add(item);
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].CompareTo(item) > 0)
                    {
                        list.Insert(i, item);
                        break;
                    }
                }
            }
        }
    }

    public struct CoroutineState
    {
        private List<Unity.Extension.CrtnExecutor.Coroutine> coroutineList;
        private int id;
        public CoroutineState(List<Unity.Extension.CrtnExecutor.Coroutine> _coroutineList, int _id)
        {
            coroutineList = _coroutineList;
            id = _id;
        }

        public Context context
        {
            get
            {
                int sourceID = id;
                return coroutineList.Find((co) => { return co.id == sourceID; }).context;
            }
        }
        public bool empty
        {
            get
            {
                int sourceID = id;
                return coroutineList.Find((co) => { return co.id == sourceID; }).enumerator == null;
            }
        }
    }

    public static class CrtnExecutor
    {
        static CrtnExecutor()
        {
            coroutineList = new List<Coroutine>();
            stopIDList = new List<int>();
        }

        private static MonoExecutor ex = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void GameWhenLoaded()
        {
            ex = GameObject.FindObjectOfType<MonoExecutor>();

            if (ex == null)
            {
                ex = new GameObject("MonoExecutor").AddComponent<MonoExecutor>();
                MonoBehaviour.DontDestroyOnLoad(ex.gameObject);
            }

            ex.update += Update;
            ex.lateUpdate += LateUpdate;
            ex.fixedUpdate += FixedUpdate;
            ex.endOfFrameUpdate += EndOfFrame;
        }

        public static CoroutineState GetState(int id)
        {
            for (int i = 0; i < coroutineList.Count; i++)
                if (coroutineList[i].id == id)
                    return new CoroutineState(coroutineList, i);

            return new CoroutineState();
        }

        private static List<Coroutine>              coroutineList;
        private static List<int>                    stopIDList;

        private static int idSource;
        public static int Play(IEnumerator<Context> _enumerator, Context _context)
        {
            coroutineList.Add(new Coroutine() { enumerator = _enumerator, id = idSource++, stop = false, context = _context });
            return idSource - 1;
        }
        public static int Play(IEnumerator<Context> _enumerator)
        {
            return Play(_enumerator, new Context() { flag = ContextFlag.Default, interruptContext = InterruptContext.Update });
        }

        private static void ExecuteCoroutine(InterruptContext interrupt)
        {
            for (int coIndex = 0; coIndex < coroutineList.Count; )
            {
                Coroutine co = coroutineList[coIndex];
                Context context = co.context;

                if (context.interruptContext == interrupt)
                {
                    if (co.stop)
                    {
                        coroutineList.RemoveAt(coIndex);
                        continue;
                    }

                    bool passToNext = false;

                    switch (context.flag)
                    {
                        case ContextFlag.Timer:
                            passToNext = context.isTimeExpired;
                            break;
                        case ContextFlag.Delegate:
                            passToNext = context.isDelegateExpired;
                            break;
                        case ContextFlag.TimerAndDelegate:
                            passToNext = context.isTimeExpired && context.isDelegateExpired;
                            break;
                        case ContextFlag.TimerOrDelegate:   
                            passToNext = context.isTimeExpired || context.isDelegateExpired;
                            break;
                        case ContextFlag.Default:
                            // fall through
                        default:
                            passToNext = true;
                            break;
                    }

                    if (passToNext)
                    {
                        if (co.enumerator.MoveNext())
                        {
                            co.context = co.enumerator.Current;
                            coroutineList[coIndex] = co;
                            coIndex++;
                        }
                        else
                        {
                            coroutineList.RemoveAt(coIndex);
                        }
                    }
                    else
                        coIndex++;
                }
                else
                    coIndex++;
            }
        }

        public static bool Stop(int id, InterruptContext eliminateInterrupt = InterruptContext.Update)
        {
            for (int i = 0; i < coroutineList.Count; i++)
            {
                if (coroutineList[i].id == id)
                {
                    Coroutine co = coroutineList[i];
                    co.stop = true;
                    coroutineList[i] = co;
                    return true;
                }
            }

            return false;
        }
        
        public static bool isPlayCoroutine(int id)
        {
            for (int i = 0; i < coroutineList.Count; i++)
            {
                if (coroutineList[i].id == id)
                    return true;
            }

            return false;
        }

        public static bool isStopReserved(long id)
        {
            return stopIDList.FindIndex((other) => { return other == id; }) >= 0;
        }

        private static void Update()
        {
            ExecuteCoroutine(InterruptContext.Update);
        }
        private static void LateUpdate()
        {
            ExecuteCoroutine(InterruptContext.LateUpdate);
        }
        private static void FixedUpdate()
        {
            ExecuteCoroutine(InterruptContext.FixedUpdate);
        }
        private static void EndOfFrame()
        {
            ExecuteCoroutine(InterruptContext.EndOfFrame);
        }

        public struct Coroutine
        {
            public int id;
            public IEnumerator<Context> enumerator;
            public bool stop;
            public Context context;
        }
    }
}
