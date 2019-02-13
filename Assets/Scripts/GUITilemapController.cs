using UnityEngine;
using UnityEngine.UI;

public class GUITilemapController : MonoBehaviour {
    [SerializeField] GridController gridController;
    [SerializeField] GridMarkerController gridMarkerController;
    [SerializeField] Image currentBrush;

    [SerializeField] Button pathFind;
    [SerializeField] Button resetCurrentPath;
    [SerializeField] Button clearAll;

    [SerializeField] InputField heuristicCoeffField;
    [SerializeField] InputField delayBeforeSteps;

    Coroutine pathFindingAlgorithmCoroutine;
    HeuristicType heuristicType = (HeuristicType)0;
    TieBreakerType breakerType = (TieBreakerType)0;

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

    public void OnHeuristicDropdownValueChange(int index) {
        heuristicType = (HeuristicType)index;
    }

    public void OnTieBrakerDropdownValueChange(int index) {
        breakerType = (TieBreakerType)index;
    }

    public void ResetCurrentPathfainding() {
        StopCoroutine(pathFindingAlgorithmCoroutine);
        gridMarkerController.ClearAllMarkers();
        gridController.PathGridState = PathGridState.Edit;
        pathFind.interactable = true;
        resetCurrentPath.interactable = false;
        clearAll.interactable = false;
    }

    public void ClearAll() {
        StopCoroutine(pathFindingAlgorithmCoroutine);
        gridMarkerController.ClearAllMarkers();
        gridController.ClearAllNodes();
        gridController.PathGridState = PathGridState.Edit;
        pathFind.interactable = true;
        resetCurrentPath.interactable = false;
        clearAll.interactable = false;
    }

    public void StratPathFindinAlgorithm() {
        pathFind.interactable = false;
        resetCurrentPath.interactable = true;
        clearAll.interactable = true;
        gridController.PathGridState = PathGridState.PathFind;
        gridController.InitGraf();
        Vector2Int goalPosition = new Vector2Int(gridController.GoalPos.x, gridController.GoalPos.y);
        Vector2Int startPosition = new Vector2Int(gridController.StartPos.x, gridController.StartPos.y);

        IHeuristicEstimate heuristic = HeuristicFactory.СreateHeuristic(heuristicType, breakerType, startPosition, int.Parse(heuristicCoeffField.text));
        float timeDelay = float.Parse(delayBeforeSteps.text);
        AStarAlgorithm algorithm = new AStarAlgorithm(gridMarkerController, gridController.Graf, heuristic, timeDelay);

        pathFindingAlgorithmCoroutine = StartCoroutine(algorithm.FindPath(gridController.GetStartVertex(), gridController.GetGoalVertex()));
    }

    private void SetBrush(BrushType newBrush) {
        gridController.SetBrush(newBrush);
        currentBrush.sprite = gridController.CurrentBrush.sprite;
    }
}
