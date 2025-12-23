using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Test : MonoBehaviour
{
    [SerializeField] private TMP_Text hp;
    [SerializeField] private TMP_Text score;
    [SerializeField] private TMP_Text sweepCd;

    [SerializeField] private GameObject resultWindow;
    [SerializeField] private TMP_Text resultWindowScore;
    public static UIManager_Test Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SetHp(int hpValue)
    {
        hp.text = $"HP: {hpValue}/100";
    }

    public void SetScore(int scoreValue)
    {
        score.text = $"Score: {scoreValue}";
        resultWindowScore.text = $"Score: {scoreValue}";
    }

    public void SetSweepCd(float cd)
    {
        sweepCd.text = $"横扫CD: {cd}";
    }

    public void ShowResultWindow()
    {
        resultWindow.SetActive(true);
    }

    public void HideResultWindow()
    {
        resultWindow.SetActive(false);
    }

    public void Play()
    {
        GameRunner.Instance.StartGame();
    }
}