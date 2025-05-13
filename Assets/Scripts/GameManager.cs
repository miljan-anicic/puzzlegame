using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour , ILocalizable
{
    public GameObject menuCanvas;
    public GameObject gameCanvas;
    public GameObject handPanel;
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI easyButtonText;
    public TextMeshProUGUI mediumButtonText;
    public TextMeshProUGUI hardButtonText;

    public Button resetProgressButton;
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;

    private bool showWinScreen = false;


    public int correctCount = 0;

    public GameObject slotPrefab;
    public Transform gridPanel;
    private GridLayoutGroup gridLayout;

    public TextMeshProUGUI timerText;

    private bool isGameRunning = false;
    private float elapsedTime = 0f;

    public Sprite img1;
    public Sprite img2;
    public Sprite img3;

    public GameObject piecePrefab;

    private int gridRows;
    private int gridCols;
    private GridLayoutGroup handLayout;

    private float penaltyTimeTotal = 0f;
    private float penaltyPerMistake = 0f;
    private bool showCorrectFeedback = true;


    void Start()
    {
        menuCanvas.SetActive(true);
        gameCanvas.SetActive(false);
        tutorialPanel.SetActive(false);

        handLayout = handPanel.GetComponent<GridLayoutGroup>();
        gridLayout = gridPanel.GetComponent<GridLayoutGroup>();
        UpdateText();

        resetProgressButton.onClick.AddListener(ResetProgress);
        LoadProgress();
        UpdateButtonStates();
    }
    private void ResetProgress()
    {
        PlayerPrefs.DeleteKey("EasyComplete");
        PlayerPrefs.DeleteKey("MediumComplete");
        UpdateButtonStates();
    }

    private void LoadProgress()
    {
        bool easyDone = PlayerPrefs.GetInt("EasyComplete", 0) == 1;
        bool mediumDone = PlayerPrefs.GetInt("MediumComplete", 0) == 1;

        mediumButton.interactable = easyDone;
        hardButton.interactable = mediumDone;
    }

    private void UpdateButtonStates()
    {
        LoadProgress();
    }


void Update()
{
    if (tutorialPanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
    {
        if (showWinScreen)
        {
            showWinScreen = false;
            tutorialPanel.SetActive(false);
            menuCanvas.SetActive(true);
        }
        else
        {
            tutorialPanel.SetActive(false);
            StartGame();
        }
    }

    if (isGameRunning)
    {
        elapsedTime += Time.deltaTime;
        timerText.text = FormatTime(elapsedTime);
        CheckGameEnd();
    }
}


    private void StartGame()
    {
        isGameRunning = true;
        elapsedTime = 0f;
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000f);
        return $"{minutes:00}:{seconds:00}:{milliseconds:000}";
    }


    public void StartEasyGame()
    {
        SetupGame(3, 4, showCorrect: true, showIncorrect: true, penaltyTime: 0.5f, allowRotation: false);
    }

    public void StartMediumGame()
    {
        SetupGame(4, 5, showCorrect: false, showIncorrect: true, penaltyTime: 1f, allowRotation: false);
    }

    public void StartHardGame()
    {
        SetupGame(5, 6, showCorrect: false, showIncorrect: false, penaltyTime: 2f, allowRotation: true);
    }

    private void SetupGame(int rows, int cols, bool showCorrect, bool showIncorrect, float penaltyTime, bool allowRotation)
    {
        menuCanvas.SetActive(false);
        gameCanvas.SetActive(true);
        tutorialPanel.SetActive(true);

        if (showCorrect && showIncorrect)
            tutorialText.text = LocalizationManager.Instance.GetText("easy_text");
        else if (!showCorrect && showIncorrect)
            tutorialText.text = LocalizationManager.Instance.GetText("medium_text");
        else if (!showCorrect && !showIncorrect)
            tutorialText.text = LocalizationManager.Instance.GetText("hard_text");

        gridRows = rows;
        gridCols = cols;

        AdjustHandLayout(rows, cols);

        ClearGrid();

        Sprite selectedImage = img1;
        if (rows == 4) selectedImage = img2;
        else if (rows == 5) selectedImage = img3;

        Sprite[,] slicedPieces = SliceImage(selectedImage, rows, cols);
        SpawnGridSlots(rows, cols);
        SpawnPieces(slicedPieces);

        penaltyPerMistake = penaltyTime; // store the penalty per wrong move
        penaltyTimeTotal = 0f; // reset total penalty
        showCorrectFeedback = showCorrect; // store for later
    }


    private void AdjustGridLayout(int rows, int cols)
    {
        if (gridLayout == null) gridLayout = gridPanel.GetComponent<GridLayoutGroup>();

        float panelWidth = ((RectTransform)gridPanel).rect.width;
        float panelHeight = ((RectTransform)gridPanel).rect.height;

        float cellWidth = panelWidth / cols;
        float cellHeight = panelHeight / rows;

        gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
    }

    private void SpawnGridSlots(int rows, int cols)
    {
        for (int y = 0; y < rows; y++) // build from bottom to top
        {
            for (int x = 0; x < cols; x++)
            {
                GameObject slot = Instantiate(slotPrefab, gridPanel);
                DropSlot dropSlot = slot.GetComponent<DropSlot>();
                dropSlot.slotRow = rows - 1 - y;
                dropSlot.slotCol = x;
            }
        }

    }


    public void AddPenalty()
    {
        penaltyTimeTotal += penaltyPerMistake;
    }


    private void ClearGrid()
    {
        foreach (Transform child in gridPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void AdjustHandLayout(int rows, int cols)
    {
        if (handLayout == null) handLayout = handPanel.GetComponent<GridLayoutGroup>();

        float baseWidth = 240f;
        float baseHeight = 135f;

        int totalPieces = rows * cols / 2 + 4;
        float scaleFactor = 12f / totalPieces;

        float finalWidth = baseWidth * scaleFactor;
        float finalHeight = baseHeight * scaleFactor;

        handLayout.cellSize = new Vector2(finalWidth, finalHeight);
    }

    private Sprite[,] SliceImage(Sprite image, int rows, int cols)
    {
        Texture2D texture = image.texture;
        int pieceWidth = texture.width / cols;
        int pieceHeight = texture.height / rows;
        Sprite[,] pieces = new Sprite[rows, cols];

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
            {
                int flippedY = rows - 1 - y;
                Rect rect = new Rect(x * pieceWidth, flippedY * pieceHeight, pieceWidth, pieceHeight);
                Vector2 pivot = new Vector2(0.5f, 0.5f);
                pieces[y, x] = Sprite.Create(texture, rect, pivot);
            }

        return pieces;
    }

    private void SpawnPieces(Sprite[,] pieces)
    {
        foreach (Transform child in handPanel.transform)
            Destroy(child.gameObject);

        List<(Sprite sprite, int row, int col)> flatPieces = new List<(Sprite, int, int)>();

        for (int y = 0; y < pieces.GetLength(0); y++)
            for (int x = 0; x < pieces.GetLength(1); x++)
                flatPieces.Add((pieces[y, x], y, x));

        for (int i = 0; i < flatPieces.Count; i++)
        {
            var temp = flatPieces[i];
            int randomIndex = Random.Range(i, flatPieces.Count);
            flatPieces[i] = flatPieces[randomIndex];
            flatPieces[randomIndex] = temp;
        }

        foreach (var entry in flatPieces)
        {
            GameObject piece = Instantiate(piecePrefab, handPanel.transform);
            piece.GetComponent<Image>().sprite = entry.sprite;
            Piece pieceScript = piece.GetComponent<Piece>();
            pieceScript.correctRow = gridRows - 1 - entry.row;


            pieceScript.correctCol = entry.col;
        }
    }

    public void UpdateText()
    {
        if (titleText != null)
            titleText.text = LocalizationManager.Instance.GetText("title_text");
        if (easyButtonText != null)
            easyButtonText.text = LocalizationManager.Instance.GetText("easy_btn");
        if (mediumButtonText != null)
            mediumButtonText.text = LocalizationManager.Instance.GetText("medium_btn");
        if (hardButtonText != null)
            hardButtonText.text = LocalizationManager.Instance.GetText("hard_btn");

        if (!tutorialPanel.activeSelf) return;

        if (showCorrectFeedback)
            tutorialText.text = LocalizationManager.Instance.GetText("easy_text");
        else if (!showCorrectFeedback && penaltyPerMistake <= 1f)
            tutorialText.text = LocalizationManager.Instance.GetText("medium_text");
        else
            tutorialText.text = LocalizationManager.Instance.GetText("hard_text");
    }



private void CheckGameEnd()
{
    if (handPanel.transform.childCount > 0)
    {
        Debug.Log("GameManager.cs:CheckGameEnd - Hand still has pieces.");
        return;
    }

    int totalSlots = gridRows * gridCols;
    

    foreach (Transform slot in gridPanel.transform)
    {
        if (slot.childCount == 0) return;

        Piece piece = slot.GetComponentInChildren<Piece>();
        DropSlot dropSlot = slot.GetComponent<DropSlot>();

        if (piece == null || dropSlot == null) return;

        if (piece.correctRow == dropSlot.slotRow && piece.correctCol == dropSlot.slotCol)
            correctCount++;
    }

    if (correctCount < totalSlots)
    {
        Debug.Log("GameManager.cs: Not all pieces are correct.");
        return;
    }

    Debug.Log("GameManager.cs: All placements correct. Game completed.");
    isGameRunning = false;

    string finalTime = FormatTime(elapsedTime);
    float totalTime = elapsedTime + penaltyTimeTotal;
    string totalTimeStr = totalTime.ToString("0.000");

    Debug.Log("Final Time: " + finalTime);
    Debug.Log("Penalty Time: " + penaltyTimeTotal.ToString("0.000") + " seconds");

    timerText.text += $"\nPenalty: {penaltyTimeTotal:0.000} seconds";

    if (gridRows == 3)
    {
        PlayerPrefs.SetInt("EasyComplete", 1);
        PlayerPrefs.SetString("EasyTime", totalTimeStr);
    }
    else if (gridRows == 4)
    {
        PlayerPrefs.SetInt("MediumComplete", 1);
        PlayerPrefs.SetString("MediumTime", totalTimeStr);
    }
    else if (gridRows == 5)
    {
        PlayerPrefs.SetString("HardTime", totalTimeStr);
    }

    string difficulty = gridRows == 3 ? "Easy" : gridRows == 4 ? "Medium" : "Hard";

    PlayerPrefs.SetString("PendingTime", totalTimeStr);
    PlayerPrefs.SetString("PendingName", "Player"); // update later with input
    PlayerPrefs.SetString("PendingDifficulty", difficulty);
    PlayerPrefs.SetString("PendingPenalty", penaltyTimeTotal.ToString("0.000"));
    PlayerPrefs.SetString("PendingScore", (1000f / totalTime).ToString("0.000")); // basic score formula
    PlayerPrefs.SetString("PendingImage", "");
    PlayerPrefs.SetInt("HasPendingSubmission", 1);

    UpdateButtonStates();
    ResetToMenu();
}

private void ResetToMenu()
{
    elapsedTime = 0f;
    penaltyTimeTotal = 0f;
    gameCanvas.SetActive(false);
    menuCanvas.SetActive(false);
    showWinScreen = true;

    float baseScore = gridRows == 3 ? 1000 : gridRows == 4 ? 2500 : 5000;
    float penalty = penaltyTimeTotal * (gridRows == 3 ? 50 : gridRows == 4 ? 100 : 200);
    float speedPenalty = elapsedTime <= 10 ? elapsedTime * 1000 : 10000 + ((elapsedTime - 10f) * 500);
    float score = Mathf.Max(0f, baseScore - penalty - speedPenalty);

    string difficulty = gridRows == 3 ? "Easy" : gridRows == 4 ? "Medium" : "Hard";
    string message = LocalizationManager.Instance.GetText("congrats_text")
        .Replace("<difficulty>", difficulty)
        .Replace("<time>", FormatTime(elapsedTime))
        .Replace("<penalty>", penaltyTimeTotal.ToString("0.000"))
        .Replace("<score>", Mathf.RoundToInt(score).ToString());

    tutorialText.text = message;
    tutorialPanel.SetActive(true);
}


}
