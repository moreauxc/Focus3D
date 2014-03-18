using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Focus3D.Resources;
using Microsoft.Devices;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.Media;
using Windows.Phone.Media.Capture;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using Nokia.Graphics.Imaging;

namespace Focus3D
{
    public partial class MainPage : PhoneApplicationPage
    {
        private int savedCounter = 0;
        PhotoCamera cam;
        MediaLibrary library = new MediaLibrary();
        PhotoCaptureDevice camManual;
        private int focusRange = 0;
        CameraFocusStatus s = CameraFocusStatus.Locked;
        private WriteableBitmap wb, wb2, wb3, wb4;
        private BackgroundWorker bw1, bw2, bw3, bw4;
        private byte[] byteBuffer;
        private byte[] prevBuffer;
        private byte[] depthBuffer;
        private int[] intBuffer;
        private int[] _sharpnessArea;
        private int[] _sharpnessMap;
        private FocusMap bufferMap, bufferMap2;
        private Windows.Foundation.Size _previewFrameSize = new Windows.Foundation.Size();
        // Constructor
        public MainPage()
        {
            
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}

        private async void InitializeCamera(CameraSensorLocation sensorLocation)
        {
            // Open camera device
            camManual = await PhotoCaptureDevice.OpenAsync(sensorLocation, new Windows.Foundation.Size(640, 480));
            // Event is fired when the PhotoCamera object has been initialized.
            camManual.PreviewFrameAvailable += photoDevice_PreviewFrameAvailable;
            //Set the VideoBrush source to the camera.
            viewfinderBrush.SetSource(camManual);

            _previewFrameSize = camManual.PreviewResolution;
            _sharpnessArea = new int[] { 1, 1 };
            // Display camera viewfinder data in XAML videoBrush element
            wb = new WriteableBitmap((int)camManual.PreviewResolution.Width, (int)camManual.PreviewResolution.Height);
            this.MainImage.Source = wb;

            wb2 = new WriteableBitmap((int)camManual.PreviewResolution.Width, (int)camManual.PreviewResolution.Height);
            this.MainImage2.Source = wb2;

            wb3 = new WriteableBitmap((int)camManual.PreviewResolution.Width, (int)camManual.PreviewResolution.Height);
            this.MainImage3.Source = wb3;

            wb4 = new WriteableBitmap((int)camManual.PreviewResolution.Width, (int)camManual.PreviewResolution.Height);
            this.MainImage4.Source = wb4;
            depthBuffer = new byte[(int)(camManual.PreviewResolution.Width * camManual.PreviewResolution.Height)];
            byteBuffer = new byte[(int)(camManual.PreviewResolution.Width * camManual.PreviewResolution.Height)];
            prevBuffer = new byte[(int)(camManual.PreviewResolution.Width * camManual.PreviewResolution.Height)];
            intBuffer = new int[(int)(camManual.PreviewResolution.Width * camManual.PreviewResolution.Height)];
        }

        void photoDevice_PreviewFrameAvailable(ICameraCaptureDevice sender, object args)
        {
            // Start the processing in the background worker if it's not busy
            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            // Check to see if the camera is available on the phone.
            if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true) ||
                 (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) == true))
            {
                // Initialize the camera, when available.
                if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary))
                {
                    // Use front-facing camera if available.
                   //cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);
                    InitializeCamera(CameraSensorLocation.Back);
                }
                else
                {
                    // Otherwise, use standard camera on back of phone.
                    cam = new Microsoft.Devices.PhotoCamera(CameraType.FrontFacing);
                }
                
                /*
                // Event is fired when the PhotoCamera object has been initialized.
                cam.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);

                // Event is fired when the capture sequence is complete.
                cam.CaptureCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_CaptureCompleted);

                // Event is fired when the capture sequence is complete and an image is available.
                cam.CaptureImageAvailable += new EventHandler<Microsoft.Devices.ContentReadyEventArgs>(cam_CaptureImageAvailable);

                // Event is fired when the capture sequence is complete and a thumbnail image is available.
                cam.CaptureThumbnailAvailable += new EventHandler<ContentReadyEventArgs>(cam_CaptureThumbnailAvailable);

                //Set the VideoBrush source to the camera.
                //viewfinderBrush.SetSource(cam);
                 * */
                bw1 = new BackgroundWorker();
                bw1.WorkerReportsProgress = false;
                bw1.WorkerSupportsCancellation = true;
                bw1.DoWork += bw_DoWork;
                bw1.RunWorkerCompleted +=bw_RunWorkerCompleted;

                bw2 = new BackgroundWorker();
                bw2.WorkerReportsProgress = false;
                bw2.WorkerSupportsCancellation = true;
                bw2.DoWork += bw_DoWork;
                bw2.RunWorkerCompleted += bw_RunWorkerCompleted;

                bw3 = new BackgroundWorker();
                bw3.WorkerReportsProgress = false;
                bw3.WorkerSupportsCancellation = true;
                bw3.DoWork += bw_DoWork;
                bw3.RunWorkerCompleted += bw_RunWorkerCompleted;

                bw4 = new BackgroundWorker();
                bw4.WorkerReportsProgress = false;
                bw4.WorkerSupportsCancellation = true;
                bw4.DoWork += bw_DoWork;
                bw4.RunWorkerCompleted += bw_RunWorkerCompleted;
            }
            else
            {
                // The camera is not supported on the phone.
                this.Dispatcher.BeginInvoke(delegate()
                {
                    // Write message.
                    txtDebug.Text = "A Camera is not available on this phone.";
                });

                // Disable UI.
                ShutterButton.IsEnabled = false;
            }
        }
        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (cam != null)
            {
                // Dispose camera to minimize power consumption and to expedite shutdown.
                cam.Dispose();

                // Release memory, ensure garbage collection.
                cam.Initialized -= cam_Initialized;
                cam.CaptureCompleted -= cam_CaptureCompleted;
                cam.CaptureImageAvailable -= cam_CaptureImageAvailable;
                cam.CaptureThumbnailAvailable -= cam_CaptureThumbnailAvailable;
            }
            if (camManual != null)
            {
                // Dispose camera to minimize power consumption and to expedite shutdown.
                camManual.Dispose();
                camManual.PreviewFrameAvailable -= photoDevice_PreviewFrameAvailable;
                // Release memory, ensure garbage collection.
            }
        }

        // Update the UI if initialization succeeds.
        void cam_Initialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            if (e.Succeeded)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    // Write message.
                    txtDebug.Text = "Camera initialized.";
                });
            }
        }

        // Ensure that the viewfinder is upright in LandscapeRight.
        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (camManual != null)
            {
                // LandscapeRight rotation when camera is on back of phone.
                int landscapeRightRotation = 180;

                // Rotate video brush from camera.
                if (e.Orientation == PageOrientation.LandscapeRight)
                {
                    // Rotate for LandscapeRight orientation.
                    viewfinderBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = landscapeRightRotation };
                }
                else
                {
                    // Rotate for standard landscape orientation.
                    viewfinderBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = 0 };
                }
            }

            base.OnOrientationChanged(e);
        }

        private async void ShutterButton_Click(object sender, RoutedEventArgs e)
        {

            if (camManual != null)
            {
                try
                {
                    /* int[] buffer = new int[(int)(camManual.PreviewResolution.Width * camManual.PreviewResolution.Height)];
                    
                     camManual.GetPreviewBufferArgb(buffer);
                     // Save thumbnail as JPEG to the local folder.
                     Deployment.Current.Dispatcher.BeginInvoke(delegate()
                 {
                     // Copy to WriteableBitmap.
                     buffer.CopyTo(wb.Pixels, 0);
                     wb.Invalidate();

                 });
                     */
                    //CAPTURE FIRST FOCUS
                    CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(CameraSensorLocation.Back, KnownCameraGeneralProperties.ManualFocusPosition);
                    UInt32 max = (UInt32)range.Max;
                    UInt32 min = (UInt32)range.Min;
                    if (!((bool)checkboxSave.IsChecked))
                    {
                        /*
                        if (focusRange >= max - 4)
                            focusRange = (int)max - 10;

                        int focusStep = (100 < ((int)max - focusRange) / 3 ? 100 : ((int)max - focusRange) / 3);
                        camManual.GetPreviewBufferY(byteBuffer);
                        byte[] b = { 255, 155, 155, 225 };

                        for (int i = 0; i < byteBuffer.Length - 1; i++)
                        {
                            b[0] = byteBuffer[i];
                            b[1] = byteBuffer[i];
                            b[2] = byteBuffer[i];
                            intBuffer[i] = BitConverter.ToInt32(b, 0);

                        }
                        */
                        for (int i = 500; i < (int)max; i += ((int)max/100))
                        {
                            camManual.SetProperty(KnownCameraGeneralProperties.ManualFocusPosition, i);
                            focusRange = i;
                            await camManual.FocusAsync();
                            camManual.GetPreviewBufferY(byteBuffer);
                            generateSharpnessMap(byteBuffer);
                        }

                        for (int i = 0; i < byteBuffer.Length; i++)
                        {
                            intBuffer[i] = BitConverter.ToInt32(new byte[] { depthBuffer[i], depthBuffer[i], depthBuffer[i], 255 }, 0);

                        }
                            Deployment.Current.Dispatcher.BeginInvoke(delegate()
                            {
                                // Copy to WriteableBitmap.
                                intBuffer.CopyTo(wb.Pixels, 0);
                                wb.Invalidate();

                            });
                        /*
                        generateSharpnessMap(byteBuffer);

                        //CAPTURE SECOND FOCUS
                        camManual.SetProperty(KnownCameraGeneralProperties.ManualFocusPosition, focusRange + focusStep);
                        await camManual.FocusAsync();
                        camManual.GetPreviewBufferY(byteBuffer);
                        for (int i = 0; i < byteBuffer.Length - 1; i++)
                        {
                            b[0] = byteBuffer[i];
                            b[1] = byteBuffer[i];
                            b[2] = byteBuffer[i];
                            intBuffer[i] = BitConverter.ToInt32(b, 0);

                        }
                        // Save thumbnail as JPEG to the local folder.
                        Deployment.Current.Dispatcher.BeginInvoke(delegate()
                        {
                            // Copy to WriteableBitmap.
                            intBuffer.CopyTo(wb2.Pixels, 0);
                            wb2.Invalidate();

                        });
                        generateSharpnessMap(byteBuffer);

                        //CAPTURE THIRD FOCUS
                        camManual.SetProperty(KnownCameraGeneralProperties.ManualFocusPosition, focusRange + focusStep);
                        await camManual.FocusAsync();
                        camManual.GetPreviewBufferY(byteBuffer);

                        for (int i = 0; i < byteBuffer.Length - 1; i++)
                        {
                            b[0] = byteBuffer[i];
                            b[1] = byteBuffer[i];
                            b[2] = byteBuffer[i];
                            intBuffer[i] = BitConverter.ToInt32(b, 0);

                        }
                        // Save thumbnail as JPEG to the local folder.
                        Deployment.Current.Dispatcher.BeginInvoke(delegate()
                        {
                            // Copy to WriteableBitmap.
                            intBuffer.CopyTo(wb3.Pixels, 0);
                            wb3.Invalidate();

                        });
                        generateSharpnessMap(byteBuffer);

                        //CAPTURE FOURTH/FINAL FOCUS
                        camManual.SetProperty(KnownCameraGeneralProperties.ManualFocusPosition, focusRange + focusStep);
                        await camManual.FocusAsync();
                        camManual.GetPreviewBufferY(byteBuffer);

                        for (int i = 0; i < byteBuffer.Length - 1; i++)
                        {
                            b[0] = byteBuffer[i];
                            b[1] = byteBuffer[i];
                            b[2] = byteBuffer[i];
                            intBuffer[i] = BitConverter.ToInt32(b, 0);

                        }
                        
                        generateSharpnessMap(byteBuffer);
                        for (int i = 0; i < byteBuffer.Length - 1; i++)
                            intBuffer[i] = BitConverter.ToInt32(new byte[] { depthBuffer[i], depthBuffer[i], depthBuffer[i], 255 }, 0);

                        // Save thumbnail as JPEG to the local folder.
                        Deployment.Current.Dispatcher.BeginInvoke(delegate()
                        {
                            // Copy to WriteableBitmap.
                            intBuffer.CopyTo(wb4.Pixels, 0);
                            wb4.Invalidate();

                        });
                         */
                    }
                    else
                    {
                        /*var wbitmap = new WriteableBitmap((int)camManual.PreviewResolution.Width, (int)camManual.PreviewResolution.Height);
                        for (int i = (int)min; i <= max; i += 20)
                        {
                            camManual.GetPreviewBufferArgb(wbitmap.Pixels);
                            camManual.SetProperty(KnownCameraGeneralProperties.ManualFocusPosition, i);
                            await camManual.FocusAsync();
                            using (var stream = new MemoryStream())
                            {

                                wbitmap.SaveJpeg(stream, wbitmap.PixelWidth, wbitmap.PixelHeight, 0, 100);
                                stream.Seek(0, SeekOrigin.Begin);
                                new MediaLibrary().SavePicture("focus_at_" + i + ".jpg", stream);
                            }
                        }
                         */
                        byte[] yBuffer = new byte[(int)camManual.PreviewResolution.Width * (int)camManual.PreviewResolution.Height];
                        for (int i = 0; i < yBuffer.Length; i++)
                            yBuffer[i] = (byte) (i/((int)camManual.PreviewResolution.Width));
                         
                        int[] sharpnessTest = edgeDetect(yBuffer, (int)camManual.PreviewResolution.Width, (int)camManual.PreviewResolution.Height);
                        var wbitmap = new WriteableBitmap((int)camManual.PreviewResolution.Width, (int)camManual.PreviewResolution.Height);
                        camManual.GetPreviewBufferArgb(wbitmap.Pixels);
                        using (var stream = new MemoryStream())
                        {
                            wbitmap.SaveJpeg(stream, wbitmap.PixelWidth, wbitmap.PixelHeight, 0, 100);
                            stream.Seek(0, SeekOrigin.Begin);
                            new MediaLibrary().SavePicture("focus_at_" + focusRange + ".jpg", stream);
                        }
                    }
                }
                catch (Exception eg)
                {

                    throw;
                }
            }
            if (cam != null)
            {
                try
                {
                    // Start image capture.
                    cam.CaptureImage();
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        // Cannot capture an image until the previous capture has completed.
                        txtDebug.Text = ex.Message;
                    });
                }
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int i = 1; i <= 10; i++)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // Perform a time consuming operation and report progress.
                    try
                    {
                        byte[] buff = e.Argument as byte[];

                    }
                    finally
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!(e.Cancelled == true && !(e.Error == null)))
            {
                BackgroundWorker bw = sender as BackgroundWorker;
            }
            else
            {
                this.txtDebug.Text = "image process error";
            }
        }



        void cam_CaptureCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            // Increments the savedCounter variable used for generating JPEG file names.
            savedCounter++;
        }

        // Informs when full resolution photo has been taken, saves to local media library and the local folder.
        void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
        {
            string fileName = savedCounter + ".jpg";

            try
            {   // Write message to the UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    txtDebug.Text = "Captured image available, saving photo.";
                });

                // Save photo to the media library camera roll.
                library.SavePictureToCameraRoll(fileName, e.ImageStream);

                // Write message to the UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    txtDebug.Text = "Photo has been saved to camera roll.";

                });

                // Set the position of the stream back to start
                e.ImageStream.Seek(0, SeekOrigin.Begin);

                // Save photo as JPEG to the local folder.
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                    {
                        // Initialize the buffer for 4KB disk pages.
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        // Copy the image to the local folder. 
                        while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            targetStream.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }

                // Write message to the UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    txtDebug.Text = "Photo has been saved to the local folder.";

                });
            }
            finally
            {
                // Close image stream
                e.ImageStream.Close();
            }

        }

        // Informs when thumbnail photo has been taken, saves to the local folder
        // User will select this image in the Photos Hub to bring up the full-resolution. 
        public void cam_CaptureThumbnailAvailable(object sender, ContentReadyEventArgs e)
        {
            string fileName = savedCounter + "_th.jpg";

            try
            {
                // Write message to UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    txtDebug.Text = "Captured image available, saving thumbnail.";
                });

                // Save thumbnail as JPEG to the local folder.
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                    {
                        // Initialize the buffer for 4KB disk pages.
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        // Copy the thumbnail to the local folder. 
                        while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            targetStream.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }

                // Write message to UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    txtDebug.Text = "Thumbnail has been saved to the local folder.";

                });
            }
            finally
            {
                // Close image stream
                e.ImageStream.Close();
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            focusRange = (int) e.NewValue;
            if (s == CameraFocusStatus.Locked)
            {
                changeFocus();
                textBox.Text = "" + focusRange;
            }
        }

        private async void changeFocus() {
            camManual.SetProperty(KnownCameraGeneralProperties.ManualFocusPosition, focusRange);
            s = await camManual.FocusAsync();

        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            int[] b = new int[wb.PixelWidth * wb.PixelHeight];
            for  (int i = 0; i < wb.PixelWidth * wb.PixelHeight; i++)
                b[i] = 0;
            b.CopyTo(wb.Pixels, 0);
            b.CopyTo(wb2.Pixels, 0);
            b.CopyTo(wb3.Pixels, 0);
            b.CopyTo(wb4.Pixels, 0);
        }

        private void generateSharpnessMap(byte[] data)
        {
            // Dimensions of the original frame
            int w = (int)camManual.PreviewResolution.Width;
            int h = (int)camManual.PreviewResolution.Height;

            // and the scaled down sub-image
            int ws = w / _sharpnessArea[0];
            int hs = h / _sharpnessArea[1];

            CameraCapturePropertyRange range = PhotoCaptureDevice.GetSupportedPropertyRange(CameraSensorLocation.Back, KnownCameraGeneralProperties.ManualFocusPosition);
                    UInt32 max = (UInt32)range.Max;
                    UInt32 min = (UInt32)range.Min;
            // Generate a new buffer for the sharpness map. CAUTION: in production code we should check the dimension as well
            // because the preview resolution might have changed.
            ///if (_sharpnessMap == null)
            //{
            //    _sharpnessMap = new int[hs * ws];
            //}
            if (bufferMap == null)
                bufferMap = new FocusMap(ws, hs);

            
            // Calculate sharpness for each sub image
            /*
            int x, y;
            int sharpness;
            for (y = 0; y < hs; y++)
            {
                for (x = 0; x < ws; x++)
                {
                    sharpness = calculateSharpness_variance(x * ws, y * hs, ws, hs, w, data);
                    bufferMap.set(y * _sharpnessArea[0] + x, sharpness);
                }
            }
             */
            bufferMap.set(edgeDetect(data, w, h));
            if (bufferMap2 == null)
            {
                bufferMap2 = new FocusMap(ws, hs);
                bufferMap2.set(bufferMap.get());
            }
            bool[] cmpSet = bufferMap > bufferMap2;
            byte focusColor = (byte) (Math.Pow((double)(focusRange - 500) / (max - 500), .5) * 255);
            int[] testIndices = new int[depthBuffer.Length];
            int ind = 0;
            int prevLoc = 0;
            for (int k = 0; k < cmpSet.Length; k++)
            {
                if (cmpSet[k])
                    bufferMap.setTrigger(k);
                else if (bufferMap2.triggered(k))
                {
                    //bufferMap.cancelTrigger(k);
                    int location = (int)((double)(k / ws) / hs * h * w + ((double)(k % ws) / ws * w));
                    
                    for (int y = 0; y < _sharpnessArea[1]; y++)
                        for (int x = 0; x < _sharpnessArea[0]; x++)
                        {
                            testIndices[ind++] = focusColor;
                            depthBuffer[location + y * w + x] = focusColor;
                        }
                    if (location != prevLoc)
                    {
                        //prevLoc = location;

                    }
                }
            }
            bufferMap2.set(bufferMap.get());
            bufferMap2.setTrigger(bufferMap.triggered());
        }

        private int calculateSharpness_variance(int x, int y, int w, int h, int stride, byte[] data)
        {
            int lumSum = 0;
            int lumSquared = 0;
            int numPixels = w * h;

            int i, j;

            for (j = y; j < y + h; j++)
            {
                for (i = x; i < x + w; i++)
                {
                    int lum = data[j * stride + i];

                    lumSum += (lum / numPixels);
                    lumSquared += ((lum * lum) / numPixels);
                }
            }

            return lumSquared - (lumSum * lumSum);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                int val =  int.Parse(textBox.Text);
                if (val > 1000)
                {
                    val = 1000;
                    textBox.Text = "1000";
                }
                else if (val < 0)
                {
                    val = 0;
                    textBox.Text = "0";
                }
                focusRange = val;
                changeFocus();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

  

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            textBox.SelectAll();
        }

        private int[] edgeDetect(byte[] inArr, int width, int height)
        {
            //int width = (int) _previewFrameSize.Width;
            //int height = (int)_previewFrameSize.Height;
            int sWidth = _sharpnessArea[0];
            int sHeight = _sharpnessArea[1];
            //byte[] midArr = new byte[inArr.Length];
            byte[] buffer = new byte[(width + 2) * 3];
            //int[] addArr = new int[sWidth * sHeight];
            int[] outArr = new int[inArr.Length];
            /*
            buffer[0] = inArr[0];
            buffer[width + 1] = inArr[width - 1];
            for (int i = 0; i < width; i++)
                buffer[i + 1] = inArr[i];
            */
            for (int i = 0; i < 2; i++)
            {
                buffer[i * (width + 2)] = inArr[0];
                buffer[i * (width + 2) + width + 1] = inArr[width - 1];
                for (int j = 0; j < width; j++)
                {
                    buffer[i*(width + 2) + j + 1] = inArr[j];
                }

            }
            Console.WriteLine();
            /* crap
            //write the corners
            buffer[0] = inArr[0];
            buffer[width + 1] = inArr[width - 1];
            buffer[(width + 2) * (height + 1)] = inArr[(width) * (height - 1)];
            buffer[buffer.Length - 1] = inArr[inArr.Length - 1];

            //write the sides
            for (int i = 0; i < height; i++)
            {
                buffer[i*(width + 2)] = inArr[i*width];
                buffer[i*(width + 2) + (width + 1)] = inArr[i*width + (width - 1)];
            }

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    buffer[(i + 1)*(width + 2) + j + 1] = inArr[i*width + j];
            */

            int buf1 = 0;
            int buf2 = 1;
            int buf3 = 2;
            //int tempInd = 0;
            /*for (int row = 0; row < height / _sharpnessArea[1]; row++)
            {
                for (int col = 0; col < width / _sharpnessArea[0]; col++)
                {
                    int bufInd = 0;
                    int temp = buffer[buf1 * 3] + 2*buffer[buf1*3 + 1] + buffer[buf1*3 + 2] - buffer[buf3 * 3] - 2*buffer[buf3*3 + 1] - buffer[buf3*3 + 2];
                    int[] temp2 = new int [2] {buffer[buf1 * 3] + 2*buffer[buf2*3] + buffer[buf3*3] - buffer[buf1 * 3 + 2] - 2*buffer[buf2*3 + 2] - buffer[buf3*3 + 2], 
                        buffer[buf1 * 3 + 1] + 2*buffer[buf2*3 + 1] + buffer[buf3*3 + 1] - buffer[buf1 * 3 + 3] - 2*buffer[buf2*3 + 3] - buffer[buf3*3 + 3]};
                    for (int j = (row * _sharpnessArea[1]) + (col * _sharpnessArea[0]); j < ((row + 1) * _sharpnessArea[1]) + (col * _sharpnessArea[0]); j += width)
                    {
                        for (int k = 0; k < _sharpnessArea[0]; k++ )
                        {
                            tempInd = k%2;
                            addArr[j + k] = temp*temp + temp2[tempInd]*temp2[tempInd];
                            temp += buffer[buf1 * 3 + k + 2] - buffer[buf3 * 3 + k + 2] - (buffer[buf1 * 3 + k] - buffer[buf3 * 3 + k]);
                            temp2[tempInd] += -() 
                        }
                        buf1 = (buf1 + 2) % 3;
                        buf2 = (buf2 + 2) % 3;
                        buf3 = (buf3 + 2) % 3;
                        int t2 = 1;
                        for (int t = j; t < j + sWidth; t++)
                            buffer[(bufInd % 3) * (sWidth + 2) + t2++] = inArr[j + t];
                        buffer[(bufInd % 3) * (sWidth + 2)] = inArr[j];
                        buffer[(bufInd % 3) * (sWidth + 2) + sWidth + 1] = inArr[j + sWidth - 1];
                        bufInd++;
                    }
                    outArr[row * (width / _sharpnessArea[0]) + col] = addArr.Sum();
                }
                
            }*/
            /*test variables */
            int[] test = new int[12];
            int horizontal;
            int vertical;
            /*
            for (int i = 0; i < outArr.Length; i++)
                outArr[i] = 0;
             */
            int y = 0;
            //int scale = width / sWidth / sHeight;
            try { 
            for (y = 0; y < height - 2; y++)
            {
                if (y + 1 == height)
                    Console.WriteLine(y);
                buffer[buf3 * (width + 2)] = inArr[(y + 1) * width];
                buffer[buf3 * (width + 2) + width + 1] = inArr[(y + 1) * width + width - 1];
                for (int i = 0; i < width; i++)
                    buffer[buf3 * (width + 2) + i + 1] = inArr[(y + 1) * width + i];
                for (int x = 1; x < width + 1; x++)
                {
                    horizontal = buffer[(buf1 * (width + 2)) + x - 1] + 2 * buffer[(buf1 * (width + 2)) + x] + buffer[(buf1 * (width + 2)) + x + 1]
                        - buffer[(buf3 * (width + 2)) + x - 1] - 2 * buffer[(buf3 * (width + 2)) + x] - buffer[(buf3 * (width + 2)) + x + 1];
                    vertical = -buffer[(buf1 * (width + 2)) + x - 1] - 2*buffer[(buf2 * (width + 2)) + x - 1] - buffer[(buf3 * (width + 2)) + x - 1]
                        + buffer[(buf1 * (width + 2)) + x + 1] + 2*buffer[(buf2 * (width + 2)) + x + 1] + buffer[(buf3 * (width + 2)) + x + 1];

                    outArr[y * width + x - 1] += horizontal * horizontal + vertical * vertical;
                }
                buf1 = (y) % 3;
                buf2 = (y + 1) % 3;
                buf3 = (y + 2) % 3;

            }
            }
            catch (Exception e)
            {
                Console.WriteLine(y);
            }
            
            for (; y < height + 1; y++)
            {
                for (int x = 1; x < width + 1; x++)
                {
                    horizontal = buffer[(buf1 * (width + 2)) + x - 1] + 2 * buffer[(buf1 * (width + 2)) + x] + buffer[(buf1 * (width + 2)) + x + 1]
                        - buffer[(buf3 * (width + 2)) + x - 1] - 2 * buffer[(buf3 * (width + 2)) + x] - buffer[(buf3 * (width + 2)) + x + 1];
                    vertical = -buffer[(buf1 * (width + 2)) + x - 1] - 2 * buffer[(buf2 * (width + 2)) + x - 1] - buffer[(buf3 * (width + 2)) + x - 1]
                        + buffer[(buf1 * (width + 2)) + x + 1] + 2 * buffer[(buf2 * (width + 2)) + x + 1] + buffer[(buf3 * (width + 2)) + x + 1];

                    outArr[(y - 1) * width + x - 1] += horizontal * horizontal + vertical * vertical;
                }
                if (y < height + 1)
                {
                    buf1 = (y - 1) % 3;
                    buf2 = y % 3;
                    buf3 = (y + 1) % 3;
                    buffer[buf1 * (width + 2)] = inArr[(height - 1) * width];
                    buffer[buf1 * (width + 2) + width + 1] = inArr[(height - 1) * width + width - 1];
                    for (int i = 0; i < width; i++)
                        buffer[((y - 1) % 3) * (width + 2) + i + 1] = inArr[(height - 1) * width + i];
                }
            }

                return outArr;
        }
    }
}