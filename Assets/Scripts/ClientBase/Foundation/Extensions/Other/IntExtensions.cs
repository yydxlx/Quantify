using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientBase
{
    public static class IntExtensions
    {
        public static int ToInt(this System.Enum e)
        {
            return e.GetHashCode();
        }
    }
}