using System.Linq;

namespace WebApiMock.Data {
    public static class DataExtensions {

        public static MockupResponse ToMockupResponse(this ResponseDefinition response) => new MockupResponse {
            Id = response.Id,
            StatusCode = response.StatusCode,
            Response = response.Response,
            MimeType = response.MimeType };

        public static MockupRequest ToMockupRequest(this RequestDefinition request) => new MockupRequest {
            Id = request.Id,
            Route = request.Route,
            HttpMethod = request.Method.ToMethodString(),
            ResponseId = request.MockupResponseId,
            Query = request.Query,
            Body = request.Body };

        public static ResponseDefinition ToResponseDefinition(this MockupResponse response) {
            if(response.Id == 0) {
                return new ResponseDefinition {
                    Id = 0,
                    StatusCode = response.StatusCode,
                    MimeType = response.MimeType,
                    Response = response.Response }; }
            using var ctx = new DataContext();
            if (!ctx.Responses.Any(r => r.Id.Equals(response.Id))) { return null; }
            var retVal = ctx.Responses.Single(r => r.Id.Equals(response.Id));
            return retVal;
        }

        public static RequestDefinition ToRequestDefinition(this MockupRequest request) {
            if(request.HttpMethod.ToMethodEnum() == HttpMethodEnum.Unknown) {
                throw new WebApiMockException($"The HTTP method '{request.HttpMethod}' is unknown.", 30); }
            if(request.Id == 0) {
                return new RequestDefinition {
                    Id = 0,
                    Method = request.HttpMethod.ToMethodEnum(),
                    Route = request.Route,
                    Body = request.Body,
                    Query = request.Query,
                    MockupResponseId = 0 }; }
            using var ctx = new DataContext();
            if (!ctx.Requests.Any(r => r.Id.Equals(request.Id))) { return null; }
            var retVal = ctx.Requests.Single(r => r.Id.Equals(request.Id));
            return retVal;
        }
    }
}
