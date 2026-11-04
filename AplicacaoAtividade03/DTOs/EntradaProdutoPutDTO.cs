namespace AplicacaoAtividade03.DTOs
{
    //DTO DE ENTRADA (PUT) : ALTERAR UMA ENTRADA
    //NAO TEM IdItem DE PROPOSITO: TROCAR O PRODUTO MEXERIA NO ESTOQUE DE DOIS PRODUTOS
    public class EntradaProdutoPutDTO
    {
        public DateOnly? DataEntrada { get; set; }
        public int Quantidade { get; set; }
        public string? Unidade { get; set; }
        public decimal ValorUnitario { get; set; }
    }
}
