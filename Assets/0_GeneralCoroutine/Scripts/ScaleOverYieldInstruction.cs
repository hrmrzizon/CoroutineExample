namespace Example.Coroutine.General
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ScaleOverYieldInstruction : CustomYieldInstruction
    {
        private Transform transform;
        private Vector3 limit;
        private bool stop;

        public ScaleOverYieldInstruction(Transform transform, Vector3 limit)
        {
            this.transform = transform;
            this.limit = limit;
            stop = false;
        }

        public void Stop()
        {
            stop = true;
        }

        public override bool keepWaiting
        {
            get
            {
                if (stop)
                {
                    stop = false;
                    return false;
                }

                Vector3 scale = transform.localScale;

                return limit.x > scale.x && limit.y > scale.y && limit.z > scale.z;
            }
        }
    }
}