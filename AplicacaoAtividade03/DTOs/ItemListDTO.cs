namespace AplicacaoAtividade03.DTOs
{
    // DTO DE SAIDA: ITEM + CATEGORIA + (SE FOR PRODUTO) DADOS DE PRODUTO E ESTOQUE
    public class ItemListDTO
    {
        public int IdItem { get; set; }
        public int IdCategoria { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Item { get; set; } = string.Empty;
        public string DescricaoItem { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string TipoItem { get; set; } = string.Empty;
        public string AtivoProdutoServico { get; set; } = string.Empty;

        // FICAM null QUANDO O ITEM E UM SERVICO
        public int? CodigoPreco { get; set; }
        public decimal? PrecoVenda { get; set; }
        public string? Unidade { get; set; }
        public int? QuantidadeEstoque { get; set; }
    }
}