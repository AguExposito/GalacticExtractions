using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void LoadScene(string name) {
        SceneManager.LoadSceneAsync(name);
    }

    public void Quit() { 
        Application.Quit();
    }
}
