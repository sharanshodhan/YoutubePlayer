//  Copyright 2016 MaterialUI for Unity http://materialunity.com
//  Please see license file for terms and conditions of use, and more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

namespace SSYTViewer
{
	public class SSYTDemo : MonoBehaviour
	{
		void Start()
		{
			print (VideoHelper.instance);
			VideoHelper.instance.VideoEnded += CloseWebView;
			VideoHelper.instance.OpenVideoWithUrl ("https://www.youtube.com/embed/9JJygm2KeQw");
		}

		void CloseWebView(object sender, System.EventArgs e) {

			Debug.Log ("Web view Closed");
			Debug.Log ("VideoID = " + (e as VideoEventArgs).videoID);
			Debug.Log ("Video Length (s) = " + (e as VideoEventArgs).videoLength);
			Debug.Log ("Time Started (YYYY-MM-DDThh:mm:ss) = " + (e as VideoEventArgs).timeStarted);
			Debug.Log ("Time Ended (YYYY-MM-DDThh:mm:ss) = " + (e as VideoEventArgs).timeEnded);
			Debug.Log ("Time Spent (s) = " + (e as VideoEventArgs).timeSpent);

			VideoHelper.instance.VideoEnded -= CloseWebView;
		}
	}
}