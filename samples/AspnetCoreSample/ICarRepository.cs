using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspnetCoreSample
{
    public interface ICarRepository
    {
        Task<Car> FindAsync(string partitionKey, string id);

        Task RegisterAsync(Car car);

        Task UpdateAsync(Car car);

        Task<IEnumerable<Car>> GetCarsByCarCategoryAsync(string carCategory);
    }
}