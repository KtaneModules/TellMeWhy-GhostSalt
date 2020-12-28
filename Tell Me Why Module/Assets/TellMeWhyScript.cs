using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;


public class TellMeWhyScript : MonoBehaviour {

    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMBombModule Module;
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMSelectable Button;
    public TextMesh Display;
    public MeshRenderer[] StageIndicators;
    
    private int[] Sequence = new int[5];
    private int SolutionNumber;
    private int EdgeworkModifier;
    private int Stage = 0;
    private int CurrentNum;
    private bool DisplayOn;
    private bool Begin;
    private bool Solved;
    private bool Holding;
    private bool FreeInteract;

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        Button.OnInteract += delegate
        {
            StartCoroutine(AnimButton(true));
            Button.AddInteractionPunch();
            ButtonPress();
            Audio.PlaySoundAtTransform("press", Button.transform);
            return false;
        };
        Button.OnInteractEnded += delegate (){ ButtonRelease(); StartCoroutine(AnimButton(false)); Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, Button.transform); };
    }

    // Poggers moment
    void Start() {
        StartCoroutine(Flicker());
    }

    // Poggers moment but further in the script
    void Update() {
        
    }

    void GenSequence() {
        string[] SequenceFormatted = new string[5];
        for (int i = 0; i < 5; i++)
        {
            Sequence[i] = Rnd.Range(0,100);
            SequenceFormatted[i] = Sequence[i].ToString("00");
        }
        Debug.LogFormat("[Tell Me Why #{0}] The displayed sequence for stage {1} is {2}.", _moduleID, Stage, SequenceFormatted.Join(", "));
        if (Sequence.Contains(69))
        {
            EdgeworkModifier = 0;
            Debug.LogFormat("[Tell Me Why #{0}] The sequence contains \"69\", therefore the edgework modifier is 0. Nice!", _moduleID);
        }
        else
        {
            int ParityCheckOdd = 0;
            int ParityCheckEven = 0;
            for (int i = 0; i < 5; i++)
            {
                if (Sequence[i] % 2 == 1)
                {
                    ParityCheckOdd++;
                }
                else
                {
                    ParityCheckEven++;
                }
            }
            if (ParityCheckOdd >= 4 || ParityCheckEven >= 4)
            {
                EdgeworkModifier = Bomb.GetSolvableModuleNames().Count;
                Debug.LogFormat("[Tell Me Why #{0}] The sequence has four or more numbers that share parity, therefore the edgework modifier is {1}.", _moduleID, EdgeworkModifier);
            }
            else if (Sequence.Sum() > 299)
            {
                EdgeworkModifier = Bomb.GetPortCount();
                Debug.LogFormat("[Tell Me Why #{0}] The sum of the numbers in the sequence is greater than 299, therefore the edgework modifier is {1}.", _moduleID, EdgeworkModifier);
            }
            else if (ParityCheckEven == 3)
            {
                EdgeworkModifier = Bomb.GetBatteryCount();
                Debug.LogFormat("[Tell Me Why #{0}] The sequence has three even numbers, therefore the edgework modifier is {1}.", _moduleID, EdgeworkModifier);
            }
            else if (Sequence[4] % 2 == 0)
            {
                EdgeworkModifier = Bomb.GetOnIndicators().Count();
                Debug.LogFormat("[Tell Me Why #{0}] The last number of the sequence is even, therefore the edgework modifier is {1}.", _moduleID, EdgeworkModifier);
            }
            else if (Sequence[0] % 2 == 0)
            {
                EdgeworkModifier = Bomb.GetOffIndicators().Count();
                Debug.LogFormat("[Tell Me Why #{0}] The first number of the sequence is even, therefore the edgework modifier is {1}.", _moduleID, EdgeworkModifier);
            }
            else if (Stage == 1)
            {
                EdgeworkModifier = Bomb.GetSerialNumberNumbers().First();
                Debug.LogFormat("[Tell Me Why #{0}] The module is in stage 1, therefore the edgework modifier is {1}.", _moduleID, EdgeworkModifier);
            }
            else if (Stage == 2)
            {
                EdgeworkModifier = Bomb.GetSerialNumberNumbers().ElementAt(1);
                Debug.LogFormat("[Tell Me Why #{0}] The module is in stage 1, therefore the edgework modifier is {1}.", _moduleID, EdgeworkModifier);
            }
            else
            {
                EdgeworkModifier = Bomb.GetSerialNumberNumbers().Last();
                Debug.LogFormat("[Tell Me Why #{0}] The module is in stage 1, therefore the edgework modifier is {1}.", _moduleID, EdgeworkModifier);
            }
        }
        if (Stage == 1)
        {
            StartCoroutine(DisplaySequence());
        }
        Debug.LogFormat("[Tell Me Why #{0}] The sum of the numbers in the sequence is {1} and therefore the solution for this stage is the number in position {2}.", _moduleID, Sequence.Sum(), (DigitalRoot(Sequence.Sum() + EdgeworkModifier) % 5) + 1);
    }

    private static int DigitalRoot(int n)
    {
        var root = 0;
        while (n > 0 || root > 9)
        {
            if (n == 0)
            {
                n = root;
                root = 0;
            }

            root += n % 10;
            n /= 10;
        }
        return root;
    }

    void ButtonPress()
    {
        if (!Solved)
        {
            if (FreeInteract)
            {
                FreeInteract = false;
            }
            else
            {
                if (Stage == 0)
                {
                    if (((int)Bomb.GetTime() % 60) % 10 == Bomb.GetSolvedModuleNames().Count() % 10)
                    {
                        Debug.LogFormat("[Tell Me Why #{0}] The button was held when the last digit of the bomb's timer was {1} while the bomb had {2} solved module(s), which was correct. Onto the first stage!", _moduleID, ((int)Bomb.GetTime() % 60) % 10, Bomb.GetSolvedModuleNames().Count());
                        Stage++;
                        GenSequence();
                    }
                    else
                    {
                        Debug.LogFormat("[Tell Me Why #{0}] The button was held when the last digit of the bomb's timer was {1} while the bomb had {2} solved module(s), which was incorrect. Strike!", _moduleID, ((int)Bomb.GetTime() % 60) % 10, Bomb.GetSolvedModuleNames().Count());
                        Module.HandleStrike();
                        Audio.PlaySoundAtTransform("buzzer", Button.transform);
                        FreeInteract = true;
                    }
                }
                else
                {
                    if (!Solved)
                    {
                        if (DigitalRoot(Sequence.Sum() + EdgeworkModifier) % 5 == CurrentNum)
                        {
                            Debug.LogFormat("[Tell Me Why #{0}] The button was held when the number in position {1} in the sequence was displayed, which was correct.", _moduleID, CurrentNum + 1);
                            Stage++;
                            GenSequence();
                        }
                        else
                        {
                            if (CurrentNum == 5)
                            {
                                Debug.LogFormat("[Tell Me Why #{0}] The button was held when the display was empty, which was (obviously) incorrect. Strike!", _moduleID, CurrentNum + 1);
                            }
                            else
                            {
                                Debug.LogFormat("[Tell Me Why #{0}] The button was held when the number in position {1} in the sequence was displayed, which was incorrect. Strike!", _moduleID, CurrentNum + 1);
                            }
                            Module.HandleStrike();
                            Audio.PlaySoundAtTransform("buzzer", Button.transform);
                            FreeInteract = true;
                        }
                    }
                }
            }
        }
    }

    void ButtonRelease()
    {
        if (!Solved)
        {
            if (FreeInteract)
            {
                FreeInteract = false;
            }
            else
            {
                if (DigitalRoot(Sequence.Sum() + EdgeworkModifier) % 5 == CurrentNum && Stage != 0)
                {
                    if (Stage >= 3)
                    {
                        Debug.LogFormat("[Tell Me Why #{0}] The button was released when the number in position {1} in the sequence was displayed, which was correct.", _moduleID, CurrentNum + 1);
                        Debug.LogFormat("[Tell Me Why #{0}] Module solved! Poggers!", _moduleID);
                        Module.HandlePass();
                        Display.text = "GG";
                        CurrentNum = 6;
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, Button.transform);
                        Solved = true;
                    }
                    else
                    {
                        Debug.LogFormat("[Tell Me Why #{0}] The button was released when the number in position {1} in the sequence was displayed, which was correct.", _moduleID, CurrentNum + 1);
                        Stage++;
                        GenSequence();
                    }
                }
                else
                {
                    if (CurrentNum == 5)
                    {
                        Debug.LogFormat("[Tell Me Why #{0}] The button was released when the display was empty, which was (obviously) incorrect. Strike!", _moduleID, CurrentNum + 1);
                    }
                    else
                    {
                        Debug.LogFormat("[Tell Me Why #{0}] The button was released when the number in position {1} in the sequence was displayed, which was incorrect. Strike!", _moduleID, CurrentNum + 1);
                    }
                    Module.HandleStrike();
                    Audio.PlaySoundAtTransform("buzzer", Button.transform);
                    FreeInteract = true;
                }
            }
        }
    }

    private IEnumerator DisplaySequence()
    {
        while (!Solved)
        {
            for (int i = 0; i < 5; i++)
            {
                if (!Solved)
                {
                    Display.text = Sequence[i].ToString("00");
                    CurrentNum = i;
                    yield return new WaitForSeconds(1f);
                }
            }
            if (!Solved)
            {
                Display.text = "";
                CurrentNum = 5;
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private IEnumerator AnimButton(bool Press)
    {
        if (Press)
        {
            for (int i = 0; i < 3; i++)
            {
                Button.transform.localPosition = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y - 0.002f, Button.transform.localPosition.z);
                yield return new WaitForSeconds(0.02f);
            }
        }
        else
        {
            for (int i = 0; i < 6; i++)
            {
                Button.transform.localPosition = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y + 0.001f, Button.transform.localPosition.z);
                yield return new WaitForSeconds(0.02f);
            }
        }
    }

    private IEnumerator Flicker() {

        while (true)
        {
            Display.color = new Color(1f, 0f, 0f, Rnd.Range(0f, 1f));
            for (int i = 0; i < 3; i++)
            {
                if (Stage > i)
                {
                    StageIndicators[i].material.color = new Color(Rnd.Range(0.25f, 0.75f), 0f, 0f, 1f);
                }
                else
                {
                    StageIndicators[i].material.color = new Color(0, 0, 0, 1f);
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    

#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} 07' to press the button, then press it again when the Display displays 07. Note that single digit numbers need to be preceded with a 0.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string validcmdsinitial = "0123456789";
        string validcmdsstage1andafter = "12345";
        if (command.Length != 1)
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
        if (Stage == 0)
        {
            if (!validcmdsinitial.Contains(command))
            {
                yield return "sendtochaterror Invalid command.";
                yield break;
            }
            while (command != (((int)Bomb.GetTime() % 60) % 10).ToString())
            {
                yield return "trycancel Button press cancelled (Tell Me Why).";
            }
            yield return null;
            Button.OnInteract();
        }
        else
        {
            if (!validcmdsstage1andafter.Contains(command))
            {
                yield return "sendtochaterror Invalid command.";
                yield break;
            }
            while (command != (CurrentNum + 1).ToString())
            {
                yield return "trycancel Button press cancelled (Tell Me Why).";
            }
            yield return null;
            if (Stage % 2 == 0)
            {
                Button.OnInteract();
            }
            else
            {
                Button.OnInteractEnded();
            }
        }
    }
    IEnumerator TwitchHandleForcedSolve() {
        while (Bomb.GetSolvedModuleNames().Count().ToString() != (((int)Bomb.GetTime() % 60) % 10).ToString())
        {
            yield return "trycancel Button press cancelled (Tell Me Why).";
        }
        yield return null;
        Button.OnInteract();
        for (int i = 0; i < 3; i++)
        {
            while ((DigitalRoot(Sequence.Sum() + EdgeworkModifier) % 5).ToString() != CurrentNum.ToString())
            {
                yield return "trycancel Button press cancelled (Tell Me Why).";
            }
            yield return null;
            if (Stage % 2 == 0)
            {
                Button.OnInteract();
            }
            else
            {
                Button.OnInteractEnded();
            }
        }
    }
    // But is it really thicc?
}