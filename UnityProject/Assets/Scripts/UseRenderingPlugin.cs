// on OpenGL ES there is no way to query texture extents from native texture id
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
	#define UNITY_GLES_RENDERER
#endif


using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;


public class UseRenderingPlugin : MonoBehaviour
{
	// Native plugin rendering events are only called if a plugin is used
	// by some script. This means we have to DllImport at least
	// one function in some active script.
	// For this example, we'll call into plugin's SetTimeFromUnity
	// function and pass the current time so the plugin can animate.

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport ("RenderingPlugin")]
#endif
	private static extern void SetTimeFromUnity(float t);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport ("RenderingPlugin")]
#endif
	private static extern Boolean OpenWebCam(int index);


//destroying web cam
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport ("RenderingPlugin")]
#endif
	private static extern void DestroyWebCam();

//native function for get remap parameters for shader remap
#if UNITY_IPHONE && !UNITY_EDITOR
[DllImport ("__Internal")]
#else
    [DllImport("RenderingPlugin", EntryPoint = "get_pose")]
#endif
	private static extern IntPtr get_pose();



	// We'll also pass native pointer to a texture in Unity.
	// The plugin will fill texture data from native code.
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport ("RenderingPlugin")]
#endif
#if UNITY_GLES_RENDERER
	private static extern void SetTextureFromUnity(System.IntPtr texture, int w, int h);
#else
	private static extern void SetTextureFromUnity(System.IntPtr texture);
#endif


#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("RenderingPlugin")]
#endif
	private static extern void SetUnityStreamingAssetsPath([MarshalAs(UnmanagedType.LPStr)] string path);


#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("RenderingPlugin")]
#endif
	private static extern IntPtr GetRenderEventFunc();

	private int width = 640;
	private int height = 480;

	private Texture2D texture;


	private float[] Pose = new float[16];
	private Matrix4x4 W2C_matrix;

	public Boolean init_success = false;
	//public UseRenderingPlugin_right render_sc_right;

	private GameObject SingleCamera;
	private GameObject ArObject;
    private GameObject Background;
    void Start()
	{
        SingleCamera = GameObject.Find("Camera");
        Camera cam = Camera.main;
        Background = GameObject.Find("Plane");
        ArObject = GameObject.Find("Sphere");
        cam.aspect = width / height;
        cam.orthographic = true;

        OpenWebCam(-1);
        texture = new Texture2D(width, height, TextureFormat.RGFloat, false);
        Material mat = GetComponent<Renderer>().material;
        mat.SetTexture("_Texture2", texture);
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                Color color;

                color.r = ((float)i-0.0f)/(float)width;
                color.g = ((float)j-0.0f)/(float)height;
                color.b = 1.0f;
                color.a = 1.0f;
                texture.SetPixel(i, j, color);
            }
        }
        texture.Apply();

        SetUnityStreamingAssetsPath(Application.streamingAssetsPath);

		CreateTextureAndPassToPlugin();
        //	yield return StartCoroutine("CallPluginAtEndOfFrames");
        InvokeRepeating("CallPluginAtEndOfFrames", 0.0f, 1.0f / 5.0f);
        //yield return;  // 30 fps
    }

	private void CreateTextureAndPassToPlugin()
	{
        // NOTE: https://en.wikipedia.org/wiki/Display_resolution

        // Create a texture
        Texture2D tex = new Texture2D(width,height, TextureFormat.ARGB32, false);
		// Set point filtering just so we can see the pixels clearly
		//tex.wrapMode = TextureWrapMode.Clamp;
		tex.filterMode = FilterMode.Point;
		// Call Apply() so it's actually uploaded to the GPU
		tex.Apply();

		// Set texture onto our matrial
		GetComponent<Renderer>().material.mainTexture = tex;

		// Pass texture pointer to the plugin
	#if UNITY_GLES_RENDERER
		SetTextureFromUnity (tex.GetNativeTexturePtr(), tex.width, tex.height);
	#else
		SetTextureFromUnity (tex.GetNativeTexturePtr());
	#endif
	}

    public static Matrix4x4 LHMatrixFromRHMatrix(Matrix4x4 rhm)
    {
        Matrix4x4 lhm = new Matrix4x4(); ;

        // Column 0.
        lhm[0, 0] = rhm[0, 0];
        lhm[1, 0] = rhm[1, 0];
        lhm[2, 0] = -rhm[2, 0];
        lhm[3, 0] = rhm[3, 0];

        // Column 1.
        lhm[0, 1] = rhm[0, 1];
        lhm[1, 1] = rhm[1, 1];
        lhm[2, 1] = -rhm[2, 1];
        lhm[3, 1] = rhm[3, 1];

        // Column 2.
        lhm[0, 2] = -rhm[0, 2];
        lhm[1, 2] = -rhm[1, 2];
        lhm[2, 2] = rhm[2, 2];
        lhm[3, 2] = -rhm[3, 2];

        // Column 3.
        lhm[0, 3] = rhm[0, 3];
        lhm[1, 3] = rhm[1, 3];
        lhm[2, 3] = -rhm[2, 3];
        lhm[3, 3] = rhm[3, 3];

        return lhm;
    }

    //get position from transform matrix
    public static Vector3 PositionFromMatrix(Matrix4x4 m)
    {
        return m.GetColumn(3);
    }

    //get rotation quaternion from matrix
    public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        // Trap the case where the matrix passed in has an invalid rotation submatrix.
        if (m.GetColumn(2) == Vector4.zero)
        {
            Debug.Log("QuaternionFromMatrix got zero matrix.");
            return Quaternion.identity;
        }
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }


	private void CallPluginAtEndOfFrames()
	{
		//while (true) {
            
            // todo: 30 FPS playback, or jsut be able to control play back speed
            //System.Threading.Thread.Sleep(100);
			//StereoCamera.transform.Rotate(Vector3.up * 50f * Time.deltaTime);

			//float time1 = Time.realtimeSinceStartup;
			// Wait until all frame rendering is done
			//yield return new WaitForEndOfFrame();

			// Set time for the plugin
			SetTimeFromUnity (Time.timeSinceLevelLoad);

			// Issue a plugin event with arbitrary integer identifier.
			// The plugin can distinguish between different
			// things it needs to do based on this ID.
			// For our simple plugin, it does not matter which ID we pass here.
			GL.IssuePluginEvent(GetRenderEventFunc(), 0);

            IntPtr PositionPtr = get_pose();
            Marshal.Copy(PositionPtr, Pose, 0, 16);
            W2C_matrix.m00 = (float)Pose[0];
            W2C_matrix.m01 = (float)Pose[1];
            W2C_matrix.m02 = (float)Pose[2];
            W2C_matrix.m03 = (float)Pose[3];
            W2C_matrix.m10 = (float)Pose[4];
            W2C_matrix.m11 = (float)Pose[5];
            W2C_matrix.m12 = (float)Pose[6];
            W2C_matrix.m13 = (float)Pose[7];
            W2C_matrix.m20 = (float)Pose[8];
            W2C_matrix.m21 = (float)Pose[9];
            W2C_matrix.m22 = (float)Pose[10];
            W2C_matrix.m23 = (float)Pose[11];
            W2C_matrix.m30 = (float)Pose[12];
            W2C_matrix.m31 = (float)Pose[13];
            W2C_matrix.m32 = (float)Pose[14];
            W2C_matrix.m33 = (float)Pose[15];
            Debug.Log(W2C_matrix);
            Matrix4x4 transformationMatrix = LHMatrixFromRHMatrix(W2C_matrix);
            Matrix4x4 pose = transformationMatrix;

            Vector3 arPosition = PositionFromMatrix(pose);
            Quaternion arRotation = QuaternionFromMatrix(pose);

            SingleCamera.transform.localPosition = arPosition;
            SingleCamera.transform.localRotation = arRotation;
            SingleCamera.transform.Rotate(Vector3.up * 180f);
            SingleCamera.transform.Rotate(Vector3.forward * 180f);
		//}
	}

	void OnApplicationQuit()
	{
		DestroyWebCam ();
		Debug.Log("quit");
	}

	public void OnClickInit()
	{
		Debug.Log ("click!click!click!click!click!click!click!click!click!click!");
		//start_slam ();
	}

	public void OnClickReset()
	{
		Debug.Log ("reset!");
		//reset_slam ();
	}
}
