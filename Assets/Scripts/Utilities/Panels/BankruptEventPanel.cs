using UnityEngine.UI;
using UnityEngine;

public class BankruptEventPanel : EventPanel {

	public void OnBusinessSave()
    {
        GameManager.SaveBusiness();
    }

    public void OnBankrupt()
    {
        GameManager.EndGame();
    }
}
