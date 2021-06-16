using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpInstructions : MonoBehaviour
{
    public Image popUp;
    public Canvas fireBreath;
    public Canvas fireWall;
    public Canvas leap;

    private bool _checkForChest = false;
    private Vector3 _playerInitialPosition;
    public float radius = 5f;
    
    public void enableFireBreath()
    {
        fireBreath.enabled = true;
        StartPopUp();
    }
    
    public void enableFireWall()
    {
        fireWall.enabled = true;
        StartPopUp();
    }
    
    public void enableLeap()
    {
        leap.enabled = true;
        StartPopUp();
    }

    private void StartPopUp()
    {
        popUp.enabled = true;
        _playerInitialPosition = transform.position;
        _checkForChest = true;
    } 

    private void StopPopUp()
    {
        popUp.enabled = false;
        fireBreath.enabled = false;
        fireWall.enabled = false;
        leap.enabled = false;
        _checkForChest = false;
    } 
    
    // Update is called once per frame
    void Update()
    {
        // When a chest is out of radius 5, the pop up disappears
        if (_checkForChest)
        {
            if ((transform.position - _playerInitialPosition).magnitude > radius)
            {
                StopPopUp();
            }
            
        }
    }
}
