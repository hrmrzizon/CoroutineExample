namespace Extension.Coroutine
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
        private List<Executor.Coroutine> coroutineList;
        private int realid;
        public CoroutineState(List<Executor.Coroutine> _coroutineList, int _id)
        {
            coroutineList = _coroutineList;
            realid = _id;
        }

        public int id { get { return realid; } }

        public Context context
        {
            get
            {
                if (coroutineList == null) 
                    return new Context() + new Tag() { str = "Not exist coroutine pair with id.." };

                int sourceID = id;
                Executor.Coroutine coroutine = coroutineList.Find((co) => { return co.id == sourceID; });

                if (!coroutine.empty)
                    return coroutine.context;
                else
                    return new Context() + new Tag() { str = "Coroutine has been expired.." };
            }
        }
        public bool empty
        {
            get
            {
                if (coroutineList == null) return true;

                int sourceID = id;
                return coroutineList.Find((co) => { return co.id == sourceID; }).enumerator == null;
            }
        }
        public bool isDone
        {
            get
            {
                return empty;
            }
        }
    }

    public static class Executor
    {
        static Executor()
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

        public static CoroutineState Play(IEnumerator<Context> _enumerator, Context _context)
        {
            coroutineList.Add(new Coroutine() { enumerator = _enumerator, id = idSource, stop = false, context = _context });
            return new CoroutineState(coroutineList, idSource++);
        }
        public static CoroutineState Play(IEnumerator<Context> _enumerator)
        {
            return Play(_enumerator, new Context() { flag = ContextFlag.Default, interruptContext = InterruptContext.Update });
        }

        public static Context PlayForContext(InterruptContext interruptContext, IEnumerator<Context> _enumerator, Context _context)
        {
            coroutineList.Add(new Coroutine() { enumerator = _enumerator, id = idSource, stop = false, context = _context });
            return Context.CreateCoroutine(interruptContext, new CoroutineState(coroutineList, idSource++));
        }
        public static Context PlayForContext(InterruptContext interruptContext, IEnumerator<Context> _enumerator)
        {
            return PlayForContext(interruptContext, _enumerator, new Context() { flag = ContextFlag.Default, interruptContext = InterruptContext.Update });
        }
        public static Context PlayForContext(IEnumerator<Context> _enumerator, Context _context)
        {
            coroutineList.Add(new Coroutine() { enumerator = _enumerator, id = idSource, stop = false, context = _context });
            return Context.CreateCoroutine(new CoroutineState(coroutineList, idSource++));
        }
        public static Context PlayForContext(IEnumerator<Context> _enumerator)
        {
            return PlayForContext(_enumerator, new Context() { flag = ContextFlag.Default, interruptContext = InterruptContext.Update });
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
                        case ContextFlag.Coroutine:
                            passToNext = context.state.isDone;
                            break;
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

            public bool empty
            {
                get
                {
                    return enumerator == null;
                }
            }
        }
    }
}
