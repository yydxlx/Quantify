using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientBase.Coroutine
{
    /// <summary>
    /// Process all defer event through CoroutineManager script
    /// </summary>
    public static class Defer
    {
        /// <summary>
        /// Wait the specified frame count.
        /// </summary>
        /// <param name="frameCount">The frame count.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static CoroutineHandle Frames(int frameCount, Action action)
        {
            return CoroutineManager.RunCoroutine(DeferFramesInternal(frameCount, action));
        }

        /// <summary>
        /// Wait the specified seconds.
        /// </summary>
        /// <param name="seconds">The seconds.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static CoroutineHandle Seconds(float seconds, Action action)
        {
            return CoroutineManager.RunCoroutine(DeferSecondsInternal(seconds, action));
        }

        /// <summary>
        /// Runs the coroutine.
        /// </summary>
        /// <param name="coroutine">The coroutine.</param>
        /// <returns></returns>
        public static UnityEngine.Coroutine RunCoroutine(IEnumerator coroutine)
        {
            return CoroutineManager.Instance.StartCoroutine(coroutine);
        }
        public static void StopCoroutine(IEnumerator coroutine)
        {
            CoroutineManager.Instance.StopCoroutine(coroutine);
        }
        public static void StopAllCoroutine()
        {
            //CoroutineManager.Instance.StopAllCoroutines();
            CoroutineManager.KillCoroutines();
        }
        private static IEnumerator<float> DeferSecondsInternal(float seconds, Action action)
        {
            yield return CoroutineManager.WaitForSeconds(seconds);
            action();
        }

        private static IEnumerator<float> DeferFramesInternal(int frameCount, Action action)
        {
            for (int i = 0; i < frameCount; i++)
                yield return CoroutineManager.WaitForOneFrame;
            action();
        }
    }
}