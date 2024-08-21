using BeanScene.Models;

namespace BeanScene.Services
{
    public interface IReservationService
    {
        
            public bool Add(Reservation model);
            public bool Update(Reservation model);
            public bool Delete(Reservation model); 
            public Reservation GetById(int id);
            public IQueryable<Reservation> List();
        
    }
}
