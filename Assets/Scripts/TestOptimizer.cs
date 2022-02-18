
using UnityEngine;
using UnityEngine.UI;

public class TestOptimizer : MonoBehaviour
{
    [SerializeField] InputField input;
    [SerializeField] Button btn;
    [SerializeField] Text inputText, decodedText, encodedText, sizeText;

    void Start()
    {
        btn.onClick.AddListener(delegate ()
        {
            string text = input.text;
            input.text = "";

            if (string.IsNullOrEmpty(text))
                return;

            string[] parts = text.Split(',');
            if (parts.Length == 3)
            {
                Vector3 v = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
                inputText.text = v.ToString();
                ulong i = encodeVectorToInt(v);
                encodedText.text = i.ToString();
                decodedText.text = decodeIntToVector(i).ToString();
                sizeText.text = (sizeof(float) * 3) + ", " + sizeof(ulong);
            }
        });
    }

    ulong encodeVectorToInt(Vector3 v)
    {
        ulong xcomp = (ulong)(Mathf.RoundToInt((v.x * 100f)) + 32768);
        ulong ycomp = (ulong)(Mathf.RoundToInt((v.y * 100f)) + 32768);
        ulong zcomp = (ulong)(Mathf.RoundToInt((v.z * 100f)) + 32768);
        return xcomp + ycomp * 65536 + zcomp * 4294967296;
    }
    Vector3 decodeIntToVector(ulong i)
    {
        ulong z = (i / 4294967296);
        ulong y = ((i - z * 4294967296) / 65536);
        ulong x = (i - y * 65536 - z * 4294967296);
        return new Vector3((x - 32768f) / 100f, (y - 32768f) / 100f, (z - 32768f) / 100f);
    }
}