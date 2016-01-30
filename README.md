# A Simple Picture Collage Screensaver

This screensaver displays pictures (JPEG, PNG, BMP, GIF) from a 
folder on your computer in a pleasing format.  It is basically
a digital photo frame on steroids.

This is an attempt to make  something better than the built-in
"Photos" screensaver on Windows 7, which is decidedly lacking!

## Features

* Easy installation and uninstallation
* Random layouts and image selection
* Centers and scales images to avoid black bars
* Unlimited monitors supported without additional tools
* Database backed image dimension cache for fast startup
* Wallpaper setting mode (use /d switch)

## Usage

### Installer
1. Download the [installer](https://raw.github.com/droud/Pictures/master/PicturesInstaller.exe) and run it
1. Select the "Pictures" screensaver in the dialog, then click on "Settings..."
1. Choose a folder and refresh delay time, then click "Save"

### Manual Install
1. Build the project in release mode (Visual Studio 2010+)
1. Rename the output "Pictures.exe" file to "Pictures.scr"
1. Right click on the "Pictures.scr" file and click "Install"
1. Choose a folder and refresh delay time, then click "Save"

### Build Installer
1. Build the project in release mode (Visual Studio 2010+)
1. Build the "pictures.nsi" file (NSIS 2.46+)

### Debug
    Be sure to provide /c, /s, or /d options in the project settings under "Debug/Command line arguments" in Visual Studio!

## Future

* Continuous wallpaper mode with notification icon
* Folder and date based event separation and compositing
* Minimum image size choices and filtering
* Animated and 3D transitions
* Border size choices
* Unit tests and frameworks
* Additional file format support (done)
* Image dimension caching (done)
* Wallpaper generation (done)