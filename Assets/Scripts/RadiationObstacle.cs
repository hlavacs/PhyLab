using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadiationObstacle : MonoBehaviour
{
    public enum ObstacleType {
        Paper,
        Aluminium,
        Led,
        Water
    }

    public GameObject Ghost;
    public GameObject Obstacle;
    public GameObject ObstacleCollision;

    public bool IsObstacleActive;
    public ObstacleType Type;

    public RadiationManager Manager;

    public void ActivateObstacle(bool active) {
        Ghost.SetActive(!active);
        Obstacle.SetActive(active);
        IsObstacleActive = active;
        Manager.UpdateObstacles();
    }

    public void ToggleObstacle() {
        ActivateObstacle(!IsObstacleActive);
    }
}
