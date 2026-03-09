using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuStore.PayExample {

    public static class Logcat {

        public static void LogWarning(string tag, string message) {
            AndroidJavaClass log = new AndroidJavaClass("android.util.Log");
            log.CallStatic<int>("w", tag, message);
        }
    }
}
