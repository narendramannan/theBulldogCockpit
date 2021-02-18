using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightSky : MonoBehaviour
{
    public int StarMagType;
    public float MagLimit;

    private ParticleSystem.Particle[] points;
    private ParticleSystem _particleSystem;

    private StarList _StarList;
    private StarList _BrightStarList;

    public string StarsFileName;

    private float timer = 0.0f;
    private float waitTime = 0.05f;
    private float angle = 0.025f;

    public static float GetScaleFromMagnitude(float magnitude)
    {
        float _minSize, _maxSize, _maxMag;
        _minSize = 0.05f;
        _maxSize = 0.5f;
        _maxMag = 8;

        float size = -magnitude * (_maxSize - _minSize) / _maxMag + _maxSize;
        return Mathf.Clamp(size, _minSize, _maxSize);
    }

    public static double QuadraticSizeApparent(double magnitude)
    {
        double _minMag = -1.5, _maxMag = 8.0;
        double _maxSize = 0.8;
        double _minSize = 0.05, _deltaSize = _minSize - _maxSize, _deltaMag = _maxMag - _minMag;
        return _minSize + (_maxSize - _minSize) * Math.Pow(_deltaMag - (magnitude - _minMag), 3) / Math.Pow(_deltaMag, 3);
    }

    public static double QuadraticSizeRealistic(double magnitude)
    {
        double _minMag = -1.5, _maxMag = 8.0;
        double _maxSize = 0.2;
        double _minSize = 0.05, _deltaSize = _minSize - _maxSize, _deltaMag = _maxMag - _minMag;
        return _minSize + (_maxSize - _minSize) * Math.Pow(_deltaMag - (magnitude - _minMag), 3) / Math.Pow(_deltaMag, 3);
    }

    public static double QuadraticSizeAbsolute(double magnitude)
    {
        double _minMag = -17, _maxMag = 8;
        double _maxSize = 0.8F;
        double _minSize = 0.05f, _deltaSize = _minSize - _maxSize, _deltaMag = _maxMag - _minMag;
        return _minSize + (_maxSize - _minSize) * Math.Pow(_deltaMag - (magnitude - _minMag), 3) / Math.Pow(_deltaMag, 3);
    }

    public static double QuadraticAlphaApparent(double magnitude)
    {
        double _minMag = -1.5F, _maxMag = 8;
        double _minSize = 0.05f, _maxSize = 1;
        double _deltaSize = _maxSize - _minSize, _deltaMag = _maxMag - _minMag;
        return _minSize + (_maxSize - _minSize) * Math.Pow(_deltaMag - (magnitude - _minMag), 2) / Math.Pow(_deltaMag, 2);

    }

    public static double QuadraticAlphaAbsolute(double magnitude)
    {
        double _minMag = -17F, _maxMag = 8;
        double _minSize = 0.05f, _maxSize = 1;
        double _deltaSize = _maxSize - _minSize, _deltaMag = _maxMag - _minMag;
        return _minSize + (_maxSize - _minSize) * Math.Pow(_deltaMag - (magnitude - _minMag), 2) / Math.Pow(_deltaMag, 2);
    }


    public void BrightStar()
    {
        _BrightStarList = new StarList();
        int j = 0;
        for (int i = 0; i < _StarList.Stars.Count; i++)
        {
            if (!_StarList.Stars[i].Name.Equals(""))
            {
                _BrightStarList.Stars.Add(_StarList.Stars[i]);
                // Set the radius to 1 to all these bright stars
                double _distance = Math.Sqrt(_StarList.Stars[i].XYZ[0] * _StarList.Stars[i].XYZ[0] + _StarList.Stars[i].XYZ[1] * _StarList.Stars[i].XYZ[1] + _StarList.Stars[i].XYZ[2] * _StarList.Stars[i].XYZ[2]);
                _BrightStarList.Stars[j].XYZ[0] /= _distance;
                _BrightStarList.Stars[j].XYZ[1] /= _distance;
                _BrightStarList.Stars[j].XYZ[2] /= _distance;
                j++;
            }
        }
        print("Total bright stars:" + _BrightStarList.Stars.Count);
    }


    public string GetCloseStar(double RAdeg, double Decdeg)
    {
        return GetCloseStar(Convert.ToSingle(RAdeg), Convert.ToSingle(Decdeg));
    }

    public string GetCloseStar(float RAdeg, float Decdeg)
    {
        string str = "";

        double distance = 0.0F;
        double x, y, z, x1, y1, z1;
        double distancemin = 5;

        double RA = Convert.ToDouble(RAdeg) * Math.PI / 180.0;
        double Dec = Convert.ToDouble(Decdeg) * Math.PI / 180.0;

        x1 = Math.Cos(RA) * Math.Cos(Dec);
        z1 = Math.Sin(RA) * Math.Cos(Dec);
        y1 = Math.Sin(Dec);

        double d1 = Math.Sqrt(x1 * x1 + y1 * y1 + z1 * z1);
        double threshold = 2.0 * Math.PI / 180.0; // 2 degrees

        for (int i = 0; i < _BrightStarList.Stars.Count; i++)
        {
            x = -_BrightStarList.Stars[i].XYZ[0];
            y = -_BrightStarList.Stars[i].XYZ[1];
            z = -_BrightStarList.Stars[i].XYZ[2];

            distance = Math.Sqrt((x1 - x) * (x1 - x) + (y1 - y) * (y1 - y) + (z1 - z) * (z1 - z));

            if ((distance < threshold) && (distance < distancemin))
            {
                distancemin = distance;
                str = _BrightStarList.Stars[i].Name + " Mag:" + _BrightStarList.Stars[i].Magnitude;
            }
        }
        return str; 
    }

    public void UpdateScene()
    {
        MagLimit = 6.5f;
        _particleSystem = new ParticleSystem();
        points = new ParticleSystem.Particle[_StarList.Stars.Count];
        double _distance;
        double alpha = 0.0;
        for (int i = 0; i < _StarList.Stars.Count; i++)
        {
            _distance = Math.Sqrt(_StarList.Stars[i].XYZ[0] * _StarList.Stars[i].XYZ[0] + _StarList.Stars[i].XYZ[1] * _StarList.Stars[i].XYZ[1] + _StarList.Stars[i].XYZ[2] * _StarList.Stars[i].XYZ[2]);
            double radius = 30.0f;
            double x = -_StarList.Stars[i].XYZ[2] * radius / _distance;
            double y = -_StarList.Stars[i].XYZ[1] * radius / _distance;
            double z = _StarList.Stars[i].XYZ[0] * radius / _distance;

            points[i].position = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), Convert.ToSingle(z));

            // Size of the stars
            if (StarMagType == 0)
            {
                points[i].startSize = Convert.ToSingle(QuadraticSizeApparent(Convert.ToSingle(_StarList.Stars[i].Magnitude))); // apparent
                alpha = QuadraticAlphaApparent(_StarList.Stars[i].Magnitude);
            }
            else if (StarMagType == 1)
            {
                points[i].startSize = Convert.ToSingle(QuadraticSizeRealistic(Convert.ToSingle(_StarList.Stars[i].Magnitude))); // "realistic"
                alpha = QuadraticAlphaApparent(_StarList.Stars[i].Magnitude);
            }
            else if (StarMagType == 2)
            {
                points[i].startSize = Convert.ToSingle(QuadraticSizeAbsolute(_StarList.Stars[i].AbsMag)); // absolute
                alpha = QuadraticAlphaAbsolute(_StarList.Stars[i].AbsMag);
            }

            // Effect of BortleScale           
            if (_StarList.Stars[i].Magnitude > MagLimit)
                alpha = 0.0;


            if (StarMagType == 1)
                points[i].startColor = new Color(255, 255, 200, Convert.ToSingle(alpha)); // white - yellowish stars
            else
                points[i].startColor = new Color(_StarList.Stars[i].RGB[0], _StarList.Stars[i].RGB[1], _StarList.Stars[i].RGB[2], Convert.ToSingle(alpha));
        }

        _particleSystem = GetComponent<ParticleSystem>();
        _particleSystem.SetParticles(points, points.Length);
    }

    public static Transform rotatePSystem;

    private void Awake()
    {
        StarMagType = 0;
        _StarList = new StarList();
    }

    void Start()
    {
        // AllStars
        string filename = Application.streamingAssetsPath + "/" + StarsFileName + ".json";
        if (System.IO.File.Exists(filename))
        {
            string str = System.IO.File.ReadAllText(filename);
            _StarList = JsonUtility.FromJson<StarList>(str);
            print("Stars Lf --> " + _StarList.Stars.Count.ToString());
            UpdateScene();
        }
        else
        {
            print("Stars file is Null");
        }
    }

    void Update()
    {
        //timer += Time.deltaTime;
        //if (timer > waitTime)
        //{
        //    // Remove the waitTime
        //    timer = timer - waitTime;
        //    this.transform.Rotate(0, angle, 0);
        //}
    }

}
