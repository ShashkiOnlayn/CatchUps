using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneChanger : MonoBehaviour
{
    private static bool _menuChanged;

    private void Start()
    {
        if (!_menuChanged)
            SceneManager.LoadScene(0);

        _menuChanged = true;
    }
}
