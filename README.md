# A Simple Picture Collage Screensaver

This screensaver displays pictures (currently JPEG only) from
a folder on your computer in a pleasing format.  It is basically
a digital photo frame on steroids.

So far as I'm aware, nothing else is available for Windows that
does this type of layout and supports multiple monitors.  The
built-in "Photos" screensaver on Windows 7 is decidedly lacking!

## Features

* Easy installation and uninstallation
* Works on standard and widescreen monitors
* Works on landscape and portrait monitors
* Multiple layout grid sizes and image spanning
* Handles vertical and horizontal images correctly
* Centers and scales images to avoid black bars
* Unlimited monitors supported without additional tools
* Low memory consumption

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
    Be sure to provide /c or /s options in the project settings under "Debug/Command line arguments" in Visual Studio!

## Future

* Additional file format support
* Minimum image size choices
* Animated and 3D transitions
* Border size choices
