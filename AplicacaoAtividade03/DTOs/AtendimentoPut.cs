using System.Reflection.Metadata.Ecma335;

namespace AplicacaoAtividade03.DTOs
{
    public class AtendimentoPut
    {
        public int IdAtendimento { get; set; }
        public int IdPessoa { get; set; }
        public DateOnly DataAtendimento { get; set; }
        public string StatusAtendimento { get; set; }
        public List<AtendimentoItemPutDto> Itens { get; set; } = new();
    }
}
