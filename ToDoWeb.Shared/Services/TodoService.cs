using Postgrest;
using ToDoWeb.Shared.Models;

namespace ToDoWeb.Shared.Services
{
    public class TodoService
    {
        private readonly Supabase.Client _client;

        public TodoService(Supabase.Client client)
        {
            _client = client;
        }

        public async Task<List<TaskItem>> GetTasksAsync(string userId)
        {
            var response = await _client.From<TaskItem>()
                .Filter("userid", Constants.Operator.Equals, userId)
                .Order("created_at", Constants.Ordering.Descending)
                .Get();
            return response.Models;
        }

        public async Task<bool> SetStatusAsync(TaskItem task, string status)
        {
            var previous = task.Status;
            task.Status = status;
            try
            {
                await task.Update<TaskItem>();
                return true;
            }
            catch
            {
                task.Status = previous;
                return false;
            }
        }

        public async Task<TaskItem?> AddAsync(TaskItem task)
        {
            var response = await _client.From<TaskItem>().Insert(task);
            return response.Models.FirstOrDefault();
        }

        public async Task<TaskItem?> GetTaskAsync(string id, string userId)
        {
            var response = await _client.From<TaskItem>()
                .Filter("id", Constants.Operator.Equals, id)
                .Filter("userid", Constants.Operator.Equals, userId)
                .Get();
            return response.Models.FirstOrDefault();
        }

        public async Task<bool> DeleteAsync(TaskItem task)
        {
            try
            {
                await task.Delete<TaskItem>();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}