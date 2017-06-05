#pragma once

#ifndef __VIDEOSOURCE_H
#define __VIDEOSOURCE_H

#define OPENCV 1
#if OPENCV
#endif

#if OPENCV
#include <opencv2/opencv.hpp>
#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc.hpp>

//#include "PTAM\PTAM\definition.h"

////image size for ar mod rendering
////current PTAM-demo machine camera setup is flipped 90.
#define OPENCV_VIDEO_W 1080
#define OPENCV_VIDEO_H 1080
#define IMAGE_NUM 450
//
////image size for tracking, usually smaller for faster frame rate.
//#define TRACK_IMAGE_W 540
//#define TRACK_IMAGE_H 540
//
////#define LOCAL_VIDEO
//#ifdef LOCAL_VIDEO
////#define FRAME_BY_FRAME
//#endif
//
////camera fliped 90 for current 
////#define CAMERA_FLIP
#include<vector>

class VideoSource
{
public:
	//members
	int index_g;
	int image_count;
	int adder;
	cv::VideoCapture cap;
	cv::VideoCapture cap_right;

	std::vector<cv::Mat> images;
	std::vector<std::vector<float>> poses;

	cv::Mat src;
	cv::Mat src_right;

	cv::Mat src_flip;
	cv::Mat src_flip_right;

	cv::Mat frame_rectify;
	cv::Mat frame_rectify_right;
	
#if TRACK_IMAGE_W != OPENCV_VIDEO_W
	cv::Mat frame_rectify_down;
	cv::Mat frame_rectify_right_down;
#endif

	cv::Mat mx1, my1, mx2, my2;
	cv::Mat mx1_f, my1_f, mx2_f, my2_f;

	//public functions
	VideoSource();
	bool open_webcam(int index);
	bool read_calib();
	bool readCamPoseFile(std::string filename);
	std::ifstream& GotoLine(std::ifstream& file, unsigned int num);
	~VideoSource();
	cv::Mat get_rgba();
	float* get_pose();
};


#endif


#endif
