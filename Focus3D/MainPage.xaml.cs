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
        private byte[] byteBuffer;
        private int[] intBuffer;
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
            // Display camera viewfinder data in XAML videoBrush element
            wb = new WriteableBitmap((int)camManual.PreviewResolution.Width, (int)camManual.PreviewResolution.Height);
            this.MainImage.Source = wb;

            wb2 = new WriteableBitmap((int)camManual.PreviewResolution.Width, (int)camManual.PreviewResolution.Height);
            this.MainImage2.Source = wb2;

            wb3 = new WriteableBitmap((int)camManual.PreviewResolution.Width, (int)camManual.PreviewResolution.Height);
            this.MainImage3.Source = wb3;

            wb4 = new WriteableBitmap((int)camManual.PreviewResolution.Width, (int)camManual.PreviewResolution.Height);
            this.MainImage4.Source = wb4;

            byteBuffer = new byte[(int)(camManual.PreviewResolution.Width * camManual.PreviewResolution.Height)];
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
            if (cam != null)
            {
                // LandscapeRight rotation when camera is on back of phone.
                int landscapeRightRotation = 180;

                // Change LandscapeRight rotation for front-facing camera.
                if (cam.CameraType == CameraType.FrontFacing) landscapeRightRotation = -180;

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
                    camManual.GetPreviewBufferY(byteBuffer);
                    byte[] b = { 255, 155, 155, 225 };

                    for (int i = 0; i < byteBuffer.Length-1; i++ )
                    {
                        b[0] = byteBuffer[i];
                        b[1] = byteBuffer[i];
                        b[2] = byteBuffer[i];
                        intBuffer[i] = BitConverter.ToInt32(b,0);

                    }
                    // Save thumbnail as JPEG to the local folder.
                    Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        // Copy to WriteableBitmap.
                        intBuffer.CopyTo(wb.Pixels, 0);
                        wb.Invalidate();

                    });


                    //CAPTURE SECOND FOCUS
                    camManual.SetProperty(KnownCameraGeneralProperties.ManualFocusPosition, focusRange + 100);
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


                    //CAPTURE THIRD FOCUS
                    camManual.SetProperty(KnownCameraGeneralProperties.ManualFocusPosition, focusRange + 200);
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


                    //CAPTURE FOURTH/FINAL FOCUS
                    camManual.SetProperty(KnownCameraGeneralProperties.ManualFocusPosition, focusRange + 300);
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
                        intBuffer.CopyTo(wb4.Pixels, 0);
                        wb4.Invalidate();

                    });
                }
                catch (Exception)
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
                changeFocus();
        }

        private async void changeFocus() {
            camManual.SetProperty(KnownCameraGeneralProperties.ManualFocusPosition, focusRange);
            s = await camManual.FocusAsync();

        }

    }
}