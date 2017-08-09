# project
cmake_minimum_required (VERSION 2.8)
project (RenderingPlugin)
SET(CMAKE_C_FLAGS "${CMAKE_CXX_FLAGS} -DSUPPORT_OPENGL_LEGACY=1 -DSUPPORT_OPENGL_UNIFIED=1 -DUNITY_LINUX=1“)
SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -DSUPPORT_OPENGL_LEGACY=1 -DSUPPORT_OPENGL_UNIFIED=1 -DUNITY_LINUX=1“)
# use opencv to read and show img
set(OpenCV_DIR "/home/long/depend/opencv/build")

find_package(OpenCV REQUIRED)
find_package( OpenGL REQUIRED )
find_package( GLEW REQUIRED )


include_directories(
${PROJECT_SOURCE_DIR}
${PROJECT_SOURCE_DIR}/Unity
${OPENCV_INCLUDE_DIRS}
${OPENGL_INCLUDE_DIR}
${GLEW_INCLUDE_DIR}
)
# use sse3 instruction set
SET(CMAKE_CXX_FLAGS "-std=c++11")

set(SOURCE_FILES RenderingPlugin.cpp VideoCap.cpp)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${PROJECT_SOURCE_DIR}/../UnityProject/Assets/Plugins/x86_64)
add_library(RenderingPlugin SHARED ${SOURCE_FILES} )
target_link_libraries(${PROJECT_NAME} ${OpenCV_LIBS} ${GLEW_LIBS})
