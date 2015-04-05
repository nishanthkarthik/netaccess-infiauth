using System.Threading.Tasks;
using RestSharp;

namespace netaccess
{
    public static class RestSharpExtensions
    {
        public static Task<IRestResponse> ExecuteAwait(this RestClient client, RestRequest request)
        {
            TaskCompletionSource<IRestResponse> taskCompletionSource = new TaskCompletionSource<IRestResponse>();
            client.ExecuteAsync(request, (response, asyncHandle) =>
            {
                if (response.ResponseStatus == ResponseStatus.Error)
                {
                    taskCompletionSource.SetException(response.ErrorException);
                }
                else
                {
                    taskCompletionSource.SetResult(response);
                }
            });
            return taskCompletionSource.Task;
        }
    }
}
