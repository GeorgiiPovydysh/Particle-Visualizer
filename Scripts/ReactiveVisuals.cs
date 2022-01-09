using UnityEngine;
using System.Linq;
using UnityEditor.PackageManager.UI;

[RequireComponent(typeof(AudioSource))]
public class ReactiveVisuals : MonoBehaviour
{
    // properties
    public GameObject prefab;
    private GameObject[] prefabObjects; //will contain an array of the prefab
    private float speed; // movement speed of the objects
    private Vector3[] originalSize;
    Vector3[] originalPos;
    float[] originalMass;
    private int numberOfObjects;

   
    // random positions and size to instantiate objects
    private float randomX;
    private float randomY;
    private float randomZ;
    private float randomSize;

   // movement related 
    private float move;
    private Vector3 destination;

    //audio
    private float[] samples = new float[4096];
    private float[] freqBands = new float[8];
    private AudioSource song;

    

   
    // Color Freq Band
    // do not confuse with the float variables of the same name!!!
    [Range(0,7)]
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

    public float speedMultiplier; //multiply the freqency speed1 and speed2 by this number
    public float restSpeed; // default speed when no sound

    public bool useMic = false;
 


    void Awake()
    {
        move = 0.0f;
        numberOfObjects = 2048;
        originalPos = new Vector3[numberOfObjects];
        originalMass = new float[numberOfObjects];
        destination = new Vector3(Random.Range(-8.0f, 8.0f), Random.Range(-4.0f, 4.0f), Random.Range(29.0f, 31.0f));
        // instantiate object 
        for (int i = 0; i < numberOfObjects; i++)
        {
            
            randomX = Random.Range(transform.position.x - 10.0f, transform.position.x + 10.0f);
            randomY = Random.Range(transform.position.y - 10.0f, transform.position.y + 10.0f);
            randomZ = Random.Range(transform.position.z - 10.0f, transform.position.z + 10.0f);
            originalPos[i] = new Vector3(randomX, randomY, randomZ);
            Instantiate(prefab, new Vector3(randomX, randomY, randomZ), Quaternion.identity);
        }
        prefabObjects = GameObject.FindGameObjectsWithTag("prefabObjects");
        originalSize = new Vector3[numberOfObjects];
      
        //find them and change their size;
        for (int j = 0; j < prefabObjects.Length; j++)
        {
            if(j < 10)
            {
                randomSize = Random.Range(1.0f, 2.5f);
                prefabObjects[j].GetComponent<Rigidbody>().mass = 100.0f * randomSize;
            }
            else if(j > 11  && j < 120)
            {
                randomSize = Random.Range(0.5f, 0.8f);
                prefabObjects[j].GetComponent<Rigidbody>().mass = 50.0f * randomSize;
            }
            else
            {
                randomSize = Random.Range(0.1f, 0.4f);
                prefabObjects[j].GetComponent<Rigidbody>().mass = 5.0f * randomSize;
            }
            
            prefabObjects[j].transform.localScale = new Vector3(randomSize, randomSize, randomSize);
            originalSize[j] = prefabObjects[j].transform.localScale;
            originalMass[j] = prefabObjects[j].GetComponent<Rigidbody>().mass;

        }
        if (useMic == false)
        {
            song = GetComponent<AudioSource>();
            song.Play();

        }
        else
        {
            song = GetComponent<AudioSource>();
            song.clip = Microphone.Start("",true, 10, 44100);
            song.loop = true;
            song.Play();

        }
     
    }

	// Update is called once per frame
	void Update ()
    {
        song.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
        FrequencyBands();
        SizeReact();
        ColorReact();
      
    }

    private void FixedUpdate()
    {
        Movement();
    }


    void Movement()
    {


        float distance = Vector3.Distance(destination, this.transform.position);

        if (distance <= 0.5f || freqBands[reactband] > 0.5f)
        {
            // if within range then generate new position 
            destination = new Vector3(Random.Range(-15.0f, 15.0f), Random.Range(-6.0f, 6.0f), Random.Range(0.0f, 5.0f));
        }
        else
        {
            move = (samples[0] * speedMultiplier) + restSpeed;
            this.transform.position = Vector3.MoveTowards(this.transform.position, destination, move * Time.deltaTime);
        }




        // All the cubes follow the new location on their own. 

        for (int i = 0; i < prefabObjects.Length; i++)
        {

            speed = (samples[i] * speedMultiplier) + restSpeed;
            prefabObjects[i].transform.LookAt(this.transform.position);
            prefabObjects[i].transform.position = Vector3.MoveTowards(prefabObjects[i].transform.position, this.transform.position, speed * Time.deltaTime);
           //  prefabObjects[i].GetComponent<Rigidbody>().AddRelativeForce(this.transform.position);



        }
    }


    void SizeReact()
    {
        
        
        for (int i = 0; i < prefabObjects.Length; i++)
        {
            // Changes Size of Cube.
            
            float prevScale = prefabObjects[i].transform.localScale.x;

            prevScale = samples[i] * 25.0f;
            prevScale = Mathf.Lerp(prevScale, samples[i] * 5.0f, Time.deltaTime);

         
            if (prevScale > 0.0005f)
            {
                Vector3 previousScale = new Vector3(prevScale, prevScale, prevScale);
                prefabObjects[i].transform.localScale = originalSize[i] + previousScale;
                prefabObjects[i].GetComponent<Rigidbody>().mass = originalMass[i] + prevScale;
                
            }
            else
            {
                prefabObjects[i].transform.localScale = originalSize[i];
                prefabObjects[i].GetComponent<Rigidbody>().mass = originalMass[i];
            }
           

        }

    }

    void ColorReact()
    {
        
        float red;
        float green;
        float blue;



        Color newColor;
        for (int i = 0; i < prefabObjects.Length; i++)
        {
                 red = samples[i] * freqBands[redReact] * redMultiplier;
                 green = samples[i] * freqBands[greenReact] * greenMultiplier;
                 blue = samples[i] * freqBands[blueReact] * blueMultiplier;

                newColor = new Color(red, green, blue);


            prefabObjects[i].GetComponent<Renderer>().material.SetColor("_myColor",newColor);
        }


    }

    void FrequencyBands() {

        int count = 0;
        for (int i = 0; i < 8 ; i++)
        {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;

            if(i == 7)
            {
                sampleCount += 2;
            }
            for(int j = 0;j < sampleCount; j++)
            {
                average += samples[count] * (count + 1);
                count++;
            }
            average = average / count;
            freqBands[i] = average * 10.0f;
            
        }
    }

    

}
