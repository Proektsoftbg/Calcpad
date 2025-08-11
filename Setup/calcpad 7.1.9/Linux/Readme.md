# How to install Calcpad on Ubuntu Linux

1. Calcpad is a .NET application, so you need .NET 8.0 to run it on Linux.
Use the following commands to install .NET 8.0 runtime:
```
sudo apt update
sudo apt-get install -y dotnet-runtime-8.0
```
If you need to uninstall older dotnet versions, run this command before the above ones:
```
sudo apt remove dotnet*
```
2. If you do not have Chromium installed, you will need it to download Calcpad and view the reports after calculation. Install it with the following command:
```
sudo snap install chromium
```
3. Download the Calcpad setup package from the following link:
https://github.com/Proektsoftbg/Calcpad/blob/main/Setup/Linux/Calcpad.7.1.9.deb

Then, install Calcpad, using the following command:
```
sudo apt-get install -y <path-to-your-downloads-folder>/Calcpad.7.1.9.deb
```
Instead of `<path-to-your-downloads-folder>` you must put the actual path, something like this:
```
sudo apt-get install -y /home/ned/snap/chromium/2795/Downloads/Calcpad.7.1.9.deb
```
If you get a message like the one bellow, please ignore it:
N: Download is performed unsandboxed as root as file '.../Calcpad.7.1.9.deb' couldn't be accessed by user '_apt'. - pkgAcquire::Run (13: Permission denied)

And that's it. You can start the Calcpad command line interpreter (CLI) by simply typing:
```
calcpad
```
You can use it to perform calculations in console mode:

![Cli](https://github.com/Proektsoftbg/Calcpad/blob/main/Setup/Linux/Images/Cli.png)

The Linux version does not include any GUI yet, but you can use some advanced code editors like Notepad++ and Sublime to write Calcpad code and Chromium to view the results.
Instructions how to install Sublime Text on Linux are provided here:
https://www.sublimetext.com/docs/linux_repositories.html

For Ubuntu, you can use the following commands:
```
wget -qO - https://download.sublimetext.com/sublimehq-pub.gpg | gpg --dearmor | sudo tee /etc/apt/trusted.gpg.d/sublimehq-archive.gpg > /dev/null
sudo apt-get update
echo "deb https://download.sublimetext.com/ apt/stable/" | sudo tee /etc/apt/sources.list.d/sublime-text.list
sudo apt-get install sublime-text
```
Then, goto https://github.com/Proektsoftbg/Calcpad/tree/main/Setup/Linux/Sublime and download the following files:

> calcpad.sublime-build<br/>
> calcpad.sublime-completions<br/>
> calcpad.sublime-syntax<br/>
> Monokai.sublime-color-scheme

Copy them to the Sublime Text user package folder: /home/&lt;user&gt;/.config/sublime-text/Packages/User

Here, &lt;user&gt; must be your actual user name.
Finally, you can open Sublime Text and Chromium with the following commands:
```
subl &
chromium &
```
Put them side to side. Start a new *.cpd file in Sublime Text or open an example from the /home/&lt;user&gt;/Calcpad folder.
Press Ctrl+B to calculate. If everything is OK, the results will show in Chromium:

![Sublime+Chromium](https://github.com/Proektsoftbg/Calcpad/blob/main/Setup/Linux/Images/Sublime+Chromium.png)

Finally, if you want to uninstall Calcpad, type the following:
```
sudo apt-get --purge remove calcpad
```
