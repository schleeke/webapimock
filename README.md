# Web API mockup service
![.NET Core](https://github.com/schleeke/webapimock/workflows/.NET%20Core/badge.svg)

Small http server for mock-up purpose. The service returns a custom HTTP response to any request that is configured. Therefore it holds a database with data for requests and responses to look up which can be altered by using several web API methods the service offers. Every request is analyzed by it's route (relative path/url), its query parameters, HTTP method and request body content.

---

* Only requests with a specific (configurable) route-prefix will be processed for mock-up.
* The quadrinity of route, method, query and body is checked for a corresponding definition and will return its response if such exists.

# Installation
The installation ist just copy & pasting the application files to their destination directory.

The service can be started from the command line by calling *webapimock.exe* or it can be installed as a service by calling *webapimock.exe install* from the command line. Please visit the [Topshelf command line reference](https://docs.topshelf-project.com/en/latest/overview/commandline.html) for further information about available command line parameters.

The service will be installed using the name '*webapimock.core*' and the display name '*.NET Core Web API Mockup Service*'.
