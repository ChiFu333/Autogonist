using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemInSlot : MonoBehaviour
{
    public bool startNull = true;
    public Item myItem;
    private Image _renderer;

    private void Start()
    {
        _renderer = GetComponent<Image>();
        if(startNull) UpdateItem(null);
        UpdateUI(false);
    }

    public void UpdateItem(Item it)
    {
        if (it == null)
        {
            myItem = null;
            _renderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, 0);
        }
        else
        {
            myItem = it;
            _renderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, 255);
        }
    }

    public void UpdateUI(bool isThisSelected)
    {
        if (myItem == null) return;
        _renderer.sprite = isThisSelected ? myItem.selectedSprite : myItem.sprite;
    }
}

[Serializable] public class Item
{
    public int id;
    public Sprite sprite;
    public Sprite selectedSprite;
}
