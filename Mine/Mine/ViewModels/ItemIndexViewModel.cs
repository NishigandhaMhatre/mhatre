using Mine.Services;
using Mine.Models;
using Mine.Views;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Linq;

namespace Mine.ViewModels
{
    /// <summary>
    /// Index View Model
    /// Manages the list of data records
    /// </summary>
    public class ItemIndexViewModel : BaseViewModel
    {
        // The Data set of records
        public ObservableCollection<ItemModel> Dataset { get; set; }

        /// <summary>
        /// Connection to the Data store
        /// </summary>
        public IDataStore<ItemModel> DataStore => DependencyService.Get<IDataStore<ItemModel>>();

        // Command to force a Load of data
        public Command LoadDatasetCommand { get; set; }

        private bool _needsRefresh;

        public bool usingDatabase = true;
        public bool usingStore = false;

        /// <summary>
        /// Constructor
        /// 
        /// The constructor subscribes message listeners for crudi operations
        /// </summary>
        public ItemIndexViewModel()
        {
            Title = "Items";

            Dataset = new ObservableCollection<ItemModel>();
            LoadDatasetCommand = new Command(async () => await ExecuteLoadDataCommand());

            // Register the Create Message
            MessagingCenter.Subscribe<ItemCreatePage, ItemModel>(this, "Create", async (obj, data) =>
            {
                await Add(data as ItemModel);
            });

            //Register the Update Message
            MessagingCenter.Subscribe<ItemUpdatePage, ItemModel>(this, "Update", async (obj, data) =>
            {
                await Update(data as ItemModel);
            });

            //Register the Delete Message
            MessagingCenter.Subscribe<ItemDeletePage, ItemModel>(this, "Delete", async (obj, data) =>
            {
                await Delete(data as ItemModel);
            });
        }

        /// <summary>
        /// API to add the Data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> Add(ItemModel data)
        {
            Dataset.Add(data);
            if (usingDatabase == true)
            {
                await App.Database.CreateAsync(data);
            }
            if (usingStore == true)
            {
                var result = await DataStore.CreateAsync(data);
            }
            return true;
        }

        /// <summary>
        /// API to Delete the Data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> Delete(ItemModel data)
        {
            var record = await Read(data.Id);
            var result = false;
            if (record == null)
            {
                return false;
            }
            if (usingStore == true)
            {
                Dataset.Remove(data);
                result = await DataStore.DeleteAsync(data.Id);
            }
            if (usingDatabase == true)
            {
                result = await App.Database.DeleteAsync(data.Id);
            }

            return result;
        }

        /// <summary>
        /// API to read the Data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>a

        public async Task<ItemModel> Read(string id)
        {
            ItemModel result = null;
            if (usingDatabase == true)
            {
                result = await App.Database.ReadAsync(id);
            }
            if (usingStore == true)
            {
                result = await DataStore.ReadAsync(id);
            }
            return result;
        }

        /// <summary>
        /// API to Update the Data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> Update(ItemModel data)
        {
            var record = await Read(data.Id);
            bool result = false;
            if (record == null)
            {
                return false;
            }
            record.Update(data);
            if (usingStore == true)
            {
                result = await DataStore.UpdateAsync(record);
            }
            if (usingDatabase == true)
            {
                result = await App.Database.UpdateAsync(record);
            }

            await ExecuteLoadDataCommand();

            return result;
        }

        #region Refresh
        // Return True if a refresh is needed
        // It sets the refresh flag to false
        public bool NeedsRefresh()
        {
            if (_needsRefresh)
            {
                _needsRefresh = false;
                return true;
            }

            return false;
        }

        // Sets the need to refresh
        public void SetNeedsRefresh(bool value)
        {
            _needsRefresh = value;
        }

        // Command that Loads the Data
        private async Task ExecuteLoadDataCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                Dataset.Clear();
                //var dataset = await DataStore.IndexAsync(true);
                var dataset = await App.Database.IndexAsync();
                // Example of how to sort the database output using a linq query.
                // Sort the list
                dataset = dataset
                    .OrderBy(a => a.Name)
                    .ThenBy(a => a.Description)
                    .ToList();

                foreach (var data in dataset)
                {
                    Dataset.Add(data);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Force data to refresh
        /// </summary>
        public void ForceDataRefresh()
        {
            // Reset
            var canExecute = LoadDatasetCommand.CanExecute(null);
            LoadDatasetCommand.Execute(null);
        }
        #endregion Refresh
    }
}