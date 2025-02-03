using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour
{

    [Header("Settings UI")]
    [SerializeField]
    private Button Settings_Button;

    [Header("Popus UI")]
    [SerializeField]
    private GameObject MainPopup_Object;

    [Header("Paytable Popup")]
    [SerializeField]
    private GameObject PaytablePopup_Object;
    [SerializeField]
    private Button PaytableExit_Button;
    [SerializeField]
    private Button PaytableEntry_Button;
    [SerializeField]
    private TMP_Text[] SymbolsText;
    [SerializeField]
    private TMP_Text FreeSpin_Text;
    [SerializeField]
    private TMP_Text Trump_Text;
    [SerializeField]
    private TMP_Text Wild_Text;
    [SerializeField]
    private Button Right_Button;
    [SerializeField]
    private Button Left_Button;
    [SerializeField]
    private GameObject[] Info_Screens;
    int screenCounter = 0;

    [Header("Settings Popup")]
    [SerializeField]
    private GameObject SettingsPopup_Object;
    [SerializeField]
    private Button SettingsExit_Button;
    [SerializeField]
    private Button Sound_Button;
    [SerializeField]
    private Button Music_Button;

    [Header("Win Popups")]
    [SerializeField]
    private GameObject BigWin_Object;
    [SerializeField]
    private GameObject Megawin_Object;
    [SerializeField]
    private GameObject WinPopupMain_Object;

    [SerializeField]
    private GameObject MusicOn_Object;
    [SerializeField]
    private GameObject MusicOff_Object;
    [SerializeField]
    private GameObject SoundOn_Object;
    [SerializeField]
    private GameObject SoundOff_Object;

    [Header("FreeSpins Popup")]
    [SerializeField]
    private GameObject FreeSpinPopup_Object;
    [SerializeField]
    private TMP_Text Free_Text;
    [SerializeField]
    private Button FreeSpin_Button;

    [Header("Splash Screen")]
    [SerializeField]
    private GameObject Loading_Object;
    [SerializeField]
    private Image Loading_Image;
    [SerializeField]
    private TMP_Text Loading_Text;
    [SerializeField]
    private TMP_Text LoadPercent_Text;
    [SerializeField]
    private Button QuitSplash_button;

    [Header("Disconnection Popup")]
    [SerializeField]
    private Button CloseDisconnect_Button;
    [SerializeField]
    private GameObject DisconnectPopup_Object;

    [Header("AnotherDevice Popup")]
    [SerializeField]
    private Button CloseAD_Button;
    [SerializeField]
    private GameObject ADPopup_Object;

    [Header("LowBalance Popup")]
    [SerializeField]
    private Button LBExit_Button;
    [SerializeField]
    private GameObject LBPopup_Object;

    [Header("Quit Popup")]
    [SerializeField]
    private GameObject QuitPopup_Object;
    [SerializeField]
    private Button YesQuit_Button;
    [SerializeField]
    private Button NoQuit_Button;
    [SerializeField]
    private Button CrossQuit_Button;
    [SerializeField]
    private Button megaWin_Disable;


    [SerializeField]
    private AudioController audioController;
    [SerializeField]
    private Button m_AwakeGameButton;

    [SerializeField]
    private Button GameExit_Button;

    [SerializeField]
    private SlotBehaviour slotManager;

    [SerializeField]
    private SocketIOManager socketManager;

    private bool isMusic = true;
    private bool isSound = true;
    private bool isExit = false;

    private int FreeSpins;
    internal int localfreespin;
    private Tween megawin_Tween;


    private void Awake()
    {
        //if (Loading_Object) Loading_Object.SetActive(true);
        //StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        StartCoroutine(LoadingTextAnimate());
        float imageFill = 0f;
        DOTween.To(() => imageFill, (val) => imageFill = val, 0.7f, 2f).OnUpdate(() =>
        {
            if (Loading_Image) Loading_Image.fillAmount = imageFill;
            if (LoadPercent_Text) LoadPercent_Text.text = (100 * imageFill).ToString("f0") + "%";
        });
        yield return new WaitForSecondsRealtime(2);
        yield return new WaitUntil(() => socketManager.isLoaded);
        DOTween.To(() => imageFill, (val) => imageFill = val, 1, 1f).OnUpdate(() =>
        {
            if (Loading_Image) Loading_Image.fillAmount = imageFill;
            if (LoadPercent_Text) LoadPercent_Text.text = (100 * imageFill).ToString("f0") + "%";
        });
        yield return new WaitForSecondsRealtime(1f);
        if (Loading_Object) Loading_Object.SetActive(false);
        StopCoroutine(LoadingTextAnimate());
    }

    private IEnumerator LoadingTextAnimate()
    {
        while (true)
        {
            if (Loading_Text) Loading_Text.text = "Loading.";
            yield return new WaitForSeconds(1f);
            if (Loading_Text) Loading_Text.text = "Loading..";
            yield return new WaitForSeconds(1f);
            if (Loading_Text) Loading_Text.text = "Loading...";
            yield return new WaitForSeconds(1f);
        }
    }

    private void Start()
    {

        if (PaytableEntry_Button) PaytableEntry_Button.onClick.RemoveAllListeners();
        if (PaytableEntry_Button) PaytableEntry_Button.onClick.AddListener(delegate { screenCounter = 1; ChangePage(false); OpenPopup(PaytablePopup_Object); });

        if (Right_Button) Right_Button.onClick.RemoveAllListeners();
        if (Right_Button) Right_Button.onClick.AddListener(delegate { ChangePage(true); });

        if (Left_Button) Left_Button.onClick.RemoveAllListeners();
        if (Left_Button) Left_Button.onClick.AddListener(delegate { ChangePage(false); });

        if (megaWin_Disable) megaWin_Disable.onClick.RemoveAllListeners();
        if (megaWin_Disable) megaWin_Disable.onClick.AddListener(disableMegaWinOnPress);

        if (PaytableExit_Button) PaytableExit_Button.onClick.RemoveAllListeners();
        if (PaytableExit_Button) PaytableExit_Button.onClick.AddListener(delegate { ClosePopup(PaytablePopup_Object); });

        if (Settings_Button) Settings_Button.onClick.RemoveAllListeners();
        if (Settings_Button) Settings_Button.onClick.AddListener(delegate { OpenPopup(SettingsPopup_Object); });

        if (SettingsExit_Button) SettingsExit_Button.onClick.RemoveAllListeners();
        if (SettingsExit_Button) SettingsExit_Button.onClick.AddListener(delegate { ClosePopup(SettingsPopup_Object); });

        if (MusicOn_Object) MusicOn_Object.SetActive(true);
        if (MusicOff_Object) MusicOff_Object.SetActive(false);

        if (SoundOn_Object) SoundOn_Object.SetActive(true);
        if (SoundOff_Object) SoundOff_Object.SetActive(false);

        if (GameExit_Button) GameExit_Button.onClick.RemoveAllListeners();
        if (GameExit_Button) GameExit_Button.onClick.AddListener(delegate { OpenPopup(QuitPopup_Object); });

        if (NoQuit_Button) NoQuit_Button.onClick.RemoveAllListeners();
        if (NoQuit_Button) NoQuit_Button.onClick.AddListener(delegate { if (!isExit) { ClosePopup(QuitPopup_Object); } });

        if (CrossQuit_Button) CrossQuit_Button.onClick.RemoveAllListeners();
        if (CrossQuit_Button) CrossQuit_Button.onClick.AddListener(delegate { if (!isExit) { ClosePopup(QuitPopup_Object); } });

        if (LBExit_Button) LBExit_Button.onClick.RemoveAllListeners();
        if (LBExit_Button) LBExit_Button.onClick.AddListener(delegate { ClosePopup(LBPopup_Object); });

        if (YesQuit_Button) YesQuit_Button.onClick.RemoveAllListeners();
        if (YesQuit_Button) YesQuit_Button.onClick.AddListener(CallOnExitFunction);

        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.RemoveAllListeners();
        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.AddListener(CallOnExitFunction);

        if (CloseAD_Button) CloseAD_Button.onClick.RemoveAllListeners();
        if (CloseAD_Button) CloseAD_Button.onClick.AddListener(CallOnExitFunction);

        if (FreeSpin_Button) FreeSpin_Button.onClick.RemoveAllListeners();
        if (FreeSpin_Button) FreeSpin_Button.onClick.AddListener(delegate{ StartFreeSpins(FreeSpins); });

        if (QuitSplash_button) QuitSplash_button.onClick.RemoveAllListeners();
        if (QuitSplash_button) QuitSplash_button.onClick.AddListener(delegate { OpenPopup(QuitPopup_Object); });

        if (audioController) audioController.ToggleMute(false);

        isMusic = true;
        isSound = true;

        if (Sound_Button) Sound_Button.onClick.RemoveAllListeners();
        if (Sound_Button) Sound_Button.onClick.AddListener(ToggleSound);

        if (Music_Button) Music_Button.onClick.RemoveAllListeners();
        if (Music_Button) Music_Button.onClick.AddListener(ToggleMusic);

    }

    private void ChangePage(bool Increment)
    {
        if (audioController) audioController.PlayButtonAudio();
        foreach (GameObject t in Info_Screens)
        {
            t.SetActive(false);
        }

        if (Increment)
        {
            if (screenCounter == Info_Screens.Length - 1)
            {
                screenCounter = 0;
            }
            else
            {
                screenCounter++;
            }
        }
        else
        {
            if (screenCounter == 0)
            {
                screenCounter = Info_Screens.Length - 1;
            }
            else
            {
                screenCounter--;
            }
        }
        Info_Screens[screenCounter].SetActive(true);
    }

    internal void LowBalPopup()
    {
        OpenPopup(LBPopup_Object);
    }

    internal void DisconnectionPopup(bool isReconnection)
    {
        if (!isExit)
        {
            OpenPopup(DisconnectPopup_Object);
        }
    }

    private void StartFreeSpins(int spins)
    {
        if (MainPopup_Object) MainPopup_Object.SetActive(false);
        if (FreeSpinPopup_Object) FreeSpinPopup_Object.SetActive(false);
        slotManager.FreeSpin(spins);
    }

    internal void FreeSpinProcess(int spins)
    {
        Debug.Log(localfreespin);
        int ExtraSpins = Mathf.Abs(localfreespin - spins);
        FreeSpins = spins;
        localfreespin = spins;
        Debug.Log("ExtraSpins: " + localfreespin);
        Debug.Log("Total Spins: " + spins);
        if (FreeSpinPopup_Object) FreeSpinPopup_Object.SetActive(true);
        if (ExtraSpins == 1)
        {
            if (Free_Text) Free_Text.text = ExtraSpins.ToString() + " Free spin awarded.";
        }
        else
        {
            if (Free_Text) Free_Text.text = ExtraSpins.ToString() + " Free spins awarded.";
        }
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        DOVirtual.DelayedCall(2f, () => {
           
            StartFreeSpins(spins);
        });
    }

    internal void ADfunction()
    {
        OpenPopup(ADPopup_Object); 
    }

    internal void InitialiseUIData(Paylines symbolsText)
    {
        PopulateSymbolsPayout(symbolsText);
    }

    internal void PopulateWin(int value)
    {
        if (WinPopupMain_Object) WinPopupMain_Object.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        switch (value)
        {
            case 1:
                if (BigWin_Object) BigWin_Object.SetActive(true);
                megawin_Tween = DOVirtual.DelayedCall(4f, () =>
                {
                    if (BigWin_Object) BigWin_Object.SetActive(false);
                    if (WinPopupMain_Object) WinPopupMain_Object.SetActive(false);
                    if (MainPopup_Object) MainPopup_Object.SetActive(false);
                    slotManager.CheckPopups = false;
                });
                break;
            case 2:
                if (Megawin_Object) Megawin_Object.SetActive(true);
                megawin_Tween = DOVirtual.DelayedCall(4f, () =>
                {
                    if (Megawin_Object) Megawin_Object.SetActive(false);
                    if (WinPopupMain_Object) WinPopupMain_Object.SetActive(false);
                    if (MainPopup_Object) MainPopup_Object.SetActive(false);
                    slotManager.CheckPopups = false;
                });
                break;
        }
    }

    private void disableMegaWinOnPress()
    {
        Debug.Log("PressedDisable");
        megawin_Tween?.Kill();
        if (BigWin_Object) BigWin_Object.SetActive(false);
        if (WinPopupMain_Object) WinPopupMain_Object.SetActive(false);
        if (MainPopup_Object) MainPopup_Object.SetActive(false);
        if (Megawin_Object) Megawin_Object.SetActive(false);
        slotManager.CheckPopups = false;
    }

    private void PopulateSymbolsPayout(Paylines paylines)
    {
        for (int i = 0; i < SymbolsText.Length; i++)
        {
            string text = null;
            if (paylines.symbols[i].Multiplier[0][0] != 0)
            {
                text += "5x - " + paylines.symbols[i].Multiplier[0][0] + "x";
            }
            if (paylines.symbols[i].Multiplier[1][0] != 0)
            {
                text += "\n4x - " + paylines.symbols[i].Multiplier[1][0] + "x";
            }
            if (paylines.symbols[i].Multiplier[2][0] != 0)
            {
                text += "\n3x - " + paylines.symbols[i].Multiplier[2][0] + "x";
            }
            if (SymbolsText[i]) SymbolsText[i].text = text;
        }

        for (int i = 0; i < paylines.symbols.Count; i++)
        {
            if (paylines.symbols[i].Name.ToUpper() == "FREESPIN")
            {
                if (FreeSpin_Text) FreeSpin_Text.text = paylines.symbols[i].description.ToString();
            }
            if (paylines.symbols[i].Name.ToUpper() == "TRUMPFREESPIN")
            {
                if (Trump_Text) Trump_Text.text = paylines.symbols[i].description.ToString();
            }
            if (paylines.symbols[i].Name.ToUpper() == "WILD")
            {
                if (Wild_Text) Wild_Text.text = paylines.symbols[i].description.ToString();
            }
        }
    }

    private void CallOnExitFunction()
    {
        isExit = true;
        audioController.PlayButtonAudio();
        slotManager.CallCloseSocket();
    }

    private void OpenPopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
    }

    private void ClosePopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(false);
        if (!DisconnectPopup_Object.activeSelf) 
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }
    }

    private void ToggleMusic()
    {
        if (audioController) audioController.PlayButtonAudio();
        isMusic = !isMusic;
        if (isMusic)
        {
            if (MusicOn_Object) MusicOn_Object.SetActive(true);
            if (MusicOff_Object) MusicOff_Object.SetActive(false);
            audioController.ToggleMute(false, "bg");
        }
        else
        {
            if (MusicOn_Object) MusicOn_Object.SetActive(false);
            if (MusicOff_Object) MusicOff_Object.SetActive(true);
            audioController.ToggleMute(true, "bg");
        }
    }

    private void ToggleSound()
    {
        Debug.Log("playingSound");
        if (audioController) audioController.PlayButtonAudio();
        isSound = !isSound;
        if (isSound)
        {
            if (SoundOn_Object) SoundOn_Object.SetActive(true);
            if (SoundOff_Object) SoundOff_Object.SetActive(false);
            if (audioController) audioController.ToggleMute(false,"button");
            if (audioController) audioController.ToggleMute(false,"wl");
        }
        else
        {
            if (SoundOn_Object) SoundOn_Object.SetActive(false);
            if (SoundOff_Object) SoundOff_Object.SetActive(true);
            if(audioController) audioController.ToggleMute(true,"button");
            if (audioController) audioController.ToggleMute(true,"wl");
        }
    }
}
