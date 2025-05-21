using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This object will self destroy in seconds
/// </summary>
public class ProjectileSelfDestroyer : MonoBehaviour
{
    [Tooltip("Time in seconds before the projectile destroys itself.")]
    public float selfDestructTime = 5f;

    void Start()
    {
        Destroy(gameObject, selfDestructTime);
    }
}
