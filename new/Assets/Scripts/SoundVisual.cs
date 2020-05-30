using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundVisual : MonoBehaviour
{
    private const int SAMPLE_SIZE = 1024;

    public float rmsValue;
    public float dbValue;
    public float pitchValue;

    public float maxVisualScale = 25.0f;
    public float visualModifier = 175.0f;
    public float smoothSpeed = 10.0f;
    public float keepPercentage = 0.25f;
    public float threshold = 0.02f;      // minimum amplitude to extract pitch

    public Material backgroundMaterial;
    public Color minColor;
    public Color maxColor;

    private AudioSource source;
    private float[] samples;
    private float[] spectrum;
    private float sampleRate;

    private Transform[] visualList;
    private float[] visualScale;
    private int amnVisual = 64;

    private void Start() {
        source = GetComponent<AudioSource>();
        samples = new float[SAMPLE_SIZE];
        spectrum = new float[SAMPLE_SIZE];
        sampleRate = AudioSettings.outputSampleRate;
        SetBackground();
        // SpawnLine();
        SpawnCircle();
    }

    private void SpawnLine() {
        visualScale = new float[amnVisual];
        visualList = new Transform[amnVisual];

        // Creates amnVisual cubes right next to each other 
        for (int i = 0; i < amnVisual; i++) {
            // Can change the primitive type for a different "look"
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere) as GameObject;
            visualList[i] = go.transform;
            visualList[i].position = Vector3.right * i;
        }
    }

    private void SpawnCircle() {
        visualScale = new float[amnVisual];
        visualList = new Transform[amnVisual]; 

        Vector3 center = Vector3.zero;
        float radius = 10.0f;

        for (int i = 0; i < amnVisual; i++) {
            float ang = i * 1.0f / amnVisual;
            ang = ang * Mathf.PI * 2;

            float x = center.x + Mathf.Cos(ang) * radius;
            float y = center.y + Mathf.Sin(ang) * radius;

            Vector3 pos = center + new Vector3(x, y, 0);
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
            go.transform.position = pos;
            go.transform.rotation = Quaternion.LookRotation(Vector3.forward, pos);
            visualList[i] = go.transform;
        }

    }

    // Called every second 
    private void Update() {
        // Music values updated every second 
        AnalyzeSound();
        // Visual is updated every second 
        UpdateVisual();
        SetBackground();
    }

    // Implement this however we want -based off of pitch and db
    private void SetBackground()
    {
        Camera.main.backgroundColor = Color.white;
    }

    private void UpdateVisual() {
        int spectrumIndex = 0;
        int averageSize = (int)((SAMPLE_SIZE * keepPercentage) / amnVisual);

        for (int visualIndex = 0; visualIndex < amnVisual; visualIndex++) {
            int j = 0;
            float sum = 0;
            while (j < averageSize) {
                sum += spectrum[spectrumIndex];
                spectrumIndex++;
                j++;
            }
            float scaleY = sum / averageSize * visualModifier;
            visualScale[visualIndex] -= Time.deltaTime * smoothSpeed;
            if (visualScale[visualIndex] < scaleY) {
                visualScale[visualIndex] = scaleY;
            }

            if (visualScale[visualIndex] > maxVisualScale) {
                visualScale[visualIndex] = maxVisualScale;
            }

            visualList[visualIndex].localScale = Vector3.one + Vector3.up * visualScale[visualIndex];
        }
    }

    // This code analyzes the input mp3 using digital signal processing -- not from us 
    private void AnalyzeSound() {
        source.GetOutputData(samples, 0);

        // get rms value
        int i = 0;
        float sum = 0;
        for (; i < SAMPLE_SIZE; i++) {
            sum += samples[i] * samples[i];
        }
        rmsValue = Mathf.Sqrt(sum / SAMPLE_SIZE);

        // get db value
        dbValue = 20 * Mathf.Log10(rmsValue / 0.1f);
        
        if (dbValue < - 160) dbValue = -160; // clipping the value 

        // get sound spectrum
        source.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        // find pitch
        float maxV = 0;
        var maxN = 0;
        for (i = 0; i < SAMPLE_SIZE; i++) {
            if (spectrum[i] > maxV && spectrum[i] > threshold) 
            {
                maxV = spectrum[i];
                maxN = i;
            }         
        }

        float freqN = maxN;
        if (maxN > 0 && maxN < SAMPLE_SIZE - 1) {
            var dL = spectrum[maxN - 1] / spectrum[maxN];
            var dR = spectrum[maxN + 1] / spectrum[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);
        }
        pitchValue = freqN * (sampleRate / 2) / SAMPLE_SIZE;
    }
}