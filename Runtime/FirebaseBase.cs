using UnityEngine;


namespace Vipera
{
    public class FirebaseBase : MonoBehaviour
    {
        public static FirebaseBase Instance;
        public static Firebase.FirebaseApp app;

        public Analytics analytics;
        public RemoteConfig remoteConfig;

        public virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }

            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    app = Firebase.FirebaseApp.DefaultInstance;
                    InitializeFirebase();
                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                }
                else
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
#endif
                }
            });
        }

        public void InitializeFirebase()
        {
            MainThreadQueue.Enqueue(() =>
            {
                analytics.InitializeFirebase();
                remoteConfig.InitializeFirebase();
                FirebaseMessages.Init();
            });
        }

        void OnDestroy()
        {
            FirebaseMessages.ClearCallbacks();
        }
    }
}
