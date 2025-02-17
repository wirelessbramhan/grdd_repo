using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSpawner : MonoBehaviour
{
    public Vector2 resolution = new(128, 72);
    public Image TilePrefab;
    public RectTransform refreshPanel;
    public float widthOffset;
    public bool _canDraw;
    public int callCount, refreshCount;

    #region 3. Single Frame "Buffer"
    public List<Image> tiles;
    public FrameBuffer buffer;
    
    //Buffered Draw Call
    private IEnumerator DrawCall(int cycles)
    {
        tiles.Clear();
        callCount = 0;
        refreshCount = 0;

        while (callCount < cycles)
        {
            //Progressive Vertical Scan
            for (int height = 0; height < resolution.y; height++)
            {
                //Draw line
                for (int width = 0; width < resolution.x; width++)
                {
                    var tile = Instantiate(TilePrefab, transform);

                    if (tile.TryGetComponent<Image>(out var tileImage))
                    {
                        tiles.Add(tileImage);
                    }

                    tile.gameObject.name = "tile" + width;

                    var rect = tile.GetComponent<RectTransform>();
                    //rect.position = new(0 + 1 * i, 0);
                    rect.transform.localPosition = new(0 + widthOffset * width, height);

                    //Debug.Log(frac);

                    Vector4 randomColor = new Vector4(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
                    tile.color = randomColor;

                    tile.gameObject.SetActive(true);
                }

                yield return null;
            }

            callCount++;

            Debug.Log("draw call" + callCount);

            yield return null;

            foreach (var tile in tiles)
            {
                GameObject.Destroy(tile.gameObject);
            }

            tiles.Clear();

            yield return null;

            refreshCount++;

            Debug.Log("vertical refresh" + refreshCount);
        }
    }

    #endregion

    //Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
   {
        #region 1. Raster scanning
        //float count = 0;

        ////Draw line
        //for (int width = 0; width < 128; width++)
        //{
        //    var tile = Instantiate(TilePrefab, transform);

        //    tile.gameObject.name = "tile" + width;

        //    var rect = tile.GetComponent<RectTransform>();
        //    rect.transform.localPosition = new(0 + widthOffset * width, height);

        //    count++;
        //    float frac = count / 128;
        //    //Debug.Log(frac);

        //    Color tileColor = new(frac, frac, frac, 1.0f);
        //    tile.color = tileColor;

        //    tile.gameObject.SetActive(true);
        //    yield return null;
        //}

        //float pixelCount = 0;

        ////Vertical Scan
        //for (int height = 0; height < resolution.y; height++)
        //{
        //    //Draw line
        //    for (int width = 0; width < resolution.x; width++)
        //    {
        //        var tile = Instantiate(TilePrefab, transform);

        //        tile.gameObject.name = "tile" + width;

        //        var rect = tile.GetComponent<RectTransform>();
        //        //rect.position = new(0 + 1 * i, 0);
        //        rect.transform.localPosition = new(0 + widthOffset * width, height);

        //        pixelCount++;
        //        float frac = pixelCount / (resolution.x * resolution.y);
        //        //Debug.Log(frac);

        //        Color tileColor = new(frac, frac, frac, 1.0f);
        //        tile.color = tileColor;

        //        tile.gameObject.SetActive(true);
        //    }

        //    yield return null;
        //}

        #endregion

        //StartCoroutine(DrawCall(10));
        
        buffer = new FrameBuffer(TilePrefab);
        
        Vector4 randomColor = new Vector4(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
        
        StartCoroutine(buffer.Init(resolution, transform));
        
        yield return new WaitUntil(() => buffer._canDraw);
        StartCoroutine(buffer.DrawPixels());
    }

    // Update is called once per frame
    void Update()
    {
        if (_canDraw)
        {
            //StartCoroutine(DrawCall(false));
            StartCoroutine(buffer.DrawPixels());
        }
    }

    #region 2. interlacing

    private IEnumerator DrawCall(bool interlace)
    {
        _canDraw = false;
 
        float pixelCount = 0;

        if (!interlace)
        {
            //Progressive Vertical Scan
            for (int height = 0; height < resolution.y; height++)
            {
                //Draw line
                for (int width = 0; width < resolution.x; width++)
                {
                    var tile = Instantiate(TilePrefab, transform);

                    tile.gameObject.name = "tile" + width;

                    var rect = tile.GetComponent<RectTransform>();
                    //rect.position = new(0 + 1 * i, 0);
                    rect.transform.localPosition = new(0 + widthOffset * width, height);

                    pixelCount++;
                    float frac = pixelCount / (resolution.x * resolution.y);
                    //Debug.Log(frac);

                    Vector4 randomColor = new Vector4(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
                    tile.color = randomColor;

                    tile.gameObject.SetActive(true);
                }

                yield return null;
            }
        }

        else
        {
            //Interlacing Vertical Scan
            int frameCount = 1;

            //Render even lines for even frames
            if (frameCount % 2 == 0)
            {
                for (int height = 0; height < resolution.y; height++)
                {
                    if (height % 2 == 0)
                    {
                        //Draw line
                        for (int width = 0; width < resolution.x; width++)
                        {
                            var tile = Instantiate(TilePrefab, transform);

                            tile.gameObject.name = "tile" + width;

                            var rect = tile.GetComponent<RectTransform>();
                            //rect.position = new(0 + 1 * i, 0);
                            rect.transform.localPosition = new(0 + widthOffset * width, height);

                            pixelCount++;
                            float frac = pixelCount / (resolution.x * resolution.y);
                            //Debug.Log(frac);

                            Vector4 randomColor = new Vector4(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
                            tile.color = randomColor;

                            tile.gameObject.SetActive(true);
                        }
                    }

                    yield return null;
                }
            }

            //Render odd lines for odd frames
            for (int height = 0; height < resolution.y; height++)
            {
                if (height % 2 != 0)
                {
                    //Draw line
                    for (int width = 0; width < resolution.x; width++)
                    {
                        var tile = Instantiate(TilePrefab, transform);

                        tile.gameObject.name = "tile" + width;

                        var rect = tile.GetComponent<RectTransform>();
                        //rect.position = new(0 + 1 * i, 0);
                        rect.transform.localPosition = new(0 + widthOffset * width, height);

                        pixelCount++;
                        float frac = pixelCount / (resolution.x * resolution.y);
                        //Debug.Log(frac);

                        Vector4 randomColor = new Vector4(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
                        tile.color = randomColor;

                        tile.gameObject.SetActive(true);
                    }
                }

                yield return null;
            }

            callCount++;
            Debug.Log("draw call" + callCount);
        }

        //Clear Screen / Cull
        for (int height = 0; height < resolution.y; height++)
        {
            //Draw line
            for (int width = 0; width < resolution.x; width++)
            {
                var tile = Instantiate(TilePrefab, refreshPanel.transform);

                tile.gameObject.name = "tile" + width;

                var rect = tile.GetComponent<RectTransform>();
                //rect.position = new(0 + 1 * i, 0);
                rect.transform.localPosition = new(0 + widthOffset * width, height);

                pixelCount++;
                float frac = pixelCount / (resolution.x * resolution.y);
                //Debug.Log(frac);
                tile.color = Color.black;

                tile.gameObject.SetActive(true);
            }

            yield return null;
        }

        refreshCount++;
        Debug.Log("Vertical Refresh" + refreshCount);
        _canDraw = true;
    }

    private IEnumerator DrawCall()
    {
        _canDraw = false;
        float pixelCount = 0;

        //Vertical Scan
        for (int height = 0; height < resolution.y; height++)
        {
            //Draw line
            for (int width = 0; width < resolution.x; width++)
            {
                var tile = Instantiate(TilePrefab, transform);

                tile.gameObject.name = "tile" + width;

                var rect = tile.GetComponent<RectTransform>();
                //rect.position = new(0 + 1 * i, 0);
                rect.transform.localPosition = new(0 + widthOffset * width, height);

                pixelCount++;
                float frac = pixelCount / (resolution.x * resolution.y);
                //Debug.Log(frac);

                Color tileColor = new(frac, frac, frac, 1.0f);
                tile.color = tileColor;

                tile.gameObject.SetActive(true);
            }

            yield return null;
        }

        if (pixelCount == (128 - 1) * (72 - 1))
        {
            _canDraw = true;
        }
    }

    #endregion
}

[System.Serializable]
public class FrameBuffer 
{
    public List<Image[]> image;
    public Image pixel;
    public bool _canDraw;

    public FrameBuffer(Image prefab)
    {
        pixel = prefab;
        image = new List<Image[]>();
    }

    public IEnumerator Init(Vector2 resolution, Transform transform)
    {
        _canDraw = false;

        for (int height = 0; height < resolution.y; height++)
        {
            Image[] line = new Image[(int)resolution.x];

            for (int width = 0; width < resolution.x; width++)
            {
                Image newPixel = GameObject.Instantiate(pixel, transform);
                newPixel.color = Color.white;
                line[width] = newPixel;
                newPixel.gameObject.SetActive(true);
            }

            yield return null;
            image.Add(line);
        }

        _canDraw = true;
    }


    public IEnumerator DrawPixels(Vector4 values)
    {
        _canDraw = false;

        for (int height = 0; height < image.Count; height++)
        {
            //VScan
            Image[] line = image[height];

            //HScan
            foreach (var pixel in line)
            {
                Color newColor = new Color(values.w, values.x, values.y, values.z);
                pixel.color = newColor;
            }
        }

        yield return null;

        _canDraw = true;
    }

    public IEnumerator DrawPixels()
    {
        _canDraw = false;

        for (int height = 0; height < image.Count; height++)
        {
            //VScan
            Image[] line = image[height];

            //HScan
            foreach (var pixel in line)
            {
                Color newColor = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
                pixel.color = newColor;
            }
        }

        yield return null;

        _canDraw = true;
    }
}
