using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSpawner : MonoBehaviour
{
    public Vector2 resolution = new(128, 72);
    public Image TilePrefab;
    public ScanType ScanType;
    private float _widthOffset = 10.0f;
    public bool ShouldDrawContinous, isInterlaced;
    private int _callCount, _refreshCount;

    #region 3. Single Frame "Buffer"
    private List<Image> tiles;
    private FrameBuffer buffer;

    //Buffered Draw Call
    private IEnumerator DrawCall(int cycles)
    {
        tiles.Clear();
        _callCount = 0;
        _refreshCount = 0;

        while (_callCount < cycles)
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
                    rect.transform.localPosition = new(0 + _widthOffset * width, height);

                    //Debug.Log(frac);

                    Vector4 randomColor = new Vector4(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
                    tile.color = randomColor;

                    tile.gameObject.SetActive(true);
                }

                yield return null;
            }

            _callCount++;

            Debug.Log("draw call" + _callCount);

            yield return null;

            foreach (var tile in tiles)
            {
                GameObject.Destroy(tile.gameObject);
            }

            tiles.Clear();

            yield return null;

            _refreshCount++;

            Debug.Log("vertical refresh" + _refreshCount);
        }
    }

    #endregion

    #region 1. Raster scanning

    private IEnumerator DrawLine(int height = 0, bool forward = true)
    {
        float count = 0;

        if (forward)
        {
            //Draw line Left to Right
            for (int width = 0; width < resolution.x; width++)
            {
                var tile = Instantiate(TilePrefab, transform);

                tile.gameObject.name = "tile" + width;

                var rect = tile.GetComponent<RectTransform>();
                rect.transform.localPosition = new(0 + _widthOffset * width, height);

                count++;
                float frac = count / 128;
                //Debug.Log(frac);

                Color tileColor = new(frac, frac, frac, 1.0f);
                tile.color = tileColor;

                tile.gameObject.SetActive(true);
                yield return null;
            }
        }

        else
        {
            //Draw line Right to Left
            for (int width = (int)resolution.x; width > 0; width--)
            {
                var tile = Instantiate(TilePrefab, transform);

                tile.gameObject.name = "tile" + width;

                var rect = tile.GetComponent<RectTransform>();
                rect.transform.localPosition = new(0 + _widthOffset * width, height);

                count++;
                float frac = count / 128;
                //Debug.Log(frac);

                Color tileColor = new(frac, frac, frac, 1.0f);
                tile.color = tileColor;

                tile.gameObject.SetActive(true);
                yield return null;
            }
        }
    }

    private IEnumerator DrawGrid()
    {
        float pixelCount = 0;

        //Vertical Scan
        for (int height = 0; height < resolution.y; height++)
        {
            StartCoroutine(DrawLine(height));

            yield return null;
        }
    }

    private IEnumerator DrawBlack()
    {
        ShouldDrawContinous = false;

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
                rect.transform.localPosition = new(0 + _widthOffset * width, height);

                tile.color = Color.black;

                tile.gameObject.SetActive(true);
            }

            yield return null;
        }

        ShouldDrawContinous = true;
    }

    #endregion

    //Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        buffer = new FrameBuffer(TilePrefab);
        
        Vector4 randomColor = new Vector4(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
        
        StartCoroutine(buffer.Init(resolution, transform));

        yield return new WaitUntil(() => buffer._canDraw);
        DrawCall(ScanType);
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldDrawContinous)
        {
            DrawCall(ScanType);
        }
    }

    public void DrawCall(ScanType scanType)
    {
        switch (scanType)
        {
            case ScanType.none:
                break;
            case ScanType.line:
                StartCoroutine(DrawLine());
                break;
            case ScanType.pixelBypixel:
                StartCoroutine(DrawGrid());
                break;
            case ScanType.rasterScan:
                
                if (isInterlaced)
                {
                    //Draw Odd lines for Odd Frames
                    //Draw Even lines for even frames, in reverse.
                    //Hald Render time, Half accuracy. Screen tearing. Artifacts.
                }

                else
                {
                    //Draw everything pixel by pixel, line by line.
                    //This Progressive scan. the "p" in 1080p.
                }

                break;
            case ScanType.singleFrameBuffer:
                StartCoroutine(buffer.DrawPixels());
                break;
        }
    }
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

public enum ScanType
{
    none,
    line,
    pixelBypixel,
    rasterScan,
    singleFrameBuffer,
}
