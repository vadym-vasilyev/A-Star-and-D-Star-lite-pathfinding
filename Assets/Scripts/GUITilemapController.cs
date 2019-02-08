using UnityEngine;
using UnityEngine.UI;

public class GUITilemapController : MonoBehaviour {
    [SerializeField] GridController gridController;
    [SerializeField] Image currentBrush;

    [SerializeField] Button pathFind;
    [SerializeField] Button resetCurrentPath;
    [SerializeField] Button clearAll;

    [SerializeField] InputField heuristicCoeffField;
    [SerializeField] InputField delayBeforeSteps;

    Coroutine pathFindingAlgorithmCoroutine;
    HeuristicType heuristicType = HeuristicType.Euclidean;

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

    public void ResetCurrentPathfainding() {
        StopCoroutine(pathFindingAlgorithmCoroutine);
        gridController.ClearAllMarkers();
        pathFind.interactable = true;
        resetCurrentPath.interactable = false;
        clearAll.interactable = false;
    }

    public void ClearAll() {
        StopCoroutine(pathFindingAlgorithmCoroutine);
        gridController.ResetAll();
        pathFind.interactable = true;
        resetCurrentPath.interactable = false;
        clearAll.interactable = false;
    }

    public void StratPathFindinAlgorithm() {
        pathFind.interactable = false;
        resetCurrentPath.interactable = true;
        clearAll.interactable = true;
        gridController.CreateGraf();
        Vector2Int goalPosition = new Vector2Int(gridController.GoalPos.x, gridController.GoalPos.y);
        IHeuristicEstimate heuristic = HeuristicFactory.GetHeuristic(heuristicType, goalPosition, int.Parse(heuristicCoeffField.text));
        float timeDelay = float.Parse(delayBeforeSteps.text);
        AStarAlgorithm algorithm = new AStarAlgorithm(gridController, heuristic, timeDelay);

        pathFindingAlgorithmCoroutine = StartCoroutine(algorithm.FindPath());
    }

    private void SetBrush(BrushType newBrush) {
        gridController.SetBrush(newBrush);
        currentBrush.sprite = gridController.CurrentBrush.sprite;
    }
}
