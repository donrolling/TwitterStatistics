namespace Domain.Models.Twitter
{
    public class Data<T> where T : class
    {
        public T data { get; set; }
        public Users includes { get; set; }
    }
}