import sys
import cv2
from cv2 import dnn_superres
import pathlib

if len(sys.argv) < 4:
    print('Usage: <path_to_image.png> <algo_string> <upscale_int> <model_path.pb>')
    print('<algo_string> options: bilinear, bicubic, edsr, espcn, fsrcnn or lapsrn')
    sys.exit()

image_path = sys.argv[1];
algorithm = sys.argv[2]
scale = float(sys.argv[3])
model_path = None

if len(sys.argv) > 4:
    model_path = sys.argv[4];

image = cv2.imread(image_path)
if image is None or image.size == 0:
    print(f'Couldn\'t load image: {image_path}')
    sys.exit(-2)

sr = dnn_superres.DnnSuperResImpl_create()
new_image = None
if algorithm == 'bilinear':
    new_image = cv2.resize(image, dsize = None, fx = scale, fy = scale, interpolation = cv2.INTER_LINEAR)
elif algorithm == 'bicubic':
    new_image = cv2.resize(image, dsize = None, fx = scale, fy = scale, interpolation = cv2.INTER_CUBIC)
elif algorithm == 'edsr' or algorithm == 'espcn' or algorithm == 'fsrcnn' or algorithm == 'lapsrn':
    sr.readModel(model_path)
    sr.setModel(algorithm, int(scale))
    new_image = sr.upsample(image)
else:
    print('Algorithm not recognized.')
    sys.exit(-3)

if new_image is None:
    print('Upsampling failed.')
    sys.exit(-2)

print('Upsampling succeeded.')

cv2.imwrite(f'./upscaled{pathlib.Path(image_path).suffix}', new_image)
