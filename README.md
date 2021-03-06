# garbage
FutogyRevolution

### Система требования
Операционная система 'Windows'
Наличие библиотек для работы с '.net 5.0'

### Системные параметры
Платформа .net5.0
Языки программирования C# 'lang9.0', python-3.6
Среда разработки Visual Studio, Visual Studio Note.
Использовались сторонние зависисмости для C#: Newtonsoft.
Использовались сторонние зависисмости для Python: Flask, OpenCV, PyTorch, сторонний open source репозиторий (описан подробнее в папке с исходнками сервера).

### Краткое описание
Проект представляет из себя клиент-серверную систему. В качестве сервера выступает сервис, написанный на языке Python. В качестве клиента - интерфейса, что был реализован на C# wpf (.net Framework). Программа предназначена для детекции и аналитики полученных кадров, с целью дальнейшей манипуляции со стороны оператора.
Прямым назначением приложения, на данный момент, является распознование контейнерных баков (в будущем и для прочих объектов, что будут расширяться модулями) и уведомление пользователя о их перегруженности.

### Старт 
Для запуска приложения, необходимо:
1) Установить Anaconda
2) Используя Anaconda Prompt, прописать:

conda activate base
pip install torch torchvision torchaudio
pip install flask
pip install opencv-python

3) Запуск сервиса осуществляется через команды, из репозитория, где находится run.py:

set FLASK_APP=run.py
python -m flask run

4) Cобрать полностью проект на .net
5) Попытаться загрузить видео или изображение.

### Заметки
Приложения написаны на скорую руку. В некоторых местах могут быть не соответсвия с принципами SOLID и ООП.
Есть множество проблем, которые касаются детектора. Расширение модулей, предпологается делать через расширения API сервера (в будущем поменять архитектуру враппера)
Распознование детектра на видео близко не является идеальным. На что влияют такие параметры, как однотипность ракурса, возможные ошибки при разметке, выбор легкой конфигурации архитектуры Yolo-tiny-v4 для ускоренного обучения и соответсвенно, его продвижения в коде через враппер. Малый набор датасета, так же повлиял на результат
обучения. 

Прочая информация о детекторе находится по пути: \garbage\yolo_flask\readme.md

### В разрабоке
Возможность расширяемости модулей (возможность хранить разные веса и конфигурации слоёв для решения разных задач, под какой-либо обёрткой) - в c# это представление чекбоксов справа.
Расширение выборки для детектора поиска мусорных баков. 
Доработка интерфейса.

### Планируется разработать
Обучение дектора для поиска мусора в общественных местах.
В будущем планируется объединить детектор мусорных баков с детектором мусора, для решения задачи определения заполнености баков.
Доработка враппера YOLO.
Доработка RTSP соединения.