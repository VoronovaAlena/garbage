# 3rdparty
import cv2
from typing import Tuple
import time

# yolov4 tools from https://github.com/Tianxiaomo/pytorch-YOLOv4
from app.yolov4_tools.utils import *
from app.yolov4_tools.torch_utils import *
from app.yolov4_tools.darknet2pytorch import Darknet


class YOLODarknet():
    '''
    Обертка над PyTorch YOLO: https://github.com/Tianxiaomo/pytorch-YOLOv4
    '''

    def __init__(self, path_to_cfg: str, path_to_weights: str, use_cuda: bool) -> None:
        '''
        Конструктор, инициализирующий детектор YOLO

        Параметры:
                path_to_cfg (str)    : путь к cfg-файлу YOLO
                path_to_weights (str): путь к весам YOLO
                use_cuda (bool)      : использовать ли GPU для инференса
    
        '''
        detector = Darknet(path_to_cfg)
        detector.load_weights(path_to_weights)

        if use_cuda:
            detector.cuda()

        self.detector = detector
        self.use_cuda = use_cuda
        


    def detect(self, image_array: np.ndarray, names_file: str, grayscale: bool) -> Tuple[dict, float]:
        '''
        Выполняет детекцию на единичном изображении

        Параметры:
                image_array (np.ndarray) : numpy.array с изображением 
                names_file (str)         : путь к файлу с названиями классов
                grayscale (bool)         : переводить ли изображение в градации серого
            
        Возвращает:
                detections_result_dict (dict): словарь с результатами детекции
                detection_time (float):        : время выполнения детекции
        '''
        #image_array                         = cv2.imread(path_to_image)
        image_height, image_width, channels = image_array.shape
        resized_image_array                 = cv2.resize(image_array, (self.detector.width, self.detector.height))
        if grayscale:
            resized_image_array             = cv2.cvtColor(resized_image_array, cv2.COLOR_BGR2GRAY)
        class_names    = load_class_names(names_file)
        start_time     = time.time()*1000
        bounding_boxes = do_detect(self.detector, resized_image_array, 0.25, 0.3, int(self.use_cuda))
        end_time       = time.time()*1000
        detection_time = round(end_time - start_time, 3)
        print('Исходные bounding boxes: ', bounding_boxes)

        detections_result_dict = {"boxes": [], "class_names": [], "confidences": []}
        for i in range(len(bounding_boxes[0])):
            bounding_box        = bounding_boxes[0][i]
            recalc_bounding_box = [
                int(bounding_box[0] * image_width),
                int(bounding_box[1] * image_height),
                int(bounding_box[2] * image_width),
                int(bounding_box[3] * image_height)
            ] 
            class_name = class_names[bounding_box[6]]
            if class_name in ["ext_vertical", "ext_horizontal"]:
                continue
            confidence = float(bounding_box[5])
            
            detections_result_dict["boxes"].append(recalc_bounding_box)
            detections_result_dict["class_names"].append(class_name)
            detections_result_dict["confidences"].append(confidence)

        print(detections_result_dict["class_names"])

        return detections_result_dict, detection_time                   
