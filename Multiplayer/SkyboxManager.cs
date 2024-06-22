using UnityEngine;

public class SkyboxManager : MonoBehaviour
{
    public SkyboxConfig[] _skyboxes;

    private void Start()
    {
        LoadSkybox();
    }

    public void LoadSkybox()
    {
        if (RenderSettings.skybox == default && StaticHolder.playersPresent == false)
        {
            int randomIndex = StaticHolder.skyboxIndex = (byte)Random.Range(0, _skyboxes.Length);

            RenderSettings.ambientSkyColor = _skyboxes[randomIndex].color;
            RenderSettings.skybox = _skyboxes[randomIndex].material;
            RenderSettings.fog = _skyboxes[randomIndex].fogEnable;
        }
        else
        {
            RenderSettings.skybox = _skyboxes[StaticHolder.skyboxIndex].material;
            RenderSettings.ambientSkyColor = _skyboxes[StaticHolder.skyboxIndex].color;
            RenderSettings.fog = _skyboxes[StaticHolder.skyboxIndex].fogEnable;
        }

        StaticHolder.playersPresent = true;
    }

}

[System.Serializable]
public struct SkyboxConfig
{
    [ColorUsage(true, true)]
    public Color color;
    public Material material;

    public bool fogEnable;
}