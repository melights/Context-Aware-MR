/*
* Autor : flankechen (flanke.chen@visionertech.com flankechen@gmail.com)
* this is the implementation based on opencv. and capturing video source data for Unity Native rendering plugin
* 
*
* INSTALLATION :
* - make sure you have correctly installed opencv(I work with 2.4.11)
* - 
* - 
LINKFLAGS = 
*	- 
* - Compile the project
* - Enjoy !
*
* Notice this code define two constants for the image width and height (OPENCV_VIDEO_W and OPENCV_VIDEO_H)
*/


#include "VideoCap.h"
#include <fstream>
#include <iostream>
#include <sstream>
#include <limits>
//#include <windows.h>
#undef max
using namespace std;
using namespace cv;
//static HWND s_hConsole = NULL;
//defalut constructor
VideoSource::VideoSource()
{

}

//constructor index_0/1 for left and right camera
bool VideoSource::open_webcam(int index)
{
	// s_hConsole = GetConsoleWindow();
	// if (s_hConsole == NULL)
	// {
	// 	AllocConsole();
	// 	s_hConsole = GetConsoleWindow();
	// }
	
	// if (s_hConsole != NULL)
	// {
	// 	freopen("CONIN$", "r", stdin);
	// 	freopen("CONOUT$", "w", stdout);
	// 	freopen("CONOUT$", "w", stderr);
	// }

	
	index_g = index;
	if (index_g == -1)
	{
		cv::Mat image;
		char filename_img[500];
		char filename_pos[500];
		//poses.reserve(451);
		for (int i = 1; i <= IMAGE_NUM; i++)
		{
			sprintf(filename_img, "/home/long/ar/UnityProject/rgb/%d.jpg", i);
			sprintf(filename_pos, "/home/long/ar/UnityProject/rgb/%d.txt", i);
			image = cv::imread(filename_img);
			images.push_back(image.clone());
			readCamPoseFile(filename_pos);
		}
		image_count = 0;
		adder = 1;
		std::cout << "Load Images Done!!" << std::endl;
		return true;
	}
	cap.open(index);
	cap.set(CV_CAP_PROP_FOURCC, CV_FOURCC('M', 'J', 'P', 'G'));
	cap.set(CV_CAP_PROP_FRAME_WIDTH, OPENCV_VIDEO_W);
	cap.set(CV_CAP_PROP_FRAME_HEIGHT, OPENCV_VIDEO_H);

	if (cap.isOpened())
	{
		return true;
	}
	else
	{
		return false;
	}
}

std::ifstream& VideoSource::GotoLine(std::ifstream& file, unsigned int num)
{
	file.seekg(std::ios::beg);
	for (int i = 0; i < num - 1; ++i)
	{
		file.ignore(std::numeric_limits<std::streamsize>::max(), '\n');
	}
	return (file);
}

bool VideoSource::readCamPoseFile(std::string filename)
{
	// ifstream myReadFile;
	// myReadFile.open(filename.c_str (), ios::in);
	// if(!myReadFile.is_open ())
	// {
	// 	return false;
	// }
	// myReadFile.seekg(ios::beg);

	// float val;
	std::vector<float> pos(16);
	// //pos.reserve(16);
	// // go to line 2 to read translations
	// GotoLine(myReadFile, 2);
	// myReadFile >> val; pos[3] = val; //TX
	// myReadFile >> val; pos[7] = val; //TY
	// myReadFile >> val; pos[11] = val; //TZ

	// // go to line 7 to read rotations
	// GotoLine(myReadFile, 7);

	// myReadFile >> val; pos[0] = val;
	// myReadFile >> val; pos[1] = val;
	// myReadFile >> val; pos[2] = val;

	// myReadFile >> val; pos[4] = val;
	// myReadFile >> val; pos[5] = val;
	// myReadFile >> val; pos[6] = val;

	// myReadFile >> val; pos[8] = val;
	// myReadFile >> val; pos[9] = val;
	// myReadFile >> val; pos[10] = val;

	// pos[12] = 0.0;
	// pos[13] = 0.0;
	// pos[14] = 0.0;
	// pos[15] = 1.0; //Scale

	// // close file
	// myReadFile.close();
	Mat TVector, RMatrix;
	FileStorage fs(filename.c_str (), FileStorage::READ);
	fs["TVector"] >> TVector;
	fs["RMatrix"] >> RMatrix;
	fs.release();
	pos[3]=TVector.at<float>(0,0);
	pos[7]=TVector.at<float>(0,1);
	pos[11]=TVector.at<float>(0,2);
	pos[0]=RMatrix.at<float>(0,0);
	pos[1]=RMatrix.at<float>(0,1);
	pos[2]=RMatrix.at<float>(0,2);
	pos[4]=RMatrix.at<float>(1,0);
	pos[5]=RMatrix.at<float>(1,1);
	pos[6]=RMatrix.at<float>(1,2);
	pos[8]=RMatrix.at<float>(2,0);
	pos[9]=RMatrix.at<float>(2,1);
	pos[10]=RMatrix.at<float>(2,2);
	pos[12] = 0.0;
	pos[13] = 0.0;
	pos[14] = 0.0;
	pos[15] = 1.0; //Scale
	poses.push_back(pos);

	return true;

}

bool VideoSource::read_calib()
{
#ifdef CAMERA_FLIP
	mx1.create(OPENCV_VIDEO_W, OPENCV_VIDEO_H, CV_16S);
	my1.create(OPENCV_VIDEO_W, OPENCV_VIDEO_H, CV_16S);
	mx2.create(OPENCV_VIDEO_W, OPENCV_VIDEO_H, CV_16S);
	my2.create(OPENCV_VIDEO_W, OPENCV_VIDEO_H, CV_16S);
#endif

//	FileStorage fs("./save_param/calib_para.yml", CV_STORAGE_READ);
//	if (fs.isOpened())
//	{
//		fs["MX1"] >> mx1;
//		fs["MX2"] >> mx2;
//		fs["MY1"] >> my1;
//		fs["MY2"] >> my2;
	mx1=0.77; 
	mx2=0.62;
	my1=0.548;
	my2=0.510 ;
		cv::convertMaps(mx1, my1, mx1_f, my1_f, CV_32FC1);
		cv::convertMaps(mx2, my2, mx2_f, my2_f, CV_32FC1);
		
		//fs.release();
		return true;
//	}
//	else return false;
	

	
}

//destructor
VideoSource::~VideoSource()
{
	cap.release();
	cap_right.release();
}

float* VideoSource::get_pose()
{
	std::cout << image_count<<"~" << std::endl;
	for (int i = 0; i < 16; i++){
		std::cout << poses[image_count][i] << ":";
	}
	std::cout <<std::endl;

	return poses[image_count].data();

}
Mat VideoSource::get_rgba()
{
	cv::Mat src_rgba;
	if (index_g == -1)
	{
		if (image_count >= 450)
			adder = -1;
		else if (image_count <= 0)
			adder = 1;
		image_count += adder;
		src=images[image_count];
	} else {
		cap >> src;
	}
	
	//flip(src.t(), src, 0);
	//imshow("src_flip", src);
	//waitKey(30);
	//cv::remap(src, frame_rectify, mx1, my1, CV_INTER_LINEAR);
	//imshow("frame_rectify", frame_rectify);
	//waitKey(30);

	//flip(frame_rectify, frame_rectify, 1);
	
	cv::cvtColor(src, src_rgba, CV_BGR2RGBA);
	return src_rgba;
}


