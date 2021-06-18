using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Game.Runtime
{
    public struct Job1 : IJob
    {
        public NativeArray<float> Values;
        public float Offset;

        public void Execute()
        {
            for (var i = 0; i < Values.Length; i++)
            {
                Values[i] += Offset;
            }
        }
    }

    public class JobTest1 : MonoBehaviour
    {
        private void Update()
        {
            var values = new NativeArray<float>(500, Allocator.TempJob);
            var job = new Job1()
            {
                Values = values,
                Offset = 5
            };
            var jobHandle = job.Schedule();
            jobHandle.Complete();
            values.Dispose();
        }
    }
}