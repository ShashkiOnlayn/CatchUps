using UnityEngine;

public class NicksCameraFacing : MonoBehaviour
{
    private CatchUpController _controller;
    private GameObject m_parent;

    private void Start()
    {
        m_parent = gameObject.transform.root.gameObject;

        _controller = m_parent.GetComponent<CatchUpController>();
    }

    private void LateUpdate()
    {
        foreach (var player in _controller.players)
        {
            if(player.gameObject == m_parent)
                continue;

            player.transform.GetChild(0).LookAt(transform.position);
        }
    }
}
