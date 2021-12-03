import numpy
import json
import requests
import cv2
import os

import sys
sys.path.append("./")

image        = r"test_frame.png"
image_array  = cv2.imread(image)
image_encode = cv2.imencode(".png", image_array)[1].tobytes()
answer       = requests.post('http://127.0.0.1:5000/api', data=image_encode)
print(answer.json())