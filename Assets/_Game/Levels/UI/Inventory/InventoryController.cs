using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private List<GameObject> buttons;
    [SerializeField] private Sprite justButton;
    [SerializeField] private Sprite selectedButton;

    private int selectedId = -1;
    void Start()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            int t = i;
            buttons[i].GetComponent<UIButtonScaling>().OnClick.AddListener(() =>
            {
                HandleClickButton(t);
            });
        }
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            HandleClickButton(-1);
        }
    }

    public void HandleClickButton(int id)
    {
        selectedId = id;
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = (i != id) ? justButton : selectedButton;
            buttons[i].transform.GetChild(1).GetChild(1).GetComponent<ItemInSlot>().UpdateUI(i == id);
        }
    }

    public void AddItemInInventory(Item it)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i].transform.GetChild(1).GetChild(1).GetComponent<ItemInSlot>().myItem == null)
            {
                buttons[i].transform.GetChild(1).GetChild(1).GetComponent<ItemInSlot>().UpdateItem(it);
                buttons[i].transform.GetChild(1).GetChild(1).GetComponent<ItemInSlot>().UpdateUI(selectedId == i);
                break;
            }
        }
    }

    public bool CheckWithItem(int id)
    {
        if (selectedId == -1) return false;
        if (buttons[selectedId].transform.GetChild(1).GetChild(1).GetComponent<ItemInSlot>().myItem == null)
            return false;
        return buttons[selectedId].transform.GetChild(1).GetChild(1).GetComponent<ItemInSlot>().myItem.id == id;
    }

    public void RemoveCurrentItem()
    {
        buttons[selectedId].transform.GetChild(1).GetChild(1).GetComponent<ItemInSlot>().UpdateItem(null);
        buttons[selectedId].transform.GetChild(1).GetChild(1).GetComponent<ItemInSlot>().UpdateUI(true);
    }
}
