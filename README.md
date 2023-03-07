# DeepNestPort
DeepNest C# Port (https://github.com/Jack000/Deepnest)

DXF Import/Export: https://github.com/IxMilia/Dxf

Also take a look at the WPF Net.Core version: https://github.com/9swampy/DeepNestSharp 

<img src="imgs/2.png"/>
<img src="imgs/3.png"/>


## Running this software using external dll

Steps to run this software-

	Steps to generate minkowski.dll file  >>>>>

		1.Install vs2022 and also install plugin named "Desktop development with C++"
	
		2.Download code.

		3.Download boost zip (link-https://www.boost.org/users/history/version_1_76_0.html)

		4.Extract it somewhere and copy its location and edit it in DeepNestPort-master\Minkowski\compile.bat file like so :-
		  cl /Ox -I "D:\boost\boost_1_76_0" /LD minkowski.cc

		5.Go to Start->All Programs->(Scroll down to see Visual Studio 2022 folder)->Tools->Dev. Command Prompt->Do cd to the place where DeepNestPort-master is location(u might have to do 'cd /d <location>' instead of normal 'cd' command to change the drive)
		  enter 'compile.bat' in command prompt and it will automatically generate a 'minkowski.dll' file for you.

	Steps to generate Software exe    >>>>>

		1. Open vs2022 and open project in explorer(select file 'DeepNestPort.sln').
		2. In visual studio press Build->Build solution. This will take 10-20 seconds and u will obtain a 'DeepNestPort.exe' in 'DeepNestPort-master\DeepNestPort\bin\Debug\' folder.
	

	To run software just copy minkowski.dll to the folder where u have exe, and u will have this project running.Tudums!

## Contributors
* https://github.com/Daniel-t-1/DeepNestPort (dxf export)
* https://github.com/9swampy/DeepNestPort (simplification features)
