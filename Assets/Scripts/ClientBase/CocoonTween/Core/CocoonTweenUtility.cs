using ClientBase;
using UnityEngine;

namespace Cocoon.Tween
{
    internal static class CocoonTweenUtility
    {
        /// <summary>
        /// Gets the curve value.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="ratio">The ratio.</param>
        /// <returns>Value</returns>
        internal static float GetCurveValue(AnimationCurve curve, float ratio)
        {
            if (curve != null)
                return curve.Evaluate(ratio);
            return 0;
        }

        /// <summary>
        /// Finds the main director.
        /// </summary>
        /// <param name="animator">The animator.</param>
        /// <returns></returns>
        internal static CocoonTweenMainDirector FindMainDirector(CocoonTweenBase animator)
        {
            var director = animator.gameObject.GetComponent<CocoonTweenMainDirector>();
            if (director == null)
                return animator.gameObject.GetComponentOnObjectOrParent<CocoonTweenMainDirector>(true);
            else
                return director;
        }

        /// <summary>
        /// Finds the related path.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <param name="root">The root.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        internal static bool FindRelatedPath(Transform child, Transform root, out string path)
        {
            //Debug.Log("childname:{0}, root:{1}.".F(child.name, root.name));
            var current = child;
            System.Collections.Generic.List<string> pathList = new System.Collections.Generic.List<string>();
            int counter = 0;
            while (current != null && counter <= 10)
            {
                pathList.Add(current.gameObject.name);
                if (current == root)
                {
                    path = ConvertListToPath(pathList);
                    return true;
                }
                current = current.parent;
                counter++;
            }
            path = null;
            return false;
        }

        /// <summary>
        /// Converts the list to path.
        /// </summary>
        /// <param name="pathList">The path list.</param>
        /// <returns></returns>
        private static string ConvertListToPath(System.Collections.Generic.List<string> pathList)
        {
            string result = "";
            for (int index = pathList.Count - 2; index >= 0; index--)
                result += "/" + pathList[index];
            return result;
        }
    }
}