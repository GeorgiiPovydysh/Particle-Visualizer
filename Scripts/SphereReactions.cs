using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereReactions : MonoBehaviour
{  // properties
    public GameObject prefab;
    public float scaleMultiplier;
    //audio
    private float[] samples = new float[512];
    private float[] freqBands = new float[8];
    private AudioSource song;

    public Material deformationMaterial;
    public float  reactThreshold;

    // Color Freq Band
    // do not confuse with the float variables of the same name!!!
    [Range(0, 7)]
    public int redReact; //variable will hold the frequency band to which the color red will react

    [Range(0, 7)]
    public int greenReact; //variable will hold the frequency band to which the color green will react

    [Range(0, 7)]
    public int blueReact; //variable will hold the frequency band to which the color blue will react

    [Range(0.0f, 255.0f)]
    public float redMultiplier; // variable will hold a float which will multiply the frequncy band of red

    [Range(0.0f, 255.0f)]
    public float greenMultiplier;  // variable will hold a float which will multiply the frequncy band of green

    [Range(0.0f, 255.0f)]
    public float blueMultiplier;  // variable will hold a float which will multiply the frequncy band of blue

    [Range(0, 7)]
    public int reactband; // movement of visualizer will be determined by this

    void Awake()
    {
       
        

        // instantiate object
        Instantiate(prefab, this.transform.position, Quaternion.identity);
        song = GetComponent<AudioSource>();
        song.Play();

       
    }

    // Update is called once per frame
    void Update()
    {
        song.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
        FrequencyBands();
        Distortion();
        ColorReact();

    }


    void Distortion()
    {

        float prevScale = prefab.transform.localScale.x;

        prevScale = freqBands[reactband] * scaleMultiplier;
        //prevScale = Mathf.Lerp(prevScale, freqBands[reactband] * scaleMultiplier, Time.deltaTime);

        Vector3 previousScale = new Vector3(prevScale, prevScale, prevScale);


        deformationMaterial.SetFloat("Vector1_79eaad4e4a92403b861d1ffd7324f2f6", prevScale); // change overall size
        deformationMaterial.SetFloat("Vector1_a146037d2e074d91b223d644ec645946", freqBands[reactband]); // amount of spikes
            
           // deformationMaterial.SetFloat("Vector1_70c9033d37bb4fdab31d19878a67d5e8", 10.0f); // spike multiplier
            


    }


    void ColorReact()
    {

        float red;
        float green;
        float blue;



        Color newColor;


        
           

            
        if (freqBands[reactband] > reactThreshold)
        {
            red = freqBands[redReact] * redMultiplier / 5.0f;
            green = freqBands[greenReact] * greenMultiplier / 5.0f;
            blue = freqBands[blueReact] * blueMultiplier / 5.0f;
            newColor = new Color(red, green, blue);

            deformationMaterial.SetColor("Color_d7206c487814480e8410c7cd94515180", newColor); // emmision color 
        }
     

       

    }

    void FrequencyBands()
    {

        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;

            if (i == 7)
            {
                sampleCount += 2;
            }
            for (int j = 0; j < sampleCount; j++)
            {
                average += samples[count] * (count + 1);
                count++;
            }
            average = average / count;
            freqBands[i] = average * 10.0f;

        }
    }
}
