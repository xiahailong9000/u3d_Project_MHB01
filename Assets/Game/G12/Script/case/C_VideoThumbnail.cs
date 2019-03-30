using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.IO;
using UnityEngine.UI;

public class C_VideoThumbnail : MonoBehaviour {
    public string videoPath;
    VideoPlayer vp;
    Texture2D videoFrameTexture;
    RenderTexture renderTexture;
    RawImage rawImage;
    void Start() {
        if (videoPath.Length < 5) {
            return;
        }
        rawImage = GetComponent<RawImage>();
        videoFrameTexture = new Texture2D(2, 2);
        vp =gameObject.GetComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.waitForFirstFrame = true;

        vp.sendFrameReadyEvents = true;
        vp.frameReady += S_OnNewFrameEvent;
        vp.Play();
    }
    int framesValue = 0;//获得视频第几帧的图片
    void S_OnNewFrameEvent(VideoPlayer source, long frameIdx) {
        Debug.LogFormat("_VideoPlayer____{0}_", frameIdx);
        framesValue++;
        if (framesValue == 1) {
            renderTexture = source.texture as RenderTexture;
            if (videoFrameTexture.width != renderTexture.width || videoFrameTexture.height != renderTexture.height) {
                videoFrameTexture.Resize(renderTexture.width, renderTexture.height);
            }
            RenderTexture.active = renderTexture;
            videoFrameTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            videoFrameTexture.Apply();
            RenderTexture.active = null;
            vp.frameReady -= S_OnNewFrameEvent;
            vp.sendFrameReadyEvents = false;
            rawImage.texture = renderTexture;
        }
    }

    void OnDisable() {
        if (videoPath.Length < 5) {
            return;
        }
        string path = videoPath + ".pic";
        if (!File.Exists(path)) {
            ScaleTexture(videoFrameTexture, 800, 400, path);
        }
    }
    //生成缩略图
    void ScaleTexture(Texture2D source, int targetWidth, int targetHeight, string savePath) {
        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.ARGB32, false);
        for (int i = 0; i < result.height; ++i) {
            for (int j = 0; j < result.width; ++j) {
                Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                result.SetPixel(j, i, newColor);
            }
        }
        result.Apply();
        File.WriteAllBytes(savePath, result.EncodeToJPG());
    }

}
