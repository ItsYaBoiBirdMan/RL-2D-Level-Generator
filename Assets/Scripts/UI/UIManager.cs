using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    [SerializeField] private Image FadeImage;
    [SerializeField] private float FadeDuration;
    [SerializeField] private GameObject GameOverScreen;
    [SerializeField] private float GameOverScreenFadeDuration;

    [SerializeField] private Image HealthBar;
    [SerializeField] private TextMeshProUGUI HealthPercentage;
    [SerializeField] private Image SurgeBar;
    [SerializeField] private TextMeshProUGUI SurgePercentage;
    [SerializeField] private Color SurgeBarNormalColor;
    [SerializeField] private Color SurgeBarFullyChargedColor;
    [SerializeField] private Color SurgeBarSpeedChargedColor;
    [SerializeField] private Color SurgeBarPowerChargedColor;

    [SerializeField] private List<Color> ListOfShiftColors;

    [SerializeField] private TextMeshProUGUI EnemyCounter;
    
    [SerializeField] private TextMeshProUGUI GameOverText;
    [SerializeField] private Button RespawnButton;
    [SerializeField] private Button QuitButton;
    [SerializeField] private TextMeshProUGUI RespawnButtonText;
    [SerializeField] private TextMeshProUGUI QuitButtonText;
    
    [SerializeField] private PcgGameManager GameManager;

    private Coroutine _fadeRoutine;

    private float _startAlpha;
    private IEnumerator FadeRoutine(float targetAlpha)
    {
        _startAlpha = FadeImage.color.a;
        float time = 0f;

        while (time < FadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(_startAlpha, targetAlpha, time / FadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        
        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        Color c = FadeImage.color;
        c.a = alpha;
        FadeImage.color = c;
    }

    private void StartFade(float targetAlpha)
    {
        if(_fadeRoutine != null) StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    public void FadeToBlack()
    {
        StartFade(1f);
    }
    
    public void FadeFromBlack()
    {
        StartFade(0f);
    }

    private IEnumerator FadeInGameOverScreenRoutine()
    {
        GameOverScreen.SetActive(true);
        GameOverText.text = "Game Over";
        float time = 0f;

        while (time < GameOverScreenFadeDuration)
        {
            time += Time.deltaTime;
            float alphaText = Mathf.Lerp(GameOverText.alpha, 1f, time / GameOverScreenFadeDuration);
            GameOverText.alpha = alphaText;
            
            float alphaButtons = Mathf.Lerp(RespawnButton.image.color.a, 1f, time / GameOverScreenFadeDuration);
            float alphaButtonsText = Mathf.Lerp(RespawnButtonText.alpha, 1f, time / GameOverScreenFadeDuration);
            Color c = RespawnButton.image.color;
            c.a = alphaButtons;
            RespawnButton.image.color = c;
            QuitButton.image.color = c;
            RespawnButtonText.alpha = alphaButtonsText;
            QuitButtonText.alpha = alphaButtonsText;
            
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        RespawnButton.interactable = true;
        QuitButton.interactable = true;
    }

    private IEnumerator FadeOutGameOverScreen()
    {
        RespawnButton.interactable = false;
        QuitButton.interactable = false;
        
        float time = 0f;

        while (time < GameOverScreenFadeDuration)
        {
            time += Time.deltaTime;
            float alphaText = Mathf.Lerp(GameOverText.alpha, 0f, time / GameOverScreenFadeDuration);
            GameOverText.alpha = alphaText;
            
            float alphaButtons = Mathf.Lerp(RespawnButton.image.color.a, 0f, time / GameOverScreenFadeDuration);
            float alphaButtonsText = Mathf.Lerp(RespawnButtonText.alpha, 0f, time / GameOverScreenFadeDuration);
            Color c = RespawnButton.image.color;
            c.a = alphaButtons;
            RespawnButton.image.color = c;
            QuitButton.image.color = c;
            RespawnButtonText.alpha = alphaButtonsText;
            QuitButtonText.alpha = alphaButtonsText;
            
            yield return null;
        }

        GameOverScreen.SetActive(false);
    }
    
    private IEnumerator FadeInVictoryScreenRoutine()
    {
        FadeToBlack();
        yield return new WaitForSeconds(FadeDuration);
        GameOverScreen.SetActive(true);
        GameOverText.text = "Victory!";

        float time = 0f;

        while (time < GameOverScreenFadeDuration)
        {
            time += Time.deltaTime;
            float alphaText = Mathf.Lerp(GameOverText.alpha, 1f, time / GameOverScreenFadeDuration);
            GameOverText.alpha = alphaText;
            
            float alphaButtons = Mathf.Lerp(RespawnButton.image.color.a, 1f, time / GameOverScreenFadeDuration);
            float alphaButtonsText = Mathf.Lerp(RespawnButtonText.alpha, 1f, time / GameOverScreenFadeDuration);
            Color c = RespawnButton.image.color;
            c.a = alphaButtons;
            RespawnButton.image.color = c;
            QuitButton.image.color = c;
            RespawnButtonText.alpha = alphaButtonsText;
            QuitButtonText.alpha = alphaButtonsText;
            
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        RespawnButton.interactable = true;
        QuitButton.interactable = true;
    }

    private IEnumerator EmptySurgeBarRoutine(float d, Color barColor)
    {
        float startFill = SurgeBar.fillAmount;
        float time = 0f;
        SurgeBar.color = barColor;

        while (time < d)
        {
            time += Time.deltaTime;
            SurgeBar.fillAmount = Mathf.Lerp(startFill, 0f, time / d);
            SurgePercentage.text = Mathf.RoundToInt(SurgeBar.fillAmount * 100f) + "%";
            yield return null;
        }

        SurgeBar.fillAmount = 0f;
        SurgeBar.color = SurgeBarNormalColor;
    }

    public IEnumerator SurgeBarColorShiftRoutine(float d, bool s)
    {
        int currentIndex = 0;

        while (s)
        {
            Color startColor = ListOfShiftColors[currentIndex];
            Color endColor = ListOfShiftColors[(currentIndex + 1) % ListOfShiftColors.Count];

            float time = 0f;

            while (time < d)
            {
                time += Time.deltaTime;
                float t = time / d;

                SurgeBar.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            currentIndex = (currentIndex + 1) % ListOfShiftColors.Count;
        }
    }

    public void EmptySurgeBarWhenDrainActivated(float duration)
    {
        StartCoroutine(EmptySurgeBarRoutine(duration, SurgeBarFullyChargedColor));
    }
    
    public void EmptySurgeBarWhenSpeedActivated(float duration)
    {
        StartCoroutine(EmptySurgeBarRoutine(duration, SurgeBarSpeedChargedColor));
    }
    
    public void EmptySurgeBarWhenPowerActivated(float duration)
    {
        StartCoroutine(EmptySurgeBarRoutine(duration, SurgeBarPowerChargedColor));
    }

    public void RespawnButtonFunction()
    {
        StartCoroutine(FadeOutGameOverScreen());
       
        //Player.RespawnPlayerFromGameOver();
        GameManager.RestartGame();
        
    }

    public void FadeInGameOverScreen()
    {
        StartCoroutine(FadeInGameOverScreenRoutine());
    }

    public void FadeInVictoryScreen()
    {
        StartCoroutine(FadeInVictoryScreenRoutine());
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        HealthBar.fillAmount = currentHealth / maxHealth;
        HealthPercentage.text = Mathf.RoundToInt(HealthBar.fillAmount * 100f) + "%";
    }

    public void UpdateSurgeBar(float currentSurge, float maxSurge)
    {
        SurgeBar.fillAmount = currentSurge / maxSurge;
        if (currentSurge >= maxSurge) SurgeBar.color = SurgeBarFullyChargedColor;
        SurgePercentage.text = Mathf.RoundToInt(SurgeBar.fillAmount * 100f) + "%";
    }


    public void UpdateEnemyCounter(int count)
    {
        EnemyCounter.text = "Remaining Enemies: " + count;
    }
    
    public void UpdateCoinCounter(int count)
    {
        EnemyCounter.text = "Remaining Coins: " + count;
    }
}
