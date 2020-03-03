# webapimock
Small http server for mock-up purposes. The service returns a custom HTTP response to any request that is configured. Therefore it holds a database with data for requests and responses to look up which can be altered by using several web API methods the service offers. Every request is analyzed by it's route (relative path/url), its query parameters, HTTP method and request body content.

![.NET Core](https://github.com/schleeke/webapimock/workflows/.NET%20Core/badge.svg)
