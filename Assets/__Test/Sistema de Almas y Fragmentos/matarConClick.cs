using UnityEngine;

public class MatarConClick : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            DropAlSpawnner[] enemigos = FindObjectsByType<DropAlSpawnner>(FindObjectsSortMode.None);

            foreach (DropAlSpawnner enemigo in enemigos)
            {
                Destroy(enemigo.gameObject);
            }
        }
    }
}
