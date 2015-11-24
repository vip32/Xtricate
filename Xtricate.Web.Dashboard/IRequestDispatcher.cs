using System.Threading.Tasks;

namespace Xtricate.Web.Dashboard
{
    public interface IRequestDispatcher
    {
        Task Dispatch(RequestDispatcherContext context);
    }
}
