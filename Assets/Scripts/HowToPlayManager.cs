using UnityEngine;
using UnityEngine.SceneManagement;

public class HowToPlayManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] stepPanels;

    private int currentStep =  0;

    public void NextStepClicked()
    {
        stepPanels[currentStep].SetActive(false);
        currentStep++;
        stepPanels[currentStep].SetActive(true);
    }

    public void StartGame()
    {
        print("loading game");
        // SceneManager.LoadScene("Game");
    }
}
