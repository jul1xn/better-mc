using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadToMenu : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
