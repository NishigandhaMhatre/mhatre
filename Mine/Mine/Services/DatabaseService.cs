using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using System.Linq;
using System.Threading.Tasks;
using Mine.Models;

namespace Mine.Services
{
    public class DatabaseService
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        });

        static SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;

        public DatabaseService()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(ItemModel).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(ItemModel)).ConfigureAwait(false);
                    initialized = true;
                }
            }
        }

        public Task<int> CreateAsync(ItemModel item)
        {
            return Database.InsertAsync(item);
        }

        public Task<ItemModel> ReadAsync(String id)
        {
            return Database.Table<ItemModel>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public Task<int> UpdateAsync(ItemModel item)
        {
            return Database.UpdateAsync(item);
        }

        public Task<int> DeleteAsync(ItemModel item)
        {
            return Database.DeleteAsync(item);
        }
    }
}
