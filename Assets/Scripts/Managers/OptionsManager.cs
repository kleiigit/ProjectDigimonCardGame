
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    private AudioManager audioManager;
    public bool muteAudio = false;

    public List<TMP_FontAsset> fontList;

    public static event Action FontUpdated;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioManager = GameManager.Instance.AudioManager;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public TMP_FontAsset GetFontClass(string classID)
    {
        switch (classID)
        {
            case "MenuText":
                return fontList[0];
            case "CardTitle":
                return fontList[1];
            case "CardBody":
                return fontList[2];
            case "CardBodyBold":
                return fontList[3];
            case "MenuTextBold":
                return fontList[4];
            case "NumberLevel":
                return fontList[5];



            default:
                return fontList[0];
        }
    }

    public void UpdateFont()
    {
        FontUpdated?.Invoke();
    }
}
