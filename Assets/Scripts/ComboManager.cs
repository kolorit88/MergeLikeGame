using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
public class ComboManager : MonoBehaviour
{
    public List<AudioClip> mergeSounds;
    public int soundCounter = 0;

    public float comboOverTime = 3;
    private float countdown;

    public TextMeshProUGUI comboText;
    public Color startComboColor;
    public Color currentComboColor;

    [Header("Wobble Effect")]
    public bool enableWobble = true;
    public float wobbleSpeedScale = 2f;
    public float wobbleAmountScale = 10f;
    private float wobbleSpeed;
    private float wobbleAmount;

    [Header("Size Pulse Effect")]
    public bool enablePulse = true;
    public float pulseSpeed = 3f;
    public float minSize = 1f;
    public float maxSize = 1.1f;

    public ParticleSystem levelUpParticles;
    public ParticleSystem loopedParticles;

    public bool enableRotationWobble = true;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Vector3 _startScale;

    void Start()
    {
        countdown = comboOverTime;

        foreach (AudioClip clip in mergeSounds) {
            clip.LoadAudioData();
        }

        originalPosition = comboText.transform.localPosition;
        originalScale = _startScale = comboText.transform.localScale;
        originalRotation = comboText.transform.localRotation;

        wobbleSpeed = wobbleSpeedScale;
        wobbleAmount  = wobbleAmountScale;

        currentComboColor = startComboColor;
        comboText.fontMaterial.SetColor("_OutlineColor", new Color(currentComboColor.r - 0.4f, currentComboColor.g - 0.4f, currentComboColor.b - 0.4f));
        loopedParticles.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().startColor = currentComboColor;

    }
    void Update()
    {
        if (soundCounter > 1)
        {
            comboText.text = "x" + (soundCounter).ToString();
            animateComboText();
            if (!loopedParticles.isPlaying && soundCounter > 1) {
                loopedParticles.Play();
            }
        }
        else
        {
            comboText.text = "";
            setDefautColour();
            loopedParticles.Stop();
        }



        if (countdown > 0)
        {
            countdown -= Time.deltaTime;

            if (countdown <= 0)
            {
                soundCounter = 0;
            }
        }
        
    }

    private void animateComboText()
    {
        if (enableWobble)
            ApplyWobbleEffect();

        if (enablePulse)
            ApplyPulseEffect();

        if (enableRotationWobble)
            ApplyRotationWobble();

        wobbleAmount = soundCounter + 2f * wobbleAmountScale;
        wobbleSpeed = (soundCounter + 1f) * 2 * wobbleSpeedScale;

        originalScale = _startScale * (soundCounter * 0.1f + 1f);


    }

    void ApplyWobbleEffect()
    {
        // Покачивание по вертикали
        float wobbleOffset = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
        comboText.transform.localPosition = originalPosition + new Vector3(0, wobbleOffset, 0);
    }

    void ApplyPulseEffect()
    {
        // Пульсация размера
        float pulseValue = Mathf.PingPong(Time.time * pulseSpeed, 1);
        float currentScale = Mathf.Lerp(minSize, maxSize, pulseValue);
        comboText.transform.localScale = originalScale * currentScale;
    }

    void ApplyRotationWobble()
    {
        // Покачивание вращением (синусоидальное движение)
        float rotationAngle = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount * 0.5f;
        comboText.transform.localRotation = originalRotation * Quaternion.Euler(0, 0, rotationAngle);
    }

    void comboPlusOneEffect() {
        enableRotationWobble = false;
        comboText.transform.DORotate(new Vector3(0, 0, 360f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad).OnComplete(() => {
            enableRotationWobble = true; 
        });
        
    }

    public void ParticlesEffect()
    {
        ParticleSystem particles = Instantiate(levelUpParticles, comboText.gameObject.transform.position, Quaternion.identity);
        particles.transform.localScale = comboText.transform.localScale * 0.1f * soundCounter;
        particles.startColor = currentComboColor;

        particles.Play();
        Destroy(particles.gameObject, particles.main.duration);

    }

    public void updateColour()
    {
        if (soundCounter == 0) { return; } // не меняем цвет если это первый апдейт цвета, чтобы первым был стартовый цвет комбы(зеленый)
        if (currentComboColor.r != 1f)
        {
            currentComboColor.r = Mathf.Clamp(currentComboColor.r + 0.6f , 0f, 1f);
        }
        else if (currentComboColor.g != 0f)
        {
            currentComboColor.g = Mathf.Clamp(currentComboColor.g - 0.6f, 0f, 1f);
        }
        currentComboColor.a = 1f;

        comboText.color = currentComboColor;
        comboText.fontMaterial.SetColor("_OutlineColor", new Color(currentComboColor.r - 0.4f, currentComboColor.g - 0.4f, currentComboColor.b - 0.4f));
        loopedParticles.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().startColor = currentComboColor;
    }

    public void setDefautColour() { 
        comboText.color = startComboColor;
        currentComboColor = startComboColor;
        comboText.fontMaterial.SetColor("_OutlineColor", new Color(currentComboColor.r - 0.4f, currentComboColor.g - 0.4f, currentComboColor.b - 0.4f));
        loopedParticles.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().startColor = currentComboColor;
    }


    public void PlayAnotherSound()
    {
        AudioSource.PlayClipAtPoint(mergeSounds[soundCounter], Vector2.zero, 2f);
        if (soundCounter + 1 < mergeSounds.Count)
        {
            soundCounter++;
            if (soundCounter != 1)
            {
                updateColour();
                comboPlusOneEffect();
                ParticlesEffect();
            }
        }
        countdown = comboOverTime;
    }

    
    public int getComboNumber()
    {
        return soundCounter;
    }


}
