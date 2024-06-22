using UnityEngine;

public class ScreenSetter : MonoBehaviour
{
    public static ScreenSetter Instance;

    [Space(5)]
    [SerializeField] private bool _enableCustomResolution = false;
    [Space(10)]
    [SerializeField] private int _width = 1270;
    [SerializeField] private int _height = 720;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            Instance = this;
        }

        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (_enableCustomResolution)
        {
            Screen.SetResolution(_width, _height, false);
        }
    }
}
