# Web API mockup service

![master](https://github.com/schleeke/webapimock/actions/workflows/dotnetcore.yml/badge.svg)

Small http server for mock-up purpose. The service returns a custom HTTP response to any request that is configured. Therefore it holds a database with data for requests and responses to look up which can be altered by using several web API methods the service offers. Every request is analyzed by it's route (relative path/url), its query parameters, HTTP method and request body content.

---

* Only requests with a specific (configurable) route-prefix will be processed for mock-up.
* The quadrinity of route, method, query and body is checked for a corresponding definition and will return its response if such exists.

## Installation
The installation ist just copy & pasting the application files to their destination directory.

The service can be started from the command line by calling *webapimock.exe* or it can be installed as a service by calling *webapimock.exe install* from the command line. Please visit the [Topshelf command line reference](https://docs.topshelf-project.com/en/latest/overview/commandline.html) for further information about available command line parameters.

Since it's .NET core, the application should run on linux as well. The code seems to be ready to be platform-independent and Topshelf uses the DotNetCoreEnvironmentBuilder instead of the WindowsHostEnvironmentBuilder...

The service will be installed using the name '*webapimock.core*' and the display name '*.NET Core Web API Mockup Service*'.

## Configuration
The application/service is configured by a file called *appsettings.json* which resides in the application's root directory.

The following configuration sections are available:

* ### HTTPS URL and Port
  The HTTPS address (and port) the web server is listening to is configured in the section **Kestrel.Endpoints.Https.Url**. Valid URLs are e.g.:
  - https://myhost:1234
  - https://myhost.mydomain.com:1234
  - https://localhost:1234
  - https://0.0.0.0:1234
  - https://192.168.0.20:1234

  The default value (if entry is absent) is https://localhost:5001 as defined in the file */Properties/launchSettings.json*.

* ### HTTP URL and Port
  The HTTP address (and port) the web server is listening to is configured in the section **Kestrel.Endpoints.Http.Url**. Valid URLs are e.g.:
  - http://myhost:1234
  - http://myhost.mydomain.com:1234
  - http://localhost:1234
  - http://0.0.0.0:1234
  - http://192.168.0.20:1234

  The default value (if entry is absent) is http://localhost:5000 as defined in the file */Properties/launchSettings.json*.

* ### <a name="mockup-config-header"></a>Mock-up route prefix
  The route's prefix to detect if a request shall be mocked is configured in the section **WebApiMockSettings.MockupPrefixRoute**.

  > If the HTTP URL is set to 'http://localhost:8080' and the route prefix ist set to 'my-fake-api' the proper beginning of a request whose response shall be mocked then has to be 'http://localhost:8080/my-fake-api/[...]'.

  The default value (if entry is absent) is **mockup**.

* ### Auto generate responses for unknown requests
  Toggles the automatic creation of responses for unknown requests by adjusting the section **WebApiMockSettings.AutoGenerateAnswer**.

  > If a mock-up request is unknown, the service will automatically generate a response- and request-definition in the database for later editing.
  > The default status code for that response will be *200* and it will return an "*application/json*" response.

  The default value (if entry is asbsent) is **false**.

  Be aware that enabling this option might "*trash*" your database if the service is externally reachably.

## <a name="addresses-header"></a>Addresses
The mock-up service offers a few default address schemas for providing additional funtionality to manage and access it.

The following (relative) addresses are available:

* http(s)://[servername]:[port]/
  The address for the internal swashbuckle/swagger UI

* http(s)://[servername]:[port]/gui
  The internal GUI for managing the mock-up data in a convenient way

* http(s)://[servername]:[port]/[mockup prefix]
  The URL for retrieving mock-up responses

  The mockup prefix can be [configured](#mockup-config-header) in the *appsettings.json* and defaults to '*mockup*' if not configured.

## Defining requests / responses
The request- and response-definitions are stored in an [SQLite database](https://en.wikipedia.org/wiki/SQLite) (file name: mock-data.db) and creating, deleting or altering data is accomplished by using the service's web API methods. Each request is defined by the combination of route, (HTTP-)method, query and body - where query and body are optional. Each request refers to a response which means that a response definition has to be created first before creating a request definition. Each response consists of an HTTP status code and an (optional) content. If a content is specified, its MIME type has to be specified as well. A response can be refered by several requests (1:n) and therefore can be 'reused' in several requests.

The service hosts two controllers - request and response - which both support several HTTP methods for this purpose. 

The API is documented using [swagger/OpenAPI](https://swagger.io/specification/) and its specification is accessible via the relative URL **/swagger/v1/swagger.json** (see [Addresses](#addresses-header)).

A small (and very technical) GUI is available via the relative URL **/** (the 'root', see [Addresses](#addresses-header)) which allows viewing the data/entity specifications as well as executing the provided web API methods.

There is a lightweight yet more swift GUI available via the relative URL **/gui** (see [Addresses](#addresses-header)).

## Troubleshooting
By default, the application's logging-output only goes to STDOUT but as the application uses [log4net](https://logging.apache.org/log4net/release/features.html), it is possible to enable file logging by simply editing the XML-file logging.config and commenting out the reference to the fileAppender in the *root*-section.
Also consider lowering the severity filter to **DEBUG** or **ALL** instead of its default **INFO**.

The output-information and -targets are highly customizable and can be filtered/routed by severity/log level. Please refer to the [log4net documentation](https://logging.apache.org/log4net/release/config-examples.html) for further details.

## Technology & Licensing
* The source code is licensed under the [GNU General Public License v3.0](https://github.com/schleeke/webapimock/blob/master/LICENSE).
* The project is a [.NET Core](https://en.wikipedia.org/wiki/.NET_Core) console application which is self hosting a [Kestrel web server](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-3.1) for providing the HTTP capabilities.
* The application uses the [Topshelf](http://topshelf-project.com/) wrapper to be able to be installed and run as a service.
* The web server provides a set of web api methods for manipulating the underlying data.
* These methods are used by the web GUI which allows an easy way of managing the database's request/response data. It consists of static HTML-, CSS- and JS-files and uses [bootstrap](https://getbootstrap.com/).
* The web server also offers a [swashbuckle GUI](https://github.com/domaindrivendev/Swashbuckle) for performing the web api method calls via GUI.
* The data for the request- and reply-definitions are stored in an [SQLite database](https://en.wikipedia.org/wiki/SQLite).
* Logging is provided by using [log4net](https://logging.apache.org/log4net/release/features.html).
* Data grid presentation in HTML was simplified using [DataTables](https://datatables.net/).
