///////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2017, STEREOLABS.
//
// All rights reserved.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
///////////////////////////////////////////////////////////////////////////


/******************************************************************************************
 ** This sample demonstrates a simple way to use the ZED as a positional tracker and     **
 **  show the result in a OpenGL window.                                                 **
 ** Even if the depth/images can be retrieved (grab is done in STANDARD mode),           **
 **  we save here the "printing" time to be more efficient.                              **
 ******************************************************************************************/

// Standard includes
#include <stdio.h>
#include <string.h>
#include <iostream>
#include <fstream>
 
// ZED includes
#include <sl/Camera.hpp>

// Sample includes
#include "TrackingViewer.hpp"


//// Using std namespace
using namespace std;

//// Create ZED object (camera, callback, pose)
sl::Camera zed; //ZED Camera
sl::Pose camera_pose; //ZED SDK pose to handle the position of the camera in space.

// OpenGL Window to display the ZED in world space
TrackingViewer viewer;

// CSV log file to store Motion Tracking data (with timestamp)
std::string txtName;

//// Sample functions
void run();
void transformPose(sl::Transform &pose, float tx);
void parse_args(int argc, char **argv, sl::InitParameters &initParameters);

int main(int argc, char **argv) {

	// Setup configuration parameters for the ZED
    sl::InitParameters initParameters;
    initParameters.camera_resolution = sl::RESOLUTION_HD720;
    initParameters.depth_mode = sl::DEPTH_MODE_PERFORMANCE;
    initParameters.coordinate_units = sl::UNIT_METER;
    initParameters.coordinate_system = sl::COORDINATE_SYSTEM_RIGHT_HANDED_Y_UP;
	initParameters.sdk_verbose = true;

	parse_args(argc, argv, initParameters);

	// Open the ZED
    sl::ERROR_CODE err = zed.open(initParameters);
    if (err != sl::SUCCESS) {
        cout << sl::errorCode2str(err) << endl;
        zed.close();
        return 1; // Quit if an error occurred
    }

	// Initialize motion tracking parameters
    sl::TrackingParameters trackingParameters;
    trackingParameters.initial_world_transform = sl::Transform::identity();
    trackingParameters.enable_spatial_memory = true;

	// Enable motion tracking
    zed.enableTracking(trackingParameters);

	run();

	zed.close();
    return 0;
}

/**
*  This function loops to get images and data from the ZED. It can be considered as a callback.
*  You can add your own code here.
**/
void run() {

	float tx, ty, tz = 0;
	float rx, ry, rz = 0;

	// Get the translation from the left eye to the center of the camera
	float camera_left_to_center = zed.getCameraInformation().calibration_parameters.T.x *0.5f;

	// Create text for GUI
	char text_rotation[256];
	char text_translation[256];

	char output_translation[256];
	char output_rotationMatrix[512];

	sl::Mat zed_image;

	// loop until quit flag has been set to true
	int frameNumber = 0;

	char key = ' ';
	while (key != 'q') {

		key = cv::waitKey(5);

		if (zed.grab() == sl::SUCCESS)
		{
			// Get camera position in World frame
			sl::TRACKING_STATE tracking_state = zed.getPosition(camera_pose, sl::REFERENCE_FRAME_WORLD);

			zed.retrieveImage(zed_image, sl::VIEW_LEFT);

			cv::Mat cvMat = cv::Mat(zed_image.getHeight(), zed_image.getWidth(), CV_8UC4, zed_image.getPtr<sl::uchar1>(sl::MEM_CPU));

			cv::imshow("VIEW", cvMat);


			// Get motion tracking confidence
			int tracking_confidence = camera_pose.pose_confidence;

			if (tracking_state == sl::TRACKING_STATE_OK)
			{
				// Extract 3x1 rotation from pose
				//sl::Vector3<float> rotation = camera_pose.getRotationVector();
				//rx = rotation.x;
				//ry = rotation.y;
				//rz = rotation.z;

				// Extract translation from pose
				sl::Vector3<float> translation = camera_pose.getTranslation();
				tx = translation.tx;
				ty = translation.ty;
				tz = translation.tz;

				// Create text file for each frame
				std::string txtFN = std::to_string(frameNumber) + ".txt";
				std::string pngFN = std::to_string(frameNumber) + ".png";

				ofstream outputFile;
				outputFile.open(txtFN);

				if (!outputFile.is_open())
				{
					cout << "ERROR: Can't create txt files. Launch the sample with Administrator rights" << endl;
				}
				else
				{
					// Fill Output Text
					sprintf(output_translation, "%3.2f\n %3.2f\n %3.2f\n", tx, ty, tz);

					// Extract 3x3 rotation from pose
					sl::Rotation rotMat = camera_pose.getRotation();

					sprintf(output_rotationMatrix, "%3.2f %3.2f %3.2f\n%3.2f %3.2f %3.2f\n%3.2f %3.2f %3.2f\n",
						rotMat.r[0], rotMat.r[1], rotMat.r[2],
						rotMat.r[3], rotMat.r[4], rotMat.r[5],
						rotMat.r[6], rotMat.r[7], rotMat.r[8]
					);

					// NOTE: There is more camera info that can be used
					// Unity SDK seems to grab stuff from the left cam usually

					// Grab camera info
					//sl::CameraInformation camInfo = zed.getCameraInformation();
					//float focal = camInfo.calibration_parameters.left_cam.fy; // ??
					//float fov = camInfo.calibration_parameters.left_cam.v_fov;


					// Also:
					// From position tracking sample README.MD
					// *NOTE:* Camera tracking is based on 3D vision only.
					// Quick and sudden camera movements can be difficult to track if the image is blurry or there is no visual information in the scene.
					// To improve tracking performance, we recommend using the ZED in HD720 mode at 60fps.


					outputFile << "TVector\n";
					outputFile << output_translation;
					outputFile << "\nRMatrix\n";
					outputFile << output_rotationMatrix;

					// Doesn't look like we actually use this data?
					// todo: what is focal? I have access to focal length x and y. Didn't seem to be used either in VideoCap.cpp

					outputFile << "\nCamera Intrinsics: focal height width";
					outputFile << "\n0 720 1280";

					outputFile.close();

					// Write Image To File
					cv::imwrite(pngFN, cvMat);
				}

				// Separate the tracking frame (reference for pose) and the camera frame (reference for the image)
				// to have the pose given at the center of the camera. If you are not using this function, the tracking frame and camera frame will be the same (Left sensor).
				// In a more generic way, the formulae is as follow : Pose(new reference frame) = M.inverse() * Pose (camera frame) * M, where M is the transform to go from one frame to another.
				// Here, to move it at the center, we just add half a baseline to "transform.translation.tx".
				transformPose(camera_pose.pose_data, camera_left_to_center);

				// Send all the information to the viewer
				viewer.updateZEDPosition(camera_pose.pose_data);

				// Advance frame number
				frameNumber++;
			}

			// Even if tracking state is not OK, send the text (state, confidence, values) to the viewer
			viewer.updateText(string(text_translation), string(text_rotation), tracking_state);

		}
		else sl::sleep_ms(1);
	}
}

/**
*  This function separates the camera frame and the motion tracking frame. 
*  In this sample, we move the motion tracking frame to the center of the ZED ( baseline/2 for the X translation) 
*  By default, the camera frame and the motion tracking frame are at the same place: the left sensor of the ZED.
**/
void transformPose(sl::Transform &pose, float tx) {
	sl::Transform transform_;
	transform_.setIdentity(); // Create the transformation matrix to separate camera frame and motion tracking frame
	transform_.tx = tx; // Move the tracking frame at the center of the ZED (between ZED's eyes)
	pose = sl::Transform::inverse(transform_) * pose * transform_; // apply the transformation
}

/**
*  This function parses and checks command line arguments
**/
void parse_args(int argc, char **argv, sl::InitParameters &initParameters) {

	// Check number of arguments. Cannot be higher than 3
	//if (argc > 3) {
	//	cout << "Only the path of a SVO or a name for a MotionTracking log file can be passed as argument." << endl;
	//	exit(0);
	//}

	// Check if we work in LIVE or SVO mode
    //if (argc > 1) {
    //    string _arg;
    //    for (int i = 1; i < argc; i++) {
    //        _arg = argv[i];
    //        if (_arg.find(".svo") != string::npos) { // if a SVO is given we save its name
    //            initParameters.svo_input_filename = argv[i];
    //        } else
    //            txtName = _arg;
    //    }
    //}
}