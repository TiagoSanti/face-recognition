using OpenCvSharp;

namespace FaceDetectionCustom
{
    internal class CameraModule
    {
        public Mat Manipulate(Mat image)
        {
            Mat edgeDetection = new Mat();

            Cv2.Canny(image, edgeDetection, 100, 200);

            return edgeDetection;
        }

        public void ShowImage(Mat image)
        {
            Cv2.ImShow("img", image);
        }
        public void Release()
        {
            Cv2.DestroyAllWindows();
        }
    }
}
