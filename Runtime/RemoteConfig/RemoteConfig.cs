using UnityEngine;
using Firebase.RemoteConfig;
using System.Threading.Tasks;
using System;
using Firebase.Messaging;

namespace Vipera
{
    public class RemoteConfig : MonoBehaviour
    {
        public RemoteSettingsBase[] SettingsScripts;

        public delegate void FetchCompleteDelegate();
        public static FetchCompleteDelegate OnFetchComplete;

        private bool isInitialized = false;
        private bool isFetchInProgress = false;
        private string lastSuccessedFetchDate;

        void Awake()
        {
            SettingsScripts = Resources.LoadAll<RemoteSettingsBase>(string.Empty);
#if VIPERA_MESSAGING
            FirebaseMessages.OnMessageReceived += OnMessageReceivedHandler;
#endif
        }

        private void Start()
        {
            if (SettingsScripts != null)
            {
                for (int i = 0; i < SettingsScripts.Length; i++)
                {
                    SettingsScripts[i]?.Init();
                }
            }
        }

        void OnDestroy()
        {
            FirebaseMessages.OnMessageReceived -= OnMessageReceivedHandler;
        }

        // Remember: The assets have to be in a folder called "Resources" to be found
        public void AssignRemoteSettings()
        {
            SettingsScripts = Resources.LoadAll<RemoteSettingsBase>(string.Empty);
        }

        public void InitializeFirebase()
        {
            isInitialized = true;
            FetchData();
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause && isInitialized && !isFetchInProgress)
            {
                FetchData();
            }
        }

        void OnMessageReceivedHandler(FirebaseMessage message)
        {
            if (message.Data.TryGetValue(MessagingConsts.CONFIG_STATE, out var state) &&
                state.Equals(MessagingConsts.ConfigState.STALE))
                StartFetchData();
        }

        void FetchData()
        {
            try
            {
                if (FetchExist())
                {
                    FirebaseRemoteConfig.ActivateFetched();
                    OnRemoteConfigFetched();
                    OnFetchComplete?.Invoke();
                }

                DateTime lastFetch = GetLastSuccessedFetchDate();
                TimeSpan timeSinceLastFetch = DateTime.Now - lastFetch;

                if (timeSinceLastFetch.TotalSeconds > GetCacheExpiration().TotalSeconds)
                {
                    StartFetchData();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        void StartFetchData()
        {
            isFetchInProgress = true;
            Task fetchTask = FirebaseRemoteConfig.FetchAsync(GetCacheExpiration());
            fetchTask.ContinueWith(FetchComplete);
        }

        private void FetchComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                Debug.Log("Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                Debug.Log("Fetch encountered an error.");
            }
            else if (fetchTask.IsCompleted)
            {
                Debug.Log("Fetch completed successfully!");
            }

            var info = FirebaseRemoteConfig.Info;
            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    FirebaseRemoteConfig.ActivateFetched();
                    OnRemoteConfigFetched();
#if VIPERA_CORE

                    MainThreadQueue.Enqueue(SetLastSuccessedFetchDate);
#else
                    SetLastSuccessedFetchDate();
#endif
                    break;
                case LastFetchStatus.Failure:
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    switch (info.LastFetchFailureReason)
                    {
                        case FetchFailureReason.Error:
                            Debug.Log("Fetch failed: " + fetchTask.Exception.ToString());

                            break;
                        case FetchFailureReason.Throttled:
                            Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
#endif
                    break;
                case LastFetchStatus.Pending:
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    Debug.Log("Latest Fetch call still pending.");
#endif
                    break;
            }
            isFetchInProgress = false;
        }

        void OnRemoteConfigFetched()
        {
            foreach (var item in SettingsScripts)
            {
                try
                {
                    item.Setup();
                }
                catch (System.Exception ex)
                {
#if UNITY_EDITOR
                    Debug.LogError("Failed to setup settings:" + ex);
#endif
                }
            }

#if VIPERA_CORE
            MainThreadQueue.Enqueue(() =>
            {
                OnFetchComplete?.Invoke();
            });
#else
            OnFetchComplete?.Invoke();
#endif
        }

        TimeSpan GetCacheExpiration()
        {
            //TimeSpan cacheExpiration = FirebaseRemoteConfig.DefaultCacheExpiration;

            //firebase allows to fetch remote config 5 times per 60 minutes with version 17.0.0 or newer
            TimeSpan cacheExpiration = TimeSpan.FromMinutes(15);//allow fetch 4 times per 60 minutes

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            cacheExpiration = TimeSpan.Zero;
#endif
            return cacheExpiration;
        }

        bool FetchExist()
        {
            return lastSuccessedFetchDate != String.Empty;
        }

        DateTime GetLastSuccessedFetchDate()
        {
            DateTime ret = DateTime.MinValue;
            if (lastSuccessedFetchDate != string.Empty)
            {
                try
                {
                    string sTime = lastSuccessedFetchDate;
                    long lTime = Convert.ToInt64(sTime);
                    ret = DateTime.FromBinary(lTime);

                }
                catch (System.Exception)
                {
                    ret = DateTime.MinValue;
                }
            }
            return ret;
        }

        void SetLastSuccessedFetchDate()
        {
            lastSuccessedFetchDate = DateTime.Now.ToBinary().ToString();
        }
    }
}