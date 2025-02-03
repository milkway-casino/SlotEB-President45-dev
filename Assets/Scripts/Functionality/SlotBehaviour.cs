using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    private List<SlotImage> Tempimages;     //class to store the result matrix

    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;
    [SerializeField]
    private GameObject SlotAnim_Object;

    [Header("Line Button Objects")]
    [SerializeField]
    private List<Image> StaticLine_Image;
    [SerializeField]
    private List<TMP_Text> StaticLine_Texts;
    [SerializeField]
    private Color TextEnabled;
    [SerializeField]
    private Color TextDisabled;
    [SerializeField]
    private Sprite Normal_Sprite;
    [SerializeField]
    private Sprite Highlighted_Sprite;

    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button AutoSpinStop_Button;
    [SerializeField]
    private Button BetPlus_Button;
    [SerializeField]
    private Button BetMinus_Button;
    [SerializeField]
    private Button Turbo_Button;
    [SerializeField]
    private Button StopSpin_Button;

    [Header("Animated Sprites")]
    [SerializeField]
    private Sprite[] FortyFive_Sprite;
    [SerializeField]
    private Sprite[] K_Sprite;
    [SerializeField]
    private Sprite[] Q_Sprite;
    [SerializeField]
    private Sprite[] J_Sprite;
    [SerializeField]
    private Sprite[] Harris_Sprite;
    [SerializeField]
    private Sprite[] USA_Sprite;
    [SerializeField]
    private Sprite[] Plane_Sprite;
    [SerializeField]
    private Sprite[] Newspaper_Sprite;
    [SerializeField]
    private Sprite[] Joker_Sprite;
    [SerializeField]
    private Sprite[] Wild_Sprite;
    [SerializeField]
    private Sprite[] Trump_Sprite;

    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text TotalWin_text;
    [SerializeField]
    private GameObject Goodluck_Object;
    [SerializeField]
    private GameObject Win_Object;

    [Header("Audio Management")]
    [SerializeField]
    private AudioController audioController;

    [SerializeField]
    private UIManager uiManager;

    [Header("Free Spins Board")]
    [SerializeField]
    private GameObject FSBoard_Object;
    [SerializeField]
    private TMP_Text FSnum_text;

    [SerializeField]
    int tweenHeight = 3182;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab

    [SerializeField]
    private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();

    private Tweener WinTween = null;

    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField]
    private SocketIOManager SocketManager;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null; 
    private Coroutine BoxAnimRoutine = null; 
    private Coroutine tweenroutine;

    private bool IsAutoSpin = false;
    private bool IsFreeSpin = false;
    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    internal bool CheckPopups = false;
    internal bool IsHoldSpin = false;
    private bool IsTurboOn;
    private bool StopSpinToggle;
    private bool WasAutoSpinOn;

    private int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    protected int Lines = 20;
    [SerializeField]
    private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing
    private int numberOfSlots = 5;
    private float SpinDelay = 0.2f;
    [SerializeField]
    Sprite[] TurboToggleSprites;//number of columns


    private void Start()
    {
        IsAutoSpin = false;

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (BetPlus_Button) BetPlus_Button.onClick.RemoveAllListeners();
        if (BetPlus_Button) BetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });
        if (BetMinus_Button) BetMinus_Button.onClick.RemoveAllListeners();
        if (BetMinus_Button) BetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        if (StopSpin_Button) StopSpin_Button.onClick.RemoveAllListeners();
        if (StopSpin_Button) StopSpin_Button.onClick.AddListener(() => { audioController.PlayButtonAudio(); StopSpinToggle = true; StopSpin_Button.gameObject.SetActive(false);});
        if (Turbo_Button) Turbo_Button.onClick.RemoveAllListeners();
        if (Turbo_Button) Turbo_Button.onClick.AddListener(TurboToggle);

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

        if (FSBoard_Object) FSBoard_Object.SetActive(false);
    }


    void TurboToggle()
    {
        audioController.PlayButtonAudio();
        if (IsTurboOn)
        {
            IsTurboOn = false;
            Turbo_Button.GetComponent<ImageAnimation>().StopAnimation();
            Turbo_Button.image.sprite = TurboToggleSprites[0];
            Turbo_Button.image.color = new Color(0.86f, 0.86f, 0.86f, 1);
        }
        else
        {
            IsTurboOn = true;
            Turbo_Button.GetComponent<ImageAnimation>().StartAnimation();
            Turbo_Button.image.color = new Color(1, 1, 1, 1);
        }
    }


    #region Autospin

    internal void StartSpinRoutine()
    {
        if (!IsSpinning)
        {
            IsHoldSpin = false;
            Invoke("AutoSpinHold", 2f);
        }
    }

    internal void StopSpinRoutine()
    {
        CancelInvoke("AutoSpinHold");
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            //if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private void StopAutoSpin()
    {
        Debug.Log("autoSpinStop");
        
        if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);       
        if (IsAutoSpin)
        {
            audioController.PlayButtonAudio();          
            StartCoroutine(StopAutoSpinCoroutine());
        }
        IsAutoSpin = false;
        WasAutoSpinOn = false;

    }

    private void AutoSpinHold()
    {
        Debug.Log("Auto Spin Started");
        IsHoldSpin = true;
        AutoSpin();
    }
    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {

            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());

        }
    }


    private IEnumerator AutoSpinCoroutine()
    {
        AutoSpinStop_Button.interactable = true;
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
        }
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }

    void callAutoSpinAgain()
    {
        Debug.Log("callAutoSpinAgain");
        if (AutoSpinStop_Button.gameObject.activeSelf)
        {
            AutoSpin();
        }
    }
    void stopautospin()
    {
        StartCoroutine(StopAutoSpinCoroutine());
    }

    #endregion

    #region FreeSpin
    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {
            if (FSnum_text) FSnum_text.text = (uiManager.localfreespin).ToString();
            if (FSBoard_Object) FSBoard_Object.SetActive(true);
            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));
        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        while (i < spinchances)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(SpinDelay);
            i++;
           
           
        }
        uiManager.localfreespin = 0;
        if (FSBoard_Object) FSBoard_Object.SetActive(false);

        IsFreeSpin = false;
        if (WasAutoSpinOn)
        {
            AutoSpin();
        }
        else
        {
            ToggleButtonGrp(true);
        }
    }
    #endregion

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
        }
    }

    #region LinesCalculation
    //Fetch Lines from backend
    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
    }

    //Generate Static Lines from button hovers
    internal void GenerateStaticLine(TMP_Text LineID_Text)
    {
        DestroyStaticLine();
        int LineID = 1;
        try
        {
            LineID = int.Parse(LineID_Text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Exception while parsing " + e.Message);
        }
        List<int> y_points = null;
        y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
        PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count);
    }

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }
    #endregion

    private void ChangeBet(bool IncDec)
    {
        if (!WasAutoSpinOn)
        {
            if (audioController) audioController.PlayButtonAudio();
            if (IncDec)
            {
                BetCounter++;
                if (BetCounter > SocketManager.initialData.Bets.Count - 1)
                {
                    BetCounter = 0;
                }
            }
            else
            {
                BetCounter--;
                if (BetCounter < 0)
                {
                    BetCounter = SocketManager.initialData.Bets.Count - 1;
                }
            }
            if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
            currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
            
        }
    }

    #region InitialFunctions
    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < images.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, 11);
                images[i].slotImages[j].sprite = myImages[randomIndex];
            }
        }
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.000";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f3");
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.paylines);
    }
    #endregion

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        Debug.Log(animScript);
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 0:
                for (int i = 0; i < FortyFive_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(FortyFive_Sprite[i]);
                }
                animScript.AnimationSpeed = 18f;
                break;
            case 1:
                for (int i = 0; i < K_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(K_Sprite[i]);
                }
                animScript.AnimationSpeed = 18f;
                break;
            case 2:
                for (int i = 0; i < Q_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Q_Sprite[i]);
                }
                animScript.AnimationSpeed = 19f;
                break;
            case 3:
                for (int i = 0; i < J_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(J_Sprite[i]);
                }
                animScript.AnimationSpeed = 20f;
                break;
            case 4:
                for (int i = 0; i < Harris_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Harris_Sprite[i]);
                }
                animScript.AnimationSpeed = 10f;
                break;
            case 5:
                for (int i = 0; i < USA_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(USA_Sprite[i]);
                }
                animScript.AnimationSpeed = 37f;
                break;
            case 6:
                for (int i = 0; i < Plane_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Plane_Sprite[i]);
                }
                animScript.AnimationSpeed = 26f;
                break;
            case 7:
                for (int i = 0; i < Newspaper_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Newspaper_Sprite[i]);
                }
                animScript.AnimationSpeed = 10f;
                break;
            case 8:
                for (int i = 0; i < Joker_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Joker_Sprite[i]);
                }
                animScript.AnimationSpeed = 10f;
                break;
            case 9:
                for (int i = 0; i < Wild_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Wild_Sprite[i]);
                }
                animScript.AnimationSpeed = 26f;
                break;
            case 10:
                for (int i = 0; i < Trump_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Trump_Sprite[i]);
                }
                animScript.AnimationSpeed = 28f;
                break;
        }
    }

    #region SlotSpin
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        audioController.PlayButtonAudio();
        if (audioController) audioController.PlaySpinButtonAudio();

        if (IsFreeSpin)
        {
            uiManager.localfreespin--;
            if (FSnum_text) FSnum_text.text = (uiManager.localfreespin).ToString();
        }

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }
        WinningsAnim(false);
        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0) 
        {
            Debug.Log("stop box routine");
            StopGameAnimation();
        }
        SocketManager.resultData = null;
        PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        if (currentBalance < currentTotalBet && !IsFreeSpin) 
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            ToggleButtonGrp(true);
            yield break;
        }
        if (audioController) audioController.PlayWLAudio("spin");
        CheckSpinAudio = true;

        IsSpinning = true;

        ToggleButtonGrp(false);

        Debug.Log("turboOn "+IsTurboOn+" freeSpin "+IsFreeSpin+" isautospin "+IsAutoSpin);
        if (!IsTurboOn && !IsFreeSpin && !IsAutoSpin)
        {
            StopSpin_Button.gameObject.SetActive(true);
        }

        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        if (!IsFreeSpin)
        {
            BalanceDeduction();
        }
        SocketManager.AccumulateResult(BetCounter);

        yield return new WaitUntil(() => SocketManager.isResultdone);
        if (IsAutoSpin)
        {
            WasAutoSpinOn = true;
        }

        for (int j = 0; j < SocketManager.resultData.resultSymbols.Count; j++)
        {
            List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
            for (int i = 0; i < 5; i++)
            {
                if (images[i].slotImages[j]) images[i].slotImages[j].sprite = myImages[resultnum[i]];
                if (Tempimages[i].slotImages[j]) Tempimages[i].slotImages[j].sprite = myImages[resultnum[i]];
                PopulateAnimationSprites(Tempimages[i].slotImages[j].gameObject.GetComponent<ImageAnimation>(), resultnum[i]);
            }
        }

        
        if (IsTurboOn || IsFreeSpin)
        {
            StopSpinToggle = true;
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.1f);
                if (StopSpinToggle)
                {
                    break;
                }
            }
            StopSpin_Button.gameObject.SetActive(false);
        }

        Debug.Log(StopSpinToggle);

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i, StopSpinToggle);
        }
        StopSpinToggle = false;
        yield return alltweens[^1].WaitForCompletion();
        KillAllTweens();
        if (SocketManager.playerdata.currentWining > 0)
        {
            SpinDelay = 1.2f;
        }
        else
        {
            SpinDelay = 0.5f;
        }

        yield return new WaitForSeconds(0.3f);
        CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.freeSpins.trumpSymbols, SocketManager.resultData.freeSpins.jokerSymbols);

        CheckPopups = true;

        if (TotalWin_text) TotalWin_text.text = "<size=35>win</size>\n" + SocketManager.playerdata.currentWining.ToString("f3");

        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f3");

        currentBalance = SocketManager.playerdata.Balance;

        CheckWinPopups();

        yield return new WaitUntil(() => !CheckPopups);

        if (SocketManager.resultData.freeSpins.isNewAdded)
        {
            if (IsFreeSpin)
            {

                IsFreeSpin = false;
                if (FreeSpinRoutine != null)
                {
                    StopCoroutine(FreeSpinRoutine);
                    FreeSpinRoutine = null;
                }
            }
            yield return new WaitForSeconds(6);
            
            uiManager.FreeSpinProcess((int)SocketManager.resultData.freeSpins.count);
            if (IsAutoSpin)
            {
                WasAutoSpinOn = true;
                StopAutoSpin();
                yield return new WaitForSeconds(0.1f);
            }
        }
        if (!IsAutoSpin && !IsFreeSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
           // yield return new WaitForSeconds(1);
            IsSpinning = false;
        }
    }

    internal void CheckWinPopups()
    {
        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("f3");
        if (SocketManager.playerdata.currentWining >= currentTotalBet * 5 && SocketManager.playerdata.currentWining < currentTotalBet * 10)
        {
            uiManager.PopulateWin(1);
            if (audioController) audioController.PlayWLAudio("megaWin");
        }
        else if(SocketManager.playerdata.currentWining >= currentTotalBet * 10)
        {
            uiManager.PopulateWin(2);
            if (audioController) audioController.PlayWLAudio("megaWin");
        }
        else
        {
            CheckPopups = false;
        }
    }

    private void BalanceDeduction()
    {
        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;

        balance = balance - bet;

        DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f3");
        });
    }

    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, List<List<string>> trumpSymbols, List<List<string>> jokerSymbols)
    {
        List<int> y_points = null;
        List<int> points_anim = null;
        if (LineId.Count > 0 || points_AnimString.Count > 0)
        {
            if (audioController) audioController.PlayWLAudio("win");

            for (int i = 0; i < LineId.Count; i++)
            {
                y_points = y_string[LineId[i]]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count);
            }
            for (int i = 0; i < points_AnimString.Count; i++)
            {
                points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                for (int k = 0; k < points_anim.Count; k++)
                {
                    if (points_anim[k] >= 10)
                    {
                        StartGameAnimation(Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject);
                    }
                    else
                    {
                        StartGameAnimation(Tempimages[0].slotImages[points_anim[k]].gameObject);
                    }
                }
            }
            WinningsAnim(true);
        }
        else
        {
            //if (audioController) audioController.PlayWLAudio("lose");
            if (audioController) audioController.StopWLAaudio();
        }

        if (trumpSymbols.Count > 0)
        {
            for (int i = 0; i < trumpSymbols.Count; i++)
            {
                for (int j = 0; j < trumpSymbols[i].Count; j++)
                {
                    points_anim = trumpSymbols[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                    int k = 0;
                    while (k < points_anim.Count)
                    {
                        StartGameAnimation(Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject);
                        k += 2;
                    }
                }
            }
        }

        if (jokerSymbols.Count > 0)
        {
            for (int i = 0; i < jokerSymbols.Count; i++)
            {
                for (int j = 0; j < jokerSymbols[i].Count; j++)
                {
                    points_anim = jokerSymbols[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                    int k = 0;
                    while (k < points_anim.Count)
                    {
                        StartGameAnimation(Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject);
                        k += 2;
                    }
                }
            }
        }

        if (LineId.Count > 0 || SocketManager.resultData.freeSpins.trumpSymbols.Count > 0 || SocketManager.resultData.freeSpins.jokerSymbols.Count > 0)
        {
            if (IsAutoSpin)
            {
                WasAutoSpinOn = true;
                IsAutoSpin = false;
                StopCoroutine(AutoSpinCoroutine());
                Debug.Log("callBoxRoutine");
            }
            if (SocketManager.resultData.freeSpins.count > 0)
            {
                AutoSpinStop_Button.interactable = false;
            }

            BoxAnimRoutine = StartCoroutine(BoxRoutine(LineId, SocketManager.resultData.symbolsToEmit, SocketManager.resultData.freeSpins.trumpSymbols, SocketManager.resultData.freeSpins.jokerSymbols));
        }
        CheckSpinAudio = false;
    }

    private IEnumerator BoxRoutine(List<int> LineIDs, List<List<string>> points_AnimString, List<List<string>> trumpSymbols, List<List<string>> jokerSymbols)
    {
        if (SlotAnim_Object) SlotAnim_Object.SetActive(true);
        PayCalculator.DontDestroyLines.Clear();
        PayCalculator.DontDestroyLines.TrimExcess();
        PayCalculator.ResetLines();
        List<int> points_anim = null;
        int localCount = 0;
        while (true)
        {
            Debug.Log(WasAutoSpinOn);
            if (SocketManager.resultData.freeSpins.count == 0)
            {
                if (WasAutoSpinOn)
                {
                    if (LineIDs.Count > 1)
                    {
                        if (localCount > 0)
                        {
                            Debug.Log("routine runningin");
                            AutoSpin();
                            break;
                        }
                        localCount++;
                    }
                    else
                    {
                        Invoke("callAutoSpinAgain", 3f);
                    }
                }
            }

            Debug.Log("routine running");
            List<int> y_points = null;
            if (LineIDs.Count > 0 || trumpSymbols.Count > 0 || jokerSymbols.Count > 0)
            {
                if (trumpSymbols.Count > 0)
                {
                    for (int i = 0; i < trumpSymbols.Count; i++)
                    {
                        for (int j = 0; j < trumpSymbols[i].Count; j++)
                        {
                            points_anim = trumpSymbols[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                            int k = 0;
                            while (k < points_anim.Count)
                            {
                                Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(true);
                                k += 2;
                            }
                        }
                    }
                    yield return new WaitForSeconds(2f);

                    for (int i = 0; i < trumpSymbols.Count; i++)
                    {
                        for (int j = 0; j < trumpSymbols[i].Count; j++)
                        {
                            points_anim = trumpSymbols[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                            int k = 0;
                            while (k < points_anim.Count)
                            {
                                Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(false);
                                k += 2;
                            }
                        }
                    }
                }
                if (jokerSymbols.Count > 0)
                {
                    for (int i = 0; i < jokerSymbols.Count; i++)
                    {
                        for (int j = 0; j < jokerSymbols[i].Count; j++)
                        {
                            points_anim = jokerSymbols[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                            int k = 0;
                            while (k < points_anim.Count)
                            {
                                Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(true);
                                k += 2;
                            }
                        }
                    }
                    yield return new WaitForSeconds(2f);

                    for (int i = 0; i < jokerSymbols.Count; i++)
                    {
                        for (int j = 0; j < jokerSymbols[i].Count; j++)
                        {
                            points_anim = jokerSymbols[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                            int k = 0;
                            while (k < points_anim.Count)
                            {
                                Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(false);
                                k += 2;
                            }
                        }
                    }
                }
                for (int i = 0; i < points_AnimString.Count; i++)
                {
                    y_points = y_string[LineIDs[i]]?.Split(',')?.Select(Int32.Parse)?.ToList();
                    PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, LineIDs[i] % 10);
                    PayCalculator.DontDestroyLines.Add(LineIDs[i]);
                    for (int j = 0; j < points_AnimString[i].Count; j++)
                    {
                        points_anim = points_AnimString[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                        int k = 0;
                        while (k < points_anim.Count)
                        {
                            Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(true);
                            k += 2;
                        }
                    }
                    if (StaticLine_Image[LineIDs[i] - 1]) StaticLine_Image[LineIDs[i] - 1].sprite = Highlighted_Sprite;
                    if (StaticLine_Texts[LineIDs[i] - 1]) StaticLine_Texts[LineIDs[i] - 1].color = TextEnabled;
                    if (LineIDs.Count < 2 && trumpSymbols.Count <= 0 && jokerSymbols.Count <= 0) 
                    {
                        yield break;
                    }
                    yield return new WaitForSeconds(2f);
                    if (StaticLine_Image[LineIDs[i] - 1]) StaticLine_Image[LineIDs[i] - 1].sprite = Normal_Sprite;
                    if (StaticLine_Texts[LineIDs[i] - 1]) StaticLine_Texts[LineIDs[i] - 1].color = TextDisabled;
                    for (int j = 0; j < points_AnimString[i].Count; j++)
                    {
                        points_anim = points_AnimString[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                        int k = 0;
                        while (k < points_anim.Count)
                        {
                            Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(false);
                            k += 2;
                        }
                    }
                    PayCalculator.DontDestroyLines.Clear();
                    PayCalculator.DontDestroyLines.TrimExcess();
                    PayCalculator.ResetLines();
                }
            }

            for (int i = 0; i < LineIDs.Count; i++)
            {
                y_points = y_string[LineIDs[i]]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, LineIDs[i] % 10);
                PayCalculator.DontDestroyLines.Add(LineIDs[i]);
                if (StaticLine_Image[LineIDs[i] - 1]) StaticLine_Image[LineIDs[i] - 1].sprite = Highlighted_Sprite;
                if (StaticLine_Texts[LineIDs[i] - 1]) StaticLine_Texts[LineIDs[i] - 1].color = TextEnabled;
            }
            for (int i = 0; i < points_AnimString.Count; i++)
            {
                y_points = y_string[LineIDs[i]]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, LineIDs[i] % 10);
                PayCalculator.DontDestroyLines.Add(LineIDs[i]);
                for (int j = 0; j < points_AnimString[i].Count; j++)
                {
                    points_anim = points_AnimString[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                    int k = 0;
                    while (k < points_anim.Count)
                    {
                        Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(true);
                        k += 2;
                    }
                }
            }
            yield return new WaitForSeconds(2f);
            for (int i = 0; i < points_AnimString.Count; i++)
            {
                for (int j = 0; j < points_AnimString[i].Count; j++)
                {
                    points_anim = points_AnimString[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                    int k = 0;
                    while (k < points_anim.Count)
                    {
                        Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(false);
                        k += 2;
                    }
                }
            }
            for (int i = 0; i < LineIDs.Count; i++)
            {
                if (StaticLine_Image[LineIDs[i] - 1]) StaticLine_Image[LineIDs[i] - 1].sprite = Normal_Sprite;
                if (StaticLine_Texts[LineIDs[i] - 1]) StaticLine_Texts[LineIDs[i] - 1].color = TextDisabled;
            }
            PayCalculator.DontDestroyLines.Clear();
            PayCalculator.DontDestroyLines.TrimExcess();
            PayCalculator.ResetLines();
            CheckPopups = false;
        }
    }

    private void WinningsAnim(bool IsStart)
    {
        if (IsStart)
        {
            if (Goodluck_Object) Goodluck_Object.SetActive(false);
            if (Win_Object) Win_Object.SetActive(true);
            WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.2f, 1.2f), 0.3f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
            if (Goodluck_Object) Goodluck_Object.SetActive(true);
            if (Win_Object) Win_Object.SetActive(false);
        }
    }

    #endregion

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }


    void ToggleButtonGrp(bool toggle)
    {
        Debug.Log("toggleGroupCalled");

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (!WasAutoSpinOn)
        {
            if (BetMinus_Button) BetMinus_Button.interactable = toggle;
            if (BetPlus_Button) BetPlus_Button.interactable = toggle;
        }
        

    }

    //start the icons animation
    private void StartGameAnimation(GameObject animObjects)
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        temp.StartAnimation();
        TempList.Add(temp);
    }

    //stop the icons animation
    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
        TempList.Clear();
        TempList.TrimExcess();
        if (BoxAnimRoutine != null)
        {
            StopCoroutine(BoxAnimRoutine);
            BoxAnimRoutine = null;
        }
        if (SlotAnim_Object) SlotAnim_Object.SetActive(false);
        for (int i = 0; i < Tempimages.Count; i++)
        {
            foreach(Image s in Tempimages[i].slotImages)
            {
                s.gameObject.SetActive(false);
            }
        }

        foreach(TMP_Text t in StaticLine_Texts)
        {
            t.color = TextDisabled;
        }

        foreach(Image i in StaticLine_Image)
        {
            i.sprite = Normal_Sprite;
        }
    }


    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }



    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index,bool isStop)
    {
        alltweens[index].Pause();
        int tweenpos = (reqpos * IconSizeFactor) - IconSizeFactor;
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100, 0.5f).SetEase(Ease.OutElastic);
        if (!isStop)
        {
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            yield return null;
        }
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}

