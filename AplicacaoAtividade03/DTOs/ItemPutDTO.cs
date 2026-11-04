namespace AplicacaoAtividade03.DTOs
{
    // DTO DE ENTRADA (PUT): ALTERAR UM ITEM. NAO TEM TipoItem, POIS O TIPO NAO MUDA
    public class ItemPutDTO
    {
        public int IdCategoria { get; set; }
        public string? Item { get; set; }
        public string? DescricaoItem { get; set; }
        public decimal Preco { get; set; }
        public string? AtivoProdutoServico { get; set; }

        // SO SAO USADOS QUANDO O ITEM JA E UM PRODUTO
        public int? CodigoPreco { get; set; }
        public decimal? PrecoVenda { get; set; }
        public string? Unidade { get; set; }
    }
}