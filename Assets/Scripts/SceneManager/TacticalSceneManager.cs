using DisplayInterface;
using InputHandler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalSceneManager : MonoBehaviour,
    ITacticalSceneController
{
    ITacticalSystemInput tacticalSystem;

    public GameManager gameManager;

    public int leftBound;
    public int rightBound;
    public int topBound;
    public int bottomBound;

    public void TerrainsInitialize()
    {

    }


    public INodeController InstantiateNode()
    {
        return null;
    }
}
