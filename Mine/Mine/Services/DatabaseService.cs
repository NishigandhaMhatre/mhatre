using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using System.Linq;
using System.Threading.Tasks;
using Mine.Models;

namespace Mine.Services
{
    //lazy initialization prevents the database loading process from delaying the app launch
    public class DatabaseService: IDataStore<ItemModel>
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        });

        static SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;

        /// <summary>
        /// Constructor for DB service
        /// </summary>
        public DatabaseService()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        /// <summary>
        /// Initialize the database 
        /// </summary>
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

        /// <summary>
        /// Create new record 
        /// </summary>
        public async Task<bool> CreateAsync(ItemModel item)
        {
            await Database.InsertAsync(item);
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Read record from database
        /// </summary>
        public async Task<ItemModel> ReadAsync(String id)
        {
            return await Database.Table<ItemModel>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// UPdate record in the database
        /// </summary>
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

        /// <summary>
        /// Delete record from database
        /// </summary>
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
        /// <summary>
        /// Sync the List with the latest data
        /// </summary>
        public async Task<IEnumerable<ItemModel>> IndexAsync(bool forceRefresh = false)
        {
            return await Database.Table<ItemModel>().ToListAsync();
        }

        /// <summary>
        /// Method to WipeDataList
        /// </summary>
        public void WipeDataList()
        {
            Database.DropTableAsync<ItemModel>().GetAwaiter().GetResult();
            Database.CreateTablesAsync(CreateFlags.None, typeof(ItemModel)).ConfigureAwait(false).GetAwaiter().GetResult();
        }

    }
}
