using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;  // Required for WindowsFormsHost
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using PictureBox = System.Windows.Forms.PictureBox;  // Alias to avoid conflict with WPF PictureBox

namespace Task_3
{
    public partial class MainWindow : Window
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice? videoSource = default;
        private PictureBox formsPictureBox; // Declare at the class level
        private bool isWindowClosed = false; // Flag to indicate whether the window is closed

        public MainWindow()
        {
            InitializeComponent();
            InitializeCamera();
        }

        private void InitializeCamera()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                formsPictureBox = new PictureBox();
                windowsFormsHost.Child = formsPictureBox;

                // Set the SizeMode of the PictureBox to ensure the image fits the size
                formsPictureBox.SizeMode = PictureBoxSizeMode.Zoom;

                // Subscribe to the NewFrame event
                videoSource.NewFrame += (s, e) =>
                {
                    // Check if the window is closed
                    if (isWindowClosed)
                        return;

                    // Access the PictureBox within the WindowsFormsHost
                    formsPictureBox.Image = (Bitmap)e.Frame.Clone();
                };

                videoSource.Start();
            }
            else
            {
                System.Windows.MessageBox.Show("No video devices found.");
            }
        }

        private BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Bmp);

            var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray());
            bitmapImage.EndInit();

            return bitmapImage;
        }

        private void btn_Capture_Click(object sender, RoutedEventArgs e)
        {
            // Capture the image at the moment the Capture button is clicked
            if (videoSource != null)
            {
                Bitmap capturedBitmap = (Bitmap)formsPictureBox.Image.Clone();

                // Display the captured image on capturedImage component
                capturedImage.Source = ConvertToBitmapSource(capturedBitmap);
            }
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            // Save the image
            if (capturedImage.Source is BitmapSource bitmapSource)
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "Bitmap Image|*.bmp|JPEG Image|*.jpg|PNG Image|*.png";

                // Check if the user clicked OK
                if (saveFileDialog.ShowDialog() == true)
                {
                    BitmapEncoder encoder = new PngBitmapEncoder(); // Change this based on your desired image format
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("No image to save. Capture an image first.");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Set the flag to indicate that the window is closed
            isWindowClosed = true;
        }
    }
}
