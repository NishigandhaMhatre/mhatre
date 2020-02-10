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

        public async Task<int> CreateAsync(ItemModel item)
        {
            return await Database.InsertAsync(item);
        }

        public async Task<ItemModel> ReadAsync(String id)
        {
            return await Database.Table<ItemModel>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(ItemModel item)
        {
            var data = await ReadAsync(item.Id);
            if (data == null)
            {
                return false;
            }
            var result = await Database.UpdateAsync(item);
            return result==1;
        }

        public async Task<bool> DeleteAsync(String id)
        {
            var item = await ReadAsync(id);
            if (item == null)
            {
                return false;
            }
            var result = Database.DeleteAsync(item);
            return true;
        }

        public async Task<List<ItemModel>> IndexAsync()
        {
            return await Database.Table<ItemModel>().ToListAsync();
        }
    }
}
