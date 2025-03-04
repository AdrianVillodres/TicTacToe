using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
public enum States
{
    CanMove,
    CantMove
}
public class GameManager : MonoBehaviour
{
    public int alpha = -1000;
    public int beta = 1000;
    public Canvas canvas;
    public static GameManager Instance;
    public new BoxCollider2D collider;
    public GameObject token1, token2;
    public int Size = 3;
    public int[,] Matrix;
    [SerializeField] private States state = States.CanMove;
    public new Camera camera;
    public TextMeshProUGUI WhoWins;
    public TextMeshProUGUI WinLose;
    public GameObject Win;
    public GameObject Draw;
    public GameObject Lose;
    void Start()
    {
        Instance = this;
        Matrix = new int[Size, Size];
        Calculs.CalculateDistances(collider, Size);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Matrix[i, j] = 0; // 0: desocupat, 1: fitxa jugador 1, -1: fitxa IA;
            }
        }
    }
    private void Update()
    {
        if (state == States.CanMove)
        {
            Vector3 m = Input.mousePosition;
            m.z = 10f;
            Vector3 mousepos = camera.ScreenToWorldPoint(m);
            if (Input.GetMouseButtonDown(0))
            {
                if (Calculs.CheckIfValidClick((Vector2)mousepos, Matrix))
                {
                    state = States.CantMove;
                    if (Calculs.EvaluateWin(Matrix) == 2)
                        StartCoroutine(WaitingABit());
                }
            }
        }
    }
    private IEnumerator WaitingABit()
    {
        yield return new WaitForSeconds(1f);
        AIMinMax();
    }

    public void AIMinMax()
    {
        int bestMove = -1;
        int bestScore = int.MinValue;
        alpha = int.MinValue;
        beta = int.MaxValue;

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (Matrix[i, j] == 0)
                {
                    Matrix[i, j] = -1;
                    int score = Minimax(Matrix, 0, false, alpha, beta);
                    Matrix[i, j] = 0;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = i * Size + j;
                    }
                }
            }
        }
        DoMove(bestMove / Size, bestMove % Size, -1);
    }

    public int Minimax(int[,] board, int depth, bool isMaximizing, int alpha, int beta)
    {
        int result = Calculs.EvaluateWin(board);
        if (result == -1) return 10 - depth;
        if (result == 1) return depth - 10;
        if (result == 0) return 0;

        if (isMaximizing)
        {
            int bestScore = int.MinValue;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = -1;
                        int score = Minimax(board, depth + 1, false, alpha, beta);
                        board[i, j] = 0;
                        bestScore = Math.Max(bestScore, score);
                        alpha = Math.Max(alpha, score);
                        if (beta <= alpha) break;
                    }
                }
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = 1;
                        int score = Minimax(board, depth + 1, true, alpha, beta);
                        board[i, j] = 0;
                        bestScore = Math.Min(bestScore, score);
                        beta = Math.Min(beta, score);
                        if (beta <= alpha) break;
                    }
                }
            }
            return bestScore;
        }
    }
    public void DoMove(int x, int y, int team)
    {
        Matrix[x, y] = team;
        if (team == 1)
            Instantiate(token1, Calculs.CalculatePoint(x, y), Quaternion.identity);
        else
            Instantiate(token2, Calculs.CalculatePoint(x, y), Quaternion.identity);

        int result = Calculs.EvaluateWin(Matrix);
        switch (result)
        {
            case 0:
                Debug.Log("Draw");
                canvas.gameObject.SetActive(true);
                WhoWins.color = Color.blue;
                WinLose.color = Color.blue;
                Draw.gameObject.SetActive(true);
                WhoWins.text = "Anybody wins!";
                WinLose.text = "Its a Draw!";
                break;
            case 1:
                Debug.Log("You Win");
                canvas.gameObject.SetActive(true);
                WhoWins.color = Color.white;
                WinLose.color = Color.white;
                Win.gameObject.SetActive(true);
                WhoWins.text = "Player wins!";
                WinLose.text = "Congratulations!";
                break;
            case -1:
                Debug.Log("You Lose");
                canvas.gameObject.SetActive(true);
                WhoWins.color = Color.black;
                WinLose.color = Color.black;
                Lose.gameObject.SetActive(true);
                WhoWins.text = "AI wins!";
                WinLose.text = "Try again!";
                break;
            case 2:
                if (state == States.CantMove)
                    state = States.CanMove;
                break;
        }
    }
}