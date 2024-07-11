# WPFSeparation_AnonymousPipes
A test skeleton of a WPF project that extracts a long running operation module into separate processes for performance

It's an empty WPF .NET 6.0 application, that starts and Hosts 10 instances of example "LongRunningProcess" module
It will automatically queue List of input Data and distribute it to idling instances of LongRunningProcess and then get the results.

Idea for the projects was as follows:
- There was a third party library that solves some problem, and returns a chunk of data, but performing operations on a larger scale would be slow.
- third party library required process separation in form of AppDomain or some IPC alternative, but could work locally and utilize machine resources freely.
- Chunks of input and output data are not too large (<1MB on average), the communication process via serialization is not too expensive

Use freely, hope it helps.
