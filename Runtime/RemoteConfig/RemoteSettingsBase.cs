using System;
using UnityEngine;
using Firebase.RemoteConfig;

namespace Vipera
{
    public abstract class RemoteSettingsBase : ScriptableObject
    {
        public string id;
        public event Action OnUpdated;

        public string ID
        {
            get
            {
                if (id.Equals(string.Empty))
                {
                    id = GetType().Name;
                    Debug.Log("[RemoteSettingsBase] id was empty. Getting id from the class name: " + id);
                }

                return id;
            }
        }

        public virtual void Init()
        {

        }

        public virtual void Setup()
        {
            TryGetRemoteConfigValues();
        }

        void DeserialiseJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

            JsonUtility.FromJsonOverwrite(json, this);
            OnUpdated?.Invoke();
        }

        void TryGetRemoteConfigValues()
        {
            var configValue = FirebaseRemoteConfig.GetValue(ID);
            try
            {
                if (configValue.Source.Equals(ValueSource.DefaultValue))
                {
                    Debug.LogWarning("[Remote config] Trying to get default value from the remote config");
                    return;
                }

                DeserialiseJson(configValue.StringValue);
                Debug.Log("[Remote config] deserialised successfuly: " + ID);
            }
            catch (ArgumentException e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        public string GetJson()
        {
            return JsonUtility.ToJson(this);
        }

        public string GetString(string name)
        {
            return FirebaseRemoteConfig.GetValue(name).StringValue;
        }

        public bool GetBool(string name)
        {
            return FirebaseRemoteConfig.GetValue(name).BooleanValue;
        }

        public long GetLong(string name)
        {
            return FirebaseRemoteConfig.GetValue(name).LongValue;
        }

        public int[] GetIntArray(char separator, string variableName)
        {
            string data = GetString(variableName);

            if (string.IsNullOrEmpty(data))
            {
                return new int[0];
            }

            try
            {
                var temp = data.Split(separator);
                int[] arr = new int[temp.Length];
                for (int i = 0; i < temp.Length; i++)
                {
                    int num;
                    if (int.TryParse(temp[i], out num))
                    {
                        arr[i] = num;
                    }
                }

                return arr;
            }
            catch (System.Exception)
            {
                return new int[0];
            }
        }

        public long[] GetLongArray(char separator, string variableName)
        {
            string data = GetString(variableName);

            if (string.IsNullOrEmpty(data))
            {
                return new long[0];
            }

            try
            {
                var temp = data.Split(separator);
                long[] arr = new long[temp.Length];
                for (int i = 0; i < temp.Length; i++)
                {
                    long num;
                    if (long.TryParse(temp[i], out num))
                    {
                        arr[i] = num;
                    }
                }

                return arr;
            }
            catch (System.Exception)
            {
                return new long[0];
            }
        }
    }
}
