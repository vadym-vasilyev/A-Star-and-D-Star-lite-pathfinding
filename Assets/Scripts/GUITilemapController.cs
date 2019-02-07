using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUITilemapController : MonoBehaviour
{
    [SerializeField] GridController gridController;
    [SerializeField] Image currentBrush;

    void Start() {
        SetBrushBlocked();
    }

    public void SetBrushBlocked() {
        gridController.SetBrush(BrushType.blocked);
        currentBrush.sprite = gridController.CurrentBrush.sprite;
    }

    public void SetBrushStandart() {
        gridController.SetBrush(BrushType.standart);
        currentBrush.sprite = gridController.CurrentBrush.sprite;
    }

    public void SetBrushDoubleCost() {
        gridController.SetBrush(BrushType.doubleCost);
        currentBrush.sprite = gridController.CurrentBrush.sprite;
    }

    public void SetBrushTripleCost() {
        gridController.SetBrush(BrushType.tripleCost);
        currentBrush.sprite = gridController.CurrentBrush.sprite;
    }

    public void SetBrushGoal() {
        gridController.SetBrush(BrushType.goal);
        currentBrush.sprite = gridController.CurrentBrush.sprite;
    }

    public void SetBrushStart() {
        gridController.SetBrush(BrushType.goal);
        currentBrush.sprite = gridController.CurrentBrush.sprite;
    }

    public void StratPathFindinAlgorithm() {
        AStarAlgorithm algorithm = new AStarAlgorithm(gridController, new ManhattanDistanceHeuristic(gridController.Graf, 1, true));
        StartCoroutine(algorithm.FindPath());
    }
}
