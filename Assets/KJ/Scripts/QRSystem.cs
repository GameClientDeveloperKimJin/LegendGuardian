using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UniversalCamera;
using ZXing;

public class QRSystem : MonoBehaviour
{
    private BarcodeReader qrReader;
    private CancellationTokenSource ct;
    [Tooltip("스캔 딜레이(초)")]
    [SerializeField]
    private float ScanDealy = 0.2f;
    private void Awake()
    {
        qrReader = new BarcodeReader();
        ct = new CancellationTokenSource();
    }

    private void Start()
    {
        StartQRScanning();
    }
    private async void StartQRScanning()
    {
        while(!ct.Token.IsCancellationRequested)
        {
            await ProcessQRScanning();
            await Awaitable.WaitForSecondsAsync(ScanDealy);
        }
    }
    private async Task ProcessQRScanning()
    {
        WebCamTexture cameraTexture = CameraBackground.Instance.CameraBackgroundTexture;

        if(cameraTexture == null)
        {
            return;
        }

        await Awaitable.MainThreadAsync(); //혹시 모르니까 아래 GetPixels32()는 메인스레드에서 사용 
        Color32[] pixelData = cameraTexture.GetPixels32();

        await Awaitable.BackgroundThreadAsync(); //아래 줄은 무겁기 때문에 백그라운드 프로세스로 실행.

        Result result = qrReader.Decode(pixelData, cameraTexture.width, cameraTexture.height);

        if(result != null)
        {
            await Awaitable.MainThreadAsync(); //아래 로그는 백그라운드 스레드가 아닌 유니티의 메인스레드이므로, 메인스레드 Async() 사용 
            Debug.Log(result.Text); //결과값 읽기는 메인 쓰레드에서 처리 
        }

    }

    private void OnDestroy()
    {
        if(ct != null)
        {
            ct.Cancel();
            ct.Dispose();
        }
    }
}
