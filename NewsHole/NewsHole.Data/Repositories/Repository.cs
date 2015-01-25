using NHibernate;

namespace NewsHole.Data.Repositories
{
    public interface IRepository<T, K>
    {
        void Add(T item);
        T Get(K key);
        void Update(T item);
        void Delete(T item);
    }

    public class Repository<T, K> : IRepository<T, K>
    {
        ISession _session;

        public Repository(ISession session)
        {
            _session = session;
        }

        public void Add(T item)
        {
            using (var transaction = _session.BeginTransaction())
            {
                _session.Save(item);
                transaction.Commit();
            }
        }

        public T Get(K key)
        {
            return (T)_session.Get(typeof(T), key);
        }

        public void Update(T item)
        {
            using (var transaction = _session.BeginTransaction())
            {
                _session.Update(item);
                transaction.Commit();
            }
        }

        public void Delete(T item)
        {
            using (var transaction = _session.BeginTransaction())
            {
                _session.Delete(item);
                transaction.Commit();
            }
        }
    }
}
