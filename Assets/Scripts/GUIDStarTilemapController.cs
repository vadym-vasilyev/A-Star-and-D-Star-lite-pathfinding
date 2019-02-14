using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.UI;

public class GUIDStarTilemapController : MonoBehaviour {
    [SerializeField] GridController gridController;
    [SerializeField] GridMarkerController gridMarkerController;
    [SerializeField] UnityEngine.UI.Image currentBrush;

    [SerializeField] UnityEngine.UI.Button pathFind;
    [SerializeField] UnityEngine.UI.Button resetCurrentPath;
    [SerializeField] UnityEngine.UI.Button clearAll;

    [SerializeField] InputField delayBeforeSteps;
    [SerializeField] InputField delayBeforeMove;

    [SerializeField] IPanel panel; 

    Coroutine pathFindingAlgorithmCoroutine;

    void Start() {
        SetBrushBlocked();
    }

    public void SetBrushBlocked() {
        SetBrush(BrushType.blocked);
    }

    public void SetBrushStandart() {
        SetBrush(BrushType.standart);
    }

    public void SetBrushDoubleCost() {
        SetBrush(BrushType.doubleCost);
    }

    public void SetBrushTripleCost() {
        SetBrush(BrushType.tripleCost);
    }

    public void SetBrushGoal() {
        SetBrush(BrushType.goal);
    }

    public void SetBrushStart() {
        SetBrush(BrushType.start);
    }

    private void OnPathSearchState() {
        pathFind.interactable = false;
        resetCurrentPath.interactable = true;
        clearAll.interactable = true;
        gridController.PathGridState = PathGridState.PathFind;
    }

    private void OnStartMove() {
        gridController.PathGridState = PathGridState.MoveByPath;
    }

    private void OnEditState() {
        gridController.PathGridState = PathGridState.Edit;
        pathFind.interactable = true;
        resetCurrentPath.interactable = false;
        clearAll.interactable = false;
    }

    public void ResetCurrentPathfainding() {
        StopCoroutine(pathFindingAlgorithmCoroutine);
        gridMarkerController.ClearAllMarkers();
        OnEditState();
    }

    public void ClearAll() {
        StopCoroutine(pathFindingAlgorithmCoroutine);
        gridMarkerController.ClearAllMarkers();
        gridController.ClearAllNodes();
        gridController.PathGridState = PathGridState.Edit;
        OnEditState();
    }

    public void StratPathFindinAlgorithm() {
        OnPathSearchState();
        gridController.InitGraf();
        Vector2Int goalPosition = new Vector2Int(gridController.GoalPos.x, gridController.GoalPos.y);
        Vector2Int startPosition = new Vector2Int(gridController.StartPos.x, gridController.StartPos.y);

        IHeuristicEstimate heuristic = HeuristicFactory.СreateHeuristic(HeuristicType.Manhattan, TieBreakerType.None, startPosition, 1);
        float timeDelay = float.Parse(delayBeforeSteps.text);
        float moveTimeDelay = float.Parse(delayBeforeMove.text);

        DStarLiteAlgorithm algorithm = new DStarLiteAlgorithm(gridMarkerController, gridController.Graf, heuristic, timeDelay);

        gridController.OnVerteciesUpdate = (vertecies) => { vertecies.ForEach(v => algorithm.UpdatePath(v)); };

        pathFindingAlgorithmCoroutine = StartCoroutine(algorithm.MoveByPath(gridController.GetStartVertex(), gridController.GetGoalVertex(), OnPathSearchState, OnStartMove));
    }

    private void SetBrush(BrushType newBrush) {
        gridController.SetBrush(newBrush);
        currentBrush.sprite = gridController.CurrentBrush.sprite;
    }
}
