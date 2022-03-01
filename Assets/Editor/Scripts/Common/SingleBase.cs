using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XTools
{
    public abstract class SingleBase<T> where T : new()
    {
        private static T instance;

        public static T Instance()
        {
            if (null == instance)
                instance = new T();

            return instance;
        }

        public static T InstanceHandle
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        protected SingleBase()
        {

        }
    }
}
