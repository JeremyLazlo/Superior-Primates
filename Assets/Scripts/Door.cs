using UnityEngine;

public class Door : MonoBehaviour
{

    public Animator doorAnim;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //Open door
            doorAnim.SetTrigger("OpenDoor");
        }
    }
}
