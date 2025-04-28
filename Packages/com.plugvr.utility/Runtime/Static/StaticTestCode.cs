using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.Utility
{
    public class StaticTestCode : MonoBehaviour
    {
        // public StaticVar<StaticTestCode> Instance
        // {
        //     get
        //     {
        //         if (instance == null)
        //         {
        //             instance = new StaticVar<StaticTestCode>(this);
        //         }
        //
        //         return instance;
        //     }
        // }
        //
        // private StaticVar<StaticTestCode> instance = null;

        public static StaticTestCode Instance
        {
            get
            {
                Debug.LogError(instance);
                
                if (instance == null)
                {
                    instance = FindObjectOfType<StaticTestCode>();
                }

                return instance;
            }
        }
        
        private static StaticTestCode instance = null;
        
        public int value = 100;
        
        [SerializeField]
        private int changeValue = 5;
        
        private static int normalVar = 0;
        
        private StaticVar<int> testInt;
        
        void Start()
        {
            if (testInt != null)
            {
                Debug.Log($"cached testInt: {testInt.Value}");
            }
            else
            {
                testInt = new StaticVar<int>(changeValue);
                Debug.Log($"testInt: {testInt.Value}");
            }
            
            Debug.Log($"before normalVar: {normalVar}");
            
            normalVar = changeValue;
            
            Debug.Log($"after normalVar: {normalVar}");
            
            Debug.Log(Instance);
        }
    }
}
