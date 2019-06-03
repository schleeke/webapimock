# WebAPI Mockup Service

A small HTTP server for easily mocking web requests.

Please read this document carefully and configure the WebAPI Mockup Service so that it will fit your environment and requirements.

<div style="margin-top: 32px;">

## Table Of Contents

1.  [Installation](#installation)
2.  [Configuration](#configuration)
3.  [Start/Run](#start_run)
4.  [Defining mock up values](#define_mock_values)
5.  [Logging](#logging)
6.  [Special URLs](#special_urls)

</div>

<div style="margin-top: 64px; background-color:whitesmoke; padding: 16px;"><a name="installation"></a>

## Installation

The program needs no special installation/setup routine and can be copied to any directory that suits your needs.

<div style="margin-left: 16px;">

#### SSL/TLS

The service itself has no capabilities for specifying certificates for a SSL/TLS connection. Please use the windows-internal tools like _netsh.exe_ to define a certificate for a certain address.

</div>

</div>

<div style="margin-top: 32px; background-color:whitesmoke; padding: 16px;"><a name="configuration"></a>

## Configuration

The application is configured by its application config file named _WebApiMock.exe.config_. It is written in XML style and can be changed by the text editor of your choice.

The values that can/should be altered are the keys _BaseUrl_ and _CreateDefaultResponse_ that can be found within the _appSetings_ node.

<div style="margin-left: 16px;">

### _BaseUrl_

Defines the address the web server is listening at. An asterisk means that the server will listen on any local IP address or to the machine's DNS name. The port that the server is listening to can also be defined in the base URL and follows the standard rules of being separated by a colon and standing right after the host name. A port must always be specified.

<pre><ins>Examples:</ins></pre>

<pre>http://*:80/MyMockUpService</pre>

<pre>http://192.168.0.1:1337/MyMockUpService</pre>

<pre>https://servername:443/Service/Mockup</pre>

</div>

<div style="margin-left: 16px;margin-top: 32px;">

### _CreateDefaultResponse_

Specifies if the service should create a default response (and a sub-directory that correlates with the query [see '[Defining mock up values](#define_mock_values)' for further details]) if none is defined. The value can be _false_ or _true_.

Please keep in mind that 'foreign' requsts will affect your HDD and your directory hierarchy beneath the application's one. We recommend setting the value to 'true' only in trusted environments.

</div>

</div>

<div style="margin-top: 32px; background-color:whitesmoke; padding: 16px;"><a name="start_run"></a>

## Start/Run

The application can be started from the command line by simply calling the WebApiMock.exe from the command line.

To install the application as a service, simply add the parameter install to the EXE call. To remove the windows service, just use the parameter uninstall to do so.

The application uses Topshelf to provide the service controller capabilites. Please refer to [their homepage](https://topshelf.readthedocs.io/en/latest/overview/commandline.html) to get detailed information about the possible parameters.

<pre><ins>Examples:</ins></pre>
<pre>WebApiMock.exe</pre>
<pre>WebApiMock.exe install</pre>
<pre>WebApiMock.exe uninstall</pre>
</div>

<div style="margin-top: 32px; background-color:whitesmoke; padding: 16px;"><a name="define_mock_values"></a>

## Defining mock up values

The value for the requests are fetched from files called response.json within subdirectories that correlate to the ones from the URL-path. Regard that only the path parts that are an extension of the configured base URL are used.

A client request of _http://*:1337/myService/mock/values/mycoolvalue_ will result in looking for values within the subdirectory path _/values/mycoolvalue[/response.json]_ if the base URL is configured as _http://*:1337/myService/mock_.

Create a file called <statuscode-number>.statuscode to return the status code that is defined by the file's name. E.g. 404.statuscode to return a status code 404 for the request.

JSON will only be returned if no custom status code for the request is specified. Otherwise a small HTML will be delivered.

</div>

<div style="margin-top: 32px; background-color:whitesmoke; padding: 16px;"><a name="logging"></a>

## Logging

Logging is provided via [log4net](https://logging.apache.org/log4net/) and is configured by the _WebApiMock.exe.config_ within the log4net-node. All log messages will be written to StdOut (in console mode) and also to a file called _WebApiMock.log_. Error messages will be written to a file called _error.log_ as well. Feel free to adjust the settings but keep in mind that [Topshelf's](http://topshelf-project.com/) output (the service controller within the EXE) is bound to log4net.

</div>

<div style="margin-top: 32px; background-color:whitesmoke; padding: 16px;"><a name="special_urls"></a>

## Special URLs

There are two special URLs that will deliver a HTML page: /index.html and /readme.html.

index.html just displays a 'welcome' in the browser an can be used for testing the general availability for the service.

readme.html contains the documentation for the service.

</div>
