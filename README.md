#FSX Playground
A collection of experiments using the Flight Simulator SDK

* **FSXBroadcast** is a Windows app that sends airplane info to the iPad every second
* **FSXTrack** is an iPad app that shows the airplane position on a map, and some other info

**WARNING** This is highly experimental code:

* It's the result of playing with a completely new SDK and platform over the course of a weekend (never programmed for Windows before), so expect bugs and awful code
* The network protocol is not very iOS friendly, the iPad app will crash when the connection is dropped, and the host name is hardcoded.
* It's a cool proof of concept, but don't rely on it as a navigation aid: the refresh rate is not optimal, and coordinates don't seem to align very well on Google Maps

## Screenshots

![FSXBroadcast](https://github.com/koke/FSXPlayground/blob/master/FSXBroadcast/Screenshot.PNG?raw=true)

![FSXTrack](https://github.com/koke/FSXPlayground/blob/master/FSXTrack/Screenshot.png?raw=true)