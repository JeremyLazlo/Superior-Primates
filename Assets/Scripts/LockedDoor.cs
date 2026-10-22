using UnityEngine;

public class LockedDoor : MonoBehaviour
{

    public Animator doorAnim;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //Open door
            //doorAnim.SetTrigger("OpenDoor");
        }
    }
}