namespace Mine.Models
{
    /// <summary>
    /// Item for the Game
    /// </summary>
    public class ItemModel : BaseModel
    {
        // Add Unique attributes for Item
        // The Value of Item
        public int Value { get; set; } = 0;

        public bool Update(ItemModel data)
        {
            //Update the base
            Name = data.Name;
            Description = data.Description;

            //Update the extended
            Value = data.Value;
            return true;
        }
    }
}