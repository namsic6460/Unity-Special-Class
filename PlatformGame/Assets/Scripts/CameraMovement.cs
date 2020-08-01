using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public GameObject player;
    Vector3 playerPosition;

    private void LateUpdate()
    {
        playerPosition = player.transform.position;
        
        if(playerPosition.y > -20)
            transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z - 11);
    }

}