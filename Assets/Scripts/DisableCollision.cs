using UnityEngine;

public class DisableCollision : MonoBehaviour
{
    [SerializeField]
    Collider m_Collision1;

    [SerializeField]
    Collider m_Collision2;


    void Start()
    {
        if (m_Collision1 != null && m_Collision2 != null)
        {
            Physics.IgnoreCollision(m_Collision1, m_Collision2);
        }
    }


}
