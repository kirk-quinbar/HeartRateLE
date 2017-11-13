# Bluetooth LE Heart Rate Device Monitor sample

This repo contains an example of using the WinRT api to pair and connect to Bluetooth LE heart rate devices. The point of 
this sample is to show how its possible to reference and utilize WinRT apis without having to write a UWP application. The
client can be any windows client that can reference a C# library (i.e. windows forms, wpf, etc)

> **Note:** This sample consists of a Visual Studio 2015 solution with C# projects.
> The UI project is a WPF application. 
> The Bluetooth project has a class wrapper for WinRT code (HeartRateMonitor) so that the client calling the library does not have to 
> know about UWP objects or coding. The client only has to instantiate basic classes and schemas and attach event handlers to
> the class. 
>
In order for this code to work past the windows 10 creator edition, i did a complete rewrite of the bluetooth code. There is a much better bluetooth api available with windows creator edition, unfortunately there was a bug in the actual windows code that prevented the Gattcharacteristic ValueChange event from firing for the HeartRateMeasurement if using a non-uwp application, like i am doing here. Fall Creator edition of Windows

Fall Creator update (if you need to manually update)
https://www.microsoft.com/en-us/software-download/windows10

10.0.16299.0 of the windows 10 sdk
https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk

Specifically, this sample shows how to:

- Enumerate nearby Bluetooth LE devices
- Query for supported services
- Query for supported characteristics
- Interogate device for information
- Subscribe to device events such as connection status changed and value changed


