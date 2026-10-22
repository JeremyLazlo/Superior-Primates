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
        //Fetch inventory controller if not already done (bad fix)
        if(inventoryController == null)
        {
            inventoryController = GameObject.FindGameObjectWithTag("InvController").GetComponent<InventoryController>();
            Debug.LogWarning("InventoryController missing for LockedDoor, had to fetch.");
        }
        if(other.CompareTag("Player"))
        {
            if(inventoryController == null)
            {
                Debug.LogError("Missing InventoryController for LockedDoor.");
                return;
            }
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