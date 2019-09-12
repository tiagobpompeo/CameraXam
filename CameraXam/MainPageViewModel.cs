using System;
using System.IO;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Plugin.Media;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

namespace CameraXam
{
    public class MainPageViewModel : ViewModelBase
    {
        public ICommand CameraCommand => new RelayCommand(OpenCameraOnly);

        public ImageSource photo;

        public ImageSource Photo
        {
            get { return this.photo; }
            set
            {
                SetValue(ref this.photo, value);
            }
        }

        public MainPageViewModel()
        {
        }


        private async void OpenCameraOnly()
        {
            var action = await Application.Current.MainPage.DisplayActionSheet("Acesso", "Cancel", null, "Câmera", "Galeria");

            if (action.ToString() == "Câmera")
            {
                try
                {
                    var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                    if (status != PermissionStatus.Granted)
                    {
                        if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Camera))
                        {
                            await Application.Current.MainPage.DisplayAlert("Permitir acesso a camera", "App precisa da camera", "OK");
                        }

                        var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera);
                        status = results[Permission.Camera];
                    }

                    if (status == PermissionStatus.Granted)
                    {

                        await CrossMedia.Current.Initialize();

                        if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                        {
                            await Application.Current.MainPage.DisplayAlert("No Camera", ":( No camera avaialble.", "OK");
                            return;
                        }

                        var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                        {
                            //SaveToAlbum = true,
                            PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
                            //              Directory = "Sample",
                            //Name = "test.png"
                        });

                        if (file == null)
                            return;

                        //upload
                        byte[] data = ReadFully(file.GetStream());






                        //DisplayAlert("File Location", file.Path, "OK");

                        Photo = ImageSource.FromStream(() =>
                        {

                            var stream = file.GetStream();
                            file.Dispose();
                            return stream;
                        });
                    }
                    else if (status != PermissionStatus.Unknown)
                    {
                        await Application.Current.MainPage.DisplayAlert("Acesso Negado", "Nao Podemos continuar, tente novamente.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                }

                //((Button)sender).IsEnabled = true;
                //busy = false;

            }

            if (action.ToString() == "Galeria")
            {

                try
                {
                    var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Photos);
                    if (status != PermissionStatus.Granted)
                    {
                        if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Photos))
                        {
                            await Application.Current.MainPage.DisplayAlert("Acesso a Galeria de Imagens", "App precisa de acesso a câmera", "OK");
                        }

                        var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Photos);
                        status = results[Permission.Photos];
                    }

                    if (status == PermissionStatus.Granted)
                    {
                        await CrossMedia.Current.Initialize();

                        if (!CrossMedia.Current.IsPickPhotoSupported)
                        {
                            await Application.Current.MainPage.DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                            return;
                        }
                        var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                        {
                            PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
                        });


                        if (file == null)
                            return;


                        byte[] data = ReadFully(file.GetStream());



                        Photo = ImageSource.FromStream(() =>
                        {
                            var stream = file.GetStream();
                            file.Dispose();
                            return stream;
                        });
                    }
                    else if (status != PermissionStatus.Unknown)
                    {
                        await Application.Current.MainPage.DisplayAlert("Galeria Negada", "Nao podemos continuar, tente novamente.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                }

            }
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

    }
}