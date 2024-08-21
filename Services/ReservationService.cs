using BeanScene.Data;
using BeanScene.Models;
using System.Linq;

namespace BeanScene.Services
{
    public class ReservationService : IReservationService
    {
        private readonly BSDBContext Dbx;

        public ReservationService(BSDBContext _Dbx)
        {
            Dbx = _Dbx;
        }

        public bool Add(Reservation model)
        {
            try
            {
                Dbx.Reservation.Add(model);
                Dbx.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Reservation GetById(int id)
        {
            return Dbx.Reservation.Find(id);
        }

        public IQueryable<Reservation> List()
        {
            return Dbx.Reservation.AsQueryable();
        }

        public bool Update(Reservation model)
        {
            try
            {
                Dbx.Reservation.Update(model);
                Dbx.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Delete(Reservation model)
        {
            try
            {
                Dbx.Reservation.Remove(model);
                Dbx.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
