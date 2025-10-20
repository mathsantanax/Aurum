namespace AurumApi.Models
{
    public class Category
    {
        public Guid Guid { get; private set; }
        public string NameCategory { get; private set; } = string.Empty!; // categoria ex: Alimentação, Moradia, etc...
        public string Description { get; private set; } = string.Empty!; // descrição da categoria de gastos ex: NameCategory = Alimentação, Descrição = Mercado, Restaurantes, Bar etc...
        public DateTime CreatedAt { get; private set; }

        public Category() { }
        public Category(string nameCategory, string descricao)
        {
            this.Guid = Guid.NewGuid();
            this.NameCategory = nameCategory;
            this.Description = descricao;
            this.CreatedAt = DateTime.Now;
        }
    }
}
