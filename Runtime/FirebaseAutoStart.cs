using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseAutoStart : MonoBehaviour
{
    public GameObject firebasePrefab;
    static FirebaseAutoStart instance;

    [RuntimeInitializeOnLoadMethod]
    public static void AutoStart()
    {
        if (instance == null)
        {
            instance = new GameObject("FirebaseAutostart").AddComponent<FirebaseAutoStart>();
        }

        if (Vipera.FirebaseBase.Instance == null)
        {
            Instantiate(instance.firebasePrefab).name = "Vipera Firebase";
        }

        Destroy(instance.gameObject);
    }
}
