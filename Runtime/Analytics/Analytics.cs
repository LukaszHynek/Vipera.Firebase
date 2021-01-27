using System;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;

namespace Vipera
{
    public class Analytics : MonoBehaviour
    {
        public static bool AnalyticsReady = false;

        private void Start()
        {
            Prefs.FirstOpenTime = DateTime.Now;
            Prefs.FirstOpenDay = DateTime.Today;
        }

        public void InitializeFirebase()
        {
            AnalyticsReady = true;

            if (!isEnabled.Value)
                SetCollectionEnabled();
        }

        static BoolPP isEnabled = new BoolPP("prop_" + AnalyticsConsts.UserProperties.CustomFirstOpenTime);

        public static bool IsEnabled
        {
            get => isEnabled.Value && AnalyticsReady;
            private set => isEnabled.Value = value;
        }

        public static void SetCollectionEnabled()
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            IsEnabled = true;
            TrySetFirstRun();
        }

        public static void LogEvent(string eventName)
        {
            if (!IsEnabled)
                return;

            TryDebugLog(eventName);

            FirebaseAnalytics.LogEvent(eventName);
        }

        #region LogEventsWithParams
        public static void LogEvent(string eventName, string paramName, string paramValue)
        {
            if (!IsEnabled)
                return;

            TryDebugLog(eventName + "with param: " + paramName);
            FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
        }

        public static void LogEvent(string eventName, params Parameter[] parameters)
        {
            if (!IsEnabled)
                return;

            TryDebugLog(eventName + "with params");
            FirebaseAnalytics.LogEvent(eventName, parameters);
        }

        public static void LogEvent(string eventName, string paramName, int paramValue)
        {
            if (!IsEnabled)
                return;

            TryDebugLog(eventName + "with param: " + paramName);
            FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
        }

        public static void LogEvent(string eventName, string paramName, long paramValue)
        {
            if (!IsEnabled)
                return;

            TryDebugLog(eventName + "with param: " + paramName);
            FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
        }

        public static void LogEvent(string eventName, string paramName, double paramValue)
        {
            if (!IsEnabled)
                return;

            TryDebugLog(eventName + "with param: " + paramName + " value: " + paramValue);
            FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
        }
        #endregion

        public static void SetUserProperty(string name, string property, bool setOnce = false)
        {
            if (!IsEnabled)
                return;

            if (setOnce)
            {
                if (PlayerPrefs.HasKey("prop_" + name))
                    return;

                PlayerPrefs.SetInt("prop_" + name, 1);
                PlayerPrefs.Save();
            }

            FirebaseAnalytics.SetUserProperty(name, property);
        }

        static void TryDebugLog(string eventName)
        {
            if (Application.isEditor || Debug.isDebugBuild)
                Debug.Log("[Analytics] LogEvent: " + eventName);
        }

        static void TrySetFirstRun()
        {
            var firstRunTimeUTC = DateTime.UtcNow.ToUniversalTime()
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            SetUserProperty(AnalyticsConsts.UserProperties.CustomFirstOpenTime, Math.Round(firstRunTimeUTC).ToString(),
                true);
        }
    }
}