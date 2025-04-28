using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject gameCanvas;
    public GameObject handPanel;
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;

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
    }

    void Update()
    {
        if (tutorialPanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            tutorialPanel.SetActive(false);
            StartGame();
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
            tutorialText.text = "Easy: Correct slots light up green, wrong ones red. Press Space to start.";
        else if (!showCorrect && showIncorrect)
            tutorialText.text = "Medium: Only wrong moves are highlighted. Press Space to begin.";
        else if (!showCorrect && !showIncorrect)
            tutorialText.text = "Hard: No visual feedback. Use rotations wisely. Press Space to start.";

        gridRows = rows;
        gridCols = cols;

        AdjustHandLayout(rows, cols);

        ClearGrid();

        Sprite selectedImage = new[] { img1, img2, img3 }[Random.Range(0, 3)];
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
        for (int y = rows - 1; y >= 0; y--) // start from top row
        {
            for (int x = 0; x < cols; x++)
            {
                GameObject slot = Instantiate(slotPrefab, gridPanel);
                DropSlot dropSlot = slot.GetComponent<DropSlot>();
                dropSlot.slotRow = y;
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
                Rect rect = new Rect(x * pieceWidth, y * pieceHeight, pieceWidth, pieceHeight);
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
            pieceScript.correctRow = entry.row;
            pieceScript.correctCol = entry.col;
        }
    }

    private void CheckGameEnd()
    {
        if (handPanel.transform.childCount == 0)
        {
            bool allCorrect = true;

            foreach (Transform slot in gridPanel.transform)
            {
                if (slot.childCount == 0)
                {
                    allCorrect = false;
                    break;
                }

                Piece piece = slot.GetComponentInChildren<Piece>();
                DropSlot dropSlot = slot.GetComponent<DropSlot>();

                if (piece == null || dropSlot == null)
                {
                    allCorrect = false;
                    break;
                }

                if (piece.correctRow != dropSlot.slotRow || piece.correctCol != dropSlot.slotCol)
                {
                    allCorrect = false;
                    break;
                }
            }

            if (allCorrect)
            {
                isGameRunning = false;
                Debug.Log("Game Completed!");
                Debug.Log("Final Time: " + FormatTime(elapsedTime));
                Debug.Log($"Penalty Time: {penaltyTimeTotal:0.000} seconds");
                timerText.text += $"\nPenalty: {penaltyTimeTotal:0.000} seconds";
                // TODO: Show Win Panel
            }
        }
    }


}
