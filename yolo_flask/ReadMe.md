# Flask сервер.
Используется как отдельное серверное приложение, для распознования и классификации мусорных контейнеров.

Приложение является обёрткой над PyTorch YOLO: https://github.com/Tianxiaomo/pytorch-YOLOv4

# Зависимости:
1) Anaconda
2) Flask
3) PyTorch
4) OpenCv

Шаги запуска сервиса:
1) Установить Anaconda
2) Используя Anaconda Prompt, прописать:

conda activate base
pip install torch torchvision torchaudio
pip install flask
pip install opencv-python

3) Запуск сервиса осуществляется через команды, из репозитория где находится run.py:

set FLASK_APP=run.py
python -m flask run

Для тестирования используется app/yolo_client.py
Все параметры запуска клиента для тестированяи прописываются в нём.

Сервер работает на localhost с портом 5000.

Лицензия Flask разрешает использовать его в некоммерческих целях. Он был использован для экономии сил и времени для написания 
клиент-серверной архитектуры приложения. С этой же целью использовался https://github.com/Tianxiaomo/pytorch-YOLOv4.

Модель обучена на 800 кадрах, в которых присутствовала валидационная выборка.
Выборка накоторой обученная нейронная сеть (yolo_v4_tiny), не является полностью репрезентативной, в следствии чего
могут возникать сильные отклонения в детекции. Выборка однообразная с преобладанием одного ракурса.
Для ликвидации проблем, рекомендуется обучить новую модель Yolo_V4. 
Подменить веса и конфигурации. Рекомендуется так, же расширить выборку. 

### Как выглядит консоль с правильно запущенным сервисом.
 * Serving Flask app "run.py"
 * Environment: production
   WARNING: This is a development server. Do not use it in a production deployment.
   Use a production WSGI server instead.
 * Debug mode: off
convalution havn't activate linear
convalution havn't activate linear
 * Running on http://127.0.0.1:5000/ (Press CTRL+C to quit)

