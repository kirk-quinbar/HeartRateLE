# Bluetooth LE Heart Rate Device Monitor sample

This repository contains an example of using the WinRT API to pair and connect to Bluetooth LE heart rate devices. The point of this sample is to show how it is possible to reference and use WinRT APIs without having to write a UWP application. The client can be any Windows client that can reference a C# library (i.e. Windows Forms, WPF, etc).

> **Note:** This sample consists of a Visual Studio 2015 solution with C# projects. The UI project is a WPF application. The Bluetooth project has a class wrapper for WinRT code (HeartRateMonitor) so that the client calling the library does not have to know about UWP objects or code. The client only has to instantiate basic classes and schemas and to attach event handlers to the class.

In order for this code to work past the Windows 10 Creator Edition, I did a complete rewrite of the Bluetooth code. There is a much better Bluetooth API available with Windows Creator Edition.

Windows Fall 2018 Creator Update (v 1709) (if you need to manually update) <https://www.microsoft.com/en-us/software-download/windows10>

Windows 10 SDK version 10.0.16299.0 <https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk>

Specifically, this sample shows how to:

- Enumerate nearby Bluetooth LE devices
- Query for supported services
- Query for supported characteristics
- Interogate device for information
- Subscribe to device events such as connection status changed and value changed

## Resources

