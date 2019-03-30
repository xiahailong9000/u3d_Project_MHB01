using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// 图片压缩工具
/// </summary>
public class CompressPictureHelper : MonoBehaviour {

    private static CompressPictureHelper _instance;

    public static CompressPictureHelper Instance {
        get {
            if (_instance == null) {  // 如果没有找到
                GameObject go = new GameObject("_CompressPictureHelper"); // 创建一个新的GameObject
                DontDestroyOnLoad(go);  // 防止被销毁
                _instance = go.AddComponent<CompressPictureHelper>(); // 将实例挂载到GameObject上
            }
            return _instance;
        }
    }

    /// <summary>
    /// 图片的限制大小
    /// </summary>
    int limitI = 800;

    /// <summary>
    /// Compresses the picture.
    /// </summary>
    /// <param name="imagePath">Image path.</param>
    /// <param name="action">成功返回原地址，失败返回null.</param>
    public void CompressPicture(string imagePath, Action<string> action) {
        if (File.Exists(imagePath)) {
            FileInfo f = new FileInfo(imagePath);

            if ((f.Length / 1024) >= limitI) {
                //Debug.Log ("开始压缩，图片原始大小为：" + f.Length/1000+"Kb");
                StartCoroutine(Compress(imagePath, delegate (string str) {
                    action(str);
                }));
            }
        }
    }

    IEnumerator Compress(string imagePath, Action<string> action) {
        int qualityI = 100;
        WWW www = new WWW("file:///" + imagePath);
        yield return www;
        if (www.error != null) {
            action(null);
            //发返回失败
        } else {
            Texture2D t2d = www.texture;
            byte[] b = t2d.EncodeToJPG(qualityI);
            //Debug.Log( "图原始读取的字节数 " + (b.Length/1000).ToString());
            while ((b.Length / 1024) >= limitI) {
                qualityI -= 5;
                b = t2d.EncodeToJPG(qualityI);
                //Debug.Log ("当前大小："+b.Length/1000);
            }
            //Debug.Log ("压缩成功，当前大小："+b.Length/1000);
            File.WriteAllBytes(imagePath, b);
            action(imagePath);
        }
    }
}