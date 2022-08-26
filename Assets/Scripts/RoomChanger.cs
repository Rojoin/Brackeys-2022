using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomChanger : MonoBehaviour
{
    public GameObject virtualCam;

    [SerializeField] private GameObject checkPoint;
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            virtualCam.SetActive(true);
            if (checkPoint != null)
            {
                checkPoint.gameObject.transform.position = new Vector3(checkPoint.transform.position.x,
                checkPoint.transform.position.y, other.transform.position.z);
                other.gameObject.GetComponent<PlayerController>().checkPoint = checkPoint.gameObject;
            }

        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            virtualCam.SetActive(false);
        }
    }
}
