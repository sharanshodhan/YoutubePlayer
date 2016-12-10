using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace SSYTViewer
{
	public class VideoEventArgs : EventArgs
	{
		public string videoID;
		public float videoLength;

		public float timeSpent;		// Total time spent watching the video -> does not include buffering time, pause time, etc.
		public string timeStarted;
		public string timeEnded;
	}

	public class VideoHelper : MonoBehaviour
	{
		private static VideoHelper m_Instance;
		public static VideoHelper instance {
			get {
				return m_Instance;
			}
		}

		private string videoID = "";
		private float timeSpent = 0f;
		private string timeStarted;
		private string timeEnded;

		private bool videoPlaying = false;
		private bool videoEnded = false;

		private float videoLength;
		private string _fileName = "SSYTViewer/youtube.html";

		public EventHandler VideoEnded;

		#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		private UniWebView _webView;
		#endif

		#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8

		void Awake() {
			m_Instance = this;
		}
		public void OpenVideoWithUrl (string url)
		{
			if (_webView != null) {
				return;
			}
				
			timeStarted = System.DateTime.Now.ToUniversalTime().ToString ("yyyy-MM-ddTHH:mm:ss");

			videoEnded = false;
			videoPlaying = false;
			timeSpent = 0f;
			videoLength = -1f;

			var splitArr = url.Split (new string[] { "https://www.youtube.com/embed/" }, System.StringSplitOptions.None);
			if (splitArr.Length == 1) {
				splitArr = url.Split (new string[] { "https://www.youtube.com/watch?v=" }, System.StringSplitOptions.None);
			}
			if (splitArr.Length == 1) {
				return;
			}
			videoID = splitArr [1];

			_webView = CreateWebView ();
			_webView.url = UniWebViewHelper.streamingAssetURLForPath (_fileName);

			int bottomInset = UniWebViewHelper.screenHeight;
			_webView.insets = new UniWebViewEdgeInsets (0, 0, 40, 0);

			_webView.OnEvalJavaScriptFinished += OnEvalJavaScriptFinished;
			_webView.OnLoadComplete += _webView_OnLoadComplete;

			_webView.OnReceivedMessage += OnReceivedMessage;

			_webView.OnWebViewShouldClose += OnShouldClose;

			_webView.Load ();
			_webView.Show ();

		}

		bool OnShouldClose (UniWebView webView)
		{
			videoPlaying = false;
			CloseWebView ();
			return false;
		}

		void OnReceivedMessage (UniWebView webView, UniWebViewMessage message)
		{

			if (message.path == "close") {
				Destroy (_webView);
				_webView = null;
			}
			if (message.path == "ytParams") {
				string duration = message.args ["duration"];
				videoLength = float.Parse (duration);
			}
			if (message.path == "ytEvent") {
				string state = message.args ["state"];
				if (state == "playing") {
					videoPlaying = true;
				} else if (state == "buffering") {
					videoPlaying = false;
					#if UNITY_EDITOR
						StartCoroutine(DummyVideoEnd());		
					#endif
				} else if (state == "ended") {
					videoPlaying = false;
					videoEnded = true;
					CloseWebView ();
				} else if (state == "paused") {
					videoPlaying = false;
				}
			}
		}

		IEnumerator DummyVideoEnd() {
			yield return new WaitForSeconds (5f);
			videoPlaying = false;
			CloseWebView ();
		}

		void CloseWebView ()
		{
			timeEnded = System.DateTime.Now.ToUniversalTime().ToString ("yyyy-MM-ddTHH:mm:ss");
			Destroy (_webView);
			_webView = null;

			if (VideoEnded != null) {
				VideoEventArgs videoEventArgs = new VideoEventArgs ();
				videoEventArgs.videoID = videoID;
				videoEventArgs.timeSpent = timeSpent;
				videoEventArgs.timeStarted = timeStarted;
				videoEventArgs.timeEnded = timeEnded;
				videoEventArgs.videoLength = videoLength;
				VideoEnded.Invoke (this, videoEventArgs);
			}
		}

		void OnEvalJavaScriptFinished (UniWebView webView, string r)
		{
			Debug.Log ("Javascript eval finished with " + r);
		}

		void _webView_OnLoadComplete (UniWebView webView, bool success, string errorMessage)
		{
			_webView.EvaluatingJavaScript ("loadVideo('" + videoID + "')");
		}

		UniWebView CreateWebView ()
		{
			var webViewGameObject = GameObject.Find ("WebView");
			if (webViewGameObject == null) {
				webViewGameObject = new GameObject ("WebView");
				webViewGameObject.transform.SetParent (GameObject.Find ("Main Camera").transform.parent);
			}

			var webView = webViewGameObject.AddComponent<UniWebView> ();
			webView.toolBarShow = true;
			return webView;
		}
		#else
	void Awake() {
		m_Instance = this;
	}
	public void OpenVideoWithUrl(string url) {
			Debug.Log ("Cannot open Video in Editor");
	}
	#endif

		void Update () {
			if (videoPlaying) {
				timeSpent += Time.deltaTime;
			}
		}
	}


}