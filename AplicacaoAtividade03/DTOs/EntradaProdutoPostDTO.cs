namespace AplicacaoAtividade03.DTOs
{
    //DTO DE ENTRADA (POST) : REGISTRAR UMA ENTRADA DE UM PRODUTO
    public class EntradaProdutoPostDTO
    {
        public int IdItem { get; set; }
        public DateOnly? DataEntrada { get; set; }
        public int Quantidade { get; set; }

        //OPCIONAL : SE NAO VIER ,USA A UNIDADE CADASTRADA NO PRODUTO
        public string? Unidade { get; set; }

        //VALOR DE AQUISICAO (CUSTO) POR UNIDADE
        public decimal ValorUnitario { get; set; }
    }
}
