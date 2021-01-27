using UnityEngine;

namespace Vipera
{
    public static class FCMMessagingHandlers
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            FirebaseMessages.AddFCMIntValueHandler("cash", amount =>
            {
                //DataManager.Instance.money += amount;
                //Debug.Log("[FCMMessagingHandlers] adding bonus cash");
            });
        }
    }
}