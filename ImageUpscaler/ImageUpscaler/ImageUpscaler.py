import cv2
from cv2 import dnn_superres
# Create an SR object - only function that differs from c++ code
sr = dnn_superres.DnnSuperResImpl_create()
print(sr)