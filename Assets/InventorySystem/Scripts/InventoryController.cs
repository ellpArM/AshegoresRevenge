using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class InventoryController : MonoBehaviour
{
    [SerializeField] UIInventoryPage inventoryPage;

    public int inventorySize = 10;

    public void Start()
    {
        inventoryPage.InitializeInventoryUI(inventorySize);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("OPENED INVENTORY");
            if (inventoryPage.isActiveAndEnabled == false) inventoryPage.Show();
            else inventoryPage.Hide();
        }
    }
}
