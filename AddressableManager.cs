using Arbor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using I2.Loc;

/*-----------------------------------------------------------------------------------------------
Addressableを使用したアセットダウンロードのスクリプト
------------------------------------------------------------------------------------------------*/
public class AddressableManager : MonoBehaviour
{
    [SerializeField] private Image testImage1 = default;
    public AssetLabelReference List;
    [SerializeField] private TextMeshProUGUI DlSize = default;
    [SerializeField] private TextMeshProUGUI NowDL = default;
    [SerializeField] GameObject Errorlog = default;

    private ArborFSM Trigger;
    private IList<object> Key;
    long dataSize;

    bool Error;

    private void Start()
    {

        Trigger = GameObject.Find("GameStart").GetComponent<ArborFSM>();
    }
    /*-----------------------------------------------------------------------------------------------
    初回のみチュートリアルのDL
    ------------------------------------------------------------------------------------------------*/
    public void Download_Tutorial()
    {
        Debug.Log("TutorialのDL 開始");
        Key = new string[] { "Tutorial", "default", "Scene", "Prefab", "DataBase", "Texture", "Font", "Audio" };
        Action<long> onSuccess = (long Result) => { dataSize = Result; CheckLoad(); };

        GetDownloadSizeAsync(Key, onSuccess);
    }
    /*-----------------------------------------------------------------------------------------------
    Addressableのカタログのチェック 更新
    ------------------------------------------------------------------------------------------------*/
    public void CatalogCheck()
    {
        Debug.Log("CheckForCatalogUpdates 開始");

        // リモートカタログの更新があるかどうか確認する
        Addressables.CheckForCatalogUpdates().Completed += checkHandle =>
        {
            Debug.Log($"CheckForCatalogUpdates 終了：{checkHandle.Status}");

            // 更新があるカタログの名前のリストを取得する
            var catalogs = checkHandle.Result;
            var count = catalogs.Count;

            Debug.Log("カタログ更新数_" + count);

            // カタログの更新が無い場合はここで終わる
            if (count == 0) { Trigger.SendTrigger("Next"); return; }

            // 更新があるカタログの名前を出力する
            foreach (var catalog in catalogs)
            {
                Debug.Log(catalog);
            }

            Debug.Log("UpdateCatalogs 開始");

            // リモートカタログを更新する
            Addressables.UpdateCatalogs(catalogs).Completed += updateHandle =>
            {
                Debug.Log($"UpdateCatalogs 終了：{updateHandle.Status}");

                // 更新したカタログのリストを取得する
                var locators = updateHandle.Result;

                foreach (var locator in locators)
                {
                    // カタログの LocatorId を出力する
                    Debug.Log($"LocatorId：{locator.LocatorId}");

                    // カタログに含まれているすべてのキーを出力する
                    var keys = locator.Keys;

                    foreach (var key in keys.Select(c => c.ToString()).OrderBy(c => c))
                    {
                        Debug.Log(key);
                    }
                }
            };
        };
    }
    public void Check()//ダウンロードサイズの確認
    {
        Key = new string[] { "Scene", "Prefab", "DataBase", "Texture", "Font", "Audio" };
        Action<long> onSuccess = (long Result) => { dataSize = Result; CheckLoad(); };

        GetDownloadSizeAsync(Key, onSuccess);
    }
    public static void GetDownloadSizeAsync
    (
    IList<object> key, // ★
    Action<long> onSuccess,
    Action onFailure = null
    )
    {
        void OnComplete(AsyncOperationHandle<long> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                onFailure?.Invoke();
                return;
            }

            onSuccess(handle.Result);
        }

        Addressables.GetDownloadSizeAsync(key).Completed += handle => OnComplete(handle);
    }
    /*-----------------------------------------------------------------------------------------------
    アセットのダウンロード
    ------------------------------------------------------------------------------------------------*/
    public void DownLoad()
    {
        NowDL.text = LocalizationManager.GetTermData("Menu/CatalogLoading").Term;
        //更新データあり、ダウンロード        
        for (int i = 0; i < Key.Count; i++)
        {
            Debug.Log("ダウンロード開始 " + Key[i].ToString());

            //MyAssetラベルの付いたデータをダウンロードする
            AsyncOperationHandle download = Addressables.DownloadDependenciesAsync(Key[i]);

            //ダウンロード中の処理
            StartCoroutine(DownloadWait(download));

            //ダウンロード完了時の処理
            download.Completed += op =>
            {
                if (AsyncOperationStatus.Succeeded == download.Status)
                {
                    //ダウンロード成功
                    Debug.Log("ダウンロード成功");

                    //Addressables.LoadSceneAsync("Home").Completed += _ => { Debug.Log("ロード完了"); };
                    //if (i == Key.Count)
                    Trigger.SendTrigger("LoadHome");
                }
                else
                {
                    errorlog("ErrorCode01");
                    Debug.LogError("失敗");
                }
            };
        }
    }

    async public void CheckLoad()
    {
        Debug.Log("ダウンロードサイズ: " + dataSize);
        if (dataSize > 0)
        {
            var NewDLSize = (dataSize) / 1000000;
            if (NewDLSize == 0) NewDLSize = (dataSize) / 100000;
            //ダウンロードするかしないか、ユーザーに選択する表示を付ける
            DlSize.text +=(NewDLSize + " MB");
            Trigger.SendTrigger("DLInfo");


        }
        else if (dataSize == 0)
        {
            Trigger.SendTrigger("LoadHome");
            NowDL.text = "NowLoading...";

        }
        else
        {
            errorlog("ErrorCode01");
            Debug.LogError("失敗");
        }

    }
    IEnumerator DownloadWait(AsyncOperationHandle download)
    {
        if (Error == true) { Debug.Log(download.PercentComplete); yield break; }
        while (!download.IsDone)
        {
            if (Error == true) { Debug.Log(download.PercentComplete); yield break; }
            //ダウンロード中の処理を書く
            Debug.Log(download.PercentComplete); //※1.1.4ではPercentCompleteは機能していない
            NowDL.text ="Nowloading..." + download.PercentComplete.ToString();
            yield return new WaitForSeconds(0.01f);
        }
    }
    void ImageLoad()
    {
        Debug.Log("事前ロード");
        Image image = GameObject.Find("LoadIcon").GetComponent<Image>();

        Addressables.DownloadDependenciesAsync(List.labelString).Completed += zizenLoad =>
        {
            GameObject.Find("GameStart").GetComponent<ArborFSM>().SendTrigger("LoadHome");
        };
    }

    private async Task<Sprite> LoadSprite(string address)
    {
        var sprite = await Addressables.LoadAssetAsync<Sprite>(address).Task;
        if (sprite != null)
            return sprite;

        Debug.LogError($"{address}がないよ");
        return null;
    }
    /*-----------------------------------------------------------------------------------------------
    アセットのキャッシュクリア
    ------------------------------------------------------------------------------------------------*/
    public void ClearChash()
    {
        var res = Caching.ClearCache();
        if (res)
        {
            Debug.Log("Clear cache!");
        }
        else
        {
            Debug.Log("Cache clear failed.");
        }
    }
    private void errorlog(string Detaile)//エラーログ
    {
        Errorlog.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Detaile;
        Errorlog.GetComponent<ArborFSM>().SendTrigger("Error");

        //ダウンロード中の処理を停止
        Error = true;
    }
}
