using System;
using System.Collections.Generic;
using Firebase.Messaging;
using UnityEngine;

namespace Vipera
{
    public static class FirebaseMessages
    {
        public static event Action<FirebaseMessage> OnMessageReceived;

        static List<Action<FirebaseMessage>> onMessageReceivedCallbacks = new List<Action<FirebaseMessage>>();

        public static void Init()
        {
            FirebaseMessaging.TokenReceived += OnTokenReceivedHandler;
            FirebaseMessaging.MessageReceived += OnMessageReceivedHandler;
            Debug.Log("[Messaging] Init");
        }

        public static void ClearCallbacks()
        {
            FirebaseMessaging.TokenReceived -= OnTokenReceivedHandler;
            FirebaseMessaging.MessageReceived -= OnMessageReceivedHandler;
            onMessageReceivedCallbacks.Clear();
        }

        public static void AddFCMIntValueHandler(string valueName, Action<int> handler)
        {
            onMessageReceivedCallbacks.Add(msg => { TryDeserializeIntValue(msg, valueName, handler); });
        }

        public static void TryDeserializeIntValue(FirebaseMessage msg, string valueName, Action<int> successHandler)
        {
            if (msg.Data.TryGetValue(valueName, out var state))
            {
                Debug.Log("[FCM] received message: " + state);
                if (msg.Data.TryGetValue(valueName, out string amount))
                    successHandler.Invoke(Int32.Parse(amount));
            }
        }

        static void OnTokenReceivedHandler(object sender, TokenReceivedEventArgs token)
        {
            Debug.Log("Received Registration Token: " + token.Token);
            FirebaseMessaging.SubscribeAsync(MessagingConsts.Topics.RemoteConfigPush);
        }

        static void OnMessageReceivedHandler(object sender, MessageReceivedEventArgs e)
        {
            Debug.Log("Received a new message from: " + e.Message.From);
            OnMessageReceived?.Invoke(e.Message);

            foreach (var c in onMessageReceivedCallbacks)
                c.Invoke(e.Message);
        }
    }
}