using UnityEngine;
using Firebase;
using Firebase.Database;

public class FirebaseTest : MonoBehaviour
{
    void Start()
    {
        // Test básico de conexión
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("🔥 Firebase listo para usar!");
            }
            else
            {
                Debug.LogError("Firebase dependencies error: " + task.Result);
            }
        });
    }
}