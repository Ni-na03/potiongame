using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PotionManager : MonoBehaviour
{
    public TMP_Text inventoryText;
    public TMP_Text customerRequestText;
    public TMP_Text timerText;
    public TMP_Text endMessageText;
    public GameObject restartButton;

    private int redPotions = 0;
    private int bluePotions = 0;
    private int greenPotions = 0;

    private int potionsSold = 0;

    private bool brewedRed = false;
    private bool brewedBlue = false;
    private bool brewedGreen = false;

    public Toggle rosePetalsToggle;
    public Toggle mouseTailToggle;
    public Toggle leavesToggle;
    public Toggle stinkBugsToggle;
    public Toggle blueBerriesToggle;
    public Toggle puddleWaterToggle;

    public TMP_Text warningText;

    private enum PotionType { Red, Blue, Green }
    private PotionType currentCustomerRequest;

    private float dayDuration = 60f;
    private float timeRemaining;

    private bool dayActive = true;

    void Start()
    {
        timeRemaining = dayDuration;
        warningText.gameObject.SetActive(false);
        UpdateInventoryText();
        GenerateNewCustomerRequest();
        UpdateTimerText();
    }

    void Update()
    {
        if (!dayActive)
            return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            dayActive = false;
            CheckThreeRules();
            EndDay();
        }
        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        int seconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = "Time left until your day ends: " + seconds.ToString();
    }

    void EndDay()
    {
        customerRequestText.text = "Day Over!";
        Debug.Log("Day ended!");
    }

    bool CheckIngredients(PotionType potion)
    {
        warningText.gameObject.SetActive(false);

        switch (potion)
        {
            case PotionType.Red:
                if (rosePetalsToggle.isOn && mouseTailToggle.isOn &&
                !leavesToggle.isOn && !stinkBugsToggle.isOn &&
                !blueBerriesToggle.isOn && !puddleWaterToggle.isOn) return true;
                break;
            case PotionType.Green:
                if (leavesToggle.isOn && stinkBugsToggle.isOn &&
                !rosePetalsToggle.isOn && !mouseTailToggle.isOn &&
                !blueBerriesToggle.isOn && !puddleWaterToggle.isOn) return true;
                break;
            case PotionType.Blue:
                if (blueBerriesToggle.isOn && puddleWaterToggle.isOn &&
                !rosePetalsToggle.isOn && !mouseTailToggle.isOn &&
                !leavesToggle.isOn && !stinkBugsToggle.isOn) return true;
                break;
        }

        warningText.text = "Missing or wrong ingredients for " + potion.ToString() + " potion!";
        warningText.gameObject.SetActive(true);
        return false;
    }

    public void ShowEndMessage(string message)
    {
        endMessageText.text = message;
        endMessageText.gameObject.SetActive(true);
        restartButton.SetActive(true);
    }

    public void BrewRed()
    {
        if (!dayActive) return;
        if (!CheckIngredients(PotionType.Red)) return;
        redPotions++;
        brewedRed = true;
        UpdateInventoryText();
        
        rosePetalsToggle.isOn = false;
        mouseTailToggle.isOn = false;
    }

    public void BrewBlue()
    {
        if (!dayActive) return;
        if (!CheckIngredients(PotionType.Blue)) return;
        bluePotions++;
        brewedBlue = true;
        UpdateInventoryText();
        blueBerriesToggle.isOn = false;
        puddleWaterToggle.isOn = false;
    }

    public void BrewGreen()
    {
        if (!dayActive) return;
        if (!CheckIngredients(PotionType.Green)) return;
        greenPotions++;
        brewedGreen = true;
        UpdateInventoryText();

        leavesToggle.isOn = false;
        stinkBugsToggle.isOn = false;
    }

    public void SellRed()
    {
        if (!dayActive) return;
        SellPotion(PotionType.Red);
    }

    public void SellBlue()
    {
        if (!dayActive) return;
        SellPotion(PotionType.Blue);
    }

    public void SellGreen()
    {
        if (!dayActive) return;
        SellPotion(PotionType.Green);
    }

    private void SellPotion(PotionType potion)
    {
        if (!dayActive) return;

        if (potion != currentCustomerRequest)
        {
            customerRequestText.text = "Trying to sell the wrong one? This customer requested a " + currentCustomerRequest.ToString() + " potion!";
            return;
        }

        bool hasPotion = false;
        switch (potion)
        {
            case PotionType.Red:
                hasPotion = (redPotions > 0);
                break;
            case PotionType.Blue:
                hasPotion = (bluePotions > 0);
                break;
            case PotionType.Green:
                hasPotion = (greenPotions > 0);
                break;
        }

        if (!hasPotion)
        {
            customerRequestText.text = "No " + potion.ToString() + " potions left! Brew some more!";
            return;
        }

        switch (potion)
        {
            case PotionType.Red:
                redPotions--;
                break;
            case PotionType.Blue:
                bluePotions--;
                break;
            case PotionType.Green:
                greenPotions--;
                break;
        }

        potionsSold++;
        customerRequestText.text = "Sold " + potion.ToString() + " potion!";

        UpdateInventoryText();
        StartCoroutine(DelayedCustomerRequest());

    }

    IEnumerator DelayedCustomerRequest()
    {
        customerRequestText.text = "Waiting for next customer...";
        yield return new WaitForSeconds(2f); 
        GenerateNewCustomerRequest();
    }


    void UpdateInventoryText()
    {
        inventoryText.text = $"Inventory:\nRed: {redPotions}\nBlue: {bluePotions}\nGreen: {greenPotions}\n\nPotions Sold: {potionsSold}";
    }

    void GenerateNewCustomerRequest()
    {
        currentCustomerRequest = (PotionType)Random.Range(0, 3);
        customerRequestText.text = "Next customer wants a " + currentCustomerRequest.ToString() + " potion!";
    }


    public void CheckThreeRules()
    {
        bool hasAnyPotions = redPotions > 0 || bluePotions > 0 || greenPotions > 0;
        bool soldEnough = potionsSold >= 5;
        bool brewedAllTypes = brewedRed && brewedBlue && brewedGreen;

        if (hasAnyPotions && soldEnough && brewedAllTypes)
        {
            Debug.Log("You Win!");
            ShowEndMessage("You Win!");
        }
        else
        {
            Debug.Log("You Lose!");
            ShowEndMessage("You Lose!");
        }
    }


    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
