namespace AplicacaoAtividade03.DTOs
{
    // DTO DE ENTRADA (POST): CADASTRAR UM ITEM (PRODUTO OU SERVICO)
    public class ItemPostDTO
    {
        // DADOS TABELA ITEM
        public int IdCategoria { get; set; }
        public string? Item { get; set; }
        public string? DescricaoItem { get; set; }
        public decimal Preco { get; set; }

        // "P" = PRODUTO / "S" = SERVICO (NAO PODE SER ALTERADO DEPOIS)
        public string? TipoItem { get; set; }
        public string? AtivoProdutoServico { get; set; }

        // DADOS TABELA PRODUTO (SO SAO USADOS QUANDO TipoItem = "P")
        public int? CodigoPreco { get; set; }
        public decimal? PrecoVenda { get; set; }
        public string? Unidade { get; set; }
    }
}