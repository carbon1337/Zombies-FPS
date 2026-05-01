using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public delegate void RoundChanged();
    public event RoundChanged OnRoundChanged;

    public int currentMoney;
    public int startMoney = 500;
    public float startTimer = 10f;

    private TMP_Text moneyText;
    private TMP_Text roundText;

    private Animator roundAnimator;
    
    public static GameManager Instance { get; private set; }

    private int _currentRound = 0;
    public int currentRound
    {
        get { return _currentRound; }
        set
        {
            _currentRound = value;
            OnRoundChanged?.Invoke(); // Trigger the event when round changes
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        currentMoney = startMoney;
        moneyText = GameObject.Find("MoneyText").GetComponent<TMP_Text>();
        roundText = GameObject.Find("RoundText").GetComponent<TMP_Text>();
        roundAnimator = roundText.GetComponent<Animator>();
    }

    private void Start()
    {
        moneyText.text = "$" + currentMoney.ToString();

        StartCoroutine(InitializeGame());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(ChangeRound());
        }
    }

    public void SpendMoney(int moneySpent)
    {
        currentMoney -= moneySpent;
        moneyText.text = "$" + currentMoney.ToString();
    }

    public IEnumerator InitializeGame()
    {
        moneyText.text = "$" + currentMoney.ToString();
        yield return new WaitForSeconds(startTimer);
        StartCoroutine(ChangeRound());
    }

    public IEnumerator ChangeRound()
    {
        currentRound++;
        roundAnimator.SetTrigger("ChangeRound");
        yield return new WaitForSeconds(1f);
        roundText.text = currentRound.ToString();
    }
}