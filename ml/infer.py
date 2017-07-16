import numpy as np
from PIL import Image
from scipy import misc,io

# to help link caffe
import sys
sys.path.append('C:/Users/SOPOT/Desktop/BUWork/WenWork/UnityProject/ml/caffe/python')
 
import caffe
from trans_to_colourMap import transToColourMap
 
def saveConfidence(out_confidence):
    print out_confidence.shape
    print out_confidence.shape[0]
    print out_confidence.shape[1]
    print out_confidence.shape[2]
    write_stream = open('./FCN_confidence.txt', 'w')
 
    for i in range(out_confidence.shape[1]):
        for j in range(out_confidence.shape[2]):
            out_confidence_exp = np.exp(out_confidence[..., i, j])
            out_confidence_exp_sum = np.sum(out_confidence_exp)
            out_confidence_exp = out_confidence_exp / out_confidence_exp_sum
            out_confidence_exp.tofile(write_stream, sep=' ', format='%s')
            write_stream.write('\n')
 
    write_stream.close()
    return
 
# init

# set device seems to block when launching from unity, but still works on GPU
#caffe.set_device(0)

caffe.set_mode_gpu()

#caffe.set_mode_cpu()

inputImagePath = './test.jpg'
outputImagePath = './test_material_colour_output.png'

print 'Number of arguments:', len(sys.argv), 'arguments.'
print 'Argument List:', str(sys.argv)

if len(sys.argv) > 1:
	inputImagePath = sys.argv[1]
	
if len(sys.argv) > 2:
	outputImagePath = sys.argv[2]
 
# load image, switch to BGR, subtract mean, and make dims C x H x W for Caffe
im = Image.open(inputImagePath)
in_ = np.array(im, dtype=np.float32)
in_ = in_[:,:,::-1]
in_ -= np.array((104.00698793,116.66876762,122.67891434))
in_ = in_.transpose((2,0,1))
 
# load net
net = caffe.Net('deploy.prototxt', './_iter_50000.caffemodel', caffe.TEST)
# shape for input (data blob is N x C x H x W), set data
net.blobs['data'].reshape(1, *in_.shape)
net.blobs['data'].data[...] = in_
# run net and take argmax for prediction
net.forward()
out = net.blobs['score'].data[0].argmax(axis=0)
colour_out = np.asarray(transToColourMap(out), dtype=np.uint8)
 
#print out
#print colour_out
 
#saveConfidence(net.blobs['score'].data[0])
 
#misc.imsave('./image/test_result.png', out)
#misc.imsave('./test_result50000.png', out.astype(np.uint8))
#io.savemat('./image/test_result.mat', {'array':np.array(out)})
 
misc.imsave(outputImagePath, colour_out)
#io.savemat('./image/test_color_result.mat', {'array':np.array(colour_out)})