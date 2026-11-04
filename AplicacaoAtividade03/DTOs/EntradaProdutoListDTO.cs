namespace AplicacaoAtividade03.DTOs
{
    // DTO DE SAIDA: ENTRADA + NOME DO PRODUTO + SALDO ATUAL DO ESTOQUE
    public class EntradaProdutoListDTO
    {
        public int IdEntradaProduto { get; set; }
        public int IdItem { get; set; }
        public string Produto { get; set; } = string.Empty;
        public DateOnly DataEntrada { get; set; }
        public int Quantidade { get; set; }
        public string Unidade { get; set; } = string.Empty;
        public decimal ValorUnitario { get; set; }

        // CALCULADO NA SAIDA: QUANTIDADE x VALOR UNITARIO
        public decimal ValorTotal { get; set; }
        public int EstoqueAtual { get; set; }
    }
}