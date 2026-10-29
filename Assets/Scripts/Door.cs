using Unity.AI.Navigation;
using UnityEngine;

public class Door : MonoBehaviour
{

    public Animator doorAnim;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !(doorAnim.GetCurrentAnimatorStateInfo(0).IsName("DoorOpen")))
        {
            //Open door
            doorAnim.SetTrigger("OpenDoor");
        }
    }
}
