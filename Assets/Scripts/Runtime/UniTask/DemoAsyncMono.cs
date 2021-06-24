using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Game.Runtime
{
    public class DemoAsyncMono : MonoBehaviour
    {
        private void Start()
        {
            DemoAsync().Preserve();
        }

        private async UniTask DemoAsync()
        {
            await UniTask.WaitForFixedUpdate();
            Debug.Log("UniTask");
        }
    }
}
