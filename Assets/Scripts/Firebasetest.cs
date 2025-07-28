using UnityEngine;
using Firebase;
using Firebase.Database;

public class FirebaseTest : MonoBehaviour
{
    void Start()
{
    Debug.Log("=== FIREBASE INIT STARTED ===");
    
    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
        Debug.Log("=== FIREBASE TASK COMPLETED ===");
        Debug.Log("Task Result: " + task.Result);
        
        if (task.Result == DependencyStatus.Available)
        {
            Debug.Log("🔥 Firebase listo para usar!");
            
            // ✅ NUEVO: Test básico de escritura
            TestFirebaseConnection();
        }
        else
        {
            Debug.LogError("Firebase dependencies error: " + task.Result);
        }
    });
}

// ✅ NUEVA FUNCIÓN: Test de conexión
void TestFirebaseConnection()
{
    Debug.Log("Testing Firebase connection...");
    
    FirebaseDatabase.DefaultInstance.RootReference
        .Child("test").SetValueAsync("Hello Firebase").ContinueWith(task => {
        
        if (task.IsCompletedSuccessfully)
        {
            Debug.Log("✅ Firebase WRITE test successful!");
        }
        else
        {
            Debug.LogError("❌ Firebase WRITE test failed: " + task.Exception);
        }
    });
}
}