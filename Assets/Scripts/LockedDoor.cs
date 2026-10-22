using Unity.VisualScripting;
using UnityEngine;

public class LockedDoor : MonoBehaviour
{

    public Animator doorAnim;
    [SerializeField] private InventoryController inventoryController;
    private bool isUnlocked = false;

    void Start()
    {
        //IS NOT CALLED FOR SOME REASON FOR LOCKED DOORS IN PREFABS
        Debug.Log("Calling Start in LockedDoor");
        GameObject temp = GameObject.FindGameObjectWithTag("InvController");
        inventoryController = temp.GetComponent<InventoryController>();
    }

    void Update()
    {
        
    }

    //Attempt to open locked door when approached
    void OnTriggerEnter(Collider other)
    {
        if(inventoryController == null)
        {
            inventoryController = GameObject.FindGameObjectWithTag("InvController").GetComponent<InventoryController>();
            Debug.LogError("Missing InventoryController for LockedDoor.");
            return;
        }
        if(other.CompareTag("Player"))
        {
            //Open door if key is present
            ItemInstance key = inventoryController.FindItem("Key");
            if(key != null)
            {
                inventoryController.UseKey(key);
                doorAnim.SetTrigger("OpenDoor");
            }
        }
    }
}