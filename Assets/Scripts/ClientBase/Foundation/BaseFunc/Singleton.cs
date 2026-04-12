using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientBase
{
    public class Singleton<T> where T : new()
    {
        private static T _instance;
        public static T Ins
        {
            get
            {
                if (_instance == null)
                    _instance = new T();
                return _instance;
            }
        }

        public static void CleanInstance()
        {
            _instance = default(T);
        }
    }

    public class SingletonThreadSafe<T> where T : new()
    {
        private static T _instance;

        // Lock synchronization object
        private static readonly object _syncLock = new object();

        // Constructor is 'private'
        private SingletonThreadSafe()
        {
        }

        public static T Ins
        {
            get
            {
                // Support multithreaded applications through
                // 'Double checked locking' pattern which (once
                // the instance exists) avoids locking each
                // time the method is invoked
                if (_instance == null)
                {
                    lock (_syncLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }

                return _instance;
            }
        }
    }
}
