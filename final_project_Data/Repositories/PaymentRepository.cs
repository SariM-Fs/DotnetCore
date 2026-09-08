using final_project_Core.Entities;
using final_project_Core.Repositories;

namespace final_project_Data.Repositories
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context)
        {
        }
    }
}
