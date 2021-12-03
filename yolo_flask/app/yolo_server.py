import sys
sys.path.insert(1, r"D:\Repositories\yolo_flask")

import os
import cv2
import time
import torch
import argparse
import numpy as np
from flask import Response, request
from app.yolo_darknet import YOLODarknet
import json

from app import app

cfg     = r".\cfg_and_weights\yolov4-tiny_garbage.cfg"
weights = r".\cfg_and_weights\yolov4-tiny_garbage_best.weights"
names   = r".\cfg_and_weights\names.list"

detector = YOLODarknet(cfg, weights, False)


@app.route("/api", methods=["POST"])
def run():
    r            = request
    bytes_       = r.data
    frame_bytes  = np.fromstring(bytes_, np.uint8)
    frame_decode = cv2.imdecode(frame_bytes, cv2.IMREAD_COLOR)
    detections   = detector.detect(frame_decode, names, False)
    response     = json.dumps(detections)
    return Response(response=response, status=200, mimetype="application/json")
    


