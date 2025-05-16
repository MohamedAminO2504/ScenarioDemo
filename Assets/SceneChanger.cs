using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Nom de la scène à charger
    public string nomScene;

    // Appelle cette fonction pour changer de scène
    public void ChangerDeScene(int s)
    {
        
            SceneManager.LoadScene(s);
       
    }
}
