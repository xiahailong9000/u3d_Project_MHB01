using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CC_Util;
using UnityEngine;
using System.Linq;

public class C_AssetsLoad {
    public static C_AssetsLoad GetInstance {
        get {
            if (instance == null) {
                instance = new C_AssetsLoad();
            }
            return instance;
        }
    }
    static C_AssetsLoad instance;
    MonoBehaviour mono;
    public void S_GetAssets(MonoBehaviour mono, Action<List<Dictionary<string, int>>> callback) {
        this.mono = mono;
        mono.StartCoroutine(I_GetAssets2(callback));
    }
    [Obsolete("第一版本方法")]
    IEnumerator I_GetAssets(Action<Dictionary<string, int>> callback) {
        string sourcePath = Application.dataPath + "/../FilePackage";
        Dictionary<string, int> assetsPathDic = new Dictionary<string, int>();
        FileInfo[] fileInfos = new DirectoryInfo(sourcePath).GetFiles("*.*", SearchOption.AllDirectories);
        for (int i = 0; i < fileInfos.Length; i++) {
            // Debug.LogFormat("____{0}____{1}___{2}", fileInfos[i].Name, fileInfos[i].Extension, fileInfos[i].FullName);
            string extension = fileInfos[i].Extension.ToLower();
            int ii = 0;
            switch (extension) {
                case ".jpg":
                    ii = 1;
                    assetsPathDic[fileInfos[i].FullName] = ii;
                    break;
                case ".png":
                    ii = 2;
                    assetsPathDic[fileInfos[i].FullName] = ii;
                    break;
                case ".mp4":
                    ii = 33;
                    assetsPathDic[fileInfos[i].FullName] = ii;
                    break;
                case ".avi":
                    ii = 34;
                    assetsPathDic[fileInfos[i].FullName] = ii;
                    break;
            }
            if (ii == 1 || ii == 2) {
                if (false) {
                    if (fileInfos[i].Length / 1024 > limitI + 5) {
                        Debug.LogFormat("压缩____{0}__{1}__", fileInfos[i].Length / 1024, fileInfos[i].FullName);
                        yield return mono.StartCoroutine(I_Compress(fileInfos[i].FullName, delegate (Texture2D tex) {
                            if (tex != null) {
                                S_Thumbnail(fileInfos[i].FullName, tex);
                            }
                        }));
                    }
                }
                if (File.Exists(fileInfos[i].FullName + ".pic") == false) {
                    yield return mono.StartCoroutine(I_Thumbnail(fileInfos[i].FullName));
                }
            }
        }
        if (callback != null) {
            callback(assetsPathDic);
        }
    }
    IEnumerator I_GetAssets2(Action<List<Dictionary<string, int>>> callback) {
        string sourcePath = Application.dataPath + "/../FilePackage";
        List<Dictionary<string, int>> assetsPathDicList = new List<Dictionary<string, int>>();
        DirectoryInfo[] directoryInfos = new DirectoryInfo(sourcePath).GetDirectories("*",SearchOption.AllDirectories);
        for (int j = 0; j < directoryInfos.Length; j++) {
            Dictionary<string, int> assetsPathDic = new Dictionary<string, int>();
            FileInfo[] fileInfos = directoryInfos[j].GetFiles("*.*");
            for (int i = 0; i < fileInfos.Length; i++) {
                // Debug.LogFormat("____{0}____{1}___{2}", fileInfos[i].Name, fileInfos[i].Extension, fileInfos[i].FullName);
                string extension = fileInfos[i].Extension.ToLower();
                int ii = 0;
                switch (extension) {
                    case ".jpg":
                        ii = 1;
                        assetsPathDic[fileInfos[i].FullName] = ii;
                        break;
                    case ".png":
                        ii = 2;
                        assetsPathDic[fileInfos[i].FullName] = ii;
                        break;
                    case ".mp4":
                        ii = 33;
                        assetsPathDic[fileInfos[i].FullName] = ii;
                        break;
                    case ".avi":
                        ii = 34;
                        assetsPathDic[fileInfos[i].FullName] = ii;
                        break;
                }
                if (ii == 1 || ii == 2) {
                    if (false) {
                        if (fileInfos[i].Length / 1024 > limitI + 5) {
                            Debug.LogFormat("压缩____{0}__{1}__", fileInfos[i].Length / 1024, fileInfos[i].FullName);
                            yield return mono.StartCoroutine(I_Compress(fileInfos[i].FullName, delegate (Texture2D tex) {
                                if (tex != null) {
                                    S_Thumbnail(fileInfos[i].FullName, tex);
                                }
                            }));
                        }
                    }
                    if (File.Exists(fileInfos[i].FullName + ".pic") == false) {
                        yield return mono.StartCoroutine(I_Thumbnail(fileInfos[i].FullName));
                    }
                }
            }
            if (assetsPathDic.Count > 0) {
                assetsPathDic = assetsPathDic.OrderBy(n => n.Key).ToDictionary(n => n.Key, n => n.Value);
                assetsPathDicList.Add(assetsPathDic);
            }
        }
        if (callback != null) {
            callback(assetsPathDicList);
        }
    }
    int limitI = 2048;
    /// <summary>
    /// 压缩
    /// </summary>
    IEnumerator I_Compress(string imagePath, Action<Texture2D> action) {
        int qualityI = 100;
        WWW www = new WWW("file:///" + imagePath);
        yield return www;
        if (www.error != null) {
            if (action != null) {
                action(null);
            }
            Debug.LogErrorFormat("缩略图生成失败__{0}__{1}".S_SetColor("ff0000"), www.error, imagePath);
        } else {
            Texture2D t2d = www.texture;
            byte[] b = t2d.EncodeToJPG(qualityI);
            //Debug.Log( "图原始读取的字节数 " + (b.Length/1000).ToString());
            while ((b.Length / 1024) >= limitI) {
                qualityI -= 5;
                b = t2d.EncodeToJPG(qualityI);
                //Debug.Log ("当前大小："+b.Length/1000);
            }
            Debug.LogFormat ("压缩成功，当前大小__{0}____{1}".S_SetColor("00ff00"),b.Length/1000, imagePath);
            File.WriteAllBytes(imagePath, b);
            if (action != null) {
                action(t2d);
            }
        }
    }
    int limitI2 = 80;
    /// <summary>
    /// 缩略图
    /// </summary>
    IEnumerator I_Thumbnail(string imagePath) {
        WWW www = new WWW("file:///" + imagePath);
        yield return www;
        if (www.error != null) {
            //action(null);
            Debug.LogErrorFormat("缩略图生成失败__{0}__{1}".S_SetColor("ff0000"), www.error, imagePath);
        } else {
            Texture2D t2d = www.texture;
            S_Thumbnail(imagePath,t2d);
        }
    }
    public static void S_WWW(MonoBehaviour mono,string url,Action<WWW> callback) {
        mono.StartCoroutine(I_WWW(url, callback));
    }
    static IEnumerator I_WWW( string url, Action<WWW> callback) {
        WWW www = new WWW(url);
        yield return www;
        if (www.error != null) {
            if (callback != null) {
                callback(null);
            }
        } else {
            if (callback != null) {
                callback(www);
            }
        }
    }
    public static int thumbnailLenght=512;
    public static void S_Thumbnail(string imagePath,Texture2D t2d) {
        int qualityI = 30;
        Texture2D nn;
        if (t2d.width >= t2d.height) {
            nn = ScaleTexture(t2d, thumbnailLenght, t2d.height * thumbnailLenght / t2d.width);
        } else {
            nn = ScaleTexture(t2d, t2d.width * thumbnailLenght / t2d.height, thumbnailLenght);
        }
        byte[] b = nn.EncodeToJPG(qualityI);
        Debug.LogFormat("缩略图_压缩成功，当前大小={0}____path={1}".S_SetColor("00ff00"), b.Length / 1000, imagePath);
        File.WriteAllBytes(imagePath + ".pic", b);
    }

    public static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight) {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);

        float incX = (1.0f / (float)targetWidth);
        float incY = (1.0f / (float)targetHeight);

        for (int i = 0; i < result.height; ++i) {
            for (int j = 0; j < result.width; ++j) {
                Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                result.SetPixel(j, i, newColor);
            }
        }
        result.Apply();
        return result;
    }
}
