using Unity.VisualScripting;
using UnityEngine;

public class LockedDoor : MonoBehaviour
{

    public Animator doorAnim;
    [SerializeField] private InventoryController inventoryController;

    void Start()
    {
        Debug.Log("Calling Start in LockedDoor");
        GameObject temp = GameObject.FindGameObjectWithTag("InvController");
        inventoryController = temp.GetComponent<InventoryController>();
    }

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if(inventoryController == null)
        {
            inventoryController = GameObject.FindGameObjectWithTag("InvController").GetComponent<InventoryController>();
            Debug.LogError("Missing InventoryController for LockedDoor.");
        }
        if(other.CompareTag("Player"))
        {
            if(inventoryController.FindItem("Key"))
            {
                //Open door
                doorAnim.SetTrigger("OpenDoor");
            }
        }
    }
}