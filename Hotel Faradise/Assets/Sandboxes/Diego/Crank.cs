using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crank : MonoBehaviour
{
    public float rotateSpeed = 10f;
    private Transform selected;
    private Rope rope;
    private int numLinks;
    public int maxLinks = 15;

    private void Awake()
    {
        selected = transform.Find("Selected");
        rope = transform.parent.GetComponent<Rope>();
        numLinks = rope.numLinks;
    }

    public void Rotate(int direction)
    {
        if (direction > 0 && rope != null && numLinks <= maxLinks)
        {
            transform.Rotate(0, 0, direction * rotateSpeed);
            rope.addLink();
            numLinks++;
        }
        else if(direction<0 && rope != null && numLinks > 1)
        {
            transform.Rotate(0,0,direction*rotateSpeed);
            rope.removeLink();
            numLinks--;
        }
    }

    public void Select()
    {
        selected.gameObject.SetActive(true);
    }

    public void Deselect()
    {
        selected.gameObject.SetActive(false);
    }
}
